#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.FileList
{
    public class FileListWindow : EditorWindow
    {
        MizoresPackageExporter _exporter;
        FileListNode _root;
        public static void Show( MizoresPackageExporter exporter, FileListNode root ) {
            var window = CreateInstance<FileListWindow>( );
            window._exporter = exporter;
            window._root = root;
            window.ShowModal( );
        }
        private void OnGUI( ) {
            using ( new EditorGUILayout.HorizontalScope( ) ) {
                var rect = EditorGUILayout.GetControlRect( GUILayout.Width( 10 ) );
                _root.foldout = EditorGUI.Foldout( rect, _root.foldout, string.Empty );
                var path = _exporter.PackageName;
                var icon = AssetDatabase.GetCachedIcon( AssetDatabase.GetAssetPath( _exporter ) );
                EditorGUILayout.LabelField( new GUIContent( path, icon ) );
            }
            if ( _root.foldout ) {
                EditorGUI.indentLevel++;
                EditorGUI.indentLevel++;
                _root.DrawEditorGUI( );
                EditorGUI.indentLevel--;
            }
        }
    }
}
#endif
