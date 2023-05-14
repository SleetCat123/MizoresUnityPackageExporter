using Const_Keys = MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterConsts_Keys;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter
{
    [System.Serializable]
    public class PackageNameSettings
    {
        public VersionSource versionSource;
        public PackagePrefsElement versionFile;
        public string versionString;
        public string versionFormat = $"-{Const_Keys.KEY_VERSION}";
        public string packageName = $"{Const_Keys.KEY_NAME}{Const_Keys.KEY_FORMATTED_VERSION}";
    }
}
