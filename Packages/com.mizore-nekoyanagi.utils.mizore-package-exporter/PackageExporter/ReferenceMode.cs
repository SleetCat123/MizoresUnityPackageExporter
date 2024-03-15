namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public enum ReferenceMode {
        Include,
        Exclude,
    }
    public static class ReferenceModeExtensions {
        public static string GetString( this ReferenceMode mode ) {
            switch ( mode ) {
                case ReferenceMode.Include:
                    return "Include";
                case ReferenceMode.Exclude:
                    return "Exclude";
                default:
                    throw new System.ArgumentOutOfRangeException( );
            }
        }
        public static ReferenceMode Parse( string value ) {
            switch ( value ) {
                case "Include":
                    return ReferenceMode.Include;
                case "Exclude":
                    return ReferenceMode.Exclude;
                default:
                    throw new System.ArgumentOutOfRangeException( );
            }
        }
    }
}
