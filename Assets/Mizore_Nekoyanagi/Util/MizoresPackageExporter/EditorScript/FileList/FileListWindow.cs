#if UNITY_EDITOR
using MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.FileList
{
    public class FileListWindow : EditorWindow
    {
        TreeViewState _treeViewState;
        FileListTreeView _treeView;

        MizoresPackageExporter[] _targets;
        ExporterEditorLogs _logs;
        FileListNode _root;

        public static void Show( ExporterEditorLogs logs, MizoresPackageExporter[] targets ) {
            var window = CreateInstance<FileListWindow>( );

            window._targets = targets;
            window._logs = logs;
            window._root = CreateFileList.Create( targets );
            window.InitTreeView( );
            window.ShowAuxWindow( );
        }
        void InitTreeView( ) {
            _treeViewState = new TreeViewState( );
            _treeView = new FileListTreeView( _treeViewState, _root );
            ReloadTreeView( );
            _treeView.ExpandAll( );
        }
        void ReloadTreeView( ) {
            _treeView.hierarchyView = ExporterEditorPrefs.FileListTreeView;
            if ( ExporterEditorPrefs.FileListTreeView ) {
                _treeView.viewFullPath = ExporterEditorPrefs.FileListTreeViewFullPath;
            } else {
                _treeView.viewFullPath = ExporterEditorPrefs.FileListFlatViewFullPath;
            }
            _treeView.Reload( );
        }
        private void OnGUI( ) {
            var height = position.height - ( 50 * 2f );
            var rect = EditorGUILayout.GetControlRect( false, height );
            _treeView.OnGUI( rect );

            using ( new EditorGUILayout.VerticalScope( EditorStyles.helpBox ) ) {
                EditorGUI.BeginChangeCheck( );
                bool treeView = EditorGUILayout.Toggle( ExporterTexts.t_FileListTreeView, ExporterEditorPrefs.FileListTreeView );
                if ( EditorGUI.EndChangeCheck( ) ) {
                    ExporterEditorPrefs.FileListTreeView = treeView;
                    ReloadTreeView( );
                }

                if ( ExporterEditorPrefs.FileListTreeView ) {
                    EditorGUI.BeginChangeCheck( );
                    bool viewFullPath = EditorGUILayout.Toggle( ExporterTexts.t_FileListViewFullPath, ExporterEditorPrefs.FileListTreeViewFullPath );
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        ExporterEditorPrefs.FileListTreeViewFullPath = viewFullPath;
                        ReloadTreeView( );
                    }
                } else {
                    EditorGUI.BeginChangeCheck( );
                    bool viewFullPath = EditorGUILayout.Toggle( ExporterTexts.t_FileListViewFullPath, ExporterEditorPrefs.FileListFlatViewFullPath );
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        ExporterEditorPrefs.FileListFlatViewFullPath = viewFullPath;
                        ReloadTreeView( );
                    }
                }

                if ( GUILayout.Button( ExporterTexts.t_ButtonExportPackage ) ) {
                    MizoresPackageExporterEditor.Export( _logs, _targets );
                    this.Close( );
                }
            }
        }
    }
}
#endif
