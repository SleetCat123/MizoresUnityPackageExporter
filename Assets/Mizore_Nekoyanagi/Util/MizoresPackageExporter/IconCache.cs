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
    }
}
