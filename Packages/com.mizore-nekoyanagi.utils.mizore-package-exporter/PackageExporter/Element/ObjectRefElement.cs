using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    [System.Serializable]
    public class ObjectRefElement : System.ICloneable, System.IEquatable<ObjectRefElement> {
        [SerializeField]
        protected Object obj;
        [SerializeField]
        protected string path;

        public ObjectRefElement( ) { }
        public ObjectRefElement( MizoresPackageExporter exporter, Object obj, bool relativePath ) {
            SetObject( exporter, obj, relativePath );
        }
        public ObjectRefElement( MizoresPackageExporter exporter, Object obj ) {
            SetObject( exporter, obj );
        }
        public ObjectRefElement( string path ) {
            this.Path = path;
        }
        public ObjectRefElement( ObjectRefElement source ) {
            this.obj = source.obj;
            this.path = source.path;
        }

        public Object GetObject( MizoresPackageExporter exporter, string batchExportKey = "" ) {
#if UNITY_EDITOR
            if ( obj != null ) {
                return obj;
            }
            if ( string.IsNullOrEmpty( path ) ) {
                return null;
            } else {
                return AssetDatabase.LoadAssetAtPath<Object>( GetConvertedPath( exporter, batchExportKey ) );
            }
#else
                return obj;
#endif
        }
        public void SetObject( MizoresPackageExporter exporter, Object value ) {
#if UNITY_EDITOR
            SetObject( exporter, value, ExporterEditorPrefs.UseRelativePath );
#endif
        }
        public void SetObject( MizoresPackageExporter exporter, Object value, bool relativePath ) {
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

        public string GetConvertedPath( MizoresPackageExporter exporter, string batchExportKey = "" ) {
            var result = Path;
            if ( PathUtils.IsDynamicPath( path ) ) {
                result = exporter.ConvertDynamicPath( path, batchExportKey );
            }
            if ( PathUtils.IsRelativePath( result ) ) {
                result = PathUtils.GetProjectAbsolutePath( exporter.GetDirectoryPath( ), result );
            }
            return result;
        }
        public string Path {
            get {
#if UNITY_EDITOR
                // Pathが相対パスでもDynamicPathでもなく、Objectがnullでない場合はAssetPathを取得
                if ( obj != null && !PathUtils.IsRelativePath( path ) && !PathUtils.IsDynamicPath( path ) ) {
                    path = AssetDatabase.GetAssetPath( obj );
                }
                if ( path != null ) {
                    path = path.Replace( "%20", " " );
                }
                UpdateObject( );
#endif
                return path;
            }
            set {
                path = value;
                UpdateObject( );
            }
        }
        void UpdateObject( ) {
#if UNITY_EDITOR
            // PathがDynamicPathではない場合はObjectを設定
            if ( !string.IsNullOrEmpty( path ) && !PathUtils.IsDynamicPath( path ) ) {
                obj = AssetDatabase.LoadAssetAtPath<Object>( path );
            } else {
                obj = null;
            }
#endif
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
