#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.MizoresPackageExporterConsts;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.FileList
{
    public class FileListWindow : EditorWindow
    {
        MizoresPackageExporter _exporter;
        FileListNode _root;

        TreeViewState _treeViewState;
        FileListTreeView _treeView;
        public static void Show( MizoresPackageExporter exporter, FileListNode root ) {
            var window = CreateInstance<FileListWindow>( );
            window._exporter = exporter;
            window._root = root;

            window._treeViewState = new TreeViewState( );
            window._treeView = new FileListTreeView( window._treeViewState, exporter, window._root );
            window._treeView.Reload( );
            window._treeView.ExpandAll( );

            window.ShowModal( );
        }
        private void OnGUI( ) {
            EditorGUI.BeginChangeCheck( );
            bool viewFullPath = EditorGUILayout.Toggle( ExporterTexts.t_FileList_ViewFullPath, EditorPrefs.GetBool( Const.EDITOR_PREF_FILELIST_VIEW_FULLPATH, true ) );
            if ( EditorGUI.EndChangeCheck( ) ) {
                EditorPrefs.SetBool( Const.EDITOR_PREF_FILELIST_VIEW_FULLPATH, viewFullPath );
            }
            var rect = EditorGUILayout.GetControlRect( false, 200 );
            _treeView.viewFullPath = viewFullPath;
            _treeView.OnGUI( rect );

            if ( GUILayout.Button( ExporterTexts.t_FileList_Close ) ) {
                this.Close( );
            }
        }
    }
}
#endif
