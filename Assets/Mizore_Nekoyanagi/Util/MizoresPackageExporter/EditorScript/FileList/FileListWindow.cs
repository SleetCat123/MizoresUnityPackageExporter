#if UNITY_EDITOR
using MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterConsts_Editor;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.FileList
{
    public class FileListWindow : EditorWindow
    {
        TreeViewState _treeViewState;
        FileListTreeView _treeView;

        Vector2 _flatViewScroll;

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
            _treeView.Reload( );
            _treeView.ExpandAll( );
        }
        private void OnGUI( ) {
            if ( EditorPrefs.GetBool( Const.EDITOR_PREF_FILELIST_TREEVIEW, true ) ) {
                var height = position.height - ( 50 * 1.5f );
                var rect = EditorGUILayout.GetControlRect( false, height );
                _treeView.OnGUI( rect );
            } else {
                _flatViewScroll = EditorGUILayout.BeginScrollView( _flatViewScroll );
                FileListFlat.Draw( 
                    _root,
                    EditorPrefsCache.GetBool( ExporterConsts_Editor.EDITOR_PREF_FILELIST_VIEW_FULLPATH, true ),
                    EditorPrefsCache.GetBool( ExporterConsts_Editor.EDITOR_PREF_FILELIST_DRAW_FOLDER, true ) 
                    );
                EditorGUILayout.EndScrollView( );
            }

            using ( new EditorGUILayout.VerticalScope( EditorStyles.helpBox ) ) {
                EditorGUI.BeginChangeCheck( );
                bool treeView = EditorGUILayout.Toggle( ExporterTexts.t_FileListTreeView, EditorPrefsCache.GetBool( Const.EDITOR_PREF_FILELIST_TREEVIEW, true ) );
                if ( EditorGUI.EndChangeCheck( ) ) {
                    EditorPrefsCache.SetBool( Const.EDITOR_PREF_FILELIST_TREEVIEW, treeView );
                }

                EditorGUI.BeginChangeCheck( );
                bool viewFullPath = EditorGUILayout.Toggle( ExporterTexts.t_FileListViewFullPath, EditorPrefsCache.GetBool( Const.EDITOR_PREF_FILELIST_VIEW_FULLPATH, true ) );
                if ( EditorGUI.EndChangeCheck( ) ) {
                    _treeView.viewFullPath = viewFullPath;
                    EditorPrefsCache.SetBool( Const.EDITOR_PREF_FILELIST_VIEW_FULLPATH, viewFullPath );
                }

                if (!treeView) {
                    EditorGUI.BeginChangeCheck( );
                    bool drawFolder = EditorGUILayout.Toggle( ExporterTexts.t_FileListDrawFolder, EditorPrefsCache.GetBool( Const.EDITOR_PREF_FILELIST_DRAW_FOLDER, true ) );
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        EditorPrefsCache.SetBool( Const.EDITOR_PREF_FILELIST_DRAW_FOLDER, drawFolder );
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
