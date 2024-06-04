
namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public enum BatchExportMode {
        Single, Texts, Folders, ListFile
    }
    [System.Serializable]
    public class BatchExportModeData : EnumData<BatchExportMode> {
        public static implicit operator BatchExportModeData( BatchExportMode value ) {
            return new BatchExportModeData { value = value };
        }
    }
}
