using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter
{
    public static class IconCache
    {
        static Texture _unityLogoIcon;
        public static Texture UnityLogoIcon {
            get {
#if UNITY_EDITOR
                if ( _unityLogoIcon == null ) {
                    _unityLogoIcon = EditorGUIUtility.IconContent( "UnityLogo" ).image;
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
                    _errorIcon = EditorGUIUtility.IconContent( "console.erroricon.sml" ).image;
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
                    _warningIcon = EditorGUIUtility.IconContent( "console.warnicon.sml" ).image;
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
                    _infoIcon = EditorGUIUtility.IconContent( "console.infoicon.sml" ).image;
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
                    _helpIcon = EditorGUIUtility.IconContent( "_Help@2x" ).image;
                }
#endif
                return _helpIcon;
            }
        }

        static Texture _AddIcon;
        public static Texture AddIcon {
            get {
#if UNITY_EDITOR
                if ( _AddIcon == null ) {
                    _AddIcon = EditorGUIUtility.IconContent( "Toolbar Plus@2x" ).image;
                }
#endif
                return _AddIcon;
            }
        }

        static Texture _RemoveIcon;
        public static Texture RemoveIcon {
            get {
#if UNITY_EDITOR
                if ( _RemoveIcon == null ) {
                    _RemoveIcon = EditorGUIUtility.IconContent( "Toolbar Minus@2x" ).image;
                }
#endif
                return _RemoveIcon;
            }
        }
    }
}
