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
        public ObjectRefElement( MizoresPackageExporter exporter, Object obj, bool relativePath = false ) {
            this.exporter = exporter;
            SetObject( obj, relativePath );
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
                    var actualPath = path;
                    if ( PathUtils.IsDynamicPath( path ) ) {
                        actualPath = exporter.ConvertDynamicPath( path );
                    }
                    if ( PathUtils.IsRelativePath( actualPath ) ) {
                        actualPath = PathUtils.GetProjectAbsolutePath( exporter.GetDirectoryPath( ), actualPath );
                    }
                    return AssetDatabase.LoadAssetAtPath<Object>( actualPath );
                }
#else
                return obj;
#endif
            }
        }
        public void SetObject( Object value, bool relativePath = false ) {
#if UNITY_EDITOR
            if ( value != null ) {
                path = AssetDatabase.GetAssetPath( value.GetInstanceID( ) );
                if ( relativePath ) {
                    path = PathUtils.GetRelativePath( exporter.GetDirectoryPath( ), path );
                }
            } else {
                path = string.Empty;
            }
            obj = value;
#else
    throw new System.NotSupportedException( "This method is only supported in the editor." );
#endif
        }

        public string Path {
            get {
                // Pathが相対パスでもDynamicPathでもなく、Objectがnullでない場合はAssetPathを取得
                if ( obj != null && !PathUtils.IsRelativePath( path ) && !PathUtils.IsDynamicPath( path ) ) {
                    path = AssetDatabase.GetAssetPath( obj );
                }
                return path;
            }
            set {
                path = value;
                // Pathが相対パスでもDynamicPathでもない場合はObjectを設定
                if ( !string.IsNullOrEmpty( path ) && !PathUtils.IsRelativePath( path ) && !PathUtils.IsDynamicPath( path ) ) {
                    obj = AssetDatabase.LoadAssetAtPath<Object>( path );
                } else {
                    obj = null;
                }
            }
        }

        public object Clone( ) {
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
    [System.Serializable]
    public class PackagePrefsElement : System.ICloneable, System.IEquatable<PackagePrefsElement> {
        [SerializeField]
        protected Object obj;
        [SerializeField]
        protected string path;

        public PackagePrefsElement( ) { }
        public PackagePrefsElement( Object obj ) {
            this.Object = obj;
        }
        public PackagePrefsElement( PackagePrefsElement source ) {
            this.obj = source.obj;
            this.path = source.path;
        }

        public virtual Object Object {
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

        public virtual string Path {
            get {
#if UNITY_EDITOR
                if ( obj != null ) {
                    path = AssetDatabase.GetAssetPath( obj );
                }
#endif
                return path;
            }
        }

        public virtual object Clone( ) {
            return new PackagePrefsElement( this );
        }

        public override bool Equals( object obj ) {
            return Equals( obj as PackagePrefsElement );
        }

        public bool Equals( PackagePrefsElement other ) {
            return this.obj == other.obj && this.path == other.path;
        }

        public override int GetHashCode( ) {
            return obj.GetHashCode( ) ^ path.GetHashCode( );
        }
    }
}
