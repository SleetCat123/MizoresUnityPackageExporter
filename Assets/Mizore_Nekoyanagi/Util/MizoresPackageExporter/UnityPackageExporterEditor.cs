using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter
{
#if UNITY_EDITOR
    [CustomEditor( typeof( MizoresPackageExporter ) ), CanEditMultipleObjects]
    public class UnityPackageExporterEditor : Editor
    {
        public static string HelpBoxText;
        public static MessageType HelpBoxMessageType;
        public static Vector2 scroll;
        public static string variable_key_temp;
        public MizoresPackageExporter t;
        private void OnEnable( ) {
            HelpBoxText = null;
            t = target as MizoresPackageExporter;
        }
        static string ToAssetsPath( string path ) {
            string datapath = Application.dataPath;
            if ( path.StartsWith( datapath ) ) {
                path = path.Substring( datapath.Length - "Assets".Length );
            }
            return path;
        }
        public string BrowseButtons( string text ) {
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
        public override void OnInspectorGUI( ) {
            if ( targets.Length != 1 ) {
                MultipleEditor.EditMultiple( this );
            } else {
                SingleEditor.EditSingle( this );
            }

            if ( string.IsNullOrEmpty( HelpBoxText ) == false ) {
                EditorGUILayout.HelpBox( HelpBoxText.Trim( ), HelpBoxMessageType );
            }
        }
    }
#endif
}
