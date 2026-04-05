using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using MizoreNekoyanagi.PublishUtil.PackageExporter;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.Tests
{
    /// <summary>
    /// Batch Export 機能のテスト。
    /// UpdateBatchExportKeys が各モード（Single/Texts/Folders/ListFile）で
    /// 正しいキー一覧を返すことを検証する。
    /// </summary>
    public class BatchExportTests
    {
        const string TEST_ROOT   = "Assets/Tests_MizorePkgExp_BatchExport";
        const string BATCH_ROOT  = TEST_ROOT + "/BatchRoot";

        MizoresPackageExporter exporter;

        [SetUp]
        public void SetUp()
        {
            exporter = ScriptableObject.CreateInstance<MizoresPackageExporter>();
            exporter.name = "BatchTestExporter";
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
                var meta = TEST_ROOT + ".meta";
                if (File.Exists(meta)) File.Delete(meta);
            }
            AssetDatabase.Refresh();
        }

        // ===== Single モード =====

        [Test]
        public void Single_UpdateBatchExportKeys_ReturnsEmptyArray()
        {
            exporter.batchExportMode = BatchExportMode.Single;
            exporter.UpdateBatchExportKeys();

            Assert.AreEqual(0, exporter.BatchExportKeys.Length,
                "Single モードではキー配列が空であること");
        }

        // ===== Texts モード =====

        [Test]
        public void Texts_UpdateBatchExportKeys_ReturnsAllTexts()
        {
            exporter.batchExportMode = BatchExportMode.Texts;
            exporter.batchExportTexts.Add("Red");
            exporter.batchExportTexts.Add("Green");
            exporter.batchExportTexts.Add("Blue");
            exporter.UpdateBatchExportKeys();

            CollectionAssert.AreEquivalent(
                new[] { "Red", "Green", "Blue" },
                exporter.BatchExportKeys,
                "Texts モードでは登録した文字列がすべてキーとして返されること");
        }

        [Test]
        public void Texts_DuplicateEntries_Deduplicated()
        {
            exporter.batchExportMode = BatchExportMode.Texts;
            exporter.batchExportTexts.Add("Red");
            exporter.batchExportTexts.Add("Red");
            exporter.batchExportTexts.Add("Blue");
            exporter.UpdateBatchExportKeys();

            Assert.AreEqual(2, exporter.BatchExportKeys.Length,
                "重複する要素は Distinct により1件に集約されること");
            CollectionAssert.Contains(exporter.BatchExportKeys, "Red");
            CollectionAssert.Contains(exporter.BatchExportKeys, "Blue");
        }

        [Test]
        public void Texts_EmptyList_ReturnsEmptyArray()
        {
            exporter.batchExportMode = BatchExportMode.Texts;
            // batchExportTexts は空のまま
            exporter.UpdateBatchExportKeys();

            Assert.AreEqual(0, exporter.BatchExportKeys.Length);
        }

        // ===== Folders モード - All =====

        [Test]
        public void Folders_All_ListsFilesAndFoldersWithoutMetaFiles()
        {
            Directory.CreateDirectory(BATCH_ROOT + "/Red");
            Directory.CreateDirectory(BATCH_ROOT + "/Green");
            File.WriteAllText(BATCH_ROOT + "/README.txt", "readme");
            AssetDatabase.Refresh();

            exporter.batchExportMode       = BatchExportMode.Folders;
            exporter.batchExportFolderMode  = BatchExportFolderMode.All;
            exporter.batchExportFolderRoot  = new ObjectRefElement(BATCH_ROOT);
            exporter.batchExportFolderRegex = string.Empty;
            exporter.UpdateBatchExportKeys();

            var keys = exporter.BatchExportKeys;
            CollectionAssert.Contains(keys, "Red",    "フォルダ Red がキーに含まれること");
            CollectionAssert.Contains(keys, "Green",  "フォルダ Green がキーに含まれること");
            CollectionAssert.Contains(keys, "README", "ファイル README.txt の拡張子なし名がキーに含まれること");
            Assert.IsFalse(keys.Any(k => k.EndsWith(".meta")),
                ".meta ファイルはキーに含まれないこと");
        }

        // ===== Folders モード - Folders のみ =====

        [Test]
        public void Folders_FoldersOnly_ExcludesFiles()
        {
            Directory.CreateDirectory(BATCH_ROOT + "/Red");
            Directory.CreateDirectory(BATCH_ROOT + "/Green");
            File.WriteAllText(BATCH_ROOT + "/README.txt", "readme");
            AssetDatabase.Refresh();

            exporter.batchExportMode       = BatchExportMode.Folders;
            exporter.batchExportFolderMode  = BatchExportFolderMode.Folders;
            exporter.batchExportFolderRoot  = new ObjectRefElement(BATCH_ROOT);
            exporter.batchExportFolderRegex = string.Empty;
            exporter.UpdateBatchExportKeys();

            var keys = exporter.BatchExportKeys;
            CollectionAssert.Contains(keys, "Red",   "フォルダ Red はキーに含まれること");
            CollectionAssert.Contains(keys, "Green",  "フォルダ Green はキーに含まれること");
            CollectionAssert.DoesNotContain(keys, "README",
                "Folders モードではファイルはキーに含まれないこと");
        }

        // ===== Folders モード - Files のみ =====

        [Test]
        public void Folders_FilesOnly_ExcludesFolders()
        {
            Directory.CreateDirectory(BATCH_ROOT + "/Red");
            File.WriteAllText(BATCH_ROOT + "/README.txt", "readme");
            File.WriteAllText(BATCH_ROOT + "/CHANGELOG.txt", "changes");
            AssetDatabase.Refresh();

            exporter.batchExportMode       = BatchExportMode.Folders;
            exporter.batchExportFolderMode  = BatchExportFolderMode.Files;
            exporter.batchExportFolderRoot  = new ObjectRefElement(BATCH_ROOT);
            exporter.batchExportFolderRegex = string.Empty;
            exporter.UpdateBatchExportKeys();

            var keys = exporter.BatchExportKeys;
            CollectionAssert.Contains(keys, "README",    "ファイル README.txt の拡張子なし名はキーに含まれること");
            CollectionAssert.Contains(keys, "CHANGELOG", "ファイル CHANGELOG.txt の拡張子なし名はキーに含まれること");
            CollectionAssert.DoesNotContain(keys, "Red",
                "Files モードではフォルダはキーに含まれないこと");
        }

        // ===== Folders モード - 正規表現フィルタ =====

        [Test]
        public void Folders_RegexFilter_OnlyMatchingNamesIncluded()
        {
            // README の例と同じ構成: Materials フォルダだけ正規表現で除外する
            Directory.CreateDirectory(BATCH_ROOT + "/Red");
            Directory.CreateDirectory(BATCH_ROOT + "/Green");
            Directory.CreateDirectory(BATCH_ROOT + "/Materials");
            AssetDatabase.Refresh();

            exporter.batchExportMode       = BatchExportMode.Folders;
            exporter.batchExportFolderMode  = BatchExportFolderMode.Folders;
            exporter.batchExportFolderRoot  = new ObjectRefElement(BATCH_ROOT);
            exporter.batchExportFolderRegex = "^(?!Materials).+$"; // README の設定例

            exporter.UpdateBatchExportKeys();

            var keys = exporter.BatchExportKeys;
            CollectionAssert.Contains(keys, "Red",
                "正規表現にマッチする Red はキーに含まれること");
            CollectionAssert.Contains(keys, "Green",
                "正規表現にマッチする Green はキーに含まれること");
            CollectionAssert.DoesNotContain(keys, "Materials",
                "正規表現にマッチしない Materials はキーに含まれないこと");
        }

        [Test]
        public void Folders_InvalidRegex_TreatedAsMatchAll()
        {
            // 本番コードは ArgumentException をキャッチして空文字列の正規表現（全マッチ）に切り替える。
            // この設計はユーザーが正規表現を入力途中でもクラッシュしないようにするためのもの。
            Directory.CreateDirectory(BATCH_ROOT + "/Alpha");
            AssetDatabase.Refresh();

            exporter.batchExportMode       = BatchExportMode.Folders;
            exporter.batchExportFolderMode  = BatchExportFolderMode.Folders;
            exporter.batchExportFolderRoot  = new ObjectRefElement(BATCH_ROOT);
            exporter.batchExportFolderRegex = "[invalid";  // 無効な正規表現

            // 例外が出ずに処理が完了し、全エントリがキーに含まれること
            exporter.UpdateBatchExportKeys();
            CollectionAssert.Contains(exporter.BatchExportKeys, "Alpha",
                "無効な正規表現は空文字列パターン（全マッチ）として扱われ、エントリがキーに含まれること");
        }

        // ===== ListFile モード =====

        [Test]
        public void ListFile_TextAsset_ReturnsLinesAsKeys()
        {
            Directory.CreateDirectory(TEST_ROOT);
            const string LIST_FILE = TEST_ROOT + "/list.txt";
            File.WriteAllText(LIST_FILE, "alpha\nbeta\ngamma");
            AssetDatabase.Refresh();

            exporter.batchExportMode      = BatchExportMode.ListFile;
            exporter.batchExportListFile  = new ObjectRefElement(LIST_FILE);
            exporter.UpdateBatchExportKeys();

            CollectionAssert.AreEquivalent(
                new[] { "alpha", "beta", "gamma" },
                exporter.BatchExportKeys,
                "テキストファイルの各行がキーとして返されること");
        }

        [Test]
        public void ListFile_NullFile_ReturnsEmptyArray()
        {
            exporter.batchExportMode     = BatchExportMode.ListFile;
            exporter.batchExportListFile = null;
            exporter.UpdateBatchExportKeys();

            Assert.AreEqual(0, exporter.BatchExportKeys.Length,
                "batchExportListFile が null のときはキーが空であること");
        }

        [Test]
        public void ListFile_WithEmptyLines_EmptyLinesIncludedAsKey()
        {
            // 本番コードでは空白行のスキップが意図的にコメントアウトされている（空行もキーとして扱う仕様）。
            // この動作を明示的に検証し、コメントアウトが誤って外された場合に検出する。
            Directory.CreateDirectory(TEST_ROOT);
            const string LIST_FILE = TEST_ROOT + "/list_with_empty.txt";
            File.WriteAllText(LIST_FILE, "alpha\n\nbeta");
            AssetDatabase.Refresh();

            exporter.batchExportMode     = BatchExportMode.ListFile;
            exporter.batchExportListFile = new ObjectRefElement(LIST_FILE);
            exporter.UpdateBatchExportKeys();

            Assert.AreEqual(3, exporter.BatchExportKeys.Length,
                "空行のスキップはコメントアウトされているため、空行もキーとして含まれること");
            CollectionAssert.Contains(exporter.BatchExportKeys, "alpha");
            CollectionAssert.Contains(exporter.BatchExportKeys, "");
            CollectionAssert.Contains(exporter.BatchExportKeys, "beta");
        }

        // ===== GetBatchExportKeysConverted =====

        [Test]
        public void GetBatchExportKeysConverted_AppliesConvertDynamicPath_ToEachKey()
        {
            // batchExportTexts に変数を含むテキストを登録し、
            // GetBatchExportKeysConverted が ConvertDynamicPath を適用することを確認する
            exporter.batchExportMode = BatchExportMode.Texts;
            exporter.batchExportTexts.Add("%name%_variant");
            exporter.UpdateBatchExportKeys();

            var converted = exporter.GetBatchExportKeysConverted();

            Assert.AreEqual(1, converted.Length);
            Assert.AreEqual("BatchTestExporter_variant", converted[0],
                "GetBatchExportKeysConverted は各キーに ConvertDynamicPath を適用すること");
        }
    }
}
