
namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public enum BatchExportFolderMode {
        All, Files, Folders
    }
    [System.Serializable]
    public class BatchExportFolderModeData : EnumData<BatchExportFolderMode> {
        public static implicit operator BatchExportFolderModeData( BatchExportFolderMode value ) {
            return new BatchExportFolderModeData { value = value };
        }
    }
}
