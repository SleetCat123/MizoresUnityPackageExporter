#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.FileList
{
    public static class FileListFlat
    {
        static HashSet<string> _foldoutClosed = new HashSet<string>( );
        static void DrawRecursive( FileListNode node, bool viewFullPath, bool drawFolder ) {
            var temp_contentColor = GUI.contentColor;
            foreach ( var item in node.childrenTable.Values ) {
                string label;
                if ( viewFullPath ) {
                    label = item.path;
                } else {
                    label = Path.GetFileName( item.path );
                }
                Texture icon;
                var result = ExporterUtils.TryGetIcon( item.path, out icon );
                switch ( result ) {
                    case ExporterUtils.GetIconResult.ExistsFile:
                        using ( new EditorGUILayout.HorizontalScope( ) ) {
                            ExporterUtils.Indent( 1 );
                            EditorGUILayout.LabelField( new GUIContent( label, icon ) );
                        }
                        break;
                    case ExporterUtils.GetIconResult.ExistsFolder:
                        GUI.contentColor = temp_contentColor * 0.85f;
                        if ( drawFolder ) {
                            using ( new EditorGUILayout.HorizontalScope( ) ) {
                                ExporterUtils.Indent( 1 );
                                EditorGUILayout.LabelField( new GUIContent( label, icon ) );
                            }
                        }
                        break;
                    default:
                    case ExporterUtils.GetIconResult.NotExistsFile:
                    case ExporterUtils.GetIconResult.NotExistsFolder:
                        label = ExporterTexts.t_ExportLogNotFoundPathPrefix + label;
                        GUI.contentColor = temp_contentColor + ( Color.red * 0.2f );
                        using ( new EditorGUILayout.HorizontalScope( ) ) {
                            ExporterUtils.Indent( 1 );
                            EditorGUILayout.LabelField( new GUIContent( label, icon ) );
                        }
                        break;
                }
                GUI.contentColor = temp_contentColor;
                if ( result == ExporterUtils.GetIconResult.ExistsFolder ) {
                    DrawRecursive( item, viewFullPath, drawFolder );
                }
            }
        }
        public static void Draw( FileListNode root, bool viewFullPath, bool drawFolder ) {
            if ( root == null ) {
                return;
            }
            bool first = true;
            foreach ( var item in root.childrenTable.Values ) {
                if ( !first ) {
                    EditorGUILayout.Separator( );
                }
                first = false;
                var rect = EditorGUILayout.GetControlRect( );
                var path = item.path;
                var label = new GUIContent( path, IconCache.UnityLogoIcon );
                var foldout = EditorGUI.BeginFoldoutHeaderGroup( rect, !_foldoutClosed.Contains( path ), label );
                EditorGUI.EndFoldoutHeaderGroup( );
                if ( foldout ) {
                    _foldoutClosed.Remove( path );
                } else {
                    _foldoutClosed.Add( path );
                    continue;
                }
                DrawRecursive( item, viewFullPath, drawFolder );
            }
        }
    }
}
#endif
