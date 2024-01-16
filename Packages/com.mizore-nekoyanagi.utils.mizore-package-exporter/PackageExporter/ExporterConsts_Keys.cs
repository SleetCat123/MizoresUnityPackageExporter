using System.Text.RegularExpressions;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter
{
    public static class ExporterConsts_Keys
    {
        public const string KEY_NAME= "%name%";
        public static readonly Regex REGEX_RELATIVE_NAME = new Regex( @"%(\.+)name%" );
        public const string KEY_VERSION = "%version%";
        public const string KEY_FORMATTED_VERSION = "%versionf%";
        public const string KEY_PACKAGE_NAME= "%packagename%";
        public const string KEY_BATCH_EXPORTER = "%batch%";
        public const string KEY_FORMATTED_BATCH_EXPORTER = "%batchf%";

        public static readonly Regex REGEX_DATE_FORMAT = new Regex( "%date:([^%]+)%" );

        public const string KEY_SAMPLE_RELATIVE_NAME = "%.name%";
        public const string KEY_SAMPLE_DATE = "%date:yyyyMMdd%";

    }
}
