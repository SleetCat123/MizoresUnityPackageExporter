using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor
{
#if UNITY_EDITOR
    public static class GUIElement_Utils
    {
        static string ToAssetsPath( string path ) {
            string datapath = Application.dataPath;
            if ( path.StartsWith( datapath ) ) {
                path = path.Substring( datapath.Length - "Assets".Length );
            }
            return path;
        }
        public static string BrowseButtons( MizoresPackageExporter t, string text ) {
            string result = text;
            if ( GUILayout.Button( ExporterTexts.TEXT_BUTTON_FOLDER, GUILayout.Width( 50 ) ) ) {
                text = EditorUtility.OpenFolderPanel( null, t.ConvertDynamicPath( text ), null );
                text = ToAssetsPath( text );
                if ( string.IsNullOrEmpty( text ) == false ) {
                    GUI.changed = true;
                    result = text;
                }
            }
            if ( GUILayout.Button( ExporterTexts.TEXT_BUTTON_FILE, GUILayout.Width( 50 ) ) ) {
                text = EditorUtility.OpenFilePanel( null, t.ConvertDynamicPath( text ), null );
                text = ToAssetsPath( text );
                if ( string.IsNullOrEmpty( text ) == false ) {
                    GUI.changed = true;
                    result = text;
                }
            }
            return result;
        }
    }
#endif
}
