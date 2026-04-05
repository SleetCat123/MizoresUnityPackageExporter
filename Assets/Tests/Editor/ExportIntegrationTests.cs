using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using MizoreNekoyanagi.PublishUtil.PackageExporter;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.Tests
{
    /// <summary>
    /// 実ファイルを生成して本番と同じフロー（GetAllPath / GetPackageName）を通す統合テスト。
    /// SetUp でテスト用ファイルを作成し、TearDown で削除する。
    /// </summary>
    public class ExportIntegrationTests
    {
        const string TEST_ROOT       = "Assets/Tests_MizorePkgExp_Generated";
        const string EXPORT_TARGET   = TEST_ROOT + "/ExportTarget";
        const string ALPHA_MAT       = EXPORT_TARGET + "/Alpha.mat";
        const string BETA_MAT        = EXPORT_TARGET + "/Beta.mat";
        const string DEBUG_LOG       = EXPORT_TARGET + "/debug.log";
        const string VERSION_JSON    = TEST_ROOT + "/version.json";

        MizoresPackageExporter exporter;

        static Material CreateTestMaterialAsset(string assetPath)
        {
            var template = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
            Material material;
            if (template != null)
            {
                material = new Material(template);
            }
            else
            {
                var shader = Shader.Find("Standard")
                    ?? Shader.Find("Sprites/Default")
                    ?? Shader.Find("Legacy Shaders/Diffuse");
                if (shader == null)
                {
                    throw new System.InvalidOperationException("テスト用 Material を生成するための Shader が見つかりません。");
                }

                material = new Material(shader);
            }

            material.name = Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.CreateAsset(material, assetPath);
            return material;
        }

        [SetUp]
        public void SetUp()
        {
            // --- テスト用アセットを生成 ---
            Directory.CreateDirectory(EXPORT_TARGET);
            File.WriteAllText(DEBUG_LOG,    "log data");
            File.WriteAllText(VERSION_JSON, "{\"version\": \"3.5.1\"}");
            AssetDatabase.Refresh();
            CreateTestMaterialAsset(ALPHA_MAT);
            CreateTestMaterialAsset(BETA_MAT);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // --- Exporter をメモリ上に生成（アセットとして保存不要） ---
            exporter = ScriptableObject.CreateInstance<MizoresPackageExporter>();
            exporter.name = "IntegrationTestExporter";

            // エクスポート対象フォルダを登録（依存解決なし）
            var targetElement = new ExportTargetObjectElement(EXPORT_TARGET);
            targetElement.searchReference = false;
            exporter.objects.Add(targetElement);

            // .log ファイルの除外パターンを登録
            exporter.excludes.Add(new SearchPath(SearchPathType.EndsWith, false, ".log"));

            // バージョン情報を JSON ファイルから読む設定
            exporter.packageNameSettings.versionSource       = VersionSource.File;
            exporter.packageNameSettings.versionFile         = new ObjectRefElement(VERSION_JSON);
            exporter.packageNameSettings.packageName         = "IntegrationTestExporter%versionf%";

            // バージョンファイルを即時読み込み（キャッシュ更新）
            exporter.UpdateAllExportVersions();
        }

        [TearDown]
        public void TearDown()
        {
            if (exporter != null)
                Object.DestroyImmediate(exporter);
            exporter = null;

            if (Directory.Exists(TEST_ROOT))
            {
                Directory.Delete(TEST_ROOT, recursive: true);
                var metaPath = TEST_ROOT + ".meta";
                if (File.Exists(metaPath))
                    File.Delete(metaPath);
            }
            AssetDatabase.Refresh();
        }

        // ===== GetAllPath =====

        /// <summary>
        /// .mat ファイル 2 件が paths に含まれること。
        /// </summary>
        [UnityTest]
        public IEnumerator GetAllPath_MatFiles_AppearInPaths()
        {
            FilePathList result = null;
            Task task = exporter.GetAllPath(list => result = list, string.Empty);
            while (!task.IsCompleted)
                yield return null;
            if (task.IsFaulted)
                throw task.Exception.InnerException ?? task.Exception;

            Assert.IsNotNull(result, "GetAllPath のコールバックが呼ばれること");
            var paths = result.paths.ToList();
            Assert.AreEqual(2, paths.Count,
                $"Alpha.mat と Beta.mat だけが paths に含まれるべき。実際: {string.Join(", ", paths)}");
            CollectionAssert.Contains(paths, ALPHA_MAT);
            CollectionAssert.Contains(paths, BETA_MAT);
        }

        /// <summary>
        /// .log ファイルが excludePaths に含まれること。
        /// </summary>
        [UnityTest]
        public IEnumerator GetAllPath_LogFile_ExcludedByEndsWith_dot_log()
        {
            FilePathList result = null;
            Task task = exporter.GetAllPath(list => result = list, string.Empty);
            while (!task.IsCompleted)
                yield return null;
            if (task.IsFaulted)
                throw task.Exception.InnerException ?? task.Exception;

            var excludePaths = result.excludePaths.ToList();
            CollectionAssert.Contains(excludePaths, DEBUG_LOG,
                "EndsWith '.log' パターンにより debug.log は excludePaths に入るべき");
            CollectionAssert.DoesNotContain(result.paths.ToList(), DEBUG_LOG,
                "debug.log は paths には含まれないこと");
        }

        /// <summary>
        /// excludeObjects にフォルダを追加した場合、プレフィックスが一致する兄弟フォルダは除外されないこと。
        /// GetAllPath でフォルダ自身を Exact、配下を StartsWith + "/" で除外する修正のリグレッションテスト。
        /// </summary>
        [UnityTest]
        public IEnumerator GetAllPath_ExcludeObjects_DoesNotAffectSiblingFolderWithMatchingPrefix()
        {
            const string SIBLING      = TEST_ROOT + "/ExportTargetSibling";
            const string SIBLING_FILE = SIBLING   + "/SiblingFile.mat";

            try
            {
                Directory.CreateDirectory(SIBLING);
                AssetDatabase.Refresh();
                CreateTestMaterialAsset(SIBLING_FILE);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                var siblingElement = new ExportTargetObjectElement(SIBLING);
                siblingElement.searchReference = false;
                exporter.objects.Add(siblingElement);

                exporter.excludeObjects.Add(new ObjectRefElement(EXPORT_TARGET));

                FilePathList result = null;
                Task task = exporter.GetAllPath(list => result = list, string.Empty);
                while (!task.IsCompleted)
                    yield return null;
                if (task.IsFaulted)
                    throw task.Exception.InnerException ?? task.Exception;

                var paths = result.paths.ToList();
                CollectionAssert.DoesNotContain(paths, ALPHA_MAT,
                    "excludeObjects に指定されたフォルダ配下のファイルは paths に含まれないこと");
                CollectionAssert.Contains(paths, SIBLING_FILE,
                    "プレフィックスが一致する兄弟フォルダ配下のファイルは paths に含まれること");
            }
            finally
            {
                if (Directory.Exists(SIBLING))
                {
                    Directory.Delete(SIBLING, recursive: true);
                }

                var meta = SIBLING + ".meta";
                if (File.Exists(meta))
                {
                    File.Delete(meta);
                }

                AssetDatabase.Refresh();
            }
        }

        // ===== GetPackageName =====

        [Test]
        public void GetPackageName_NameVariable_ReplacedWithExporterName()
        {
            exporter.packageNameSettings.packageName = "%name%_pkg";

            var result = exporter.GetPackageName(string.Empty);

            Assert.AreEqual("IntegrationTestExporter_pkg", result);
        }

        /// <summary>
        /// version.json から読んだバージョンがファイル名用に正規化され、
        /// %versionf% を通じてパッケージ名に反映されること。
        /// </summary>
        [Test]
        public void GetPackageName_VersionReadFromJsonFile_ReflectedInPackageName()
        {
            var packageName = exporter.GetPackageName(string.Empty);
            Assert.AreEqual("IntegrationTestExporter-3_5_1", packageName,
                "version.json の version フィールドはファイル名用に '.' -> '_' へ正規化されたうえで versionf 経由で展開されること");
        }

        /// <summary>
        /// プレーンテキストのバージョンファイル（非 JSON）の場合、最初の非空行が
        /// ファイル名用に正規化されたうえでバージョンとして使われること。
        /// UpdateExportVersion の JSON 以外のコードパスを検証する。
        /// </summary>
        [Test]
        public void GetPackageName_VersionReadFromPlainTextFile_ReflectedInPackageName()
        {
            const string VERSION_TXT = TEST_ROOT + "/version.txt";
            File.WriteAllText(VERSION_TXT, "\n2.0.0\n");   // 空行の後に "2.0.0"

            var exp = ScriptableObject.CreateInstance<MizoresPackageExporter>();
            try
            {
                exp.name = "PlainTextVersionExporter";
                exp.packageNameSettings.versionSource = VersionSource.File;
                exp.packageNameSettings.versionFile   = new ObjectRefElement(VERSION_TXT);
                exp.packageNameSettings.packageName   = "PlainTextVersionExporter%versionf%";
                exp.UpdateAllExportVersions();

                Assert.AreEqual("PlainTextVersionExporter-2_0_0", exp.GetPackageName(string.Empty),
                    "プレーンテキストファイルの最初の非空行 '2.0.0' はファイル名用に '.' -> '_' へ正規化されたうえで使われること");
            }
            finally
            {
                Object.DestroyImmediate(exp);
            }
        }

        [Test]
        public void GetFormattedVersion_WhenVersionIsEmpty_ReturnsEmpty()
        {
            var exp = ScriptableObject.CreateInstance<MizoresPackageExporter>();
            try
            {
                exp.name = "NoVersionExporter";
                exp.packageNameSettings.versionSource = VersionSource.String;
                exp.packageNameSettings.versionString = "";
                exp.UpdateAllExportVersions();

                Assert.AreEqual(string.Empty, exp.GetFormattedVersion(string.Empty));
            }
            finally
            {
                Object.DestroyImmediate(exp);
            }
        }

        [Test]
        public void GetFormattedVersion_WhenVersionSet_ReturnsVersionWithFormat()
        {
            var result = exporter.GetFormattedVersion(string.Empty);

            Assert.AreEqual("-3_5_1", result,
                "versionFormat のデフォルト '-%version%' に、ファイル名用に正規化された version '3_5_1' を代入した結果");
        }

        [Test]
        public void GetExportFileName_AppendsDotUnitypackage()
        {
            exporter.packageNameSettings.packageName = "TestPackage";

            var result = exporter.GetExportFileName(string.Empty);

            Assert.AreEqual("TestPackage.unitypackage", result);
        }

        /// <summary>
        /// variables ディクショナリに登録したカスタム変数が %key% 形式でパッケージ名に展開されること。
        /// </summary>
        [Test]
        public void GetPackageName_CustomVariable_ReplacedInPackageName()
        {
            exporter.variables["platform"] = "Quest";
            exporter.packageNameSettings.packageName = "%name%_%platform%";

            var result = exporter.GetPackageName(string.Empty);

            Assert.AreEqual("IntegrationTestExporter_Quest", result);
        }

        /// <summary>
        /// packageNameSettingsOverride に登録されたキーの設定が優先されること。
        /// useOverride_packageName=true のオーバーライドはベースの packageName を無視する。
        /// </summary>
        [Test]
        public void GetPackageName_WithBatchExportKeyOverride_UsesOverriddenPackageName()
        {
            var overrideSettings = new PackageNameSettings();
            overrideSettings.useOverride_packageName = true;
            overrideSettings.packageName = "OverriddenPkg";
            exporter.packageNameSettingsOverride["beta"] = overrideSettings;

            var result = exporter.GetPackageName("beta");

            Assert.AreEqual("OverriddenPkg", result,
                "useOverride_packageName=true のオーバーライドはベースの packageName を上書きすること");
        }

        /// <summary>
        /// packageNameSettingsOverride に登録されていないキーを渡した場合、
        /// ベース設定の packageName が使われること（設計仕様）。
        /// </summary>
        [Test]
        public void GetPackageName_WithUnregisteredBatchExportKey_UsesBaseSettings()
        {
            exporter.packageNameSettings.packageName = "%name%_base";

            var result = exporter.GetPackageName("unknown_key");

            Assert.AreEqual("IntegrationTestExporter_base", result,
                "オーバーライドが登録されていないキーはベース設定の packageName を使うこと");
        }

        /// <summary>
        /// version だけはベースを継承しつつ、batchFormat と packageName だけを override した場合でも、
        /// 最終的なパッケージ名が各フラグどおりに組み立てられること。
        /// </summary>
        [Test]
        public void GetPackageName_WithMixedOverrideFlags_CombinesInheritedAndOverriddenValues()
        {
            exporter.packageNameSettings.versionSource = VersionSource.String;
            exporter.packageNameSettings.versionString = "9.8.7";
            exporter.packageNameSettings.versionFormat = "_v%version%";
            exporter.packageNameSettings.batchFormat = "_%batch%";
            exporter.packageNameSettings.packageName = "%name%%batchf%%versionf%";
            exporter.variables["platform"] = "Quest";

            var overrideSettings = new PackageNameSettings();
            overrideSettings.useOverride_version = false;
            overrideSettings.versionSource = VersionSource.String;
            overrideSettings.versionString = "0.0.1";
            overrideSettings.useOverride_versionFormat = false;
            overrideSettings.versionFormat = "ignored-%version%";
            overrideSettings.useOverride_batchFormat = true;
            overrideSettings.batchFormat = "[%batch%]";
            overrideSettings.useOverride_packageName = true;
            overrideSettings.packageName = "%name%%batchf%%versionf%_%platform%";
            exporter.packageNameSettingsOverride["beta"] = overrideSettings;

            exporter.UpdateAllExportVersions();

            var result = exporter.GetPackageName("beta");

            Assert.AreEqual("IntegrationTestExporter[beta]_v9.8.7_Quest", result,
                "useOverride_version=false ではベース版数を継承しつつ、batchFormat と packageName は override 側が使われること");
        }

        // ===== GetAllPath バウンダリ =====

        /// <summary>
        /// objects が空のとき、GetAllPath の paths および excludePaths は両方空であること。
        /// </summary>
        [UnityTest]
        public IEnumerator GetAllPath_EmptyObjectsList_ReturnsBothPathsEmpty()
        {
            var exp = ScriptableObject.CreateInstance<MizoresPackageExporter>();
            // objects は空のまま
            FilePathList result = null;
            Task task = exp.GetAllPath(list => result = list, string.Empty);
            while (!task.IsCompleted)
                yield return null;
            if (task.IsFaulted)
                throw task.Exception.InnerException ?? task.Exception;

            Object.DestroyImmediate(exp);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.paths.Count(),
                "objects が空なら paths は空であること");
            Assert.AreEqual(0, result.excludePaths.Count(),
                "objects が空なら excludePaths も空であること");
        }

        // ===== References =====

        /// <summary>
        /// references に Include ファイルがある状態で searchReference = false のオブジェクトは直接 paths に追加される。
        /// </summary>
        [UnityTest]
        public IEnumerator GetAllPath_SearchReferenceFalse_FilesAddedDirectly_EvenWhenReferencesExist()
        {
            exporter.references.Add(new ReferenceElement(new ObjectRefElement(VERSION_JSON), ReferenceMode.Include));
            // SetUp の objects は searchReference = false なのでそのまま使う

            FilePathList result = null;
            Task task = exporter.GetAllPath(list => result = list, string.Empty);
            while (!task.IsCompleted)
                yield return null;
            if (task.IsFaulted)
                throw task.Exception.InnerException ?? task.Exception;

            var paths = result.paths.ToList();
            CollectionAssert.Contains(paths, ALPHA_MAT,
                "searchReference = false のオブジェクトは references があっても直接 paths に追加される");
            CollectionAssert.Contains(paths, BETA_MAT);
            CollectionAssert.DoesNotContain(paths, DEBUG_LOG,
                "excludes '.log' パターンにより debug.log は paths に入らない");
        }

        /// <summary>
        /// searchReference = true のオブジェクトは GetDependencies 経由でファイル自身が paths に入る。
        /// </summary>
        [UnityTest]
        public IEnumerator GetAllPath_SearchReferenceTrue_FileItselfAppearsInPathsViaSelfDependency()
        {
            exporter.references.Add(new ReferenceElement(new ObjectRefElement(VERSION_JSON), ReferenceMode.Include));

            exporter.objects.Clear();
            var matElement = new ExportTargetObjectElement(ALPHA_MAT);
            matElement.searchReference = true;
            exporter.objects.Add(matElement);

            FilePathList result = null;
            Task task = exporter.GetAllPath(list => result = list, string.Empty);
            while (!task.IsCompleted)
                yield return null;
            if (task.IsFaulted)
                throw task.Exception.InnerException ?? task.Exception;

            CollectionAssert.Contains(result.paths.ToList(), ALPHA_MAT,
                "searchReference = true かつ references あり: GetDependencies が自身を返すため paths に入る");
        }

        /// <summary>
        /// objects に Dynamic Path（%name%）を含む要素を登録した場合、
        /// GetAllPath がパスを解決してそのフォルダ配下のファイルを収集すること。
        /// </summary>
        [UnityTest]
        public IEnumerator GetAllPath_DynamicPathInObject_NameVariableResolvedToExporterName()
        {
            // exporter.name = "IntegrationTestExporter" のため
            // TEST_ROOT + "/%name%" = "Assets/Tests_MizorePkgExp_Generated/IntegrationTestExporter"
            const string DYNAMIC_DIR  = TEST_ROOT + "/IntegrationTestExporter";
            const string DYNAMIC_FILE = DYNAMIC_DIR + "/DynamicFile.mat";

            Directory.CreateDirectory(DYNAMIC_DIR);
            AssetDatabase.Refresh();
            CreateTestMaterialAsset(DYNAMIC_FILE);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            exporter.objects.Clear();
            exporter.excludes.Clear();
            var element = new ExportTargetObjectElement(TEST_ROOT + "/%name%/DynamicFile.mat");
            element.searchReference = false;
            exporter.objects.Add(element);

            FilePathList result = null;
            Task task = exporter.GetAllPath(list => result = list, string.Empty);
            while (!task.IsCompleted)
                yield return null;
            if (task.IsFaulted)
                throw task.Exception.InnerException ?? task.Exception;

            CollectionAssert.Contains(result.paths.ToList(), DYNAMIC_FILE,
                "Dynamic Path の %name% はエクスポーター名で解決され、該当ファイルが paths に含まれること");
        }

        /// <summary>
        /// References に Exclude モードのエントリだけを登録した場合、
        /// Include が存在しないため useReference = false となり、
        /// searchReference = true のオブジェクトでも直接 paths に追加されること。
        /// </summary>
        [UnityTest]
        public IEnumerator GetAllPath_ReferenceExcludeOnly_UseReferenceIsFalse_FilesAddedDirectly()
        {
            // Include なし・Exclude のみ → GetReferencesPath の結果 = [] → useReference = false
            exporter.references.Add(
                new ReferenceElement(new ObjectRefElement(VERSION_JSON), ReferenceMode.Exclude));

            exporter.objects.Clear();
            var matElement = new ExportTargetObjectElement(ALPHA_MAT);
            matElement.searchReference = true;
            exporter.objects.Add(matElement);

            FilePathList result = null;
            Task task = exporter.GetAllPath(list => result = list, string.Empty);
            while (!task.IsCompleted)
                yield return null;
            if (task.IsFaulted)
                throw task.Exception.InnerException ?? task.Exception;

            CollectionAssert.Contains(result.paths.ToList(), ALPHA_MAT,
                "Include が存在しない場合は useReference=false となり ALPHA_MAT が直接 paths に追加されること");
            CollectionAssert.DoesNotContain(result.paths.ToList(), VERSION_JSON,
                "Exclude 指定のファイルは paths に含まれないこと");
        }

        /// <summary>
        /// Include と Exclude が同じファイルを参照して references 結果が空になった場合、
        /// searchReference = true のオブジェクトでも直接 paths に追加される。
        /// </summary>
        [UnityTest]
        public IEnumerator GetAllPath_ReferencesIncludeAndExcludeSameFile_FallsBackToDirectAddition()
        {
            exporter.references.Add(new ReferenceElement(new ObjectRefElement(VERSION_JSON), ReferenceMode.Include));
            exporter.references.Add(new ReferenceElement(new ObjectRefElement(VERSION_JSON), ReferenceMode.Exclude));

            exporter.objects.Clear();
            var matElement = new ExportTargetObjectElement(ALPHA_MAT);
            matElement.searchReference = true;
            exporter.objects.Add(matElement);

            FilePathList result = null;
            Task task = exporter.GetAllPath(list => result = list, string.Empty);
            while (!task.IsCompleted)
                yield return null;
            if (task.IsFaulted)
                throw task.Exception.InnerException ?? task.Exception;

            CollectionAssert.Contains(result.paths.ToList(), ALPHA_MAT,
                "references が空になった場合 searchReference = true でも直接 paths に追加される");
        }

        // ===== GetAllPath_Batch filter =====

        /// <summary>
        /// GetAllPath_Batch に filter を渡した場合、filter に含まれないエクスポートパスはスキップされること。
        /// Batch モード（Texts）で 2 つのキーのうち 1 つだけを filter に含め、
        /// コールバックのテーブルに 1 件しか追加されないことを検証する。
        /// </summary>
        [UnityTest]
        public IEnumerator GetAllPath_Batch_FilterArg_OnlyMatchingPathIsProcessed()
        {
            // %batch% をパッケージ名に含めることで Red と Blue が異なるエクスポートパスになるようにする。
            // 含めない場合は両キーが同じパスに解決されてしまい、filter による絞り込みを検証できない。
            exporter.packageNameSettings.packageName = "%name%_%batch%";
            exporter.batchExportMode = BatchExportMode.Texts;
            exporter.batchExportTexts.Add("Red");
            exporter.batchExportTexts.Add("Blue");
            exporter.UpdateBatchExportKeys();

            // Red のエクスポートパスだけを filter に含める
            var redPath  = exporter.GetExportPath("Red");
            var bluePath = exporter.GetExportPath("Blue");
            Assert.AreNotEqual(redPath, bluePath, "前提: Red と Blue のエクスポートパスは異なること");
            var filter = new[] { redPath };

            Dictionary<string, FilePathList> resultTable = null;
            bool finished = false;
            Task task = exporter.GetAllPath_Batch(filter, (table, max, currentPath, isFinished) =>
            {
                if (isFinished)
                {
                    resultTable = table;
                    finished = true;
                }
            });

            while (!task.IsCompleted)
                yield return null;
            if (task.IsFaulted)
                throw task.Exception.InnerException ?? task.Exception;

            Assert.IsTrue(finished);
            Assert.AreEqual(1, resultTable.Count,
                "filter に含まれるパスは 1 件のみのため、結果テーブルも 1 件であること");
            Assert.IsTrue(resultTable.ContainsKey(redPath),
                "filter に指定した Red のエクスポートパスが結果に含まれること");
            Assert.IsFalse(resultTable.ContainsKey(bluePath),
                "filter に含まれない Blue のエクスポートパスは結果に含まれないこと");
        }
    }
}
