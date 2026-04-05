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
}
