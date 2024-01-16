namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    [System.Serializable]
    public class ReferenceElement {
        public PackagePrefsElement element;
        public ReferenceMode mode;
        public ReferenceElement( ) {
            element = new PackagePrefsElement( );
            mode = ReferenceMode.Include;
        }
        public ReferenceElement( PackagePrefsElement element, ReferenceMode mode ) {
            this.element = element;
            this.mode = mode;
        }
        public ReferenceElement( ReferenceElement other ) {
            element = new PackagePrefsElement( other.element );
            mode = other.mode;
        }
    }
}
