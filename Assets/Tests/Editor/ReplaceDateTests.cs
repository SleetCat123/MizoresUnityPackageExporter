using NUnit.Framework;
using MizoreNekoyanagi.PublishUtil.PackageExporter;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.Tests
{
    /// <summary>
    /// MizoresPackageExporter.ReplaceDate() のテスト。
    /// ReplaceDate は内部で DateTime.Now を使うため注入不可。
    /// 時刻依存テストは呼び出し前後で時刻をキャプチャし、どちらかに一致すれば OK とする方式を統一適用する。
    /// </summary>
    public class ReplaceDateTests
    {
        [Test]
        public void PlainText_ReturnedAsIs()
        {
            var result = MizoresPackageExporter.ReplaceDate("no-date-format");
            Assert.AreEqual("no-date-format", result);
        }

        [Test]
        public void EmptyString_ReturnsEmpty()
        {
            var result = MizoresPackageExporter.ReplaceDate(string.Empty);
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void PlainText_FormatErrorIsNull()
        {
            MizoresPackageExporter.ReplaceDate("plain-text", out var error);
            Assert.IsNull(error);
        }

        [Test]
        public void YYYYMMdd_Format_ReplacedWithCurrentDate()
        {
            var before = System.DateTime.Now;
            var result  = MizoresPackageExporter.ReplaceDate("%date:yyyyMMdd%");
            var after   = System.DateTime.Now;

            Assert.That(result,
                Is.EqualTo(before.ToString("yyyyMMdd")).Or.EqualTo(after.ToString("yyyyMMdd")),
                $"Expected {before:yyyyMMdd} or {after:yyyyMMdd}, got {result}");
        }

        [Test]
        public void YYYY_Format_ReplacedWithCurrentYear()
        {
            var before = System.DateTime.Now;
            var result  = MizoresPackageExporter.ReplaceDate("%date:yyyy%");
            var after   = System.DateTime.Now;

            Assert.That(result,
                Is.EqualTo(before.ToString("yyyy")).Or.EqualTo(after.ToString("yyyy")),
                $"Expected {before:yyyy} or {after:yyyy}, got {result}");
        }

        [Test]
        public void MultipleFormats_AllReplacedIndependently()
        {
            var before = System.DateTime.Now;
            var result  = MizoresPackageExporter.ReplaceDate("%date:yyyy%-%date:MM%");
            var after   = System.DateTime.Now;

            var expected1 = $"{before:yyyy}-{before:MM}";
            var expected2 = $"{after:yyyy}-{after:MM}";
            Assert.That(result, Is.EqualTo(expected1).Or.EqualTo(expected2),
                $"Expected {expected1} or {expected2}, got {result}");
        }

        [Test]
        public void SurroundingText_Preserved()
        {
            var before = System.DateTime.Now;
            var result  = MizoresPackageExporter.ReplaceDate("MyPackage_%date:yyyy%_release");
            var after   = System.DateTime.Now;

            Assert.That(result,
                Is.EqualTo($"MyPackage_{before:yyyy}_release").Or.EqualTo($"MyPackage_{after:yyyy}_release"),
                $"Surrounding text must be preserved around the date substitution");
        }

        [Test]
        public void UnsupportedSingleCharacterFormat_ReturnsOriginalTextAndSetsFormatError()
        {
            var result = MizoresPackageExporter.ReplaceDate("%date:Q%", out var error);
            Assert.IsNotNull(error, "formatError should be set for an invalid format specifier");
            Assert.AreEqual("%date:Q%", result, "Original text should be returned unchanged");
        }

        [Test]
        public void MixedValidAndInvalid_ValidPartConverted_InvalidPartUnchanged()
        {
            var before = System.DateTime.Now;
            var result  = MizoresPackageExporter.ReplaceDate("%date:yyyy%_%date:Q%", out var error);
            var after   = System.DateTime.Now;

            Assert.IsNotNull(error, "formatError should be set because one format is invalid");
            Assert.That(result,
                Is.EqualTo($"{before:yyyy}_%date:Q%").Or.EqualTo($"{after:yyyy}_%date:Q%"),
                "Valid format converted, invalid format left as-is, no extra text inserted");
        }
    }
}
