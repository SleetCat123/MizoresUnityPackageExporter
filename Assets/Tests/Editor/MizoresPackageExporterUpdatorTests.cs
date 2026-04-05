using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using MizoreNekoyanagi.PublishUtil.PackageExporter;
using MizoreNekoyanagi.PublishUtil.PackageExporterV1;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.Tests
{
    public class MizoresPackageExporterUpdatorTests
    {
        const string TEST_ROOT = "Assets/Tests_MizorePkgExp_Updator";
        const string LEGACY_ASSET_PATH = TEST_ROOT + "/LegacyExporter.asset";
        const string EXISTING_BAK_PATH = TEST_ROOT + "/LegacyExporter_bak.asset";
        const string VERSION_FILE_PATH = TEST_ROOT + "/version.txt";
        const string SEARCHPATH_ASSET_PATH = TEST_ROOT + "/SearchPathConversionTest.asset";
        const string V1_VERSION1_ASSET_PATH = TEST_ROOT + "/V1Version1.asset";
        const string OBJECTS_MERGE_ASSET_PATH = TEST_ROOT + "/ObjectsMergeTest.asset";

        [TearDown]
        public void TearDown()
        {
            Selection.objects = new UnityEngine.Object[0];

            if (Directory.Exists(TEST_ROOT))
            {
                Directory.Delete(TEST_ROOT, recursive: true);
            }

            var metaPath = TEST_ROOT + ".meta";
            if (File.Exists(metaPath))
            {
                File.Delete(metaPath);
            }

            AssetDatabase.Refresh();
        }

        [Test]
        public void NewExporter_DefaultVersion_IsCurrentVersion()
        {
            var exporter = ScriptableObject.CreateInstance<MizoresPackageExporter>();
            try
            {
                Assert.AreEqual(MizoresPackageExporter.CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION, exporter.packageExporterVersion);
                Assert.IsTrue(MizoresPackageExporterUpdator.IsLatest(exporter));
            }
            finally
            {
                Object.DestroyImmediate(exporter);
            }
        }

        [Test]
        public void ConvertToLatest_OutdatedV2_UpdatesVersionInPlace()
        {
            var exporter = ScriptableObject.CreateInstance<MizoresPackageExporter>();
            try
            {
                exporter.packageExporterVersion = MizoresPackageExporter.CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION - 1;

                var converted = MizoresPackageExporterUpdator.ConvertToLatest(exporter);

                Assert.AreSame(exporter, converted);
                Assert.AreEqual(MizoresPackageExporter.CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION, exporter.packageExporterVersion);
                Assert.IsTrue(MizoresPackageExporterUpdator.IsLatest(exporter));
            }
            finally
            {
                Object.DestroyImmediate(exporter);
            }
        }

#pragma warning disable 612, 618
        [Test]
        public void ConvertToLatest_V1Asset_ConvertsDataAndCreatesUniqueBakAssetWhenBakAlreadyExists()
        {
            Directory.CreateDirectory(TEST_ROOT);
            File.WriteAllText(VERSION_FILE_PATH, "1.2.3");
            AssetDatabase.Refresh();

            var existingBak = ScriptableObject.CreateInstance<MizoresPackageExporter>();
            AssetDatabase.CreateAsset(existingBak, EXISTING_BAK_PATH);

            var legacy = ScriptableObject.CreateInstance<MizoresPackageExporterV1>();
            legacy.name = "LegacyExporter";
            legacy.packageExporterVersion = MizoresPackageExporter.INITIAL_PACKAGE_EXPORTER_OBJECT_VERSION;
            legacy.objects.Add(new MizoresPackageExporterV1.PackagePrefsElement { path = TEST_ROOT + "/Export/Alpha.mat" });
            legacy.dynamicpath.Add("%name%/%batch%");
            legacy.variables["channel"] = "stable";
            legacy.excludeObjects.Add(new MizoresPackageExporterV1.PackagePrefsElement { path = TEST_ROOT + "/Export/Ignored" });
            legacy.excludes.Add(new MizoresPackageExporterV1.SearchPath
            {
                searchType = MizoresPackageExporterV1.SearchPath.SearchPathType.Exact,
                value = TEST_ROOT + "/Exact.txt",
            });
            legacy.excludes.Add(new MizoresPackageExporterV1.SearchPath
            {
                searchType = MizoresPackageExporterV1.SearchPath.SearchPathType.Regex,
                value = ".*\\.log$",
            });
            legacy.references.Add(new MizoresPackageExporterV1.PackagePrefsElement { path = VERSION_FILE_PATH });
            legacy.versionFile = new MizoresPackageExporterV1.PackagePrefsElement { path = VERSION_FILE_PATH };
            legacy.versionFormat = "_v%version%";
            legacy.packageName = "LegacyPkg";
            legacy.packageNameSettings.versionSource = MizoresPackageExporterV1.VersionSource.String;
            legacy.packageNameSettings.versionFile = new MizoresPackageExporterV1.PackagePrefsElement();
            legacy.packageNameSettings.versionString = "0.0.1";
            legacy.packageNameSettings.batchFormat = "_%batch%";
            legacy.packageNameSettings.packageName = "BaseShouldBeOverwritten";

            var overrideSettings = new MizoresPackageExporterV1.PackageNameSettings
            {
                versionSource = MizoresPackageExporterV1.VersionSource.String,
                versionFile = new MizoresPackageExporterV1.PackagePrefsElement(),
                versionString = "9.9.9",
                versionFormat = "v%version%",
                batchFormat = "[%batch%]",
                packageName = "OverridePkg",
                useOverride_version = false,
                useOverride_versionFormat = true,
                useOverride_batchFormat = true,
                useOverride_packageName = true,
            };
            legacy.packageNameSettingsOverride["beta"] = overrideSettings;
            legacy.batchExportMode = MizoresPackageExporterV1.BatchExportMode.Texts;
            legacy.batchExportFolderMode = MizoresPackageExporterV1.BatchExportFolderMode.Folders;
            legacy.batchExportTexts.Add("beta");
            legacy.batchExportFolderRoot = new MizoresPackageExporterV1.PackagePrefsElement { path = TEST_ROOT + "/BatchRoot" };
            legacy.batchExportListFile = new MizoresPackageExporterV1.PackagePrefsElement { path = TEST_ROOT + "/list.txt" };
            legacy.batchExportFolderRegex = "^beta$";

            AssetDatabase.CreateAsset(legacy, LEGACY_ASSET_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var converted = MizoresPackageExporterUpdator.ConvertToLatest(legacy) as MizoresPackageExporter;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Assert.IsNotNull(converted);

            var reloaded = AssetDatabase.LoadAssetAtPath<MizoresPackageExporter>(LEGACY_ASSET_PATH);
            Assert.IsNotNull(reloaded, "元のパスに V2 アセットが再作成されること");
            Assert.AreEqual(MizoresPackageExporter.CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION, reloaded.packageExporterVersion);

            // AreEqual で順序も検証: V1.objects が先、V1.dynamicpath が後
            CollectionAssert.AreEqual(
                new[] { TEST_ROOT + "/Export/Alpha.mat", "%name%/%batch%" },
                reloaded.objects.Select(v => v.Path),
                "V1 の objects が先、dynamicpath が後の順で V2 の objects に統合されること");

            Assert.AreEqual("stable", reloaded.variables["channel"]);
            Assert.AreEqual(TEST_ROOT + "/Export/Ignored", reloaded.excludeObjects.Single().Path);

            Assert.AreEqual(2, reloaded.excludes.Count);
            Assert.AreEqual(SearchPathType.Exact, reloaded.excludes.Single(v => v.Value == TEST_ROOT + "/Exact.txt").searchType.value);
            Assert.AreEqual(SearchPathType.Regex, reloaded.excludes.Single(v => v.Value == ".*\\.log$").searchType.value);

            Assert.AreEqual(VERSION_FILE_PATH, reloaded.references.Single().element.Path);
            Assert.AreEqual(ReferenceMode.Include, reloaded.references.Single().mode.value);

            Assert.AreEqual(VersionSource.File, reloaded.packageNameSettings.versionSource.value);
            Assert.AreEqual(VERSION_FILE_PATH, reloaded.packageNameSettings.versionFile.Path);
            Assert.AreEqual("_v%version%", reloaded.packageNameSettings.versionFormat);
            Assert.AreEqual("LegacyPkg", reloaded.packageNameSettings.packageName);

            var migratedOverride = reloaded.packageNameSettingsOverride["beta"];
            Assert.IsFalse(migratedOverride.useOverride_version);
            Assert.IsTrue(migratedOverride.useOverride_versionFormat);
            Assert.IsTrue(migratedOverride.useOverride_batchFormat);
            Assert.IsTrue(migratedOverride.useOverride_packageName);
            Assert.AreEqual("[%batch%]", migratedOverride.batchFormat);
            Assert.AreEqual("OverridePkg", migratedOverride.packageName);

            Assert.AreEqual(BatchExportMode.Texts, reloaded.batchExportMode.value);
            Assert.AreEqual(BatchExportFolderMode.Folders, reloaded.batchExportFolderMode.value);
            CollectionAssert.AreEqual(new[] { "beta" }, reloaded.batchExportTexts);
            Assert.AreEqual(TEST_ROOT + "/BatchRoot", reloaded.batchExportFolderRoot.Path);
            Assert.AreEqual(TEST_ROOT + "/list.txt", reloaded.batchExportListFile.Path);
            Assert.AreEqual("^beta$", reloaded.batchExportFolderRegex);

            var bakAssets = Directory.GetFiles(TEST_ROOT, "LegacyExporter_bak*.asset")
                .Select(v => v.Replace('\\', '/'))
                .OrderBy(v => v)
                .ToArray();
            Assert.AreEqual(2, bakAssets.Length,
                "既存の *_bak.asset があっても、変換元は別の *_bak*.asset に退避されること");
            CollectionAssert.Contains(bakAssets, EXISTING_BAK_PATH);
            var generatedBakPath = bakAssets.Single(v => v != EXISTING_BAK_PATH);
            StringAssert.StartsWith(TEST_ROOT + "/LegacyExporter_bak", generatedBakPath);
        }
        [Test]
        public void ConvertToLatest_V1Asset_ObjectsMergePreservesOrderAndSearchReference()
        {
            Directory.CreateDirectory(TEST_ROOT);
            AssetDatabase.Refresh();

            // V1 では objects（静的パス）と dynamicpath（%変数パス）が別リストだった
            // V2 では一つの objects リストに統合される
            // 順序：V1.objects が先、V1.dynamicpath が後に連結される
            var legacy = ScriptableObject.CreateInstance<MizoresPackageExporterV1>();
            legacy.packageExporterVersion = 1;
            legacy.objects.Add(new MizoresPackageExporterV1.PackagePrefsElement { path = "Assets/Pkg/MeshA.fbx" });
            legacy.objects.Add(new MizoresPackageExporterV1.PackagePrefsElement { path = "Assets/Pkg/MaterialB.mat" });
            legacy.objects.Add(new MizoresPackageExporterV1.PackagePrefsElement { path = "Assets/Pkg/TextureC.png" });
            legacy.dynamicpath.Add("%name%/%batch%");
            legacy.dynamicpath.Add("Assets/Generated/%version%/output.asset");

            AssetDatabase.CreateAsset(legacy, OBJECTS_MERGE_ASSET_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            MizoresPackageExporterUpdator.ConvertToLatest(legacy);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var converted = AssetDatabase.LoadAssetAtPath<MizoresPackageExporter>(OBJECTS_MERGE_ASSET_PATH);
            Assert.IsNotNull(converted);
            Assert.AreEqual(5, converted.objects.Count, "V1.objects(3件) + V1.dynamicpath(2件) = 5件に統合されること");

            // V1.objects が先、V1.dynamicpath が後の順序で結合されていること
            Assert.AreEqual("Assets/Pkg/MeshA.fbx",                   converted.objects[0].Path, "V1.objects[0] が先頭");
            Assert.AreEqual("Assets/Pkg/MaterialB.mat",                converted.objects[1].Path, "V1.objects[1] が 2 番目");
            Assert.AreEqual("Assets/Pkg/TextureC.png",                 converted.objects[2].Path, "V1.objects[2] が 3 番目");
            Assert.AreEqual("%name%/%batch%",                          converted.objects[3].Path, "V1.dynamicpath[0] が 4 番目");
            Assert.AreEqual("Assets/Generated/%version%/output.asset", converted.objects[4].Path, "V1.dynamicpath[1] が末尾");

            // searchReference は静的パス・動的パス問わず true であること
            // （エクスポート時に依存アセット検索を行うため両方 true が正しい）
            for (int i = 0; i < converted.objects.Count; i++)
            {
                Assert.IsTrue(converted.objects[i].searchReference,
                    $"objects[{i}] (path={converted.objects[i].Path}) の searchReference は true であること");
            }
        }

        [Test]
        public void ConvertToLatest_V1Asset_AllSearchPathTypesConvertWithCorrectPreserveCase()
        {
            Directory.CreateDirectory(TEST_ROOT);
            AssetDatabase.Refresh();

            var legacy = ScriptableObject.CreateInstance<MizoresPackageExporterV1>();
            legacy.packageExporterVersion = 1; // INITIAL 変換済み状態
            legacy.excludes.Add(new MizoresPackageExporterV1.SearchPath { searchType = MizoresPackageExporterV1.SearchPath.SearchPathType.Disabled, value = "disabled_val" });
            legacy.excludes.Add(new MizoresPackageExporterV1.SearchPath { searchType = MizoresPackageExporterV1.SearchPath.SearchPathType.Exact, value = "exact_val" });
            legacy.excludes.Add(new MizoresPackageExporterV1.SearchPath { searchType = MizoresPackageExporterV1.SearchPath.SearchPathType.Partial, value = "partial_val" });
            legacy.excludes.Add(new MizoresPackageExporterV1.SearchPath { searchType = MizoresPackageExporterV1.SearchPath.SearchPathType.Partial_IgnoreCase, value = "partial_ignorecase_val" });
            legacy.excludes.Add(new MizoresPackageExporterV1.SearchPath { searchType = MizoresPackageExporterV1.SearchPath.SearchPathType.Regex, value = ".*\\.log$" });
            legacy.excludes.Add(new MizoresPackageExporterV1.SearchPath { searchType = MizoresPackageExporterV1.SearchPath.SearchPathType.Regex_IgnoreCase, value = ".*\\.txt$" });

            AssetDatabase.CreateAsset(legacy, SEARCHPATH_ASSET_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            MizoresPackageExporterUpdator.ConvertToLatest(legacy);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var converted = AssetDatabase.LoadAssetAtPath<MizoresPackageExporter>(SEARCHPATH_ASSET_PATH);
            Assert.IsNotNull(converted, "元のパスに V2 アセットが作成されること");

            SearchPath GetExclude(string val) => converted.excludes.Single(e => e.Value == val);

            // Disabled → 大文字小文字を区別しない設定は無効時に意味を持たないため問わない
            var disabled = GetExclude("disabled_val");
            Assert.AreEqual(SearchPathType.Disabled, disabled.searchType.value, "Disabled は Disabled に変換されること");

            // Exact → 大文字小文字を区別する (preserveCase=true)
            var exact = GetExclude("exact_val");
            Assert.AreEqual(SearchPathType.Exact, exact.searchType.value, "Exact は Exact に変換されること");
            Assert.IsTrue(exact.PreserveCase, "Exact は大文字小文字を区別する (preserveCase=true) こと");

            // Partial → 大文字小文字を区別する (preserveCase=true)
            var partial = GetExclude("partial_val");
            Assert.AreEqual(SearchPathType.Partial, partial.searchType.value, "Partial は Partial に変換されること");
            Assert.IsTrue(partial.PreserveCase, "Partial (IgnoreCase なし) は大文字小文字を区別する (preserveCase=true) こと");

            // Partial_IgnoreCase → 大文字小文字を無視する (preserveCase=false)
            var partialIgnoreCase = GetExclude("partial_ignorecase_val");
            Assert.AreEqual(SearchPathType.Partial, partialIgnoreCase.searchType.value, "Partial_IgnoreCase は Partial に変換されること");
            Assert.IsFalse(partialIgnoreCase.PreserveCase, "Partial_IgnoreCase は大文字小文字を無視する (preserveCase=false) こと");

            // Regex → 大文字小文字を区別する (preserveCase=true)
            var regex = GetExclude(".*\\.log$");
            Assert.AreEqual(SearchPathType.Regex, regex.searchType.value, "Regex は Regex に変換されること");
            Assert.IsTrue(regex.PreserveCase, "Regex (IgnoreCase なし) は大文字小文字を区別する (preserveCase=true) こと");

            // Regex_IgnoreCase → 大文字小文字を無視する (preserveCase=false)
            var regexIgnoreCase = GetExclude(".*\\.txt$");
            Assert.AreEqual(SearchPathType.Regex, regexIgnoreCase.searchType.value, "Regex_IgnoreCase は Regex に変換されること");
            Assert.IsFalse(regexIgnoreCase.PreserveCase, "Regex_IgnoreCase は大文字小文字を無視する (preserveCase=false) こと");
        }

        [Test]
        public void ConvertToLatest_V1AssetWithVersion1_SkipsInitialMigrationAndConvertsToV2()
        {
            Directory.CreateDirectory(TEST_ROOT);
            AssetDatabase.Refresh();

            // version=1 は INITIAL 変換（versionFile → packageNameSettings への移動）が完了済みの状態
            var legacy = ScriptableObject.CreateInstance<MizoresPackageExporterV1>();
            legacy.packageExporterVersion = 1;
            legacy.versionFile = null;
            legacy.versionFormat = null;
            legacy.packageName = null;
            legacy.packageNameSettings.versionSource = MizoresPackageExporterV1.VersionSource.String;
            legacy.packageNameSettings.versionString = "2.0.0";
            legacy.packageNameSettings.packageName = "V1MigratedPkg";
            legacy.objects.Add(new MizoresPackageExporterV1.PackagePrefsElement { path = "Assets/SomeAsset.prefab" });

            AssetDatabase.CreateAsset(legacy, V1_VERSION1_ASSET_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            MizoresPackageExporterUpdator.ConvertToLatest(legacy);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var converted = AssetDatabase.LoadAssetAtPath<MizoresPackageExporter>(V1_VERSION1_ASSET_PATH);
            Assert.IsNotNull(converted, "元のパスに V2 アセットが作成されること");
            Assert.AreEqual(MizoresPackageExporter.CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION, converted.packageExporterVersion);
            // INITIAL 変換で設定済みだった packageNameSettings の値が V2 に正しく引き継がれること
            Assert.AreEqual("V1MigratedPkg", converted.packageNameSettings.packageName);
            Assert.AreEqual("2.0.0", converted.packageNameSettings.versionString);
            Assert.AreEqual(VersionSource.String, converted.packageNameSettings.versionSource.value);
            Assert.AreEqual("Assets/SomeAsset.prefab", converted.objects.Single().Path);
        }
#pragma warning restore 612, 618

        [Test]
        public void IsLatest_V1Asset_ReturnsFalse()
        {
            var v1 = ScriptableObject.CreateInstance<MizoresPackageExporterV1>();
            try
            {
                Assert.IsFalse(MizoresPackageExporterUpdator.IsLatest(v1),
                    "V1 アセットは最新バージョンではないので false を返すこと");
            }
            finally
            {
                Object.DestroyImmediate(v1);
            }
        }

        [Test]
        public void IsCompatible_V1Asset_ReturnsTrue()
        {
#pragma warning disable 612, 618
            var v1 = ScriptableObject.CreateInstance<MizoresPackageExporterV1>();
            try
            {
                Assert.IsTrue(MizoresPackageExporterUpdator.IsCompatible(v1),
                    "V1 アセットは変換可能なので true を返すこと");
            }
            finally
            {
                Object.DestroyImmediate(v1);
            }
#pragma warning restore 612, 618
        }

        [Test]
        public void IsCompatible_CurrentV2Asset_ReturnsTrue()
        {
            var v2 = ScriptableObject.CreateInstance<MizoresPackageExporter>();
            try
            {
                Assert.IsTrue(MizoresPackageExporterUpdator.IsCompatible(v2),
                    "現在のバージョンの V2 アセットは互換性あるので true を返すこと");
            }
            finally
            {
                Object.DestroyImmediate(v2);
            }
        }

        [Test]
        public void IsCompatible_V2AssetWithFutureVersion_ReturnsFalse()
        {
            var v2 = ScriptableObject.CreateInstance<MizoresPackageExporter>();
            try
            {
                v2.packageExporterVersion = MizoresPackageExporter.CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION + 1;
                Assert.IsFalse(MizoresPackageExporterUpdator.IsCompatible(v2),
                    "未来のバージョンの V2 アセットは互換性なしとして false を返すこと");
            }
            finally
            {
                Object.DestroyImmediate(v2);
            }
        }
    }
}
