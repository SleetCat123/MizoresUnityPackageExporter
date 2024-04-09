using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    [System.Serializable]
    public class ObjectRefElement : System.ICloneable, System.IEquatable<ObjectRefElement> {
        [System.NonSerialized]
        public MizoresPackageExporter exporter;

        [SerializeField]
        protected Object obj;
        [SerializeField]
        protected string path;

        public ObjectRefElement( ) { }
        public ObjectRefElement( MizoresPackageExporter exporter, Object obj, bool relativePath ) {
            this.exporter = exporter;
            SetObject( obj, relativePath );
        }
        public ObjectRefElement( MizoresPackageExporter exporter, Object obj ) {
            this.exporter = exporter;
            SetObject( obj );
        }
        public ObjectRefElement( string path ) {
            this.Path = path;
        }
        public ObjectRefElement( ObjectRefElement source ) {
            this.obj = source.obj;
            this.path = source.path;
        }

        public Object Object {
            get {
#if UNITY_EDITOR
                if ( obj != null ) {
                    return obj;
                }
                if ( string.IsNullOrEmpty( path ) ) {
                    return null;
                } else {
                    return AssetDatabase.LoadAssetAtPath<Object>( ConvertedPath );
                }
#else
                return obj;
#endif
            }
            set {
                SetObject( value );
            }
        }
        public void SetObject( Object value ) {
#if UNITY_EDITOR
            SetObject( value, ExporterEditorPrefs.UseRelativePath );
#endif
        }
        public void SetObject( Object value, bool relativePath ) {
#if UNITY_EDITOR
            if ( value != null ) {
                path = AssetDatabase.GetAssetPath( value.GetInstanceID( ) );
                if ( relativePath ) {
                    path = PathUtils.GetRelativePath( exporter.GetDirectoryPath( ), path );
                }
            } else {
                ExporterUtils.DebugLog( "Set Path to empty" );
                path = string.Empty;
            }
            obj = value;
#else
    throw new System.NotSupportedException( "This method is only supported in the editor." );
#endif
        }

        public string ConvertedPath {
            get {
                var result = Path;
                if ( PathUtils.IsDynamicPath( path ) ) {
                    result = exporter.ConvertDynamicPath( path );
                }
                if ( PathUtils.IsRelativePath( result ) ) {
                    result = PathUtils.GetProjectAbsolutePath( exporter.GetDirectoryPath( ), result );
                }
                return result;
            }
        }
        public string Path {
            get {
#if UNITY_EDITOR
                // Pathが相対パスでもDynamicPathでもなく、Objectがnullでない場合はAssetPathを取得
                if ( obj != null && !PathUtils.IsRelativePath( path ) && !PathUtils.IsDynamicPath( path ) ) {
                    path = AssetDatabase.GetAssetPath( obj );
                }
#endif
                return path;
            }
            set {
                path = value;
#if UNITY_EDITOR
                // PathがDynamicPathではない場合はObjectを設定
                if ( !string.IsNullOrEmpty( path ) && !PathUtils.IsDynamicPath( path ) ) {
                    obj = AssetDatabase.LoadAssetAtPath<Object>( path );
                } else {
                    obj = null;
                }
#endif
            }
        }

        public virtual object Clone( ) {
            return new ObjectRefElement( this );
        }

        public override bool Equals( object obj ) {
            return Equals( obj as ObjectRefElement );
        }

        public bool Equals( ObjectRefElement other ) {
            return this.obj == other.obj && this.path == other.path;
        }

        public override int GetHashCode( ) {
            return obj.GetHashCode( ) ^ path.GetHashCode( );
        }
    }
}
