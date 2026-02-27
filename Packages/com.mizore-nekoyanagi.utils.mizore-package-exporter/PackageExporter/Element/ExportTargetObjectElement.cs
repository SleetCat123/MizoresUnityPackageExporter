using UnityEngine;
#if UNITY_EDITOR
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    [System.Serializable]
    public class ExportTargetObjectElement : ObjectRefElement, System.ICloneable, System.IEquatable<ExportTargetObjectElement> {
        public bool searchReference = true;
        public ExportTargetObjectElement( ) { }
        public ExportTargetObjectElement( string path ) : base( path ) { }
        public ExportTargetObjectElement( ExportTargetObjectElement source ) : base( source ) {
            this.searchReference = source.searchReference;
        }
        public override object Clone( ) {
            return new ExportTargetObjectElement( this );
        }
        public bool Equals( ExportTargetObjectElement other ) {
            return base.Equals( other ) && this.searchReference == other.searchReference;
        }
        public override int GetHashCode( ) {
            return base.GetHashCode( ) ^ searchReference.GetHashCode( );
        }
    }
}
