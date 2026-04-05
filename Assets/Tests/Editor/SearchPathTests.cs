using System.Linq;
using NUnit.Framework;
using MizoreNekoyanagi.PublishUtil.PackageExporter;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.Tests
{
    /// <summary>
    /// SearchPath.IsMatch / GetMatchPaths / Equals / Clone / GetHashCode のテスト
    /// </summary>
    public class SearchPathTests
    {
        // ===== IsMatch =====

        [Test]
        public void Disabled_AlwaysReturnsFalse()
        {
            var sp = new SearchPath(SearchPathType.Disabled, false, "Assets/Scripts");
            Assert.IsFalse(sp.IsMatch("Assets/Scripts"));
            Assert.IsFalse(sp.IsMatch("Assets/Scripts/Foo.cs"));
            Assert.IsFalse(sp.IsMatch("anything"));
            Assert.IsFalse(sp.IsMatch(""));
        }

        [Test]
        public void Exact_MatchesOnlyExactString()
        {
            var sp = new SearchPath(SearchPathType.Exact, false, "Assets/Scripts/Foo.cs");
            Assert.IsTrue(sp.IsMatch("Assets/Scripts/Foo.cs"));
            Assert.IsFalse(sp.IsMatch("Assets/Scripts"));
            Assert.IsFalse(sp.IsMatch("Assets/Scripts/Foo.cs/extra"));
            Assert.IsFalse(sp.IsMatch("prefix/Assets/Scripts/Foo.cs"));
        }

        [Test]
        public void Exact_IsCaseInsensitiveByDefault()
        {
            var sp = new SearchPath(SearchPathType.Exact, false, "Assets/Scripts/Foo.cs");
            Assert.IsTrue(sp.IsMatch("ASSETS/SCRIPTS/FOO.CS"));
            Assert.IsTrue(sp.IsMatch("assets/scripts/foo.cs"));
        }

        [Test]
        public void Exact_PreserveCase_ReturnsFalseOnCaseMismatch()
        {
            var sp = new SearchPath(SearchPathType.Exact, true, "Assets/Scripts/Foo.cs");
            Assert.IsTrue(sp.IsMatch("Assets/Scripts/Foo.cs"));
            Assert.IsFalse(sp.IsMatch("assets/scripts/foo.cs"));
            Assert.IsFalse(sp.IsMatch("ASSETS/SCRIPTS/FOO.CS"));
        }

        [Test]
        public void Partial_MatchesWhenValueIsContained()
        {
            var sp = new SearchPath(SearchPathType.Partial, false, "Scripts");
            Assert.IsTrue(sp.IsMatch("Assets/Scripts/Foo.cs"));
            Assert.IsTrue(sp.IsMatch("Assets/MyScripts"));
            Assert.IsTrue(sp.IsMatch("Scripts"));
            Assert.IsFalse(sp.IsMatch("Assets/Prefabs/Player.prefab"));
        }

        [Test]
        public void Partial_IsCaseInsensitiveByDefault()
        {
            var sp = new SearchPath(SearchPathType.Partial, false, "scripts");
            Assert.IsTrue(sp.IsMatch("Assets/Scripts/Foo.cs"));
        }

        [Test]
        public void Partial_PreserveCase_MatchesWhenCaseMatches()
        {
            var sp = new SearchPath(SearchPathType.Partial, true, "Scripts");
            Assert.IsTrue(sp.IsMatch("Assets/Scripts/Foo.cs"));
        }

        [Test]
        public void Partial_PreserveCase_ReturnsFalseOnCaseMismatch()
        {
            var sp = new SearchPath(SearchPathType.Partial, true, "Scripts");
            Assert.IsFalse(sp.IsMatch("Assets/SCRIPTS/Foo.cs"));
            Assert.IsFalse(sp.IsMatch("Assets/scripts/Foo.cs"));
        }

        [Test]
        public void StartsWith_MatchesOnlyWhenPathStartsWithValue()
        {
            var sp = new SearchPath(SearchPathType.StartsWith, false, "Assets/Scripts");
            Assert.IsTrue(sp.IsMatch("Assets/Scripts/Foo.cs"));
            Assert.IsTrue(sp.IsMatch("Assets/Scripts"));
            Assert.IsFalse(sp.IsMatch("Other/Assets/Scripts/Foo.cs"));
            Assert.IsFalse(sp.IsMatch("Assets/Prefabs"));
        }

        [Test]
        public void StartsWith_IsCaseInsensitiveByDefault()
        {
            var sp = new SearchPath(SearchPathType.StartsWith, false, "Assets/Scripts");
            Assert.IsTrue(sp.IsMatch("ASSETS/SCRIPTS/Foo.cs"));
            Assert.IsTrue(sp.IsMatch("assets/scripts/Foo.cs"));
        }

        [Test]
        public void StartsWith_PreserveCase_MatchesWhenCaseMatches()
        {
            var sp = new SearchPath(SearchPathType.StartsWith, true, "Assets/Scripts");
            Assert.IsTrue(sp.IsMatch("Assets/Scripts/Foo.cs"));
        }

        [Test]
        public void StartsWith_PreserveCase_ReturnsFalseOnCaseMismatch()
        {
            var sp = new SearchPath(SearchPathType.StartsWith, true, "Assets/Scripts");
            Assert.IsFalse(sp.IsMatch("assets/scripts/Foo.cs"));
            Assert.IsFalse(sp.IsMatch("ASSETS/SCRIPTS/Foo.cs"));
        }

        [Test]
        public void EndsWith_MatchesOnlyWhenPathEndsWithValue()
        {
            var sp = new SearchPath(SearchPathType.EndsWith, false, ".cs");
            Assert.IsTrue(sp.IsMatch("Assets/Scripts/Foo.cs"));
            Assert.IsTrue(sp.IsMatch("Foo.cs"));
            Assert.IsFalse(sp.IsMatch("Foo.cs.meta"));
            Assert.IsFalse(sp.IsMatch("Foo.prefab"));
        }

        [Test]
        public void EndsWith_IsCaseInsensitiveByDefault()
        {
            var sp = new SearchPath(SearchPathType.EndsWith, false, ".CS");
            Assert.IsTrue(sp.IsMatch("Assets/Scripts/Foo.cs"));
            Assert.IsTrue(sp.IsMatch("Foo.cs"));
        }

        [Test]
        public void EndsWith_PreserveCase_MatchesWhenCaseMatches()
        {
            var sp = new SearchPath(SearchPathType.EndsWith, true, ".cs");
            Assert.IsTrue(sp.IsMatch("Assets/Scripts/Foo.cs"));
        }

        [Test]
        public void EndsWith_PreserveCase_ReturnsFalseOnCaseMismatch()
        {
            var sp = new SearchPath(SearchPathType.EndsWith, true, ".cs");
            Assert.IsFalse(sp.IsMatch("Foo.CS"));
            Assert.IsFalse(sp.IsMatch("Foo.Cs"));
        }

        [Test]
        public void Regex_MatchesWhenRegexMatches()
        {
            var sp = new SearchPath(SearchPathType.Regex, false, @"\.cs$");
            Assert.IsTrue(sp.IsMatch("Assets/Scripts/Foo.cs"));
            Assert.IsTrue(sp.IsMatch("Bar.cs"));
            Assert.IsFalse(sp.IsMatch("Foo.cs.meta"));
            Assert.IsFalse(sp.IsMatch("Foo.prefab"));
        }

        [Test]
        public void Regex_IsCaseInsensitiveByDefault()
        {
            var sp = new SearchPath(SearchPathType.Regex, false, @"foo\.cs$");
            Assert.IsTrue(sp.IsMatch("Assets/Scripts/Foo.cs"));
            Assert.IsTrue(sp.IsMatch("FOO.CS"));
        }

        [Test]
        public void Regex_PreserveCase_RespectsLetterCase()
        {
            var sp = new SearchPath(SearchPathType.Regex, true, @"Foo\.cs$");
            Assert.IsTrue(sp.IsMatch("Assets/Scripts/Foo.cs"));
            Assert.IsFalse(sp.IsMatch("Assets/Scripts/foo.cs"));
        }

        [Test]
        public void Regex_InvalidPattern_ReturnsFalseAndSetsRegexError()
        {
            var sp = new SearchPath(SearchPathType.Regex, false, "[invalid regex");
            Assert.IsFalse(sp.IsMatch("anything"));
            Assert.IsNotNull(sp.RegexError, "RegexError should be set for an invalid pattern");
        }

        [Test]
        public void Regex_ValidPattern_RegexErrorIsNull()
        {
            var sp = new SearchPath(SearchPathType.Regex, false, @"\.cs$");
            sp.IsMatch("test.cs");
            Assert.IsNull(sp.RegexError);
        }

        // ===== GetMatchPaths =====

        [Test]
        public void GetMatchPaths_ReturnsAllMatchingPaths()
        {
            var sp = new SearchPath(SearchPathType.EndsWith, false, ".cs");
            var paths = new[]
            {
                "Assets/Scripts/Foo.cs",
                "Assets/Scripts/Bar.cs",
                "Assets/Prefabs/Player.prefab",
            };
            var result = sp.GetMatchPaths(paths, includeSubfiles: false).ToList();
            Assert.AreEqual(2, result.Count);
            CollectionAssert.Contains(result, "Assets/Scripts/Foo.cs");
            CollectionAssert.Contains(result, "Assets/Scripts/Bar.cs");
        }

        [Test]
        public void GetMatchPaths_NonMatchingPathsAreExcluded()
        {
            var sp = new SearchPath(SearchPathType.EndsWith, false, ".cs");
            var paths = new[]
            {
                "Assets/Prefabs/Player.prefab",
                "Assets/Textures/Logo.png",
            };
            var result = sp.GetMatchPaths(paths, includeSubfiles: false).ToList();
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetMatchPaths_IncludeSubfilesTrue_AlsoReturnsChildPaths()
        {
            var sp = new SearchPath(SearchPathType.Exact, false, "Assets/Scripts");
            var paths = new[]
            {
                "Assets/Scripts",
                "Assets/Scripts/Foo.cs",
                "Assets/Scripts/Bar.cs",
                "Assets/Prefabs",
                "Assets/Prefabs/Player.prefab",
            };
            var result = sp.GetMatchPaths(paths, includeSubfiles: true).ToList();
            Assert.AreEqual(3, result.Count);
            CollectionAssert.Contains(result, "Assets/Scripts");
            CollectionAssert.Contains(result, "Assets/Scripts/Foo.cs");
            CollectionAssert.Contains(result, "Assets/Scripts/Bar.cs");
            CollectionAssert.DoesNotContain(result, "Assets/Prefabs");
            CollectionAssert.DoesNotContain(result, "Assets/Prefabs/Player.prefab");
        }

        [Test]
        public void GetMatchPaths_IncludeSubfilesFalse_OnlyDirectMatchesReturned()
        {
            var sp = new SearchPath(SearchPathType.Exact, false, "Assets/Scripts");
            var paths = new[]
            {
                "Assets/Scripts",
                "Assets/Scripts/Foo.cs",
            };
            var result = sp.GetMatchPaths(paths, includeSubfiles: false).ToList();
            Assert.AreEqual(1, result.Count);
            CollectionAssert.Contains(result, "Assets/Scripts");
        }

        [Test]
        public void GetMatchPaths_Disabled_ReturnsEmpty()
        {
            var sp = new SearchPath(SearchPathType.Disabled, false, "Assets/Scripts");
            var paths = new[] { "Assets/Scripts/Foo.cs", "Assets/Scripts" };
            var result = sp.GetMatchPaths(paths, includeSubfiles: false).ToList();
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetMatchPaths_EmptyValue_ReturnsEmpty()
        {
            var sp = new SearchPath(SearchPathType.Partial, false, "");
            var paths = new[] { "Assets/Scripts/Foo.cs" };
            var result = sp.GetMatchPaths(paths, includeSubfiles: false).ToList();
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetMatchPaths_EmptyPathList_ReturnsEmpty()
        {
            var sp = new SearchPath(SearchPathType.Partial, false, "Scripts");
            var result = sp.GetMatchPaths(new string[0], includeSubfiles: false).ToList();
            Assert.AreEqual(0, result.Count);
        }

        // ===== Equals / GetHashCode / Clone =====

        [Test]
        public void Equals_SameValues_ReturnsTrue()
        {
            var sp1 = new SearchPath(SearchPathType.Partial, false, "Scripts");
            var sp2 = new SearchPath(SearchPathType.Partial, false, "Scripts");
            Assert.IsTrue(sp1.Equals(sp2));
            Assert.IsTrue(sp1 == sp2);
        }

        [Test]
        public void Equals_DifferentSearchType_ReturnsFalse()
        {
            var sp1 = new SearchPath(SearchPathType.Partial, false, "Scripts");
            var sp2 = new SearchPath(SearchPathType.Exact, false, "Scripts");
            Assert.IsFalse(sp1.Equals(sp2));
            Assert.IsTrue(sp1 != sp2);
        }

        [Test]
        public void Equals_DifferentValue_ReturnsFalse()
        {
            var sp1 = new SearchPath(SearchPathType.Partial, false, "Scripts");
            var sp2 = new SearchPath(SearchPathType.Partial, false, "Other");
            Assert.IsFalse(sp1.Equals(sp2));
        }

        [Test]
        public void Equals_DifferentPreserveCase_ReturnsFalse()
        {
            var sp1 = new SearchPath(SearchPathType.Partial, false, "Scripts");
            var sp2 = new SearchPath(SearchPathType.Partial, true, "Scripts");
            Assert.IsFalse(sp1.Equals(sp2));
        }

        [Test]
        public void Equals_Null_ReturnsFalse()
        {
            var sp = new SearchPath(SearchPathType.Partial, false, "Scripts");
            Assert.IsFalse(sp.Equals(null));
            Assert.IsFalse(sp == null);
            Assert.IsTrue(sp != null);
        }

        [Test]
        public void GetHashCode_EqualObjects_ReturnSameHash()
        {
            var sp1 = new SearchPath(SearchPathType.Partial, false, "Scripts");
            var sp2 = new SearchPath(SearchPathType.Partial, false, "Scripts");
            Assert.AreEqual(sp1.GetHashCode(), sp2.GetHashCode());
        }

        [Test]
        public void Clone_CreatesIndependentCopy()
        {
            var original = new SearchPath(SearchPathType.Partial, true, "Scripts");
            var clone = (SearchPath)original.Clone();

            Assert.IsTrue(original.Equals(clone), "Clone should be equal to original");
            Assert.AreNotSame(original, clone, "Clone should be a different instance");

            // Modifying clone must not affect original
            clone.Value = "Other";
            Assert.AreEqual("Scripts", original.Value, "Changing clone should not affect original");
        }

        [Test]
        public void Clone_CopiesAllFields()
        {
            var original = new SearchPath(SearchPathType.EndsWith, true, ".cs");
            var clone = (SearchPath)original.Clone();
            Assert.AreEqual(original.searchType.value, clone.searchType.value);
            Assert.AreEqual(original.PreserveCase, clone.PreserveCase);
            Assert.AreEqual(original.Value, clone.Value);
        }
    }
}
