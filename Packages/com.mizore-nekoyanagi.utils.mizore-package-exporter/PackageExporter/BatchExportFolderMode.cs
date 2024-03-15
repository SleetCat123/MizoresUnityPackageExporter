
namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public enum BatchExportFolderMode {
        All, Files, Folders
    }
    public static class BatchExportFolderModeExtensions {
        public static string GetString( this BatchExportFolderMode mode ) {
            switch ( mode ) {
                case BatchExportFolderMode.All:
                    return "All";
                case BatchExportFolderMode.Files:
                    return "Files";
                case BatchExportFolderMode.Folders:
                    return "Folders";
                default:
                    throw new System.ArgumentOutOfRangeException( );
            }
        }
        public static BatchExportFolderMode Parse( string value ) {
            switch ( value ) {
                case "All":
                    return BatchExportFolderMode.All;
                case "Files":
                    return BatchExportFolderMode.Files;
                case "Folders":
                    return BatchExportFolderMode.Folders;
                default:
                    throw new System.ArgumentOutOfRangeException( );
            }
        }
    }
}
