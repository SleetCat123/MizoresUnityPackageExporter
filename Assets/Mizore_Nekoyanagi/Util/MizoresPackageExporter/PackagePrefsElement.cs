﻿using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter
{
    [System.Serializable]
    public class PackagePrefsElement : System.ICloneable
    {
        [SerializeField]
        private Object obj;
        [SerializeField]
        private string path;

        public PackagePrefsElement( ) { }
        public PackagePrefsElement( Object obj ) {
            this.Object = obj;
        }
        public PackagePrefsElement( PackagePrefsElement source ) {
            this.obj = source.obj;
            this.path = source.path;
        }

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
                    path = AssetDatabase.GetAssetPath( obj );
                }
#endif
                return path;
            }
        }

        public object Clone( ) {
            return new PackagePrefsElement( this );
        }
    }
}
