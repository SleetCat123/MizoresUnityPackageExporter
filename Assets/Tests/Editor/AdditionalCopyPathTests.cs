using NUnit.Framework;
using UnityEngine;
using MizoreNekoyanagi.PublishUtil.PackageExporter;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.Tests
{
    /// <summary>
    /// AdditionalCopyPath.GetConvertedDestName / Clone のテスト
    /// </summary>
    public class AdditionalCopyPathTests
    {
        // ===== GetConvertedDestName =====

        [Test]
        public void GetConvertedDestName_EmptyDestName_ReturnsNull()
        {
            var exporter = ScriptableObject.CreateInstance<MizoresPackageExporter>();
            try
            {
                var copyPath = new AdditionalCopyPath("Assets/Foo.cs", destName: "");
                var result = copyPath.GetConvertedDestName(exporter, string.Empty);
                Assert.IsNull(result);
            }
            finally { Object.DestroyImmediate(exporter); }
        }

        [Test]
        public void GetConvertedDestName_WhitespaceDestName_ReturnsNull()
        {
            var exporter = ScriptableObject.CreateInstance<MizoresPackageExporter>();
            try
            {
                var copyPath = new AdditionalCopyPath("Assets/Foo.cs", destName: "   ");
                var result = copyPath.GetConvertedDestName(exporter, string.Empty);
                Assert.IsNull(result);
            }
            finally { Object.DestroyImmediate(exporter); }
        }

        [Test]
        public void GetConvertedDestName_PlainDestName_ReturnedAsIs()
        {
            var exporter = ScriptableObject.CreateInstance<MizoresPackageExporter>();
            try
            {
                var copyPath = new AdditionalCopyPath("Assets/Foo.cs", destName: "README.txt");
                var result = copyPath.GetConvertedDestName(exporter, string.Empty);
                Assert.AreEqual("README.txt", result);
            }
            finally { Object.DestroyImmediate(exporter); }
        }

        [Test]
        public void GetConvertedDestName_NameVariable_ReplacedWithExporterName()
        {
            var exporter = ScriptableObject.CreateInstance<MizoresPackageExporter>();
            try
            {
                exporter.name = "MyExporter";
                var copyPath = new AdditionalCopyPath("Assets/Foo.cs", destName: "%name%_output.txt");
                var result = copyPath.GetConvertedDestName(exporter, string.Empty);
                Assert.AreEqual("MyExporter_output.txt", result);
            }
            finally { Object.DestroyImmediate(exporter); }
        }

        // ===== Clone =====

        [Test]
        public void Clone_CreatesEqualButDistinctInstance()
        {
            var original = new AdditionalCopyPath("Assets/Foo.cs", destName: "output.txt");
            var clone = (AdditionalCopyPath)original.Clone();

            Assert.AreEqual(original.destName, clone.destName);
            Assert.AreNotSame(original, clone);
        }

        [Test]
        public void Clone_SourcePathIsIndependentCopy()
        {
            var original = new AdditionalCopyPath("Assets/Foo.cs", destName: "out.txt");
            var clone = (AdditionalCopyPath)original.Clone();

            clone.sourcePath.SetPath("Assets/Bar.cs");
            Assert.AreEqual("Assets/Foo.cs", original.sourcePath.Path,
                "Modifying clone's sourcePath should not affect original");
        }
    }
}
