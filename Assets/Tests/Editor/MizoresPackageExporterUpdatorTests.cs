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

            CollectionAssert.AreEquivalent(
                new[] { TEST_ROOT + "/Export/Alpha.mat", "%name%/%batch%" },
                reloaded.objects.Select(v => v.Path),
                "V1 の objects と dynamicpath が V2 の objects に統合されること");

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
#pragma warning restore 612, 618
    }
}
