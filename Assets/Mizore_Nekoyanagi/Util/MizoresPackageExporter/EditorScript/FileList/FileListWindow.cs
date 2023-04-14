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
            _treeView.Reload( );
        }
        private void OnGUI( ) {
            float tooltipHeight = 100;
            if ( !_treeView.HasSelection( ) ) {
                tooltipHeight = 0;
            }

            var height = position.height - ( 50 * 2f ) - tooltipHeight;
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
                                    EditorGUILayout.LabelField( ExporterTexts.t_FileListNotFoundPathPrefix + node.path );
                                } else {
                                    EditorGUILayout.LabelField( node.path );
                                }
                                break;
                            case NodeType.Excludes:
                                if ( node.ChildCount == 0 ) {
                                    EditorGUILayout.LabelField( ExporterTexts.t_FileListExcludesPathPrefix + node.path );
                                } else {
                                    EditorGUILayout.LabelField( node.path );
                                }
                                break;
                            case NodeType.References:
                                if ( node.args != null && node.args.Count != 0 ) {
                                    EditorGUILayout.LabelField( ExporterTexts.t_FileListReferencesPathPrefix + node.path );
                                    using ( new EditorGUILayout.HorizontalScope( ) ) {
                                        ExporterUtils.Indent( 1 );
                                        EditorGUILayout.LabelField( ExporterTexts.t_FileListTooltipReferencedBy );
                                    }
                                    foreach ( var arg in node.args ) {
                                        using ( new EditorGUILayout.HorizontalScope( ) ) {
                                            ExporterUtils.Indent( 2 );
                                            EditorGUILayout.LabelField( arg );
                                        }
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
