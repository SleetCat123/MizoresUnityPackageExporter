using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    [System.Serializable]
    public class ObjectRefElement : System.ICloneable, System.IEquatable<ObjectRefElement> {
        [SerializeField]
        protected string path;
        public string Path => path;

        [SerializeField]
        protected bool isGUID;
        public bool IsGUID => isGUID;

        public ObjectRefElement( ) { }
        public ObjectRefElement( Object obj, bool isGUID = false ) {
            if ( isGUID ) {
                SetGUID( obj );
            } else {
                SetPath( obj );
            }
        }
        public ObjectRefElement( string path ) {
            this.path = path;
        }
        public ObjectRefElement( MizoresPackageExporter exporter, Object obj ) {
            SetPathAutoDetect( exporter, obj );
        }
        public ObjectRefElement( ObjectRefElement source ) {
            this.path = source.path;
            this.isGUID = source.isGUID;
        }

        public Object GetObject( MizoresPackageExporter exporter, string batchExportKey = "" ) {
#if UNITY_EDITOR
            if ( string.IsNullOrEmpty( path ) ) {
                return null;
            } else if ( isGUID ) {
                return AssetDatabase.LoadAssetAtPath<Object>( AssetDatabase.GUIDToAssetPath( path ) );
            } else {
                return AssetDatabase.LoadAssetAtPath<Object>( GetConvertedPath( exporter, batchExportKey ) );
            }
#else
                return null;
#endif
        }

        public void SetGUID( string value ) {
            isGUID = true;
            path = value;
        }
        public void SetGUID( Object value ) {
#if UNITY_EDITOR
            isGUID = true;
            path = AssetDatabase.AssetPathToGUID( AssetDatabase.GetAssetPath( value ) );
#else
            throw new System.NotImplementedException( );
#endif
        }

        public void SetPath( string value ) {
            isGUID = false;
            path = value.Replace( "%20", " " );
        }
        public void SetPath( Object value ) {
#if UNITY_EDITOR
            isGUID = false;
            path = AssetDatabase.GetAssetPath( value );
#else
            throw new System.NotImplementedException( );
#endif
        }
        public void SetPathAutoDetect( MizoresPackageExporter exporter, Object value ) {
            isGUID = false;
            PathType pathType = PathType.Absolute;
#if UNITY_EDITOR
            pathType = ExporterEditorPrefs.DefaultPathType;
#endif
            switch ( pathType ) {
                case PathType.Relative:
                    SetRelativePath( exporter, value );
                    break;
                case PathType.Absolute:
                    SetPath( value );
                    break;
                case PathType.GUID:
                    SetGUID( value );
                    break;
            }
        }

        public void SetRelativePath( MizoresPackageExporter exporter, Object value ) {
#if UNITY_EDITOR
            isGUID = false;
            path = PathUtils.GetRelativePath( exporter.GetDirectoryPath( ), AssetDatabase.GetAssetPath( value ) );
#else
            throw new System.NotImplementedException( );
#endif
        }
        public void SetPathAutoDetect( MizoresPackageExporter exporter, string path ) {
#if UNITY_EDITOR
            PathType pathType = PathType.Absolute;
            pathType = ExporterEditorPrefs.DefaultPathType;
            path = path.Replace( "%20", " " );
            switch ( pathType ) {
                case PathType.Relative:
                    isGUID = false;
                    this.path = PathUtils.GetRelativePath( exporter.GetDirectoryPath( ), path );
                    break;
                case PathType.Absolute:
                    isGUID = false;
                    this.path = path;
                    break;
                case PathType.GUID:
                    isGUID = true;
                    this.path = AssetDatabase.AssetPathToGUID( path );
                    break;
            }
#else
            throw new System.NotImplementedException( );
#endif
        }

        public string GetConvertedPath( MizoresPackageExporter exporter, string batchExportKey = "" ) {
#if UNITY_EDITOR
            if ( isGUID ) {
                return AssetDatabase.GUIDToAssetPath( path );
            } else {
                var result = path;
                if ( PathUtils.IsDynamicPath( path ) ) {
                    result = exporter.ConvertDynamicPath( path, batchExportKey );
                }
                if ( PathUtils.IsRelativePath( result ) ) {
                    result = PathUtils.GetProjectAbsolutePath( exporter.GetDirectoryPath( ), result );
                }
                return result;
            }
#else
            throw new System.NotImplementedException( );
#endif
        }

        public virtual object Clone( ) {
            return new ObjectRefElement( this );
        }

        public void CopyFrom( ObjectRefElement source ) {
            this.path = source.path;
            this.isGUID = source.isGUID;
        }

        public override bool Equals( object obj ) {
            return Equals( obj as ObjectRefElement );
        }

        public bool Equals( ObjectRefElement other ) {
            if ( other is null ) return false;
            return this.path == other.path && this.isGUID == other.isGUID;
        }

        public override int GetHashCode( ) {
            return ( path?.GetHashCode( ) ?? 0 ) ^ isGUID.GetHashCode( );
        }

        public override string ToString( ) {
            if ( isGUID ) {
                return $"[GUID]{path}";
            } else {
                return path;
            }
        }
    }
}
