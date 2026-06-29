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

        // ===== StartsWith スラッシュなし時の仕様 =====

        /// <summary>
        /// StartsWith にトレーリングスラッシュを付けない場合、
        /// プレフィックスが一致する兄弟フォルダにもマッチする。
        /// 例: "Assets/Scripts" は "Assets/ScriptsExtra/Foo.cs" にもマッチする。
        /// フォルダだけを除外したい場合はトレーリングスラッシュが必要。
        /// </summary>
        [Test]
        public void StartsWith_WithoutTrailingSlash_AlsoMatchesSiblingFolderWithLongerName()
        {
            var sp = new SearchPath(SearchPathType.StartsWith, false, "Assets/Scripts");
            Assert.IsTrue(sp.IsMatch("Assets/Scripts/Foo.cs"),
                "フォルダ配下のファイルにマッチすること");
            Assert.IsTrue(sp.IsMatch("Assets/Scripts"),
                "フォルダ自身にマッチすること");
            Assert.IsTrue(sp.IsMatch("Assets/ScriptsExtra/Foo.cs"),
                "スラッシュなしでは名前が長い兄弟フォルダにもマッチしてしまう");
            Assert.IsTrue(sp.IsMatch("Assets/ScriptsGenerated"),
                "スラッシュなしでは文字列として前方一致する兄弟フォルダ自身にもマッチする");
        }

        // ===== Regex 部分マッチ仕様 =====

        /// <summary>
        /// Regex は ^ や $ なしの場合、パスの一部にマッチすれば true を返す（部分マッチ）。
        /// フルマッチさせるには ^ と $ で囲む必要がある。
        /// </summary>
        [Test]
        public void Regex_WithoutAnchors_PartialMatchReturnsTrue()
        {
            var sp = new SearchPath(SearchPathType.Regex, false, @"foo");
            Assert.IsTrue(sp.IsMatch("Assets/foobar/test.cs"),
                "パターンが部分的に含まれていればマッチすること（部分マッチ）");
            Assert.IsTrue(sp.IsMatch("foo.cs"),
                "先頭にパターンがある場合もマッチすること");
            Assert.IsTrue(sp.IsMatch("test_foo"),
                "末尾にパターンがある場合もマッチすること");
        }

        /// <summary>
        /// Regex に ^ と $ を付けるとフルマッチになり、部分一致ではマッチしない。
        /// </summary>
        [Test]
        public void Regex_WithAnchors_OnlyFullMatchReturnsTrue()
        {
            var sp = new SearchPath(SearchPathType.Regex, false, @"^foo\.cs$");
            Assert.IsTrue(sp.IsMatch("foo.cs"),
                "完全一致はマッチすること");
            Assert.IsFalse(sp.IsMatch("Assets/foo.cs"),
                "^ があるためパスの途中に foo.cs があってもマッチしないこと");
            Assert.IsFalse(sp.IsMatch("foo.cs.meta"),
                "$ があるため末尾が異なればマッチしないこと");
        }

        // ===== GetMatchPaths + includeSubfiles: true（各タイプ） =====

        /// <summary>
        /// Partial でフォルダパスにマッチした場合、includeSubfiles: true でそのフォルダ配下も返ること。
        /// </summary>
        [Test]
        public void GetMatchPaths_Partial_IncludeSubfilesTrue_ReturnsChildPaths()
        {
            var sp = new SearchPath(SearchPathType.Partial, false, "Scripts");
            var paths = new[]
            {
                "Assets/Scripts",
                "Assets/Scripts/Foo.cs",
                "Assets/Scripts/Bar.cs",
                "Assets/Prefabs/Player.prefab",
            };

            var result = sp.GetMatchPaths(paths, includeSubfiles: true).ToList();

            CollectionAssert.Contains(result, "Assets/Scripts",
                "Partial でマッチしたパス自身が含まれること");
            CollectionAssert.Contains(result, "Assets/Scripts/Foo.cs",
                "includeSubfiles: true でマッチしたフォルダの子パスも含まれること");
            CollectionAssert.Contains(result, "Assets/Scripts/Bar.cs");
            CollectionAssert.DoesNotContain(result, "Assets/Prefabs/Player.prefab",
                "マッチしないパスは含まれないこと");
        }

        /// <summary>
        /// Partial の includeSubfiles: false では、フォルダ配下のパスでも
        /// パス文字列自体に検索値が含まれていれば直接マッチして返される。
        ///
        /// Partial("Scripts") の場合：
        ///   "Assets/Scripts"      → パスに "Scripts" が含まれる → 直接マッチ ✓
        ///   "Assets/Scripts/Foo.cs" → パスに "Scripts" が含まれる → 直接マッチ ✓
        ///
        /// つまり Partial では子パスも直接マッチするため、includeSubfiles に関係なく
        /// 子パスが返される。includeSubfiles が意味を持つのは Exact などフォルダ自身だけに
        /// マッチするタイプに限られる。
        /// </summary>
        [Test]
        public void GetMatchPaths_Partial_IncludeSubfilesFalse_ChildPathAlsoMatchesDirectly_BothReturned()
        {
            var sp = new SearchPath(SearchPathType.Partial, false, "Scripts");
            var paths = new[]
            {
                "Assets/Scripts",
                "Assets/Scripts/Foo.cs",
            };

            var result = sp.GetMatchPaths(paths, includeSubfiles: false).ToList();

            // 両方とも "Scripts" を含むため直接マッチする（subfile 経由ではない）
            Assert.AreEqual(2, result.Count,
                "Partial('Scripts') は 'Assets/Scripts' と 'Assets/Scripts/Foo.cs' の両方に直接マッチすること");
            CollectionAssert.Contains(result, "Assets/Scripts");
            CollectionAssert.Contains(result, "Assets/Scripts/Foo.cs",
                "Foo.cs のパスには 'Scripts' が含まれるため直接マッチし includeSubfiles: false でも返されること");
        }

        /// <summary>
        /// StartsWith でフォルダパスにマッチした場合、includeSubfiles: true でそのフォルダ配下も返ること。
        /// </summary>
        [Test]
        public void GetMatchPaths_StartsWith_IncludeSubfilesTrue_ReturnsChildPaths()
        {
            var sp = new SearchPath(SearchPathType.StartsWith, false, "Assets/Scripts");
            var paths = new[]
            {
                "Assets/Scripts",
                "Assets/Scripts/Foo.cs",
                "Assets/Scripts/Sub/Bar.cs",
                "Assets/Prefabs/Player.prefab",
            };

            var result = sp.GetMatchPaths(paths, includeSubfiles: true).ToList();

            CollectionAssert.Contains(result, "Assets/Scripts");
            CollectionAssert.Contains(result, "Assets/Scripts/Foo.cs",
                "includeSubfiles: true でフォルダ配下の子パスも含まれること");
            CollectionAssert.Contains(result, "Assets/Scripts/Sub/Bar.cs",
                "深い階層の子パスも含まれること");
            CollectionAssert.DoesNotContain(result, "Assets/Prefabs/Player.prefab");
        }

        /// <summary>
        /// EndsWith でファイルにマッチした場合、そのファイルの「子パス」は存在しえないため
        /// includeSubfiles: true でも追加のパスは返ってこないこと（マッチしたファイル自身のみ）。
        /// </summary>
        [Test]
        public void GetMatchPaths_EndsWith_IncludeSubfilesTrue_NoExtraChildPaths()
        {
            var sp = new SearchPath(SearchPathType.EndsWith, false, ".cs");
            var paths = new[]
            {
                "Assets/Scripts/Foo.cs",
                "Assets/Scripts/Bar.cs",
                "Assets/Scripts/Foo.cs/impossible_child",  // 実際には存在しないが仕様を確認するため
                "Assets/Prefabs/Player.prefab",
            };

            var result = sp.GetMatchPaths(paths, includeSubfiles: true).ToList();

            CollectionAssert.Contains(result, "Assets/Scripts/Foo.cs");
            CollectionAssert.Contains(result, "Assets/Scripts/Bar.cs");
            // includeSubfiles の subfile 検索は "Foo.cs/" で始まるパスを探す
            // "Assets/Scripts/Foo.cs/impossible_child" は "Assets/Scripts/Foo.cs/" で始まるのでマッチする
            CollectionAssert.Contains(result, "Assets/Scripts/Foo.cs/impossible_child",
                "EndsWith でマッチしたパスを疑似フォルダとして扱い、その子パスも含まれること（includeSubfiles の仕様）");
            CollectionAssert.DoesNotContain(result, "Assets/Prefabs/Player.prefab");
        }

        /// <summary>
        /// Regex でフォルダパスにマッチした場合、includeSubfiles: true でそのフォルダ配下も返ること。
        /// </summary>
        [Test]
        public void GetMatchPaths_Regex_IncludeSubfilesTrue_ReturnsChildPaths()
        {
            var sp = new SearchPath(SearchPathType.Regex, false, @"^Assets/Scripts$");
            var paths = new[]
            {
                "Assets/Scripts",
                "Assets/Scripts/Foo.cs",
                "Assets/Scripts/Sub/Bar.cs",
                "Assets/Prefabs/Player.prefab",
            };

            var result = sp.GetMatchPaths(paths, includeSubfiles: true).ToList();

            CollectionAssert.Contains(result, "Assets/Scripts",
                "Regex でマッチしたパス自身が含まれること");
            CollectionAssert.Contains(result, "Assets/Scripts/Foo.cs",
                "includeSubfiles: true でフォルダ配下の子パスも含まれること");
            CollectionAssert.Contains(result, "Assets/Scripts/Sub/Bar.cs",
                "深い階層の子パスも含まれること");
            CollectionAssert.DoesNotContain(result, "Assets/Prefabs/Player.prefab");
        }

        /// <summary>
        /// Regex でマッチしたパスを includeSubfiles: true で展開する場合、
        /// regex が複数パスにマッチするときすべての子パスが返ること。
        /// </summary>
        [Test]
        public void GetMatchPaths_Regex_IncludeSubfilesTrue_MultipleMatchedFolders_AllChildrenReturned()
        {
            var sp = new SearchPath(SearchPathType.Regex, false, @"^Assets/(Scripts|Shaders)$");
            var paths = new[]
            {
                "Assets/Scripts",
                "Assets/Scripts/Foo.cs",
                "Assets/Shaders",
                "Assets/Shaders/Unlit.shader",
                "Assets/Prefabs/Player.prefab",
            };

            var result = sp.GetMatchPaths(paths, includeSubfiles: true).ToList();

            CollectionAssert.Contains(result, "Assets/Scripts");
            CollectionAssert.Contains(result, "Assets/Scripts/Foo.cs");
            CollectionAssert.Contains(result, "Assets/Shaders");
            CollectionAssert.Contains(result, "Assets/Shaders/Unlit.shader");
            CollectionAssert.DoesNotContain(result, "Assets/Prefabs/Player.prefab");
        }

        // ===== ドットを含むフォルダ名への対応 =====

        /// <summary>
        /// StartsWith でトレーリングスラッシュを付けた場合、
        /// フォルダ配下の子パスにマッチし、名前がより長い兄弟フォルダにはマッチしないこと。
        /// excludeObjects でフォルダ除外時に "Assets/Folder/" + "/" を使う設計の検証。
        /// </summary>
        [Test]
        public void StartsWith_WithTrailingSlash_DoesNotMatchSiblingFolderHavingLongerName()
        {
            var sp = new SearchPath(SearchPathType.StartsWith, false, "Assets/Scripts/");
            Assert.IsTrue(sp.IsMatch("Assets/Scripts/Foo.cs"),
                "トレーリングスラッシュ付きはフォルダ配下のファイルにマッチすること");
            Assert.IsFalse(sp.IsMatch("Assets/Scripts"),
                "フォルダ自身（スラッシュなし）にはマッチしないこと");
            Assert.IsFalse(sp.IsMatch("Assets/ScriptsExtra/Foo.cs"),
                "プレフィックスが一致しても余分な文字が続く兄弟フォルダにはマッチしないこと");
            Assert.IsFalse(sp.IsMatch("Assets/ScriptsMore/SubFolder/Foo.cs"),
                "2階層以上深い兄弟フォルダにもマッチしないこと");
        }

        /// <summary>
        /// ドットを含むフォルダ名（例: "my.package.v1"）にトレーリングスラッシュを付けた場合、
        /// フォルダ配下の子パスにマッチし、名前がより長い兄弟フォルダにはマッチしないこと。
        /// </summary>
        [Test]
        public void StartsWith_DotFolderWithTrailingSlash_MatchesChildrenNotSiblings()
        {
            var sp = new SearchPath(SearchPathType.StartsWith, true, "Assets/my.package.v1/");
            Assert.IsTrue(sp.IsMatch("Assets/my.package.v1/File.cs"),
                "ドットフォルダ配下の直接の子ファイルにマッチすること");
            Assert.IsTrue(sp.IsMatch("Assets/my.package.v1/SubFolder/Deep.shader"),
                "ドットフォルダ配下の深いパスにもマッチすること");
            Assert.IsFalse(sp.IsMatch("Assets/my.package.v1"),
                "フォルダ自身（スラッシュなし）にはマッチしないこと");
            Assert.IsFalse(sp.IsMatch("Assets/my.package.v1Extra/File.cs"),
                "プレフィックスが一致するだけの兄弟ドットフォルダにはマッチしないこと");
            Assert.IsFalse(sp.IsMatch("Assets/my.package.v10/File.cs"),
                "数字が続く別バージョンの兄弟フォルダにもマッチしないこと");
        }

        /// <summary>
        /// EndsWith でドット付き拡張子を指定した場合、
        /// フォルダ自身のパスにはマッチするが、その配下の別拡張子ファイルにはマッチしないこと。
        /// （例: EndsWith ".v1" は "my.v1" にはマッチするが "my.v1/File.cs" にはマッチしない）
        /// </summary>
        [Test]
        public void EndsWith_DotExtension_MatchesFolderNameButNotItsChildren()
        {
            var sp = new SearchPath(SearchPathType.EndsWith, false, ".v1");
            Assert.IsTrue(sp.IsMatch("Assets/my.v1"),
                "EndsWith '.v1' はドット名フォルダ自身にマッチすること");
            Assert.IsFalse(sp.IsMatch("Assets/my.v1/File.cs"),
                "EndsWith '.v1' はドットフォルダ配下の別拡張子ファイルにはマッチしないこと");
            Assert.IsFalse(sp.IsMatch("Assets/my.v1/Sub/Deep.shader"),
                "EndsWith '.v1' はドットフォルダ配下の深いパスにもマッチしないこと");
        }

        /// <summary>
        /// GetMatchPaths で StartsWith（トレーリングスラッシュ付き）を使用した場合、
        /// ドットを含むフォルダ配下のファイルのみ除外し、
        /// 名前がより長い兄弟フォルダは除外しないこと。
        /// </summary>
        [Test]
        public void GetMatchPaths_DotFolderStartsWith_OnlyMatchesFolderContents()
        {
            var sp = new SearchPath(SearchPathType.StartsWith, true, "Assets/my.package.v1/");
            var paths = new[]
            {
                "Assets/my.package.v1/FileA.mat",
                "Assets/my.package.v1/FileB.mat",
                "Assets/my.package.v1/SubDir/FileC.mat",
                "Assets/my.package.v1Extended/FileD.mat",
                "Assets/other.package.v1/FileE.mat",
            };

            var matched = sp.GetMatchPaths(paths, includeSubfiles: false).ToList();

            CollectionAssert.Contains(matched, "Assets/my.package.v1/FileA.mat",
                "ドットフォルダ直下のファイルはマッチすること");
            CollectionAssert.Contains(matched, "Assets/my.package.v1/FileB.mat");
            CollectionAssert.Contains(matched, "Assets/my.package.v1/SubDir/FileC.mat",
                "ドットフォルダのサブディレクトリ配下のファイルもマッチすること");
            CollectionAssert.DoesNotContain(matched, "Assets/my.package.v1Extended/FileD.mat",
                "名前が長い兄弟フォルダのファイルはマッチしないこと");
            CollectionAssert.DoesNotContain(matched, "Assets/other.package.v1/FileE.mat",
                "別のドットフォルダのファイルはマッチしないこと");
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
