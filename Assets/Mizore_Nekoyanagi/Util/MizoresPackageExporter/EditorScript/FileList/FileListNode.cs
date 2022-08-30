using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MizoreNekoyanagi.PublishUtil.PackageExporter.FileList
{
    public class FileListNode
    {
        public string path;
        public Dictionary<string, FileListNode> childrenTable = new Dictionary<string, FileListNode>( );
        public bool foldout = true;

        public int ChildCount { get => childrenTable.Count; }

        public void Add( string path ) {
            path = path.Replace( '\\', '/' );
            var splittedPath = path.Split( '/' );
            FileListNode node = this;
            string tempPath = null;
            for ( int i = 0; i < splittedPath.Length; i++ ) {
                var item = splittedPath[i];
                if ( tempPath == null ) {
                    tempPath = item;
                } else {
                    tempPath = tempPath + "/" + item;
                }
                FileListNode child;
                if ( !node.childrenTable.TryGetValue( item, out child ) ) {
                    child = new FileListNode( );
                    child.path = string.Join( "/", tempPath );
                    node.childrenTable.Add( item, child );
                }
                node = child;
            }
        }

        public static FileListNode CreateList( IEnumerable<string> paths ) {
            FileListNode root = new FileListNode( );
            foreach ( var item in paths ) {
                root.Add( item );
            }
            return root;
        }

        private static void DrawEditorGUIRecursive( FileListNode node, int indent ) {
            foreach ( var kvp in node.childrenTable ) {
                EditorGUI.indentLevel = indent;
                var value = kvp.Value;
                using ( new EditorGUILayout.HorizontalScope( ) ) {
                    if ( value.ChildCount != 0 ) {
                        EditorGUI.indentLevel--;
                    var rect = EditorGUILayout.GetControlRect( GUILayout.Width( 10 ) );
                        value.foldout = EditorGUI.Foldout( rect, value.foldout, string.Empty );
                    }
                    var path = value.path;
                    var label = path;
                    Texture icon;
                    if ( Path.GetExtension( path ).Length != 0 ) {
                        if ( File.Exists( path ) ) {
                            icon = AssetDatabase.GetCachedIcon( path );
                        } else {
                            label = ExporterTexts.t_ExportLog_NotFoundPathPrefix + label;
                            icon = EditorGUIUtility.IconContent( "Error" ).image;
                        }
                    } else if ( Directory.Exists( path ) ) {
                        icon = AssetDatabase.GetCachedIcon( path );
                    } else {
                        label = ExporterTexts.t_ExportLog_NotFoundPathPrefix + label;
                        icon = EditorGUIUtility.IconContent( "Error" ).image;
                    }
                    // var label = System.IO.Path.GetFileName( path );
                    var content = new GUIContent( label, icon );
                    EditorGUILayout.LabelField( content );
                }
                if ( value.ChildCount != 0 && value.foldout ) {
                    DrawEditorGUIRecursive( kvp.Value, indent + 1 );
                }
            }
        }
        public void DrawEditorGUI( ) {
#if UNITY_EDITOR
            var indentTemp = EditorGUI.indentLevel;
            DrawEditorGUIRecursive( this, EditorGUI.indentLevel );
            EditorGUI.indentLevel = indentTemp;
#endif
        }
    }
}
