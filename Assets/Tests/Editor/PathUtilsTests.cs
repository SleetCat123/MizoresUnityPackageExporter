using NUnit.Framework;
using MizoreNekoyanagi.PublishUtil.PackageExporter;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.Tests
{
    /// <summary>
    /// PathUtils の静的メソッドのテスト。
    /// PathUtils は System.IO と System.Uri のみに依存する純粋な文字列操作ユーティリティ。
    /// </summary>
    public class PathUtilsTests
    {
        // ===== IsRelativePath =====

        [Test]
        public void IsRelativePath_DotSlash_ReturnsTrue()
        {
            Assert.IsTrue(PathUtils.IsRelativePath("./Assets/Scripts/Foo.cs"));
        }

        [Test]
        public void IsRelativePath_DotDotSlash_ReturnsTrue()
        {
            Assert.IsTrue(PathUtils.IsRelativePath("../Assets/Scripts"));
        }

        [Test]
        public void IsRelativePath_JustDot_ReturnsTrue()
        {
            Assert.IsTrue(PathUtils.IsRelativePath("."));
        }

        [Test]
        public void IsRelativePath_AbsoluteAssetPath_ReturnsFalse()
        {
            Assert.IsFalse(PathUtils.IsRelativePath("Assets/Scripts/Foo.cs"));
        }

        [Test]
        public void IsRelativePath_Null_ReturnsFalse()
        {
            Assert.IsFalse(PathUtils.IsRelativePath(null));
        }

        [Test]
        public void IsRelativePath_Empty_ReturnsFalse()
        {
            Assert.IsFalse(PathUtils.IsRelativePath(string.Empty));
        }

        // ===== GetProjectAbsolutePath =====

        [Test]
        public void GetProjectAbsolutePath_DotSlashChild_ResolvesCorrectly()
        {
            var result = PathUtils.GetProjectAbsolutePath("Assets/Scripts/", "./Foo.cs");
            Assert.AreEqual("Assets/Scripts/Foo.cs", result);
        }

        [Test]
        public void GetProjectAbsolutePath_DotSlashNested_ResolvesCorrectly()
        {
            var result = PathUtils.GetProjectAbsolutePath("Assets/Scripts/", "./Sub/Foo.cs");
            Assert.AreEqual("Assets/Scripts/Sub/Foo.cs", result);
        }

        [Test]
        public void GetProjectAbsolutePath_DotDotSlash_GoesUpOneLevel()
        {
            var result = PathUtils.GetProjectAbsolutePath("Assets/Scripts/", "../Textures/Logo.png");
            Assert.AreEqual("Assets/Textures/Logo.png", result);
        }

        [Test]
        public void GetProjectAbsolutePath_AlreadyAbsolute_ReturnsAsIs()
        {
            var result = PathUtils.GetProjectAbsolutePath("Assets/Scripts/", "Assets/Textures/Logo.png");
            Assert.AreEqual("Assets/Textures/Logo.png", result);
        }

        [Test]
        public void GetProjectAbsolutePath_RoundTrip_MatchesOriginal()
        {
            // GetRelativePath → GetProjectAbsolutePath で元のパスに戻ること
            var original = "Assets/Textures/Logo.png";
            var basePath = "Assets/Scripts/";
            var relative = PathUtils.GetRelativePath(basePath, original);
            var restored = PathUtils.GetProjectAbsolutePath(basePath, relative);
            Assert.AreEqual(original, restored,
                "GetRelativePath の結果を GetProjectAbsolutePath に渡すと元のパスに戻ること");
        }

        // ===== GetRelativePath =====

        [Test]
        public void GetRelativePath_ChildPath_ReturnsDotSlashRelative()
        {
            var result = PathUtils.GetRelativePath("Assets/Scripts/", "Assets/Scripts/Foo.cs");
            Assert.AreEqual("./Foo.cs", result);
        }

        [Test]
        public void GetRelativePath_AlreadyRelative_ReturnsAsIs()
        {
            var result = PathUtils.GetRelativePath("Assets/Scripts/", "./Foo.cs");
            Assert.AreEqual("./Foo.cs", result);
        }

        [Test]
        public void GetRelativePath_SiblingFolder_ResultIsRelativeAndRoundTrips()
        {
            var original = "Assets/Textures/Logo.png";
            var basePath = "Assets/Scripts/";
            var result = PathUtils.GetRelativePath(basePath, original);

            Assert.IsTrue(PathUtils.IsRelativePath(result),
                "兄弟フォルダへのパスは相対パス（'.' 始まり）として返されること");
            // ラウンドトリップで元のパスに戻ることも検証
            Assert.AreEqual(original, PathUtils.GetProjectAbsolutePath(basePath, result),
                "GetRelativePath の結果を GetProjectAbsolutePath に渡すと元のパスに戻ること");
        }

        [Test]
        public void GetRelativePath_PathContainingSpace_PreservesSpace()
        {
            var original = "Assets/My Folder/Foo.cs";
            var basePath = "Assets/Scripts/";
            var relative = PathUtils.GetRelativePath(basePath, original);
            Assert.IsFalse(relative.Contains("%20"),
                "スペースが %20 にエンコードされないこと");
            var restored = PathUtils.GetProjectAbsolutePath(basePath, relative);
            Assert.AreEqual(original, restored,
                "スペースを含むパスでもラウンドトリップが成立すること");
        }

        [Test]
        public void GetRelativePath_PathContainingPercent_PreservesPercent()
        {
            // %を含むパスの相対化・絶対化でパーセント文字が化けないこと
            var original = "Assets/My%20Package/Foo.cs";
            var basePath = "Assets/Scripts/";
            var relative = PathUtils.GetRelativePath(basePath, original);
            var restored = PathUtils.GetProjectAbsolutePath(basePath, relative);
            Assert.AreEqual(original, restored,
                "パスに '%' が含まれる場合もラウンドトリップが成立すること");
        }

        // ===== GetRelativePath (ドットを含むフォルダ名) =====

        /// <summary>
        /// ターゲットパスにドットを含むフォルダ名が含まれる場合（例: "my.package.v1"）でも
        /// GetRelativePath → GetProjectAbsolutePath のラウンドトリップが成立すること。
        /// System.Uri がドットをファイル拡張子と誤認しないことを確認するテスト。
        /// </summary>
        [Test]
        public void GetRelativePath_TargetPathWithDotInFolderName_RoundTrips()
        {
            var original = "Assets/my.package.v1/SomeFile.cs";
            var basePath = "Assets/Scripts/";
            var relative = PathUtils.GetRelativePath(basePath, original);
            Assert.IsTrue(PathUtils.IsRelativePath(relative),
                "ドットを含むフォルダへのパスは相対パスとして返されること");
            var restored = PathUtils.GetProjectAbsolutePath(basePath, relative);
            Assert.AreEqual(original, restored,
                "ドットを含むフォルダ名（例: my.package.v1）へのラウンドトリップが成立すること");
        }

        /// <summary>
        /// ベースパスにドットを含むフォルダ名が含まれる場合でもラウンドトリップが成立すること。
        /// </summary>
        [Test]
        public void GetRelativePath_BasePathWithDotInFolderName_RoundTrips()
        {
            var original = "Assets/Textures/Logo.png";
            var basePath = "Assets/my.tools.v1/";
            var relative = PathUtils.GetRelativePath(basePath, original);
            Assert.IsTrue(PathUtils.IsRelativePath(relative),
                "ドットを含むベースフォルダからの相対パスは '.' 始まりで返されること");
            var restored = PathUtils.GetProjectAbsolutePath(basePath, relative);
            Assert.AreEqual(original, restored,
                "ベースパスにドットを含むフォルダ名（例: my.tools.v1）がある場合もラウンドトリップが成立すること");
        }

        /// <summary>
        /// UPM パッケージスタイル（com.example.my-package）のようなフォルダ名でも
        /// ラウンドトリップが成立すること。
        /// </summary>
        [Test]
        public void GetRelativePath_UPMStyleFolderName_RoundTrips()
        {
            var original = "Assets/com.example.my-package/Runtime/Scripts/Foo.cs";
            var basePath = "Assets/Scripts/";
            var relative = PathUtils.GetRelativePath(basePath, original);
            Assert.IsTrue(PathUtils.IsRelativePath(relative),
                "UPM スタイルのフォルダ名へのパスは相対パスとして返されること");
            var restored = PathUtils.GetProjectAbsolutePath(basePath, relative);
            Assert.AreEqual(original, restored,
                "com.example.my-package 形式のフォルダ名でもラウンドトリップが成立すること");
        }

        /// <summary>
        /// ベースパスとターゲットパスの両方にドットを含むフォルダ名がある場合でも
        /// ラウンドトリップが成立すること。
        /// </summary>
        [Test]
        public void GetRelativePath_BothPathsContainDotFolders_RoundTrips()
        {
            var original = "Assets/my.package.v1/SubFolder/File.shader";
            var basePath = "Assets/my.tools.v1/";
            var relative = PathUtils.GetRelativePath(basePath, original);
            Assert.IsTrue(PathUtils.IsRelativePath(relative),
                "両方のパスにドットを含むフォルダがある場合も相対パスとして返されること");
            var restored = PathUtils.GetProjectAbsolutePath(basePath, relative);
            Assert.AreEqual(original, restored,
                "ベース・ターゲット双方にドットを含むフォルダがある場合もラウンドトリップが成立すること");
        }

        // ===== IsDynamicPath =====

        [Test]
        public void IsDynamicPath_PathWithPercent_ReturnsTrue()
        {
            Assert.IsTrue(PathUtils.IsDynamicPath("Assets/%name%/Foo.cs"));
        }

        [Test]
        public void IsDynamicPath_SinglePercent_ReturnsTrue()
        {
            Assert.IsTrue(PathUtils.IsDynamicPath("%name%"));
        }

        [Test]
        public void IsDynamicPath_PathWithoutPercent_ReturnsFalse()
        {
            Assert.IsFalse(PathUtils.IsDynamicPath("Assets/Scripts/Foo.cs"));
        }

        [Test]
        public void IsDynamicPath_Null_ReturnsFalse()
        {
            Assert.IsFalse(PathUtils.IsDynamicPath(null));
        }

        [Test]
        public void IsDynamicPath_Empty_ReturnsFalse()
        {
            Assert.IsFalse(PathUtils.IsDynamicPath(string.Empty));
        }
    }

    /// <summary>
    /// ExporterUtils.FindDuplicates のユニットテスト。
    /// </summary>
    public class FindDuplicatesTests
    {
        [Test]
        public void FindDuplicates_ExactDuplicate_Detected()
        {
            var items = new[] { "Foo.unitypackage", "Bar.unitypackage", "Foo.unitypackage" };
            var result = ExporterUtils.FindDuplicates(items);

            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("Foo.unitypackage", result[0]);
        }

        [Test]
        public void FindDuplicates_CaseOnlyDifference_DetectedAsDuplicate()
        {
            var items = new[] { "Foo.unitypackage", "foo.unitypackage", "Bar.unitypackage" };
            var result = ExporterUtils.FindDuplicates(items);

            Assert.AreEqual(1, result.Length,
                "大文字小文字のみ異なる要素が重複として検出されること");
        }

        [Test]
        public void FindDuplicates_NoDuplicates_ReturnsEmpty()
        {
            var items = new[] { "Foo.unitypackage", "Bar.unitypackage", "Baz.unitypackage" };
            var result = ExporterUtils.FindDuplicates(items);

            Assert.AreEqual(0, result.Length);
        }

        [Test]
        public void FindDuplicates_Empty_ReturnsEmpty()
        {
            var items = new string[0];
            var result = ExporterUtils.FindDuplicates(items);

            Assert.AreEqual(0, result.Length);
        }

        [Test]
        public void FindDuplicates_MultipleDuplicateGroups_AllDetected()
        {
            var items = new[] { "A.unitypackage", "B.unitypackage", "a.unitypackage", "b.unitypackage", "C.unitypackage" };
            var result = ExporterUtils.FindDuplicates(items);

            Assert.AreEqual(2, result.Length,
                "複数の重複グループがすべて検出されること");
        }
    }
}
