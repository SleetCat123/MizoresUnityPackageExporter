using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    [System.Serializable]
    public class ReferenceElement {
        public ObjectRefElement element;
        public ReferenceModeData mode;
        public ReferenceElement( ) {
            element = new ObjectRefElement( );
            mode = ReferenceMode.Include;
        }
        public ReferenceElement( ObjectRefElement element, ReferenceMode mode ) {
            this.element = element;
            this.mode = mode;
        }
        public ReferenceElement( ReferenceElement other ) {
            element = new ObjectRefElement( other.element );
            mode = other.mode.value;
        }
    }
}
