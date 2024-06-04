namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public enum ReferenceMode {
        Include,
        Exclude,
    }
    [System.Serializable]
    public class ReferenceModeData : EnumData<ReferenceMode> {
        public static implicit operator ReferenceModeData( ReferenceMode value ) {
            return new ReferenceModeData { value = value };
        }
    }
}
