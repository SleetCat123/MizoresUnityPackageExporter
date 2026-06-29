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

        /// <summary>
        /// テクスチャアセットを生成して assetPath に保存する。
        /// Material に割り当てることで AssetDatabase.GetDependencies が拾う実依存を作るために使う。
        /// </summary>
        static Texture2D CreateTextureAsset(string assetPath)
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            AssetDatabase.CreateAsset(tex, assetPath);
            return tex;
        }

        /// <summary>
        /// テクスチャへの依存を持つ Material アセットを生成する。
        /// texPath のテクスチャを mainTexture に割り当てることで
        /// AssetDatabase.GetDependencies(matPath) に texPath が含まれるようにする。
        /// </summary>
        static Material CreateMaterialWithTextureDependency(string matPath, string texPath)
        {
            var loadedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            Assert.IsNotNull(loadedTex, $"テクスチャが見つかりません: {texPath}");

            var template = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
            Material mat;
            if (template != null)
            {
                mat = new Material(template);
            }
            else
            {
                var shader = Shader.Find("Standard")
                    ?? Shader.Find("Sprites/Default")
                    ?? Shader.Find("Legacy Shaders/Diffuse");
                if (shader == null)
                    throw new System.InvalidOperationException("テスト用 Material を生成するための Shader が見つかりません。");
                mat = new Material(shader);
            }
            mat.name = Path.GetFileNameWithoutExtension(matPath);
            mat.mainTexture = loadedTex;
            AssetDatabase.CreateAsset(mat, matPath);
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return mat;
        }

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
        /// excludes に Exact パスで特定ファイルを指定した場合、そのファイルだけが除外され
        /// 同じフォルダの他のファイルは paths に残ること。
        /// </summary>
        [UnityTest]
        public IEnumerator GetAllPath_Excludes_ExactFilePath_OnlyThatFileExcluded()
        {
            // EXPORT_TARGET フォルダ配下の ALPHA_MAT だけを Exact パスで除外
            exporter.excludes.Add(new SearchPath(SearchPathType.Exact, false, ALPHA_MAT));

            FilePathList result = null;
            Task task = exporter.GetAllPath(list => result = list, string.Empty);
            while (!task.IsCompleted)
                yield return null;
            if (task.IsFaulted)
                throw task.Exception.InnerException ?? task.Exception;

            var paths = result.paths.ToList();
            CollectionAssert.DoesNotContain(paths, ALPHA_MAT,
                "excludes に Exact で指定したファイルは paths に含まれないこと");
            CollectionAssert.Contains(paths, BETA_MAT,
                "Exact 指定の対象外ファイルは paths に残ること");
            CollectionAssert.Contains(result.excludePaths.ToList(), ALPHA_MAT,
                "除外されたファイルは excludePaths に含まれること");
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

        // ===== バージョン参照元エッジケース =====

        /// <summary>
        /// VersionSource.File で versionFile = null の場合、バージョンが空になり
        /// %versionf% がパッケージ名に展開されないこと。
        /// </summary>
        [Test]
        public void GetPackageName_VersionSource_File_NullFile_TreatedAsEmptyVersion()
        {
            var exp = ScriptableObject.CreateInstance<MizoresPackageExporter>();
            try
            {
                exp.name = "NullFileVersionExporter";
                exp.packageNameSettings.versionSource = VersionSource.File;
                exp.packageNameSettings.versionFile   = null;
                exp.packageNameSettings.packageName   = "%name%%versionf%";
                exp.UpdateAllExportVersions();

                Assert.AreEqual("NullFileVersionExporter", exp.GetPackageName(string.Empty),
                    "versionFile=null の場合はバージョンが空になり %versionf% が空文字に展開されること");
            }
            finally { Object.DestroyImmediate(exp); }
        }

        /// <summary>
        /// VersionSource.File でファイルの全行が空白のみの場合、バージョンが空になること。
        /// </summary>
        [Test]
        public void GetPackageName_VersionSource_File_WhitespaceOnlyContent_TreatedAsEmptyVersion()
        {
            const string VERSION_TXT = TEST_ROOT + "/version_whitespace.txt";
            File.WriteAllText(VERSION_TXT, "\n   \n\t\n");

            var exp = ScriptableObject.CreateInstance<MizoresPackageExporter>();
            try
            {
                exp.name = "WhitespaceVersionExporter";
                exp.packageNameSettings.versionSource = VersionSource.File;
                exp.packageNameSettings.versionFile   = new ObjectRefElement(VERSION_TXT);
                exp.packageNameSettings.packageName   = "%name%%versionf%";
                exp.UpdateAllExportVersions();

                Assert.AreEqual("WhitespaceVersionExporter", exp.GetPackageName(string.Empty),
                    "空白行のみのバージョンファイルはバージョン空として扱われ %versionf% が空文字に展開されること");
            }
            finally { Object.DestroyImmediate(exp); }
        }

        /// <summary>
        /// VersionSource.File で JSON の version フィールドが空文字の場合、バージョンが空になること。
        /// </summary>
        [Test]
        public void GetPackageName_VersionSource_File_JsonWithEmptyVersionField_TreatedAsEmptyVersion()
        {
            const string VERSION_JSON_EMPTY = TEST_ROOT + "/version_empty_field.json";
            File.WriteAllText(VERSION_JSON_EMPTY, "{\"version\":\"\"}");

            var exp = ScriptableObject.CreateInstance<MizoresPackageExporter>();
            try
            {
                exp.name = "EmptyJsonVersionExporter";
                exp.packageNameSettings.versionSource = VersionSource.File;
                exp.packageNameSettings.versionFile   = new ObjectRefElement(VERSION_JSON_EMPTY);
                exp.packageNameSettings.packageName   = "%name%%versionf%";
                exp.UpdateAllExportVersions();

                Assert.AreEqual("EmptyJsonVersionExporter", exp.GetPackageName(string.Empty),
                    "JSON の version フィールドが空文字の場合はバージョン空として扱われること");
            }
            finally { Object.DestroyImmediate(exp); }
        }

        /// <summary>
        /// VersionSource.File ではバージョン文字列の '.' が InvalidFileCharsRegex により '_' に置換されること。
        /// ExporterUtils.InvalidFileCharsRegex は Path.GetInvalidFileNameChars() に '.' を追加して構築される。
        /// </summary>
        [Test]
        public void GetPackageName_VersionSource_File_DotsInVersion_ReplacedWithUnderscores()
        {
            const string VERSION_TXT = TEST_ROOT + "/version_dots.txt";
            File.WriteAllText(VERSION_TXT, "2.1.0-beta");

            var exp = ScriptableObject.CreateInstance<MizoresPackageExporter>();
            try
            {
                exp.name = "DotsVersionExporter";
                exp.packageNameSettings.versionSource = VersionSource.File;
                exp.packageNameSettings.versionFile   = new ObjectRefElement(VERSION_TXT);
                exp.packageNameSettings.packageName   = "%name%%versionf%";
                exp.UpdateAllExportVersions();

                Assert.AreEqual("DotsVersionExporter-2_1_0-beta", exp.GetPackageName(string.Empty),
                    "ファイルから読んだバージョンの '.' は InvalidFileCharsRegex により '_' に置換されること");
            }
            finally { Object.DestroyImmediate(exp); }
        }

        /// <summary>
        /// VersionSource.String ではバージョン文字列のサニタイズが行われないこと。
        /// File ソースと異なり、versionString の値がそのまま使われる。
        /// （UpdateExportVersion で String ソースは _exportVersion = versionString のみ実行し置換しない）
        /// </summary>
        [Test]
        public void GetPackageName_VersionSource_String_DotsNotSanitized_UnlikeFileSource()
        {
            var exp = ScriptableObject.CreateInstance<MizoresPackageExporter>();
            try
            {
                exp.name = "StringVersionExporter";
                exp.packageNameSettings.versionSource = VersionSource.String;
                exp.packageNameSettings.versionString = "2.1.0";
                exp.packageNameSettings.packageName   = "%name%%versionf%";
                exp.UpdateAllExportVersions();

                Assert.AreEqual("StringVersionExporter-2.1.0", exp.GetPackageName(string.Empty),
                    "VersionSource.String では '.' は置換されず versionString がそのまま使われること（File ソースの '.' → '_' 置換とは異なる）");
            }
            finally { Object.DestroyImmediate(exp); }
        }

        // ===== 複数エクスポート + キー別オーバーライド =====

        /// <summary>
        /// 複数のバッチキーがそれぞれ独自バージョンをオーバーライドした場合、
        /// GetPackageName が各キーのオーバーライドバージョンを使うこと。
        /// </summary>
        [Test]
        public void GetPackageName_TwoBatchKeys_EachWithOwnVersionOverride_ProduceDifferentVersions()
        {
            exporter.packageNameSettings.versionSource = VersionSource.String;
            exporter.packageNameSettings.versionString = "1.0.0";
            exporter.packageNameSettings.packageName   = "%name%%versionf%";

            var questOverride = new PackageNameSettings();
            questOverride.useOverride_version  = true;
            questOverride.versionSource        = VersionSource.String;
            questOverride.versionString        = "2.0.0";
            exporter.packageNameSettingsOverride["Quest"] = questOverride;

            var pcOverride = new PackageNameSettings();
            pcOverride.useOverride_version = true;
            pcOverride.versionSource       = VersionSource.String;
            pcOverride.versionString       = "3.0.0";
            exporter.packageNameSettingsOverride["PC"] = pcOverride;

            exporter.UpdateAllExportVersions();

            Assert.AreEqual("IntegrationTestExporter-1.0.0", exporter.GetPackageName(string.Empty),
                "オーバーライドなし（空キー）はベースのバージョン 1.0.0 を使うこと");
            Assert.AreEqual("IntegrationTestExporter-2.0.0", exporter.GetPackageName("Quest"),
                "Quest キーはオーバーライドのバージョン 2.0.0 を使うこと");
            Assert.AreEqual("IntegrationTestExporter-3.0.0", exporter.GetPackageName("PC"),
                "PC キーはオーバーライドのバージョン 3.0.0 を使うこと");
        }

        /// <summary>
        /// useOverride_versionFormat=true のオーバーライドが独自の versionFormat を使い、
        /// useOverride_version=false でベースのバージョン値を継承しながら
        /// フォーマットだけを変えたパッケージ名を生成すること。
        /// </summary>
        [Test]
        public void GetPackageName_Override_UseOverrideVersionFormatTrue_UsesOwnVersionFormat()
        {
            exporter.packageNameSettings.versionSource = VersionSource.String;
            exporter.packageNameSettings.versionString = "1.0.0";
            exporter.packageNameSettings.versionFormat = "-v%version%";
            exporter.packageNameSettings.packageName   = "%name%%versionf%";

            var questOverride = new PackageNameSettings();
            questOverride.useOverride_version       = false;  // ベースのバージョン "1.0.0" を継承
            questOverride.useOverride_versionFormat = true;   // 独自フォーマット
            questOverride.versionFormat             = "(%version%)";
            questOverride.useOverride_packageName   = false;  // ベースの packageName を継承
            exporter.packageNameSettingsOverride["Quest"] = questOverride;

            exporter.UpdateAllExportVersions();

            Assert.AreEqual("IntegrationTestExporter-v1.0.0", exporter.GetPackageName(string.Empty),
                "ベースは -v%version% フォーマットを使うこと");
            Assert.AreEqual("IntegrationTestExporter(1.0.0)", exporter.GetPackageName("Quest"),
                "Quest はバージョン値をベースから継承しつつ、独自の (%version%) フォーマットを使うこと");
        }

        /// <summary>
        /// バッチモードで複数キーにキー別オーバーライドを設定した場合、
        /// GetAllExportFileName が各キーのオーバーライド設定を反映したファイル名を返すこと。
        /// </summary>
        [Test]
        public void GetAllExportFileName_BatchMode_PerKeyOverrides_EachKeyHasExpectedFilename()
        {
            exporter.packageNameSettings.versionSource = VersionSource.String;
            exporter.packageNameSettings.versionString = "1.0.0";
            exporter.packageNameSettings.packageName   = "%name%%versionf%";
            exporter.batchExportMode = BatchExportMode.Texts;
            exporter.batchExportTexts.Add("Quest");
            exporter.batchExportTexts.Add("PC");
            exporter.UpdateBatchExportKeys();

            // Quest: パッケージ名を完全にオーバーライド（バージョンは自身では持たない → 空）
            var questOverride = new PackageNameSettings();
            questOverride.useOverride_packageName = true;
            questOverride.packageName             = "%name%_Quest_Edition";
            exporter.packageNameSettingsOverride["Quest"] = questOverride;

            // PC: バージョンだけオーバーライド（パッケージ名テンプレートはベースを継承）
            var pcOverride = new PackageNameSettings();
            pcOverride.useOverride_version = true;
            pcOverride.versionSource       = VersionSource.String;
            pcOverride.versionString       = "2.0.0";
            exporter.packageNameSettingsOverride["PC"] = pcOverride;

            exporter.UpdateAllExportVersions();

            var fileNames = exporter.GetAllExportFileName(string.Empty);

            CollectionAssert.Contains(fileNames, "IntegrationTestExporter_Quest_Edition.unitypackage",
                "Quest キーのパッケージ名オーバーライドが GetAllExportFileName に反映されること");
            CollectionAssert.Contains(fileNames, "IntegrationTestExporter-2.0.0.unitypackage",
                "PC キーのバージョンオーバーライドが GetAllExportFileName に反映され、ベースのパッケージ名テンプレートを使うこと");
        }

        // ===== unitypackage 名重複時の対応 =====

        /// <summary>
        /// バッチモードで全キーが同じパッケージ名に解決される場合、
        /// GetAllExportFileName が Distinct により重複を除いた 1 件を返すこと。
        /// </summary>
        [Test]
        public void GetAllExportFileName_BatchMode_AllKeysSameName_DistinctApplied_ReturnsOneEntry()
        {
            // %batch% を含まない packageName → 全キーが同じファイル名に解決される
            exporter.packageNameSettings.packageName = "%name%";
            exporter.batchExportMode = BatchExportMode.Texts;
            exporter.batchExportTexts.Add("Quest");
            exporter.batchExportTexts.Add("PC");
            exporter.batchExportTexts.Add("Mobile");
            exporter.UpdateBatchExportKeys();

            var fileNames = exporter.GetAllExportFileName(string.Empty);

            Assert.AreEqual(1, fileNames.Length,
                "全キーが同じファイル名に解決される場合、Distinct により 1 件だけ返されること");
            Assert.AreEqual("IntegrationTestExporter.unitypackage", fileNames[0]);
        }

        /// <summary>
        /// バッチモードで一部のキーのみ同名に解決される場合、
        /// GetAllExportFileName が重複分を除いた正しい件数を返すこと。
        ///
        /// Quest と PC が同じ packageName オーバーライドを持つ → 1 件に集約
        /// Mobile と Switch はそれぞれ一意 → 2 件
        /// 合計: 3 件（Quest/PC + Mobile + Switch）
        /// </summary>
        [Test]
        public void GetAllExportFileName_BatchMode_PartialDuplicates_CorrectCountAfterDedup()
        {
            exporter.packageNameSettings.packageName = "%name%_%batch%";
            exporter.batchExportMode = BatchExportMode.Texts;
            exporter.batchExportTexts.Add("Quest");
            exporter.batchExportTexts.Add("PC");
            exporter.batchExportTexts.Add("Mobile");
            exporter.batchExportTexts.Add("Switch");
            exporter.UpdateBatchExportKeys();

            // Quest と PC に同じパッケージ名を設定 → 重複
            var questOverride = new PackageNameSettings();
            questOverride.useOverride_packageName = true;
            questOverride.packageName             = "%name%_Platform";
            exporter.packageNameSettingsOverride["Quest"] = questOverride;

            var pcOverride = new PackageNameSettings();
            pcOverride.useOverride_packageName = true;
            pcOverride.packageName             = "%name%_Platform";  // Quest と同名
            exporter.packageNameSettingsOverride["PC"] = pcOverride;

            // Mobile・Switch はデフォルト → "IntegrationTestExporter_Mobile", "IntegrationTestExporter_Switch"

            var fileNames = exporter.GetAllExportFileName(string.Empty);

            Assert.AreEqual(3, fileNames.Length,
                "Quest と PC が同名に解決されるため 4 件 → Distinct で 3 件になること");
            CollectionAssert.Contains(fileNames, "IntegrationTestExporter_Platform.unitypackage",
                "Quest と PC が集約されたファイル名");
            CollectionAssert.Contains(fileNames, "IntegrationTestExporter_Mobile.unitypackage");
            CollectionAssert.Contains(fileNames, "IntegrationTestExporter_Switch.unitypackage");
        }

        /// <summary>
        /// GetAllPath_Batch で複数キーが同じエクスポートパスに解決される場合、
        /// 最初のキーのファイル一覧が採用され、2 番目以降のキーはスキップされること。
        ///
        /// 検証方法: Dynamic Path (%batch%) でキーごとに参照先ディレクトリを変える。
        /// Quest → DynamicDir/Quest/FileA.mat
        /// PC    → DynamicDir/PC/FileB.mat
        /// 両キーのエクスポートパスが同一になるよう packageName に %batch% を含めない。
        /// → GetAllPath_Batch は最初の Quest のデータを採用し FileA.mat のみ収集。
        ///   FileB.mat（PC のデータ）は採用されない。
        /// </summary>
        [UnityTest]
        public IEnumerator GetAllPath_Batch_DuplicatePaths_FirstKeyDataAdopted_SecondKeySkipped()
        {
            const string DYN_ROOT    = TEST_ROOT + "/DynDir";
            const string QUEST_DIR   = DYN_ROOT  + "/Quest";
            const string PC_DIR      = DYN_ROOT  + "/PC";
            const string QUEST_FILE  = QUEST_DIR + "/FileQuest.mat";
            const string PC_FILE     = PC_DIR    + "/FilePC.mat";

            try
            {
                Directory.CreateDirectory(QUEST_DIR);
                Directory.CreateDirectory(PC_DIR);
                AssetDatabase.Refresh();
                CreateTestMaterialAsset(QUEST_FILE);
                CreateTestMaterialAsset(PC_FILE);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // %batch% でキーごとにオブジェクトを切り替えつつ、
                // packageName には %batch% を含めないことで全キーが同じ exportPath に解決される
                exporter.packageNameSettings.packageName = "%name%";
                exporter.batchExportMode = BatchExportMode.Texts;
                exporter.batchExportTexts.Add("Quest");
                exporter.batchExportTexts.Add("PC");
                exporter.UpdateBatchExportKeys();

                exporter.objects.Clear();
                exporter.excludes.Clear();
                var dynElement = new ExportTargetObjectElement(DYN_ROOT + "/%batch%");
                dynElement.searchReference = false;
                exporter.objects.Add(dynElement);

                // Quest と PC が同じ export path に解決されることを前提確認
                var questPath = exporter.GetExportPath("Quest");
                var pcPath    = exporter.GetExportPath("PC");
                Assert.AreEqual(questPath, pcPath, "前提: %batch% なし packageName では全キーが同じ exportPath に解決されること");

                Dictionary<string, FilePathList> resultTable = null;
                Task task = exporter.GetAllPath_Batch((table, max, currentPath, isFinished) =>
                {
                    if (isFinished) resultTable = table;
                });
                while (!task.IsCompleted)
                    yield return null;
                if (task.IsFaulted)
                    throw task.Exception.InnerException ?? task.Exception;

                Assert.AreEqual(1, resultTable.Count,
                    "同一 exportPath は 1 件にまとめられること");

                var filePathList = resultTable[questPath];
                var paths = filePathList.paths.ToList();

                // Quest が最初に処理されるため Quest のデータが採用される
                CollectionAssert.Contains(paths, QUEST_FILE,
                    "最初に処理された Quest のファイルが採用されること");
                CollectionAssert.DoesNotContain(paths, PC_FILE,
                    "2 番目以降のキー（PC）はスキップされるため PC のファイルは採用されないこと");
            }
            finally
            {
                if (Directory.Exists(DYN_ROOT))
                    Directory.Delete(DYN_ROOT, recursive: true);
                var meta = DYN_ROOT + ".meta";
                if (File.Exists(meta)) File.Delete(meta);
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// packageName に無効な日付フォーマット（%date:C%）を含む場合、
        /// GetAllExportFileName が例外を投げずにファイル名を返しつつ
        /// out 引数の formatError にエラー文字列を設定すること。
        ///
        /// ReplaceDate はフォーマットエラーを catch して元のプレースホルダを残し、
        /// 呼び出し元が formatError 経由でエラーを検知できるようにする設計。
        /// </summary>
        [Test]
        public void GetAllExportFileName_InvalidDateFormatInPackageName_FormatErrorSet_NoException()
        {
            // "C" は .NET の DateTime 標準フォーマット指定子として認識されないため FormatException を発生させる
            exporter.packageNameSettings.packageName = "%name%-%date:C%";
            exporter.batchExportMode = BatchExportMode.Single;

            string formatError = null;
            string[] fileNames = null;
            Assert.DoesNotThrow(() => fileNames = exporter.GetAllExportFileName(string.Empty, out formatError),
                "無効な日付フォーマットがあっても GetAllExportFileName は例外を投げないこと");

            Assert.IsNotNull(formatError,
                "無効な日付フォーマット 'C' が検出され formatError が設定されること");
            Assert.AreEqual("C", formatError,
                "formatError には無効なフォーマット文字列 'C' が入ること");
            Assert.AreEqual(1, fileNames.Length,
                "エラーがあってもファイル名は 1 件返されること");
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

        // ===== ドットを含むフォルダ名のリグレッションテスト =====

        /// <summary>
        /// 名前にドットを含むフォルダ（例: "my.package.v1"）をエクスポート対象に指定した場合、
        /// フォルダ配下のファイルが paths に含まれ、フォルダ自身は paths に含まれないこと。
        ///
        /// 過去の不具合:
        ///   Path.GetExtension("Assets/my.package.v1") が ".v1" を返すため、
        ///   GetAllPath の内部処理でフォルダをファイルとして誤認識し、
        ///   フォルダ自身が paths に追加されてしまっていた。
        ///   その後 AllFileExists が File.Exists(folderPath) = false と判定して
        ///   「ファイルが見つかりません」エラーが発生していた。
        /// </summary>
        [UnityTest]
        public IEnumerator GetAllPath_FolderWithDotInName_FilesAppearInPaths_FolderItselfDoesNot()
        {
            const string DOT_FOLDER     = TEST_ROOT + "/my.package.v1";
            const string DOT_FILE_A     = DOT_FOLDER + "/MaterialA.mat";
            const string DOT_FILE_B     = DOT_FOLDER + "/MaterialB.mat";

            try
            {
                Directory.CreateDirectory(DOT_FOLDER);
                AssetDatabase.Refresh();
                CreateTestMaterialAsset(DOT_FILE_A);
                CreateTestMaterialAsset(DOT_FILE_B);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                var exp = ScriptableObject.CreateInstance<MizoresPackageExporter>();
                exp.name = "DotFolderExporter";
                var target = new ExportTargetObjectElement(DOT_FOLDER);
                target.searchReference = false;
                exp.objects.Add(target);

                FilePathList result = null;
                Task task = exp.GetAllPath(list => result = list, string.Empty);
                while (!task.IsCompleted)
                    yield return null;
                if (task.IsFaulted)
                    throw task.Exception.InnerException ?? task.Exception;

                Object.DestroyImmediate(exp);

                var paths = result.paths.ToList();
                CollectionAssert.Contains(paths, DOT_FILE_A,
                    "ドットを含むフォルダ配下の .mat ファイルは paths に含まれること");
                CollectionAssert.Contains(paths, DOT_FILE_B,
                    "ドットを含むフォルダ配下の .mat ファイルは paths に含まれること");
                CollectionAssert.DoesNotContain(paths, DOT_FOLDER,
                    "ドットを含むフォルダ自身は paths に含まれないこと（フォルダはエクスポート対象外）");
            }
            finally
            {
                if (Directory.Exists(DOT_FOLDER))
                    Directory.Delete(DOT_FOLDER, recursive: true);
                var meta = DOT_FOLDER + ".meta";
                if (File.Exists(meta))
                    File.Delete(meta);
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// ドットを含むフォルダを excludeObjects に指定した場合、
        /// プレフィックスが一致するだけの兄弟ドットフォルダは除外されないこと。
        /// （例: "my.package.v1" を除外しても "my.package.v1Extra" は除外されない）
        /// </summary>
        [UnityTest]
        public IEnumerator GetAllPath_ExcludeObjects_DotFolderDoesNotExcludeSiblingDotFolder()
        {
            const string DOT_FOLDER         = TEST_ROOT + "/my.package.v1";
            const string DOT_FOLDER_FILE    = DOT_FOLDER + "/FileA.mat";
            const string DOT_SIBLING        = TEST_ROOT + "/my.package.v1Extra";
            const string DOT_SIBLING_FILE   = DOT_SIBLING + "/FileB.mat";

            try
            {
                Directory.CreateDirectory(DOT_FOLDER);
                Directory.CreateDirectory(DOT_SIBLING);
                AssetDatabase.Refresh();
                CreateTestMaterialAsset(DOT_FOLDER_FILE);
                CreateTestMaterialAsset(DOT_SIBLING_FILE);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                var exp = ScriptableObject.CreateInstance<MizoresPackageExporter>();
                exp.name = "DotFolderSiblingExporter";
                var targetA = new ExportTargetObjectElement(DOT_FOLDER);
                targetA.searchReference = false;
                exp.objects.Add(targetA);
                var targetB = new ExportTargetObjectElement(DOT_SIBLING);
                targetB.searchReference = false;
                exp.objects.Add(targetB);
                exp.excludeObjects.Add(new ObjectRefElement(DOT_FOLDER));

                FilePathList result = null;
                Task task = exp.GetAllPath(list => result = list, string.Empty);
                while (!task.IsCompleted)
                    yield return null;
                if (task.IsFaulted)
                    throw task.Exception.InnerException ?? task.Exception;

                Object.DestroyImmediate(exp);

                var paths = result.paths.ToList();
                CollectionAssert.DoesNotContain(paths, DOT_FOLDER_FILE,
                    "excludeObjects に指定されたドットフォルダ配下のファイルは paths に含まれないこと");
                CollectionAssert.Contains(paths, DOT_SIBLING_FILE,
                    "プレフィックスが一致するだけの兄弟ドットフォルダ配下のファイルは除外されないこと");
            }
            finally
            {
                foreach (var dir in new[] { DOT_FOLDER, DOT_SIBLING })
                {
                    if (Directory.Exists(dir))
                        Directory.Delete(dir, recursive: true);
                    var meta = dir + ".meta";
                    if (File.Exists(meta))
                        File.Delete(meta);
                }
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// ドットを含むフォルダ配下のファイルに対して excludes（EndsWith パターン）が正しく機能すること。
        /// ドットフォルダ自身がパターンにマッチすることで配下のファイルまで巻き込んで除外されないことも確認する。
        /// </summary>
        [UnityTest]
        public IEnumerator GetAllPath_ExcludesPattern_DotFolderContents_CorrectlyFiltered()
        {
            const string DOT_FOLDER     = TEST_ROOT + "/my.package.v1";
            const string MAT_FILE       = DOT_FOLDER + "/Material.mat";
            const string LOG_FILE       = DOT_FOLDER + "/debug.log";

            try
            {
                Directory.CreateDirectory(DOT_FOLDER);
                File.WriteAllText(LOG_FILE, "log data");
                AssetDatabase.Refresh();
                CreateTestMaterialAsset(MAT_FILE);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                var exp = ScriptableObject.CreateInstance<MizoresPackageExporter>();
                exp.name = "DotFolderExcludeExporter";
                var target = new ExportTargetObjectElement(DOT_FOLDER);
                target.searchReference = false;
                exp.objects.Add(target);
                // .log を除外、.v1 は除外しない（フォルダ名が ".v1" で終わるが意図的に除外しない）
                exp.excludes.Add(new SearchPath(SearchPathType.EndsWith, false, ".log"));

                FilePathList result = null;
                Task task = exp.GetAllPath(list => result = list, string.Empty);
                while (!task.IsCompleted)
                    yield return null;
                if (task.IsFaulted)
                    throw task.Exception.InnerException ?? task.Exception;

                Object.DestroyImmediate(exp);

                var paths = result.paths.ToList();
                var excludePaths = result.excludePaths.ToList();
                CollectionAssert.Contains(paths, MAT_FILE,
                    ".mat ファイルは '.log' パターンにマッチしないため paths に含まれること");
                CollectionAssert.DoesNotContain(paths, LOG_FILE,
                    ".log ファイルは excludes にマッチするため paths に含まれないこと");
                CollectionAssert.Contains(excludePaths, LOG_FILE,
                    ".log ファイルは excludePaths に含まれること");
            }
            finally
            {
                if (Directory.Exists(DOT_FOLDER))
                    Directory.Delete(DOT_FOLDER, recursive: true);
                var meta = DOT_FOLDER + ".meta";
                if (File.Exists(meta))
                    File.Delete(meta);
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// excludeObjects と excludes（SearchPath）が共存する場合、
        /// それぞれ独立して機能すること。
        /// excludeObjects はフォルダ単位で、excludes はパターンで除外する。
        /// </summary>
        [UnityTest]
        public IEnumerator GetAllPath_CombinedExcludeObjectsAndExcludesPattern_BothApplyIndependently()
        {
            const string EXTRA_FOLDER       = TEST_ROOT + "/ExtraFolder";
            const string EXTRA_FILE_MAT     = EXTRA_FOLDER + "/Extra.mat";
            const string EXTRA_FILE_TXT     = EXTRA_FOLDER + "/extra_data.txt";

            try
            {
                Directory.CreateDirectory(EXTRA_FOLDER);
                File.WriteAllText(EXTRA_FILE_TXT, "extra data");
                AssetDatabase.Refresh();
                CreateTestMaterialAsset(EXTRA_FILE_MAT);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // SetUp の objects（EXPORT_TARGET）を追加で利用
                var extraElement = new ExportTargetObjectElement(EXTRA_FOLDER);
                extraElement.searchReference = false;
                exporter.objects.Add(extraElement);

                // EXPORT_TARGET フォルダ全体を excludeObjects で除外
                exporter.excludeObjects.Add(new ObjectRefElement(EXPORT_TARGET));
                // .txt ファイルを excludes パターンで除外
                exporter.excludes.Add(new SearchPath(SearchPathType.EndsWith, false, ".txt"));

                FilePathList result = null;
                Task task = exporter.GetAllPath(list => result = list, string.Empty);
                while (!task.IsCompleted)
                    yield return null;
                if (task.IsFaulted)
                    throw task.Exception.InnerException ?? task.Exception;

                var paths = result.paths.ToList();
                CollectionAssert.DoesNotContain(paths, ALPHA_MAT,
                    "excludeObjects で除外されたフォルダ配下のファイルは paths に含まれないこと");
                CollectionAssert.DoesNotContain(paths, BETA_MAT,
                    "excludeObjects で除外されたフォルダ配下の全ファイルが除外されること");
                CollectionAssert.DoesNotContain(paths, EXTRA_FILE_TXT,
                    "excludes パターンにマッチする .txt ファイルは paths に含まれないこと");
                CollectionAssert.Contains(paths, EXTRA_FILE_MAT,
                    "excludeObjects にも excludes パターンにもマッチしない .mat ファイルは paths に含まれること");
            }
            finally
            {
                if (Directory.Exists(EXTRA_FOLDER))
                    Directory.Delete(EXTRA_FOLDER, recursive: true);
                var meta = EXTRA_FOLDER + ".meta";
                if (File.Exists(meta))
                    File.Delete(meta);
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// searchReference = true のオブジェクト自身は references Exclude に関係なく常に paths に追加されること。
        ///
        /// GetAllPath の実装では、GetDependencies の結果を走査する際に
        /// 「dp == item.path（自分自身）の場合は無条件で paths.Add」する（自己依存バイパス）。
        /// よって references 候補から Exclude されていても、objects に含まれるファイル自身は必ず paths に入る。
        ///
        /// 検証ポイント:
        ///   - Include = EXPORT_TARGET → 候補 = {ALPHA_MAT, BETA_MAT, debug.log}
        ///   - Exclude = ALPHA_MAT → 候補から ALPHA_MAT が除外される（他ファイルへの依存には効果あり）
        ///   - object = ALPHA_MAT（searchReference=true）
        ///   → ALPHA_MAT は自己依存バイパスにより candidates に関係なく paths に追加される
        /// </summary>
        [UnityTest]
        public IEnumerator GetAllPath_ReferencesExclude_SelfDependencyBypass_ObjectAlwaysAddedToPaths()
        {
            // EXPORT_TARGET フォルダ全体を Include → ALPHA_MAT, BETA_MAT, debug.log が候補
            // ALPHA_MAT を Exclude → 他ファイルの依存として登場した場合は除外されるが…
            exporter.references.Add(new ReferenceElement(new ObjectRefElement(EXPORT_TARGET), ReferenceMode.Include));
            exporter.references.Add(new ReferenceElement(new ObjectRefElement(ALPHA_MAT), ReferenceMode.Exclude));

            // object を ALPHA_MAT（searchReference = true）に変更
            exporter.objects.Clear();
            var alphaElement = new ExportTargetObjectElement(ALPHA_MAT);
            alphaElement.searchReference = true;
            exporter.objects.Add(alphaElement);

            FilePathList result = null;
            Task task = exporter.GetAllPath(list => result = list, string.Empty);
            while (!task.IsCompleted)
                yield return null;
            if (task.IsFaulted)
                throw task.Exception.InnerException ?? task.Exception;

            // references Exclude されていても objects に含まれるファイル自身は
            // 自己依存バイパス（dp == item.path → 無条件 Add）により paths に追加される
            CollectionAssert.Contains(result.paths.ToList(), ALPHA_MAT,
                "references から Exclude されていても objects に含まれるファイル自身は自己依存バイパスにより paths に追加されること");
        }

        // ===== References Include + Exclude の組み合わせ（実依存チェーン） =====

        /// <summary>
        /// references Include に依存先ファイルを登録した場合、
        /// searchReference = true のオブジェクトの依存ファイルが paths に追加されること。
        ///
        /// 自己依存バイパスだけでなく、実際の依存チェーン（Material → Texture）を通じて
        /// Include が機能することを検証する。
        /// </summary>
        [UnityTest]
        public IEnumerator GetAllPath_References_Include_FileDependency_TextureAppearsInPaths()
        {
            const string TEX_PATH = TEST_ROOT + "/DepTexture_Test1.asset";
            const string MAT_PATH = TEST_ROOT + "/DepMaterial_Test1.mat";

            try
            {
                // Material → Texture の実依存を持つアセットを生成
                CreateTextureAsset(TEX_PATH);
                AssetDatabase.SaveAssets();
                CreateMaterialWithTextureDependency(MAT_PATH, TEX_PATH);

                // 前提: GetDependencies が TEX_PATH を返すこと
                var deps = AssetDatabase.GetDependencies(MAT_PATH, true);
                Assert.IsTrue(System.Array.IndexOf(deps, TEX_PATH) >= 0,
                    "前提: Material が Texture に依存していること（依存関係の生成に失敗した場合はヘルパーを確認）");

                // TEX_PATH 単体を Include → 候補 = {TEX_PATH}
                exporter.references.Add(
                    new ReferenceElement(new ObjectRefElement(TEX_PATH), ReferenceMode.Include));

                exporter.objects.Clear();
                exporter.excludes.Clear();  // SetUp の .log 除外をクリア
                var matElem = new ExportTargetObjectElement(MAT_PATH);
                matElem.searchReference = true;
                exporter.objects.Add(matElem);

                FilePathList result = null;
                Task task = exporter.GetAllPath(list => result = list, string.Empty);
                while (!task.IsCompleted)
                    yield return null;
                if (task.IsFaulted)
                    throw task.Exception.InnerException ?? task.Exception;

                var paths = result.paths.ToList();
                CollectionAssert.Contains(paths, MAT_PATH,
                    "Material 自身は自己依存バイパスにより paths に含まれること");
                CollectionAssert.Contains(paths, TEX_PATH,
                    "references 候補に含まれる依存テクスチャは paths に含まれること");
            }
            finally
            {
                foreach (var p in new[] { TEX_PATH, MAT_PATH })
                {
                    if (File.Exists(p)) File.Delete(p);
                    var meta = p + ".meta";
                    if (File.Exists(meta)) File.Delete(meta);
                }
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// references Include（フォルダ）+ Exclude（同フォルダ内の特定ファイル）を設定した場合、
        /// Exclude したファイルが依存であっても paths に追加されないこと。
        ///
        /// Include と Exclude の範囲が部分的に重なるケース（フォルダ Include → ファイル Exclude）の検証。
        /// </summary>
        [UnityTest]
        public IEnumerator GetAllPath_References_IncludeFolder_ExcludeSpecificDep_ExcludedDepNotInPaths()
        {
            const string DEPS_FOLDER = TEST_ROOT + "/DepFolder_Test2";
            const string TEX_PATH    = DEPS_FOLDER + "/Texture.asset";
            const string MAT_PATH    = TEST_ROOT    + "/DepMaterial_Test2.mat";

            try
            {
                Directory.CreateDirectory(DEPS_FOLDER);
                CreateTextureAsset(TEX_PATH);
                AssetDatabase.SaveAssets();
                CreateMaterialWithTextureDependency(MAT_PATH, TEX_PATH);

                var deps = AssetDatabase.GetDependencies(MAT_PATH, true);
                Assert.IsTrue(System.Array.IndexOf(deps, TEX_PATH) >= 0,
                    "前提: Material が Texture に依存していること");

                // Include(フォルダ) → 候補に TEX_PATH が含まれる
                // Exclude(TEX_PATH) → 候補から TEX_PATH が除外される
                // 結果: 候補は DEPS_FOLDER 内の TEX_PATH 以外のファイル（.meta 等）のみ
                exporter.references.Add(
                    new ReferenceElement(new ObjectRefElement(DEPS_FOLDER), ReferenceMode.Include));
                exporter.references.Add(
                    new ReferenceElement(new ObjectRefElement(TEX_PATH), ReferenceMode.Exclude));

                exporter.objects.Clear();
                exporter.excludes.Clear();
                var matElem = new ExportTargetObjectElement(MAT_PATH);
                matElem.searchReference = true;
                exporter.objects.Add(matElem);

                FilePathList result = null;
                Task task = exporter.GetAllPath(list => result = list, string.Empty);
                while (!task.IsCompleted)
                    yield return null;
                if (task.IsFaulted)
                    throw task.Exception.InnerException ?? task.Exception;

                var paths = result.paths.ToList();
                CollectionAssert.Contains(paths, MAT_PATH,
                    "Material 自身は自己依存バイパスにより paths に含まれること");
                CollectionAssert.DoesNotContain(paths, TEX_PATH,
                    "Exclude した依存テクスチャは candidates から外れているため paths に含まれないこと");
            }
            finally
            {
                if (Directory.Exists(DEPS_FOLDER))
                    Directory.Delete(DEPS_FOLDER, recursive: true);
                var metaDir = DEPS_FOLDER + ".meta";
                if (File.Exists(metaDir)) File.Delete(metaDir);
                foreach (var p in new[] { MAT_PATH })
                {
                    if (File.Exists(p)) File.Delete(p);
                    var meta = p + ".meta";
                    if (File.Exists(meta)) File.Delete(meta);
                }
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// references Include フォルダ + Exclude サブフォルダ のケース。
        /// Include 範囲と Exclude 範囲が部分的に重なる（Exclude ⊂ Include）。
        ///
        /// - Include(PARENT_FOLDER) → 候補 = PARENT_FOLDER 全ファイル（サブフォルダ含む）
        /// - Exclude(PARENT_FOLDER/sub) → サブフォルダ内ファイルが候補から外れる
        /// - 結果: サブフォルダ外の TEX_A は候補に残り依存として paths に追加される
        ///         サブフォルダ内の TEX_B は候補から除外されるため paths に追加されない
        /// </summary>
        [UnityTest]
        public IEnumerator GetAllPath_References_IncludeFolder_ExcludeSubfolder_PartialOverlap_OnlyOutsideDepInPaths()
        {
            const string PARENT_FOLDER = TEST_ROOT + "/ParentFolder_Test3";
            const string SUB_FOLDER    = PARENT_FOLDER + "/sub";
            const string TEX_A_PATH    = PARENT_FOLDER + "/TexA.asset";   // サブフォルダ外 → 候補に残る
            const string TEX_B_PATH    = SUB_FOLDER    + "/TexB.asset";   // サブフォルダ内 → Exclude で候補外
            const string MAT_A_PATH    = TEST_ROOT + "/MatA_Test3.mat";   // TEX_A に依存
            const string MAT_B_PATH    = TEST_ROOT + "/MatB_Test3.mat";   // TEX_B に依存

            try
            {
                Directory.CreateDirectory(PARENT_FOLDER);
                Directory.CreateDirectory(SUB_FOLDER);

                CreateTextureAsset(TEX_A_PATH);
                CreateTextureAsset(TEX_B_PATH);
                AssetDatabase.SaveAssets();
                CreateMaterialWithTextureDependency(MAT_A_PATH, TEX_A_PATH);
                CreateMaterialWithTextureDependency(MAT_B_PATH, TEX_B_PATH);

                // 前提: 依存関係が正しく作られていること
                Assert.IsTrue(System.Array.IndexOf(AssetDatabase.GetDependencies(MAT_A_PATH, true), TEX_A_PATH) >= 0,
                    "前提: MatA が TexA に依存していること");
                Assert.IsTrue(System.Array.IndexOf(AssetDatabase.GetDependencies(MAT_B_PATH, true), TEX_B_PATH) >= 0,
                    "前提: MatB が TexB に依存していること");

                // Include(PARENT_FOLDER) → 候補に TEX_A と TEX_B が含まれる
                // Exclude(SUB_FOLDER) → TEX_B が候補から外れる（Exclude ⊂ Include の部分重複）
                exporter.references.Add(
                    new ReferenceElement(new ObjectRefElement(PARENT_FOLDER), ReferenceMode.Include));
                exporter.references.Add(
                    new ReferenceElement(new ObjectRefElement(SUB_FOLDER), ReferenceMode.Exclude));

                exporter.objects.Clear();
                exporter.excludes.Clear();
                foreach (var (path, searchRef) in new[] { (MAT_A_PATH, true), (MAT_B_PATH, true) })
                {
                    var elem = new ExportTargetObjectElement(path);
                    elem.searchReference = searchRef;
                    exporter.objects.Add(elem);
                }

                FilePathList result = null;
                Task task = exporter.GetAllPath(list => result = list, string.Empty);
                while (!task.IsCompleted)
                    yield return null;
                if (task.IsFaulted)
                    throw task.Exception.InnerException ?? task.Exception;

                var paths = result.paths.ToList();
                // 両 Material 自身は自己依存バイパスで追加される
                CollectionAssert.Contains(paths, MAT_A_PATH, "MatA 自身は自己依存バイパスで paths に含まれること");
                CollectionAssert.Contains(paths, MAT_B_PATH, "MatB 自身は自己依存バイパスで paths に含まれること");
                // TEX_A はサブフォルダ外 → Exclude されていない → 依存として paths に追加される
                CollectionAssert.Contains(paths, TEX_A_PATH,
                    "Exclude されていない TEX_A は candidates に残り、MatA の依存として paths に含まれること");
                // TEX_B はサブフォルダ内 → Exclude により candidates から外れる → paths に追加されない
                CollectionAssert.DoesNotContain(paths, TEX_B_PATH,
                    "Exclude サブフォルダ内の TEX_B は candidates から除外されるため paths に含まれないこと");
            }
            finally
            {
                if (Directory.Exists(PARENT_FOLDER))
                    Directory.Delete(PARENT_FOLDER, recursive: true);
                var metaDir = PARENT_FOLDER + ".meta";
                if (File.Exists(metaDir)) File.Delete(metaDir);
                foreach (var p in new[] { MAT_A_PATH, MAT_B_PATH })
                {
                    if (File.Exists(p)) File.Delete(p);
                    var meta = p + ".meta";
                    if (File.Exists(meta)) File.Delete(meta);
                }
                AssetDatabase.Refresh();
            }
        }

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

        /// <summary>
        /// Single モードの GetAllPath_Batch は GetAllPath(string.Empty) を1回呼び出し、
        /// 結果テーブルに1件のエントリが入ること。
        /// </summary>
        [UnityTest]
        public IEnumerator GetAllPath_Batch_SingleMode_ResultHasExactlyOneEntry()
        {
            exporter.batchExportMode = BatchExportMode.Single;

            Dictionary<string, FilePathList> resultTable = null;
            bool finished = false;
            Task task = exporter.GetAllPath_Batch((table, max, currentPath, isFinished) =>
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

            Assert.IsTrue(finished, "コールバックが finished=true で呼ばれること");
            Assert.IsNotNull(resultTable);
            Assert.AreEqual(1, resultTable.Count,
                "Single モードでは結果テーブルに1件だけエントリが入ること");

            var expectedPath = exporter.GetExportPath(string.Empty);
            Assert.IsTrue(resultTable.ContainsKey(expectedPath),
                "結果テーブルのキーが GetExportPath(string.Empty) と一致すること");

            var filePathList = resultTable[expectedPath];
            Assert.IsNotNull(filePathList);
            CollectionAssert.Contains(filePathList.paths.ToList(), ALPHA_MAT,
                "Single モードのファイル一覧に ALPHA_MAT が含まれること");
        }

        /// <summary>
        /// フィルタなしで GetAllPath_Batch を呼んだ場合、全キーが処理されること。
        /// Texts モードで 2 キーを登録し、結果テーブルに 2 件入ることを検証する。
        /// </summary>
        [UnityTest]
        public IEnumerator GetAllPath_Batch_NoFilter_AllKeysProcessed()
        {
            exporter.packageNameSettings.packageName = "%name%_%batch%";
            exporter.batchExportMode = BatchExportMode.Texts;
            exporter.batchExportTexts.Add("Red");
            exporter.batchExportTexts.Add("Blue");
            exporter.UpdateBatchExportKeys();

            Dictionary<string, FilePathList> resultTable = null;
            bool finished = false;
            Task task = exporter.GetAllPath_Batch((table, max, currentPath, isFinished) =>
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
            Assert.AreEqual(2, resultTable.Count,
                "フィルタなしでは全キーが処理され、結果テーブルに2件入ること");

            var redPath  = exporter.GetExportPath("Red");
            var bluePath = exporter.GetExportPath("Blue");
            Assert.IsTrue(resultTable.ContainsKey(redPath),  "Red のパスが結果に含まれること");
            Assert.IsTrue(resultTable.ContainsKey(bluePath), "Blue のパスが結果に含まれること");
        }

        /// <summary>
        /// 複数のキーが同じエクスポートパスに解決される場合、結果テーブルには1件しか入らないこと。
        /// （%batch% を含まないパッケージ名ではキーが異なっても同じパスになる）
        /// </summary>
        [UnityTest]
        public IEnumerator GetAllPath_Batch_DuplicateExportPaths_OnlyOneResultPerPath()
        {
            // %batch% を含まないパッケージ名 → 全キーが同じエクスポートパスに解決される
            exporter.packageNameSettings.packageName = "%name%";
            exporter.batchExportMode = BatchExportMode.Texts;
            exporter.batchExportTexts.Add("Red");
            exporter.batchExportTexts.Add("Blue");
            exporter.UpdateBatchExportKeys();

            var redPath  = exporter.GetExportPath("Red");
            var bluePath = exporter.GetExportPath("Blue");
            Assert.AreEqual(redPath, bluePath,
                "前提: %batch% を含まないパッケージ名では Red と Blue が同じエクスポートパスに解決されること");

            Dictionary<string, FilePathList> resultTable = null;
            Task task = exporter.GetAllPath_Batch((table, max, currentPath, isFinished) =>
            {
                if (isFinished) resultTable = table;
            });

            while (!task.IsCompleted)
                yield return null;
            if (task.IsFaulted)
                throw task.Exception.InnerException ?? task.Exception;

            Assert.AreEqual(1, resultTable.Count,
                "同じエクスポートパスに解決される複数キーは結果テーブルで1件に集約されること");
        }

        // ===== case-insensitive 重複検出 =====

        /// <summary>
        /// 大文字小文字のみ異なるパッケージ名が重複として検出されること。
        /// Windows等のcase-insensitiveファイルシステムでは同一ファイルになるため。
        /// </summary>
        [Test]
        public void GetAllExportFileName_CaseOnlyDifference_DetectedAsDuplicate()
        {
            exporter.packageNameSettings.packageName = "%name%_%batch%";
            exporter.batchExportMode = BatchExportMode.Texts;
            exporter.batchExportTexts.Add("quest");
            exporter.batchExportTexts.Add("Quest");
            exporter.UpdateBatchExportKeys();

            var fileNames = exporter.GetAllExportFileName(string.Empty, out _, out var duplicates);

            Assert.AreEqual(1, fileNames.Length,
                "大文字小文字のみ異なるファイル名は Distinct(OrdinalIgnoreCase) により1件に集約されること");
            Assert.AreEqual(1, duplicates.Length,
                "大文字小文字のみ異なるファイル名が重複として検出されること");
        }

        /// <summary>
        /// 大文字小文字が完全に同一の場合も引き続き重複として検出されること（既存動作の回帰確認）。
        /// </summary>
        [Test]
        public void GetAllExportFileName_ExactDuplicate_StillDetected()
        {
            exporter.packageNameSettings.packageName = "%name%";
            exporter.batchExportMode = BatchExportMode.Texts;
            exporter.batchExportTexts.Add("A");
            exporter.batchExportTexts.Add("B");
            exporter.UpdateBatchExportKeys();

            var fileNames = exporter.GetAllExportFileName(string.Empty, out _, out var duplicates);

            Assert.AreEqual(1, fileNames.Length,
                "完全一致の重複も引き続き1件に集約されること");
            Assert.GreaterOrEqual(duplicates.Length, 1,
                "完全一致の重複も引き続き検出されること");
        }

        /// <summary>
        /// GetAllPath_Batch で大文字小文字のみ異なるエクスポートパスが
        /// case-insensitive で1件に集約されること。
        /// </summary>
        [UnityTest]
        public IEnumerator GetAllPath_Batch_CaseOnlyDifference_CollapsedToOneEntry()
        {
            // Quest → "IntegrationTestExporter_quest.unitypackage"
            // PC    → "IntegrationTestExporter_Quest.unitypackage"  (case-only difference)
            exporter.packageNameSettings.packageName = "%name%_%batch%";
            exporter.batchExportMode = BatchExportMode.Texts;
            exporter.batchExportTexts.Add("quest");
            exporter.batchExportTexts.Add("Quest");
            exporter.UpdateBatchExportKeys();

            Dictionary<string, FilePathList> resultTable = null;
            Task task = exporter.GetAllPath_Batch((table, max, currentPath, isFinished) =>
            {
                if (isFinished) resultTable = table;
            });

            while (!task.IsCompleted)
                yield return null;
            if (task.IsFaulted)
                throw task.Exception.InnerException ?? task.Exception;

            Assert.AreEqual(1, resultTable.Count,
                "大文字小文字のみ異なるエクスポートパスはcase-insensitive辞書により1件に集約されること");
        }
    }
}
