using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.Tests
{
    /// <summary>
    /// ランタイムアセンブリ（MizoresPackageExporter.asmdef）に含まれる .cs ファイルに
    /// #if UNITY_EDITOR ガードなしでエディタ専用 API が混入していないことを検証する静的解析テスト。
    ///
    /// UnityEditor 名前空間はエディタ専用のため、ランタイムビルド（Android・iOS 等）では
    /// 利用できない。ガードなしの使用はビルドエラーの原因になる。
    ///
    /// 【対象】MizoresPackageExporter.asmdef が管理する全 .cs ファイル
    ///   = パッケージ直下の全 .cs（EditorScript/EditorOnly/ 配下は除外）
    /// </summary>
    public class EditorApiLeakTests
    {
        const string PACKAGE_RELATIVE_PATH =
            "Packages/com.mizore-nekoyanagi.utils.mizore-package-exporter";

        /// <summary>
        /// エディタ専用 asmdef に含まれるディレクトリ（スキャン対象外）。
        /// includePlatforms = ["Editor"] のアセンブリに属するため、
        /// UnityEditor の直接使用が許可されている。
        /// </summary>
        static readonly string[] EDITOR_ONLY_DIRS = new[]
        {
            "EditorScript/EditorOnly",
        };

        /// <summary>
        /// #if UNITY_EDITOR ガードなしに存在してはならないエディタ専用 API のパターン。
        /// エディタビルド以外では UnityEditor 名前空間が存在しないためビルドエラーになる。
        /// </summary>
        static readonly string[] EDITOR_API_PATTERNS = new[]
        {
            "using UnityEditor",
            "AssetDatabase.",
            "EditorApplication.",
            "EditorUtility.",
            "EditorPrefs.",
            "EditorGUILayout.",
            "EditorGUI.",
            "EditorWindow",
            "PrefabUtility.",
            "Handles.",
            "SceneView.",
            "Undo.",
            "EditorGUIUtility.",
            "EditorStyles.",
            "Selection.",
            "BuildPipeline.",
            "SerializedObject",
            "SerializedProperty",
            "EditorSceneManager.",
            "AnimationUtility.",
            "ObjectFactory.",
            "GameObjectUtility.",
            "EditorBuildSettings.",
            "PlayerSettings.",
        };

        // プリプロセッサ条件の状態
        enum ConditionalState { EditorGuarded, NonEditorGuarded, Other }

        [Test]
        public void RuntimeAssembly_HasNoUnguardedEditorApiUsage()
        {
            var projectRoot = Path.GetFullPath(
                Path.Combine(Application.dataPath, "..")).Replace('\\', '/');
            var packageRoot = Path.Combine(projectRoot, PACKAGE_RELATIVE_PATH)
                .Replace('\\', '/');

            Assert.IsTrue(Directory.Exists(packageRoot),
                $"パッケージディレクトリが存在すること: {packageRoot}");

            // スキャン対象外ディレクトリの絶対パス（末尾スラッシュなし）
            var excludedDirs = EDITOR_ONLY_DIRS
                .Select(d => (packageRoot + "/" + d).TrimEnd('/'))
                .ToArray();

            // ランタイムアセンブリに含まれる .cs ファイルを列挙
            var csFiles = Directory.GetFiles(packageRoot, "*.cs", SearchOption.AllDirectories)
                .Select(p => p.Replace('\\', '/'))
                .Where(p => !excludedDirs.Any(d => p.StartsWith(d + "/")))
                .OrderBy(p => p)
                .ToList();

            Assert.IsTrue(csFiles.Count > 0,
                "スキャン対象の .cs ファイルが少なくとも 1 件見つかること");

            var violations = new List<string>();
            foreach (var filePath in csFiles)
            {
                ScanFile(filePath, packageRoot, violations);
            }

            if (violations.Count > 0)
            {
                Assert.Fail(
                    $"{violations.Count} 件のエディタ API 混入を検出しました:\n" +
                    string.Join("\n", violations));
            }
        }

        static void ScanFile(string filePath, string packageRoot, List<string> violations)
        {
            var lines = File.ReadAllLines(filePath);
            var relPath = filePath.Substring(packageRoot.Length).TrimStart('/');
            var stack = new Stack<ConditionalState>();

            for (int lineIdx = 0; lineIdx < lines.Length; lineIdx++)
            {
                var trimmed = lines[lineIdx].Trim();

                // 空行・純コメント行はスキップ
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("//"))
                    continue;

                // インラインコメントを除去してコード部分のみを検査
                var code = StripLineComment(trimmed);
                if (string.IsNullOrWhiteSpace(code))
                    continue;

                // プリプロセッサディレクティブを処理
                if (code.StartsWith("#"))
                {
                    ProcessDirective(code, stack);
                    continue;
                }

                // エディタガード外でエディタ API が使われていないか検査
                bool isGuarded = stack.Any(s => s == ConditionalState.EditorGuarded);
                if (!isGuarded)
                {
                    foreach (var pattern in EDITOR_API_PATTERNS)
                    {
                        if (code.Contains(pattern))
                        {
                            violations.Add(
                                $"  {relPath}:{lineIdx + 1}: " +
                                $"'{pattern}' が #if UNITY_EDITOR ガードなしで使用されています");
                            break; // 1行につき1件だけ報告
                        }
                    }
                }
            }
        }

        static void ProcessDirective(string directive, Stack<ConditionalState> stack)
        {
            // "#if <condition>"
            if (directive.StartsWith("#if ") || directive == "#if")
            {
                var cond = directive.Substring("#if".Length).Trim();
                stack.Push(ClassifyCondition(cond));
            }
            // "#elif <condition>"
            else if (directive.StartsWith("#elif ") || directive == "#elif")
            {
                var cond = directive.Substring("#elif".Length).Trim();
                if (stack.Count > 0) stack.Pop();
                stack.Push(ClassifyCondition(cond));
            }
            // "#else" → ブロックの論理を反転
            else if (directive.StartsWith("#else"))
            {
                if (stack.Count > 0)
                {
                    var top = stack.Pop();
                    var flipped = top == ConditionalState.EditorGuarded
                        ? ConditionalState.NonEditorGuarded
                        : top == ConditionalState.NonEditorGuarded
                            ? ConditionalState.EditorGuarded
                            : ConditionalState.Other;
                    stack.Push(flipped);
                }
            }
            // "#endif"
            else if (directive.StartsWith("#endif"))
            {
                if (stack.Count > 0) stack.Pop();
            }
        }

        /// <summary>
        /// プリプロセッサ条件文字列を解析してブロックの状態を返す。
        /// "UNITY_EDITOR" または "UNITY_EDITOR &amp;&amp; ..." → EditorGuarded
        /// "!UNITY_EDITOR"                                    → NonEditorGuarded
        /// その他                                              → Other
        /// </summary>
        static ConditionalState ClassifyCondition(string condition)
        {
            if (condition == "UNITY_EDITOR")
                return ConditionalState.EditorGuarded;

            // "UNITY_EDITOR && SOMETHING" のような AND 複合条件は EditorGuarded に準じる
            if (condition.StartsWith("UNITY_EDITOR ") || condition.StartsWith("UNITY_EDITOR&"))
                return ConditionalState.EditorGuarded;

            if (condition == "!UNITY_EDITOR")
                return ConditionalState.NonEditorGuarded;

            return ConditionalState.Other;
        }

        /// <summary>
        /// コード行からインラインコメント "//" 以降を除去する。
        /// 通常文字列リテラル（"..."）および verbatim 文字列（@"..."）内の "//" は除去しない。
        /// </summary>
        static string StripLineComment(string code)
        {
            bool inString = false;
            bool inVerbatim = false;
            bool inChar = false;
            int i = 0;
            while (i < code.Length)
            {
                char c = code[i];
                if (inVerbatim)
                {
                    if (c == '"')
                    {
                        if (i + 1 < code.Length && code[i + 1] == '"')
                        {
                            i += 2; continue; // verbatim 内の "" はエスケープされた引用符
                        }
                        inVerbatim = false;
                    }
                }
                else if (inString)
                {
                    if (c == '\\') { i += 2; continue; }
                    if (c == '"') inString = false;
                }
                else if (inChar)
                {
                    if (c == '\\') { i += 2; continue; }
                    if (c == '\'') inChar = false;
                }
                else
                {
                    if (c == '@' && i + 1 < code.Length && code[i + 1] == '"')
                    {
                        inVerbatim = true;
                        i += 2; continue;
                    }
                    if (c == '"') { inString = true; }
                    else if (c == '\'') { inChar = true; }
                    else if (c == '/' && i + 1 < code.Length && code[i + 1] == '/')
                    {
                        return code.Substring(0, i).TrimEnd();
                    }
                }
                i++;
            }
            return code;
        }
    }
}
