
namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public enum VersionSource {
        String, File
    }
    [System.Serializable]
    public class VersionSourceData : EnumData<VersionSource> {
        public static implicit operator VersionSourceData( VersionSource value ) {
            return new VersionSourceData { value = value };
        }
    }
}
