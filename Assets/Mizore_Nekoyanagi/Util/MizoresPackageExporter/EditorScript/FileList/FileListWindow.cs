#if UNITY_EDITOR
using MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor;
using System;
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
        TreeViewState _treeViewState;
        FileListTreeView _treeView;
        MizoresPackageExporter[] _targets;
        PrePostPostProcessing _action;
        ExporterEditorLogs _logs;

        public static void Show( ExporterEditorLogs logs, MizoresPackageExporter[] targets, PrePostPostProcessing action = null ) {
            var window = CreateInstance<FileListWindow>( );

            window._treeViewState = new TreeViewState( );
            window._targets = targets;
            window._logs = logs;
            window._action = action;
            window._treeView = new FileListTreeView( window._treeViewState, targets, action );
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
                    MizoresPackageExporterEditor.Export( _logs, _targets, _action );
                    this.Close( );
                }
            }
        }
    }
}
#endif
