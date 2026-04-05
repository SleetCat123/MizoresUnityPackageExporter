using NUnit.Framework;
using MizoreNekoyanagi.PublishUtil.PackageExporter;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.Tests
{
    /// <summary>
    /// PackageNameSettings.SetBase() のoverrideフラグ動作テスト
    /// </summary>
    public class PackageNameSettingsTests
    {
        // ===== useOverride_version =====

        [Test]
        public void SetBase_UseOverrideVersionFalse_InheritsVersionFromBase()
        {
            var baseSettings = new PackageNameSettings();
            baseSettings.versionSource = VersionSource.String;
            baseSettings.versionString = "2.0.0";

            var settings = new PackageNameSettings();
            settings.useOverride_version = false;
            settings.versionString = "1.0.0"; // should be overwritten

            settings.SetBase(baseSettings);

            Assert.AreEqual("2.0.0", settings.versionString);
        }

        [Test]
        public void SetBase_UseOverrideVersionFalse_InheritsVersionSourceFromBase()
        {
            // SetBase は versionString だけでなく versionSource も継承する。
            // このフィールドが継承対象から漏れたときに検出するためのテスト。
            var baseSettings = new PackageNameSettings();
            baseSettings.versionSource = VersionSource.File;

            var settings = new PackageNameSettings();
            settings.useOverride_version = false;
            settings.versionSource = VersionSource.String; // should be overwritten

            settings.SetBase(baseSettings);

            Assert.IsTrue(settings.versionSource == VersionSource.File,
                "useOverride_version=false のとき versionSource もベースから継承されること");
        }

        [Test]
        public void SetBase_UseOverrideVersionFalse_InheritsVersionFileReferenceFromBase()
        {
            // SetBase は versionFile の参照もベースから引き継ぐ。
            var baseSettings = new PackageNameSettings();
            var expectedFile = new ObjectRefElement("Assets/version.json");
            baseSettings.versionFile = expectedFile;

            var settings = new PackageNameSettings();
            settings.useOverride_version = false;
            settings.versionFile = null; // should be overwritten

            settings.SetBase(baseSettings);

            Assert.AreEqual(expectedFile, settings.versionFile,
                "useOverride_version=false のとき versionFile もベースから継承されること");
        }

        [Test]
        public void SetBase_UseOverrideVersionTrue_KeepsOwnVersion()
        {
            var baseSettings = new PackageNameSettings();
            baseSettings.versionSource = VersionSource.String;
            baseSettings.versionString = "2.0.0";

            var settings = new PackageNameSettings();
            settings.useOverride_version = true;
            settings.versionString = "1.0.0";

            settings.SetBase(baseSettings);

            Assert.AreEqual("1.0.0", settings.versionString);
        }

        // ===== useOverride_versionFormat =====

        [Test]
        public void SetBase_UseOverrideVersionFormatFalse_InheritsFormatFromBase()
        {
            var baseSettings = new PackageNameSettings();
            baseSettings.versionFormat = "_v%version%";

            var settings = new PackageNameSettings();
            settings.useOverride_versionFormat = false;
            settings.versionFormat = "own-format";

            settings.SetBase(baseSettings);

            Assert.AreEqual("_v%version%", settings.versionFormat);
        }

        [Test]
        public void SetBase_UseOverrideVersionFormatTrue_KeepsOwnFormat()
        {
            var baseSettings = new PackageNameSettings();
            baseSettings.versionFormat = "_v%version%";

            var settings = new PackageNameSettings();
            settings.useOverride_versionFormat = true;
            settings.versionFormat = "own-format";

            settings.SetBase(baseSettings);

            Assert.AreEqual("own-format", settings.versionFormat);
        }

        // ===== useOverride_batchFormat =====

        [Test]
        public void SetBase_UseOverrideBatchFormatFalse_InheritsBatchFormatFromBase()
        {
            var baseSettings = new PackageNameSettings();
            baseSettings.batchFormat = "-%batch%-custom";

            var settings = new PackageNameSettings();
            settings.useOverride_batchFormat = false;
            settings.batchFormat = "own-batch";

            settings.SetBase(baseSettings);

            Assert.AreEqual("-%batch%-custom", settings.batchFormat);
        }

        [Test]
        public void SetBase_UseOverrideBatchFormatTrue_KeepsOwnBatchFormat()
        {
            var baseSettings = new PackageNameSettings();
            baseSettings.batchFormat = "-%batch%-custom";

            var settings = new PackageNameSettings();
            settings.useOverride_batchFormat = true;
            settings.batchFormat = "own-batch";

            settings.SetBase(baseSettings);

            Assert.AreEqual("own-batch", settings.batchFormat);
        }

        // ===== useOverride_packageName =====

        [Test]
        public void SetBase_UseOverridePackageNameFalse_InheritsPackageNameFromBase()
        {
            var baseSettings = new PackageNameSettings();
            baseSettings.packageName = "BasePkg%name%";

            var settings = new PackageNameSettings();
            settings.useOverride_packageName = false;
            settings.packageName = "OwnPkg";

            settings.SetBase(baseSettings);

            Assert.AreEqual("BasePkg%name%", settings.packageName);
        }

        [Test]
        public void SetBase_UseOverridePackageNameTrue_KeepsOwnPackageName()
        {
            var baseSettings = new PackageNameSettings();
            baseSettings.packageName = "BasePkg%name%";

            var settings = new PackageNameSettings();
            settings.useOverride_packageName = true;
            settings.packageName = "OwnPkg";

            settings.SetBase(baseSettings);

            Assert.AreEqual("OwnPkg", settings.packageName);
        }

        // ===== コピーコンストラクタ =====

        [Test]
        public void CopyConstructor_CopiesAllUseOverrideFlags()
        {
            // useOverride_* フラグをデフォルトと異なる値に設定してコピーし、
            // コピー先に正しく引き継がれることを検証する。
            // （コピーコンストラクタからこれらのフィールドが抜けると全て失敗する）
            var source = new PackageNameSettings();
            source.useOverride_version       = false;  // デフォルト true → 反転
            source.useOverride_versionFormat = true;   // デフォルト false → 反転
            source.useOverride_batchFormat   = true;   // デフォルト false → 反転
            source.useOverride_packageName   = true;   // デフォルト false → 反転

            var copy = new PackageNameSettings(source);

            Assert.IsFalse(copy.useOverride_version,       "useOverride_version should be copied");
            Assert.IsTrue(copy.useOverride_versionFormat,  "useOverride_versionFormat should be copied");
            Assert.IsTrue(copy.useOverride_batchFormat,    "useOverride_batchFormat should be copied");
            Assert.IsTrue(copy.useOverride_packageName,    "useOverride_packageName should be copied");
        }

        // ===== デフォルト値の確認 =====

        [Test]
        public void DefaultValues_UseOverrideVersion_IsTrue()
        {
            // version は override=true がデフォルト（自分自身の値を優先）
            var settings = new PackageNameSettings();
            Assert.IsTrue(settings.useOverride_version);
        }

        [Test]
        public void DefaultValues_UseOverrideFormat_IsFalse()
        {
            // format 系は override=false がデフォルト（ベースから継承）
            var settings = new PackageNameSettings();
            Assert.IsFalse(settings.useOverride_versionFormat);
            Assert.IsFalse(settings.useOverride_batchFormat);
            Assert.IsFalse(settings.useOverride_packageName);
        }
    }
}
