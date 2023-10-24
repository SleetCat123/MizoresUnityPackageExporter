#if UNITY_EDITOR
using MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.FileList {
    public class FileListWindow : EditorWindow {
        TreeViewState _treeViewState;
        FileListTreeView _treeView;

        MizoresPackageExporter[] _targets;
        ExporterEditorLogs _logs;
        FileListNode _root;

        Vector2 _tooltipScroll;

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
            _treeView.viewReferencedFiles = ExporterEditorPrefs.FileListViewReferencedFiles;
            _treeView.viewExcludeFiles = ExporterEditorPrefs.FileListViewExcludeFiles;
            _treeView.Reload( );
        }
        private void OnGUI( ) {
            float tooltipHeight = 100;
            if ( !_treeView.HasSelection( ) ) {
                tooltipHeight = 0;
            }

            var height = position.height - ( 50 * 4f ) - tooltipHeight;
            var rect = EditorGUILayout.GetControlRect( false, height );
            _treeView.OnGUI( rect );

            if ( tooltipHeight != 0 ) {
                using ( new EditorGUILayout.VerticalScope( EditorStyles.helpBox ) ) {
                    _tooltipScroll = EditorGUILayout.BeginScrollView( _tooltipScroll );
                    EditorGUILayout.LabelField( "Info", EditorStyles.boldLabel );
                    var selected = _treeView.GetSelection( );
                    foreach ( var id in selected ) {
                        var node = _treeView.GetNode( id );
                        if ( node.path[0] == '[' ) {
                            continue;
                        }
                        switch ( node.type ) {
                            default:
                                EditorGUILayout.LabelField( node.path );
                                break;
                            case NodeType.NotFound:
                                if ( node.ChildCount == 0 ) {
                                    EditorGUILayout.LabelField( ExporterTexts.FileListNotFoundPathPrefix + node.path );
                                } else {
                                    EditorGUILayout.LabelField( node.path );
                                }
                                break;
                            case NodeType.Excludes:
                                if ( node.ChildCount == 0 ) {
                                    EditorGUILayout.LabelField( ExporterTexts.FileListExcludesPathPrefix + node.path );
                                } else {
                                    EditorGUILayout.LabelField( node.path );
                                }
                                break;
                            case NodeType.References:
                                if ( node.args != null && node.args.Count != 0 ) {
                                    EditorGUILayout.LabelField( ExporterTexts.FileListReferencesPathPrefix + node.path );
                                    EditorGUI.indentLevel++;
                                    EditorGUILayout.LabelField( ExporterTexts.FileListTooltipReferencedBy );
                                    EditorGUI.indentLevel--;
                                    foreach ( var arg in node.args ) {
                                        EditorGUI.indentLevel += 2;
                                        EditorGUILayout.LabelField( arg );
                                        EditorGUI.indentLevel -= 2;
                                    }
                                    EditorGUILayout.Separator( );
                                } else {
                                    EditorGUILayout.LabelField( node.path );
                                }
                                break;
                        }
                    }
                    EditorGUILayout.EndScrollView( );
                }
            }

            using ( new EditorGUILayout.VerticalScope( EditorStyles.helpBox ) ) {
                EditorGUI.BeginChangeCheck( );
                bool treeView = EditorGUILayout.Toggle( ExporterTexts.FileListTreeView, ExporterEditorPrefs.FileListTreeView );
                if ( EditorGUI.EndChangeCheck( ) ) {
                    ExporterEditorPrefs.FileListTreeView = treeView;
                    ReloadTreeView( );
                }

                if ( ExporterEditorPrefs.FileListTreeView ) {
                    EditorGUI.BeginChangeCheck( );
                    bool viewFullPath = EditorGUILayout.Toggle( ExporterTexts.FileListViewFullPath, ExporterEditorPrefs.FileListTreeViewFullPath );
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        ExporterEditorPrefs.FileListTreeViewFullPath = viewFullPath;
                        ReloadTreeView( );
                    }
                } else {
                    EditorGUI.BeginChangeCheck( );
                    bool viewFullPath = EditorGUILayout.Toggle( ExporterTexts.FileListViewFullPath, ExporterEditorPrefs.FileListFlatViewFullPath );
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        ExporterEditorPrefs.FileListFlatViewFullPath = viewFullPath;
                        ReloadTreeView( );
                    }
                }

                EditorGUI.BeginChangeCheck( );
                bool referencedFiles = EditorGUILayout.Toggle( ExporterTexts.FileListViewReferencedFiles, ExporterEditorPrefs.FileListViewReferencedFiles );
                if ( EditorGUI.EndChangeCheck( ) ) {
                    ExporterEditorPrefs.FileListViewReferencedFiles = referencedFiles;
                    ReloadTreeView( );
                }

                EditorGUI.BeginChangeCheck( );
                bool excludeFiles = EditorGUILayout.Toggle( ExporterTexts.FileListViewExcludeFiles, ExporterEditorPrefs.FileListViewExcludeFiles );
                if ( EditorGUI.EndChangeCheck( ) ) {
                    ExporterEditorPrefs.FileListViewExcludeFiles = excludeFiles;
                    ReloadTreeView( );
                }

                if ( GUILayout.Button( ExporterTexts.ButtonExportPackage ) ) {
                    MizoresPackageExporterEditor.Export( _logs, _targets );
                    this.Close( );
                }
            }
        }
    }
}
#endif
