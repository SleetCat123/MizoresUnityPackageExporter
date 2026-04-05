#if UNITY_2022_1_OR_NEWER
using System.IO.Compression;
#endif
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using MizoreNekoyanagi.PublishUtil.PackageExporter;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.Tests
{
    /// <summary>
    /// MizoresPackageExporter.ExecutePostExport のテスト。
    /// フォルダ整理・追加コピーパス・ZIP作成の各機能を検証する。
    ///
    /// テスト用の unitypackage はシステム temp フォルダに作成し、TearDown で削除する。
    /// </summary>
    public class PostExportTests
    {
        string _tempDir;
        MizoresPackageExporter _exporter;
        ExporterEditorLogs _logs;

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(
                Path.GetTempPath(),
                "MizorePkgExpPostTest_" + System.Guid.NewGuid().ToString("N")
            ).Replace('\\', '/');
            Directory.CreateDirectory(_tempDir);

            _exporter = ScriptableObject.CreateInstance<MizoresPackageExporter>();
            _exporter.name = "PostExportTestPkg";

            _logs = new ExporterEditorLogs();
        }

        [TearDown]
        public void TearDown()
        {
            if (_exporter != null)
                Object.DestroyImmediate(_exporter);
            _exporter = null;

            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }

        /// <summary>
        /// _tempDir 内にダミーの unitypackage ファイルを作成して絶対パスを返す。
        /// </summary>
        string CreateFakePackage(string filename = "TestPackage.unitypackage")
        {
            var path = Path.Combine(_tempDir, filename).Replace('\\', '/');
            File.WriteAllBytes(path, new byte[] { 0x50, 0x4B, 0x03, 0x04 }); // minimal ZIP header
            return path;
        }

        // ===== Organize in Folder =====

        [Test]
        public void OrganizeInFolder_PackageMovedToSubfolderNamedAfterPackage()
        {
            _exporter.organizeInFolder = true;
            var exportPath = CreateFakePackage("TestPackage.unitypackage");

            MizoresPackageExporter.ExecutePostExport(
                _exporter, string.Empty, exportPath, new FilePathList(), _logs);

            var expectedFolder  = Path.Combine(_tempDir, "TestPackage").Replace('\\', '/');
            var expectedPackage = Path.Combine(expectedFolder, "TestPackage.unitypackage").Replace('\\', '/');

            Assert.IsTrue(Directory.Exists(expectedFolder),
                "パッケージ名と同名のフォルダが作成されること");
            Assert.IsTrue(File.Exists(expectedPackage),
                "unitypackage がフォルダ内に移動されること");
            Assert.IsFalse(File.Exists(exportPath),
                "元の unitypackage は移動後に元の場所に存在しないこと");
        }

        [Test]
        public void OrganizeInFolder_CustomFolderName_UsesOrganizeFolderName()
        {
            _exporter.organizeInFolder  = true;
            _exporter.organizeFolderName = "CustomFolder";
            var exportPath = CreateFakePackage("TestPackage.unitypackage");

            MizoresPackageExporter.ExecutePostExport(
                _exporter, string.Empty, exportPath, new FilePathList(), _logs);

            var expectedFolder = Path.Combine(_tempDir, "CustomFolder").Replace('\\', '/');
            Assert.IsTrue(Directory.Exists(expectedFolder),
                "organizeFolderName で指定したフォルダ名が使用されること");
        }

        [Test]
        public void OrganizeInFolder_FolderNameWithNameVariable_VariableReplaced()
        {
            _exporter.organizeInFolder   = true;
            _exporter.organizeFolderName = "%name%_output";
            var exportPath = CreateFakePackage("TestPackage.unitypackage");

            MizoresPackageExporter.ExecutePostExport(
                _exporter, string.Empty, exportPath, new FilePathList(), _logs);

            var expectedFolder = Path.Combine(_tempDir, "PostExportTestPkg_output").Replace('\\', '/');
            Assert.IsTrue(Directory.Exists(expectedFolder),
                "organizeFolderName の %name% はエクスポーター名で置換されること");
        }

        [Test]
        public void OrganizeInFolder_ExistingFolderRenamed_WithTimestampSuffix()
        {
            _exporter.organizeInFolder = true;
            var exportPath = CreateFakePackage("TestPackage.unitypackage");

            // 同名フォルダを事前に作成
            var preexistingFolder = Path.Combine(_tempDir, "TestPackage").Replace('\\', '/');
            Directory.CreateDirectory(preexistingFolder);

            MizoresPackageExporter.ExecutePostExport(
                _exporter, string.Empty, exportPath, new FilePathList(), _logs);

            // 既存フォルダはタイムスタンプ付きにリネームされて残り、新フォルダが作成される
            var foldersAfter = Directory.GetDirectories(_tempDir);
            Assert.AreEqual(2, foldersAfter.Length,
                "既存フォルダはリネームされ新フォルダが追加作成されるため合計2フォルダになること");
        }

        [Test]
        public void OrganizeOff_PackageNotMoved_ReturnsNull()
        {
            _exporter.organizeInFolder = false;
            var exportPath = CreateFakePackage("TestPackage.unitypackage");

            var result = MizoresPackageExporter.ExecutePostExport(
                _exporter, string.Empty, exportPath, new FilePathList(), _logs);

            Assert.IsNull(result,
                "organizeInFolder=false かつ createZip=false の場合は null が返ること");
            Assert.IsTrue(File.Exists(exportPath),
                "organizeInFolder=false の場合、unitypackage は移動されないこと");
        }

        // ===== Additional Copy Paths =====

        [Test]
        public void AdditionalCopyPaths_FileNoDestName_CopiedWithOriginalName()
        {
            _exporter.organizeInFolder = true;
            var srcFile = Path.Combine(_tempDir, "README.txt").Replace('\\', '/');
            File.WriteAllText(srcFile, "readme content");
            _exporter.additionalCopyPaths.Add(new AdditionalCopyPath(srcFile, destName: ""));

            var exportPath = CreateFakePackage("TestPackage.unitypackage");
            MizoresPackageExporter.ExecutePostExport(
                _exporter, string.Empty, exportPath, new FilePathList(), _logs);

            var destFile = Path.Combine(_tempDir, "TestPackage", "README.txt").Replace('\\', '/');
            Assert.IsTrue(File.Exists(destFile),
                "destName が空の場合は元のファイル名でコピーされること");
            Assert.AreEqual("readme content", File.ReadAllText(destFile),
                "コピー後のファイル内容が元と同一であること");
        }

        [Test]
        public void AdditionalCopyPaths_FileWithDestName_CopiedWithGivenName()
        {
            _exporter.organizeInFolder = true;
            var srcFile = Path.Combine(_tempDir, "README.txt").Replace('\\', '/');
            File.WriteAllText(srcFile, "content");
            _exporter.additionalCopyPaths.Add(
                new AdditionalCopyPath(srcFile, destName: "readme_renamed.txt"));

            var exportPath = CreateFakePackage("TestPackage.unitypackage");
            MizoresPackageExporter.ExecutePostExport(
                _exporter, string.Empty, exportPath, new FilePathList(), _logs);

            var destFile = Path.Combine(_tempDir, "TestPackage", "readme_renamed.txt")
                .Replace('\\', '/');
            Assert.IsTrue(File.Exists(destFile),
                "destName を指定した場合は指定した名前でコピーされること");
        }

        [Test]
        public void AdditionalCopyPaths_FileWithSubdirDestName_CopiedIntoSubdir()
        {
            // destName が "/" で終わる場合は指定サブフォルダ内に元のファイル名でコピーされる
            _exporter.organizeInFolder = true;
            var srcFile = Path.Combine(_tempDir, "README.txt").Replace('\\', '/');
            File.WriteAllText(srcFile, "content");
            _exporter.additionalCopyPaths.Add(
                new AdditionalCopyPath(srcFile, destName: "docs/"));

            var exportPath = CreateFakePackage("TestPackage.unitypackage");
            MizoresPackageExporter.ExecutePostExport(
                _exporter, string.Empty, exportPath, new FilePathList(), _logs);

            var destFile = Path.Combine(_tempDir, "TestPackage", "docs", "README.txt")
                .Replace('\\', '/');
            Assert.IsTrue(File.Exists(destFile),
                "destName が '/' で終わる場合はサブフォルダ内に元のファイル名でコピーされること");
        }

        [Test]
        public void AdditionalCopyPaths_Folder_CopiesContentsPreservingStructure()
        {
            _exporter.organizeInFolder = true;

            var srcDir = Path.Combine(_tempDir, "SrcDir").Replace('\\', '/');
            Directory.CreateDirectory(srcDir + "/sub");
            File.WriteAllText(srcDir + "/file.txt",         "content");
            File.WriteAllText(srcDir + "/sub/nested.txt",   "nested");
            File.WriteAllText(srcDir + "/ignored.meta",     "meta");  // .meta は除外される

            _exporter.additionalCopyPaths.Add(new AdditionalCopyPath(srcDir, destName: ""));

            var exportPath = CreateFakePackage("TestPackage.unitypackage");
            MizoresPackageExporter.ExecutePostExport(
                _exporter, string.Empty, exportPath, new FilePathList(), _logs);

            var dest = Path.Combine(_tempDir, "TestPackage").Replace('\\', '/');
            Assert.IsTrue(File.Exists(Path.Combine(dest, "file.txt").Replace('\\', '/')),
                "フォルダコピー時はルート直下のファイルがコピーされること");
            Assert.IsTrue(
                File.Exists(Path.Combine(dest, "sub", "nested.txt").Replace('\\', '/')),
                "フォルダコピー時はサブフォルダの構造が維持されること");
            Assert.IsFalse(
                File.Exists(Path.Combine(dest, "ignored.meta").Replace('\\', '/')),
                ".meta ファイルはコピーされないこと");
        }

        [Test]
        public void AdditionalCopyPaths_FolderWithDestName_CopiedIntoNamedSubdir()
        {
            _exporter.organizeInFolder = true;

            var srcDir = Path.Combine(_tempDir, "SrcDir").Replace('\\', '/');
            Directory.CreateDirectory(srcDir);
            File.WriteAllText(srcDir + "/file.txt", "content");

            _exporter.additionalCopyPaths.Add(
                new AdditionalCopyPath(srcDir, destName: "extras"));

            var exportPath = CreateFakePackage("TestPackage.unitypackage");
            MizoresPackageExporter.ExecutePostExport(
                _exporter, string.Empty, exportPath, new FilePathList(), _logs);

            var destFile = Path.Combine(_tempDir, "TestPackage", "extras", "file.txt")
                .Replace('\\', '/');
            Assert.IsTrue(File.Exists(destFile),
                "フォルダに destName を指定した場合は指定名のサブフォルダ内にコピーされること");
        }

#if UNITY_2022_1_OR_NEWER
        // ===== Create ZIP =====

        [Test]
        public void CreateZip_OrganizeOff_ZipsOnlyUnitypackage()
        {
            _exporter.organizeInFolder = false;
            _exporter.createZip        = true;
            _exporter.zipFolderName    = "_zip";
            var exportPath = CreateFakePackage("TestPackage.unitypackage");

            MizoresPackageExporter.ExecutePostExport(
                _exporter, string.Empty, exportPath, new FilePathList(), _logs);

            var zipPath = Path.Combine(_tempDir, "_zip", "TestPackage.zip").Replace('\\', '/');
            Assert.IsTrue(File.Exists(zipPath), "ZIP ファイルが _zip フォルダに生成されること");

            using (var zip = ZipFile.OpenRead(zipPath))
            {
                Assert.AreEqual(1, zip.Entries.Count,
                    "organizeInFolder=false のとき ZIP には unitypackage 1 件のみ含まれること");
                Assert.AreEqual("TestPackage.unitypackage", zip.Entries[0].FullName,
                    "ZIP エントリ名が unitypackage のファイル名と一致すること");
            }
        }

        [Test]
        public void CreateZip_OrganizeOn_ZipsFolderContentsIncludingAdditionalFiles()
        {
            _exporter.organizeInFolder = true;
            _exporter.createZip        = true;
            _exporter.zipFolderName    = "_zip";

            var srcFile = Path.Combine(_tempDir, "README.txt").Replace('\\', '/');
            File.WriteAllText(srcFile, "readme");
            _exporter.additionalCopyPaths.Add(new AdditionalCopyPath(srcFile, ""));

            var exportPath = CreateFakePackage("TestPackage.unitypackage");
            MizoresPackageExporter.ExecutePostExport(
                _exporter, string.Empty, exportPath, new FilePathList(), _logs);

            var zipPath = Path.Combine(_tempDir, "_zip", "TestPackage.zip").Replace('\\', '/');
            Assert.IsTrue(File.Exists(zipPath), "ZIP ファイルが生成されること");

            using (var zip = ZipFile.OpenRead(zipPath))
            {
                var names = zip.Entries.Select(e => e.FullName).ToList();
                Assert.IsTrue(names.Any(n => n == "TestPackage.unitypackage"),
                    "ZIP に unitypackage が含まれること");
                Assert.IsTrue(names.Any(n => n == "README.txt"),
                    "ZIP に additionalCopyPaths で追加したファイルが含まれること");
            }
        }

        [Test]
        public void CreateZip_EmptyZipFolderName_ZipCreatedBesidePackage()
        {
            _exporter.organizeInFolder = false;
            _exporter.createZip        = true;
            _exporter.zipFolderName    = "";  // 空の場合はパッケージと同じ場所
            var exportPath = CreateFakePackage("TestPackage.unitypackage");

            MizoresPackageExporter.ExecutePostExport(
                _exporter, string.Empty, exportPath, new FilePathList(), _logs);

            var zipPath = Path.Combine(_tempDir, "TestPackage.zip").Replace('\\', '/');
            Assert.IsTrue(File.Exists(zipPath),
                "zipFolderName が空の場合、ZIP はパッケージと同じ場所に生成されること");
        }

        [Test]
        public void CreateZip_ReturnsZipFilePath()
        {
            _exporter.createZip     = true;
            _exporter.zipFolderName = "_zip";
            var exportPath = CreateFakePackage("TestPackage.unitypackage");

            var result = MizoresPackageExporter.ExecutePostExport(
                _exporter, string.Empty, exportPath, new FilePathList(), _logs);

            Assert.IsNotNull(result, "createZip=true の場合は ZIP パスが返ること");
            StringAssert.EndsWith(".zip", result,
                "返されたパスは .zip で終わること");
            Assert.IsTrue(File.Exists(result),
                "返されたパスに実際に ZIP ファイルが存在すること");
        }
#endif
    }
}
