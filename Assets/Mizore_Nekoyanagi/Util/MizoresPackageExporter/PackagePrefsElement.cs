using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter
{
    [System.Serializable]
    public class PackagePrefsElement
    {
        [SerializeField]
        private Object obj;
        [SerializeField]
        private string path;

        public Object Object {
            get {
#if UNITY_EDITOR
                if ( obj == null && !string.IsNullOrEmpty( path ) ) {
                    obj = AssetDatabase.LoadAssetAtPath<Object>( path );
                }
#endif
                return obj;
            }
            set {
#if UNITY_EDITOR
                //if ( obj != value ) {
                if ( value != null ) {
                    path = AssetDatabase.GetAssetPath( value.GetInstanceID( ) );
                } else {
                    path = string.Empty;
                }
                //}
#endif
                obj = value;
            }
        }

        public string Path {
            get {
#if UNITY_EDITOR
                if ( obj != null ) {
                    path = AssetDatabase.GetAssetPath( obj.GetInstanceID( ) );
                }
#endif
                return path;
            }
        }
    }
}
