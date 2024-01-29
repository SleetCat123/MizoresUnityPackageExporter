using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public static class IconCache {
        static Texture _unityLogoIcon;
        public static Texture UnityLogoIcon {
            get {
#if UNITY_EDITOR
                if ( _unityLogoIcon == null ) {
                    _unityLogoIcon = EditorGUIUtility.Load( "UnityLogo" ) as Texture;
                }
#endif
                return _unityLogoIcon;
            }
        }

        static Texture _errorIcon;
        public static Texture ErrorIcon {
            get {
#if UNITY_EDITOR
                if ( _errorIcon == null ) {
                    _errorIcon = EditorGUIUtility.Load( "console.erroricon.sml" ) as Texture;
                }
#endif
                return _errorIcon;
            }
        }

        static Texture _warningIcon;
        public static Texture WarningIcon {
            get {
#if UNITY_EDITOR
                if ( _warningIcon == null ) {
                    _warningIcon = EditorGUIUtility.Load( "console.warnicon.sml" ) as Texture;
                }
#endif
                return _warningIcon;
            }
        }

        static Texture _infoIcon;
        public static Texture InfoIcon {
            get {
#if UNITY_EDITOR
                if ( _infoIcon == null ) {
                    _infoIcon = EditorGUIUtility.Load( "console.infoicon.sml" ) as Texture;
                }
#endif
                return _infoIcon;
            }
        }

        static Texture _helpIcon;
        public static Texture HelpIcon {
            get {
#if UNITY_EDITOR
                if ( _helpIcon == null ) {
                    _helpIcon = EditorGUIUtility.Load( "_Help@2x" ) as Texture;
                }
#endif
                return _helpIcon;
            }
        }

        static GUIContent _AddIconContent;
        public static GUIContent AddIconContent {
            get {
#if UNITY_EDITOR
                if ( _AddIconContent == null ) {
                    _AddIconContent = EditorGUIUtility.IconContent( "Toolbar Plus@2x" );
                }
#endif
                return _AddIconContent;
            }
        }
        public static Texture AddIcon {
            get {
                return AddIconContent.image;
            }
        }

        static GUIContent _RemoveIconContent;
        public static GUIContent RemoveIconContent {
            get {
#if UNITY_EDITOR
                if ( _RemoveIconContent == null ) {
                    _RemoveIconContent = EditorGUIUtility.IconContent( "Toolbar Minus@2x" );
                }
#endif
                return _RemoveIconContent;
            }
        }
        public static Texture RemoveIcon {
            get {
                return RemoveIconContent.image;
            }
        }

        static GUIContent _FolderIconContent;
        public static GUIContent FolderIconContent {
            get {
#if UNITY_EDITOR
                if ( _FolderIconContent == null ) {
                    _FolderIconContent = EditorGUIUtility.IconContent( "d_FolderOpened Icon" );
                }
#endif
                return _FolderIconContent;
            }
        }
        public static Texture FolderIcon {
            get {
                return FolderIconContent.image;
            }
        }

        static GUIContent _FileIconContent;
        public static GUIContent FileIconContent {
            get {
#if UNITY_EDITOR
                if ( _FileIconContent == null ) {
                    _FileIconContent = EditorGUIUtility.IconContent( "d_DefaultAsset Icon" );
                }
#endif
                return _FileIconContent;
            }
        }
        public static Texture FileIcon {
            get {
                return FileIconContent.image;
            }
        }
    }
}
