using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    [System.Serializable]
    public class ReferenceElement : ISerializationCallbackReceiver {
        public ObjectRefElement element;
        // ReferenceMode実装と同時にstring保存を実装したためNonSerializedで問題ない
        [System.NonSerialized]
        public ReferenceMode mode;
        [SerializeField]string s_mode;
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
            mode = other.mode;
        }

        public void OnBeforeSerialize( ) {
            s_mode = mode.GetString( );
        }

        public void OnAfterDeserialize( ) {
            if ( string.IsNullOrEmpty( s_mode ) ) {
            } else {
                mode = ReferenceModeExtensions.Parse( s_mode );
            }
        }
    }
}
