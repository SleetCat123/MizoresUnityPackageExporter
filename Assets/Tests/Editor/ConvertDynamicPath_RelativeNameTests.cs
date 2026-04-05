using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using MizoreNekoyanagi.PublishUtil.PackageExporter;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.Tests
{
    /// <summary>
    /// %.name% / %..name% (相対名変数) のテスト。
    /// GetDirectoryPath() が正しいパスを返すために、exporter を AssetDatabase に保存してから検証する。
    ///
    /// %.name%  → exporter が置かれているフォルダ名（1階層上）
    /// %..name% → 2階層上のフォルダ名
    /// </summary>
    public class ConvertDynamicPath_RelativeNameTests
    {
        const string TEST_ROOT    = "Assets/Tests_ConvertDynamic_RelName";
        const string PARENT_DIR   = TEST_ROOT + "/ParentFolder";
        const string ASSET_PATH   = PARENT_DIR + "/TestExporter.asset";

        MizoresPackageExporter exporter;

        [SetUp]
        public void SetUp()
        {
            Directory.CreateDirectory(PARENT_DIR);
            exporter = ScriptableObject.CreateInstance<MizoresPackageExporter>();
            exporter.name = "TestExporter";
            AssetDatabase.CreateAsset(exporter, ASSET_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            // 保存済みアセットを再ロード（GetDirectoryPath が正しいパスを返すように）
            exporter = AssetDatabase.LoadAssetAtPath<MizoresPackageExporter>(ASSET_PATH);
        }

        [TearDown]
        public void TearDown()
        {
            exporter = null;
            if (Directory.Exists(TEST_ROOT))
            {
                Directory.Delete(TEST_ROOT, recursive: true);
                var meta = TEST_ROOT + ".meta";
                if (File.Exists(meta)) File.Delete(meta);
            }
            AssetDatabase.Refresh();
        }

        [Test]
        public void RelativeName_SingleDot_ReplacedWithImmediateParentFolderName()
        {
            // exporter は "Assets/Tests_ConvertDynamic_RelName/ParentFolder/TestExporter.asset" に保存されているため
            // %.name% は "ParentFolder" に展開されること
            var result = exporter.ConvertDynamicPath("%.name%", string.Empty);
            Assert.AreEqual("ParentFolder", result,
                "%.name% は exporter が置かれているフォルダ名（ParentFolder）に展開されること");
        }

        [Test]
        public void RelativeName_TwoDots_ReplacedWithGrandparentFolderName()
        {
            // %..name% は 2 階層上（Tests_ConvertDynamic_RelName）に展開されること
            var result = exporter.ConvertDynamicPath("%..name%", string.Empty);
            Assert.AreEqual("Tests_ConvertDynamic_RelName", result,
                "%..name% は exporter の 2 階層上のフォルダ名（Tests_ConvertDynamic_RelName）に展開されること");
        }

        [Test]
        public void RelativeName_EmbeddedInPath_ReplacedCorrectly()
        {
            // パス文字列の中に埋め込まれた場合も正しく展開されること
            var result = exporter.ConvertDynamicPath("Exports/%.name%/%name%", string.Empty);
            Assert.AreEqual("Exports/ParentFolder/TestExporter", result,
                "%.name% がパス文字列の途中にあっても正しく展開されること");
        }
    }
}
