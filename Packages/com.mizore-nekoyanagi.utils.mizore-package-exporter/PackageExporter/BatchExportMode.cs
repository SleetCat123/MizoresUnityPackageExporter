
namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public enum BatchExportMode {
        Single, Texts, Folders, ListFile
    }
    public static class BatchExportModeExtensions {
        public static string GetString( this BatchExportMode mode ) {
            switch ( mode ) {
                case BatchExportMode.Single:
                    return "Single";
                case BatchExportMode.Texts:
                    return "Texts";
                case BatchExportMode.Folders:
                    return "Folders";
                case BatchExportMode.ListFile:
                    return "ListFile";
                default:
                    throw new System.ArgumentOutOfRangeException( );
            }
        }
        public static BatchExportMode Parse( string value ) {
            switch ( value ) {
                case "Single":
                    return BatchExportMode.Single;
                case "Texts":
                    return BatchExportMode.Texts;
                case "Folders":
                    return BatchExportMode.Folders;
                case "ListFile":
                    return BatchExportMode.ListFile;
                default:
                    throw new System.ArgumentOutOfRangeException( );
            }
        }
    }
}
