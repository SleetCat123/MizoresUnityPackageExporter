﻿#if UNITY_EDITOR
using MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using static MizoreNekoyanagi.PublishUtil.PackageExporter.FileList.CreateFileList;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.FileList {
    public class FileListWindow : EditorWindow {
        TreeViewState _treeViewState;
        FileListTreeView _treeView;

        MizoresPackageExporter[] _targets;
        ExporterEditorLogs _logs;

        Vector2 _tooltipScroll;

        List<string> exportPaths;

        public static void Show( ExporterEditorLogs logs, MizoresPackageExporter[] targets ) {
            var window = CreateInstance<FileListWindow>( );
            window.titleContent = new GUIContent( ExporterTexts.FileListWindowTitle );
            window._targets = targets;
            window._logs = logs;
            var data = CreateFileList.Create( targets );
            window.InitTreeView( data );
            window.ShowAuxWindow( );
        }
        void InitTreeView( FileListData data ) {
            _treeViewState = new TreeViewState( );
            _treeView = new FileListTreeView( _treeViewState, data.rootNode );
            _treeView.ignorePaths.Clear( );
            exportPaths = data.packages;
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
                using ( new EditorGUILayout.HorizontalScope( ) ) {
                    if ( GUILayout.Button( ExporterTexts.ButtonExportAll, GUILayout.Width( 70 ) ) ) {
                        _treeView.ignorePaths.Clear( );
                    }
                    if ( GUILayout.Button( ExporterTexts.ButtonExportNone, GUILayout.Width( 70 ) ) ) {
                        _treeView.ignorePaths = new HashSet<string>( exportPaths );
                    }
                }


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

                if ( _treeView.HasReferencedFile ) {
                    EditorGUI.BeginChangeCheck( );
                    bool referencedFiles = EditorGUILayout.Toggle( ExporterTexts.FileListViewReferencedFiles, ExporterEditorPrefs.FileListViewReferencedFiles );
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        ExporterEditorPrefs.FileListViewReferencedFiles = referencedFiles;
                        ReloadTreeView( );
                    }
                }

                if ( _treeView.HasExcludeFile ) {
                    EditorGUI.BeginChangeCheck( );
                    bool excludeFiles = EditorGUILayout.Toggle( ExporterTexts.FileListViewExcludeFiles, ExporterEditorPrefs.FileListViewExcludeFiles );
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        ExporterEditorPrefs.FileListViewExcludeFiles = excludeFiles;
                        ReloadTreeView( );
                    }
                }

                if ( GUILayout.Button( ExporterTexts.ButtonExportPackage, GUILayout.Height( 50 ) ) ) {
                    MizoresPackageExporterEditor.Export( _logs, _targets, _treeView.ignorePaths );
                    this.Close( );
                }
            }
        }
    }
}
#endif
