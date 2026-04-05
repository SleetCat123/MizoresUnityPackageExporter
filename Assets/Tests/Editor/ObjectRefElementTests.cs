using NUnit.Framework;
using MizoreNekoyanagi.PublishUtil.PackageExporter;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.Tests
{
    /// <summary>
    /// ObjectRefElement.SetPath / SetGUID / Equals / Clone / GetHashCode のテスト
    /// </summary>
    public class ObjectRefElementTests
    {
        // ===== SetPath(string) =====

        [Test]
        public void SetPath_NormalPath_StoredAsIs()
        {
            var element = new ObjectRefElement();
            element.SetPath("Assets/Scripts/Foo.cs");
            Assert.AreEqual("Assets/Scripts/Foo.cs", element.Path);
            Assert.IsFalse(element.IsGUID);
        }

        [Test]
        public void SetPath_PercentTwentyEncoding_ReplacedWithSpace()
        {
            var element = new ObjectRefElement();
            element.SetPath("Assets/My%20Package/Foo.cs");
            Assert.AreEqual("Assets/My Package/Foo.cs", element.Path);
        }

        [Test]
        public void SetPath_MultiplePercentTwenty_AllReplacedWithSpace()
        {
            var element = new ObjectRefElement();
            element.SetPath("Assets/My%20Cool%20Package/Foo.cs");
            Assert.AreEqual("Assets/My Cool Package/Foo.cs", element.Path);
        }

        [Test]
        public void SetPath_SetsIsGUIDToFalse()
        {
            var element = new ObjectRefElement();
            element.SetGUID("someguid");  // まず GUID に設定
            element.SetPath("Assets/Foo.cs");  // パスに切り替え
            Assert.IsFalse(element.IsGUID);
        }

        // ===== SetGUID(string) =====

        [Test]
        public void SetGUID_String_SetsPathAndIsGUID()
        {
            var element = new ObjectRefElement();
            element.SetGUID("abc123def456");
            Assert.IsTrue(element.IsGUID);
            Assert.AreEqual("abc123def456", element.Path);
        }

        // ===== Constructor(string) =====

        [Test]
        public void Constructor_WithStringPath_DoesNotDecodePercentTwenty()
        {
            // SetPath は %20 → space 変換を行うが、コンストラクタは変換しない
            var element = new ObjectRefElement("Assets/My%20Package/Foo.cs");
            Assert.AreEqual("Assets/My%20Package/Foo.cs", element.Path,
                "Constructor should store path as-is; %20 decoding is SetPath's responsibility");
            Assert.IsFalse(element.IsGUID);
        }

        // ===== Equals =====

        [Test]
        public void Equals_SamePath_ReturnsTrue()
        {
            var e1 = new ObjectRefElement("Assets/Scripts/Foo.cs");
            var e2 = new ObjectRefElement("Assets/Scripts/Foo.cs");
            Assert.IsTrue(e1.Equals(e2));
        }

        [Test]
        public void Equals_DifferentPath_ReturnsFalse()
        {
            var e1 = new ObjectRefElement("Assets/Scripts/Foo.cs");
            var e2 = new ObjectRefElement("Assets/Scripts/Bar.cs");
            Assert.IsFalse(e1.Equals(e2));
        }

        [Test]
        public void Equals_DifferentIsGUID_ReturnsFalse()
        {
            var e1 = new ObjectRefElement("somevalue");
            var e2 = new ObjectRefElement();
            e2.SetGUID("somevalue");
            Assert.IsFalse(e1.Equals(e2));
        }

        [Test]
        public void Equals_Null_ReturnsFalse()
        {
            var element = new ObjectRefElement("Assets/Scripts/Foo.cs");
            Assert.IsFalse(element.Equals(null));
        }

        [Test]
        public void Equals_SameGUID_ReturnsTrue()
        {
            var e1 = new ObjectRefElement();
            var e2 = new ObjectRefElement();
            e1.SetGUID("abc123");
            e2.SetGUID("abc123");
            Assert.IsTrue(e1.Equals(e2));
        }

        // ===== GetHashCode =====

        [Test]
        public void GetHashCode_EqualObjects_ReturnSameHash()
        {
            var e1 = new ObjectRefElement("Assets/Scripts/Foo.cs");
            var e2 = new ObjectRefElement("Assets/Scripts/Foo.cs");
            Assert.AreEqual(e1.GetHashCode(), e2.GetHashCode());
        }

        [Test]
        public void GetHashCode_NullPath_DoesNotThrow()
        {
            var element = new ObjectRefElement();
            Assert.DoesNotThrow(() => element.GetHashCode());
        }

        [Test]
        public void GetHashCode_TwoNullPathInstances_ReturnSameHash()
        {
            // null パスの ObjectRefElement が2つあるとき、
            // Equals が true を返すなら GetHashCode も一致しなければならない（契約）
            var e1 = new ObjectRefElement(); // path == null
            var e2 = new ObjectRefElement(); // path == null
            Assert.IsTrue(e1.Equals(e2), "前提: null パス同士は等値であること");
            Assert.AreEqual(e1.GetHashCode(), e2.GetHashCode(),
                "等値オブジェクトは同じハッシュコードを返すこと");
        }

        // ===== Clone =====

        [Test]
        public void Clone_CreatesEqualButDistinctInstance()
        {
            var original = new ObjectRefElement("Assets/Scripts/Foo.cs");
            var clone = (ObjectRefElement)original.Clone();

            Assert.IsTrue(original.Equals(clone), "Clone should equal original");
            Assert.AreNotSame(original, clone, "Clone should be a different instance");
        }

        [Test]
        public void Clone_CopiesIsGUIDField()
        {
            var original = new ObjectRefElement();
            original.SetGUID("abc123");
            var clone = (ObjectRefElement)original.Clone();

            Assert.IsTrue(clone.IsGUID);
            Assert.AreEqual("abc123", clone.Path);
        }

        [Test]
        public void Clone_ModifyingCloneDoesNotAffectOriginal()
        {
            var original = new ObjectRefElement("Assets/Scripts/Foo.cs");
            var clone = (ObjectRefElement)original.Clone();

            clone.SetPath("Assets/Scripts/Bar.cs");
            Assert.AreEqual("Assets/Scripts/Foo.cs", original.Path,
                "Modifying clone should not change original");
        }
    }
}
