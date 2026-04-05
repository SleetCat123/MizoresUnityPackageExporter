using NUnit.Framework;
using UnityEngine;
using MizoreNekoyanagi.PublishUtil.PackageExporter;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.Tests
{
    /// <summary>
    /// MizoresPackageExporter.ConvertDynamicPath の変数展開テスト。
    /// README に記載されている各変数（%name%, カスタム変数, %batch%, %batchf%,
    /// %version%, %versionf%, %packagename%）が正しく展開されることを検証する。
    /// </summary>
    public class ConvertDynamicPathTests
    {
        MizoresPackageExporter exporter;

        [SetUp]
        public void SetUp()
        {
            exporter = ScriptableObject.CreateInstance<MizoresPackageExporter>();
            exporter.name = "TestExporter";
        }

        [TearDown]
        public void TearDown()
        {
            if (exporter != null)
                Object.DestroyImmediate(exporter);
            exporter = null;
        }

        // ===== %name% =====

        [Test]
        public void Name_Standalone_ReplacedWithExporterName()
        {
            var result = exporter.ConvertDynamicPath("%name%", string.Empty);
            Assert.AreEqual("TestExporter", result);
        }

        [Test]
        public void Name_InPathString_ReplacedInPlace()
        {
            var result = exporter.ConvertDynamicPath("Assets/%name%/Pkg", string.Empty);
            Assert.AreEqual("Assets/TestExporter/Pkg", result);
        }

        // ===== カスタム変数 =====

        [Test]
        public void CustomVariable_SingleVariable_Replaced()
        {
            exporter.variables["channel"] = "stable";
            var result = exporter.ConvertDynamicPath("Assets/%channel%/Pkg", string.Empty);
            Assert.AreEqual("Assets/stable/Pkg", result);
        }

        [Test]
        public void CustomVariable_MultipleVariables_AllReplaced()
        {
            exporter.variables["platform"] = "Quest";
            exporter.variables["tier"] = "pro";
            var result = exporter.ConvertDynamicPath("%platform%_%tier%", string.Empty);
            Assert.AreEqual("Quest_pro", result);
        }

        [Test]
        public void CustomVariable_UndefinedKey_LeftAsIs()
        {
            // 未定義の変数キーはそのまま残る
            var result = exporter.ConvertDynamicPath("%undefined%", string.Empty);
            Assert.AreEqual("%undefined%", result);
        }

        // ===== %batch% =====

        [Test]
        public void Batch_WithNonEmptyKey_ReplacedWithKey()
        {
            var result = exporter.ConvertDynamicPath("%batch%", "Red");
            Assert.AreEqual("Red", result);
        }

        [Test]
        public void Batch_WithEmptyKey_ReplacedWithEmpty()
        {
            var result = exporter.ConvertDynamicPath("%batch%", string.Empty);
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void Batch_InPathString_ReplacedInPlace()
        {
            var result = exporter.ConvertDynamicPath("Assets/%batch%/Textures", "Green");
            Assert.AreEqual("Assets/Green/Textures", result);
        }

        // ===== %batchf% =====

        [Test]
        public void BatchFormatted_WithNonEmptyKey_AppliesDefaultBatchFormat()
        {
            // デフォルト batchFormat = "_%batch%"
            var result = exporter.ConvertDynamicPath("%batchf%", "Red");
            Assert.AreEqual("_Red", result);
        }

        [Test]
        public void BatchFormatted_WithEmptyKey_ReturnsEmpty()
        {
            var result = exporter.ConvertDynamicPath("%batchf%", string.Empty);
            Assert.AreEqual(string.Empty, result,
                "バッチキーが空の場合 %batchf% は空文字に展開されること");
        }

        [Test]
        public void BatchFormatted_CustomBatchFormat_Applied()
        {
            exporter.packageNameSettings.batchFormat = "[%batch%]";
            var result = exporter.ConvertDynamicPath("%batchf%", "Green");
            Assert.AreEqual("[Green]", result);
        }

        // ===== %version% =====

        [Test]
        public void Version_WithVersionSourceString_ReplacedWithVersionStringAsIs()
        {
            // VersionSource.String は文字列をそのまま使う（正規化なし）。
            // ファイル名用の正規化（. → _）は VersionSource.File のみで行われる。
            exporter.packageNameSettings.versionSource = VersionSource.String;
            exporter.packageNameSettings.versionString = "1.2.3";
            exporter.UpdateAllExportVersions();

            var result = exporter.ConvertDynamicPath("%version%", string.Empty);
            Assert.AreEqual("1.2.3", result,
                "VersionSource.String の場合、バージョン文字列は正規化されずそのまま展開されること");
        }

        [Test]
        public void Version_WithEmptyVersionString_ReplacedWithEmpty()
        {
            exporter.packageNameSettings.versionSource = VersionSource.String;
            exporter.packageNameSettings.versionString = "";
            exporter.UpdateAllExportVersions();

            var result = exporter.ConvertDynamicPath("%version%", string.Empty);
            Assert.AreEqual(string.Empty, result);
        }

        // ===== %versionf% =====

        [Test]
        public void VersionFormatted_WithNonEmptyVersion_AppliesDefaultVersionFormat()
        {
            // デフォルト versionFormat = "-%version%"
            // VersionSource.String は正規化なしのため "2.0.0" はそのまま使われる
            exporter.packageNameSettings.versionSource = VersionSource.String;
            exporter.packageNameSettings.versionString = "2.0.0";
            exporter.UpdateAllExportVersions();

            var result = exporter.ConvertDynamicPath("%versionf%", string.Empty);
            Assert.AreEqual("-2.0.0", result);
        }

        [Test]
        public void VersionFormatted_WithEmptyVersion_ReturnsEmpty()
        {
            exporter.packageNameSettings.versionSource = VersionSource.String;
            exporter.packageNameSettings.versionString = "";
            exporter.UpdateAllExportVersions();

            var result = exporter.ConvertDynamicPath("%versionf%", string.Empty);
            Assert.AreEqual(string.Empty, result,
                "バージョンが空の場合 %versionf% は空文字に展開されること");
        }

        [Test]
        public void VersionFormatted_CustomVersionFormat_Applied()
        {
            // VersionSource.String は正規化なしのため "3.0.0" はそのまま使われる
            exporter.packageNameSettings.versionSource = VersionSource.String;
            exporter.packageNameSettings.versionString = "3.0.0";
            exporter.packageNameSettings.versionFormat = "_v%version%";
            exporter.UpdateAllExportVersions();

            var result = exporter.ConvertDynamicPath("%versionf%", string.Empty);
            Assert.AreEqual("_v3.0.0", result);
        }

        // ===== %packagename% =====

        [Test]
        public void PackageName_ExpandedToResolvedPackageName()
        {
            exporter.packageNameSettings.packageName = "MyPackage";
            var result = exporter.ConvertDynamicPath("prefix_%packagename%_suffix", string.Empty);
            Assert.AreEqual("prefix_MyPackage_suffix", result);
        }

        [Test]
        public void PackageName_ContainingInvalidChars_SanitizedInExpansion()
        {
            // packageName 展開時はファイル名用に無効文字（Path.GetInvalidFileNameChars に含まれる '/' ':' など）が
            // '_' に置換される（ExporterUtils.InvalidFileCharsRegex の置換結果）
            exporter.packageNameSettings.packageName = "My/Package:Name";
            var result = exporter.ConvertDynamicPath("%packagename%", string.Empty);
            Assert.AreEqual("My_Package_Name", result,
                "展開された packagename の '/' と ':' が '_' に置換されること");
        }

        // ===== %.name% (相対名変数) =====
        // %.name% は GetDirectoryPath() が正しいパスを返す「保存済みアセット」でのみ機能する。
        // 未保存の ScriptableObject では GetDirectoryPath() が空パスを返すため、
        // 別テストクラス ConvertDynamicPath_RelativeNameTests で保存済みアセットを使って検証する。

        // ===== Dynamic Path in ObjectRefElement.GetConvertedPath =====

        [Test]
        public void ObjectRefElement_DynamicPath_NameVariableReplaced()
        {
            var element = new ExportTargetObjectElement("Assets/%name%/Textures");
            var converted = element.GetConvertedPath(exporter, string.Empty);
            Assert.AreEqual("Assets/TestExporter/Textures", converted,
                "ExportTargetObjectElement のパスに %name% が含まれる場合、ConvertDynamicPath で展開されること");
        }

        [Test]
        public void ObjectRefElement_DynamicPathWithBatch_BatchKeyReplaced()
        {
            var element = new ExportTargetObjectElement("Assets/%batch%/Textures");
            var converted = element.GetConvertedPath(exporter, "Red");
            Assert.AreEqual("Assets/Red/Textures", converted,
                "ExportTargetObjectElement のパスに %batch% が含まれる場合、batchExportKey で展開されること");
        }

        [Test]
        public void ObjectRefElement_CustomVariableInPath_Replaced()
        {
            exporter.variables["color"] = "Blue";
            var element = new ExportTargetObjectElement("Assets/%color%/Material");
            var converted = element.GetConvertedPath(exporter, string.Empty);
            Assert.AreEqual("Assets/Blue/Material", converted);
        }

    }
}
