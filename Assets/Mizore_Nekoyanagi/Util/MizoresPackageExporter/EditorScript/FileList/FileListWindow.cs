#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.MizoresPackageExporterConsts;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.FileList
{
    public class FileListWindow : EditorWindow
    {
        MizoresPackageExporterEditor _exporterEditor;

        TreeViewState _treeViewState;
        FileListTreeView _treeView;

        public static void Show( MizoresPackageExporterEditor exporterEditor ) {
            var window = CreateInstance<FileListWindow>( );
            window._exporterEditor = exporterEditor;

            window._treeViewState = new TreeViewState( );
            var targetlist = exporterEditor.targets.Select( v => v as MizoresPackageExporter );
            window._treeView = new FileListTreeView( window._treeViewState, targetlist );
            window._treeView.Reload( );
            window._treeView.ExpandAll( );

            window.ShowAuxWindow( );
        }
        private void OnGUI( ) {
            var rect = EditorGUILayout.GetControlRect( false, position.height - 50 );
            _treeView.OnGUI( rect );

            using ( new EditorGUILayout.VerticalScope( EditorStyles.helpBox ) ) {
                EditorGUI.BeginChangeCheck( );
                bool viewFullPath = EditorGUILayout.Toggle( ExporterTexts.t_FileList_ViewFullPath, EditorPrefs.GetBool( Const.EDITOR_PREF_FILELIST_VIEW_FULLPATH, true ) );
                if ( EditorGUI.EndChangeCheck( ) ) {
                    _treeView.viewFullPath = viewFullPath;
                    EditorPrefs.SetBool( Const.EDITOR_PREF_FILELIST_VIEW_FULLPATH, viewFullPath );
                }

                if ( GUILayout.Button( ExporterTexts.t_Button_ExportPackage ) ) {
                    _exporterEditor.Export( );
                    this.Close( );
                }
            }
        }
    }
}
#endif
