#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.FileList
{
    public class FileListTreeView : TreeView
    {
        MizoresPackageExporter _exporter;
        FileListNode _root;
        Dictionary<int, FileListNode> table_id_node = new Dictionary<int, FileListNode>( );
        Dictionary<FileListNode, int> table_node_id = new Dictionary<FileListNode, int>( );
        public bool viewFullPath;

        const int ROOT_ID = 0;
        const int START_NODE_ID = ROOT_ID + 1;

        public FileListTreeView( TreeViewState treeViewState, MizoresPackageExporter exporter, FileListNode root ) : base( treeViewState ) {
            _exporter = exporter;
            _root = root;
        }
        private void BuildIDRecursive( FileListNode node, ref int id ) {
            table_id_node.Add( id, node );
            table_node_id.Add( node, id );
            id++;
            foreach ( var kvp in node.childrenTable ) {
                BuildIDRecursive( kvp.Value, ref id );
            }
        }
        private void BuildID( int startID ) {
            table_id_node.Clear( );
            table_node_id.Clear( );
            int id = startID;
            BuildIDRecursive( _root, ref id );
        }


        protected override TreeViewItem BuildRoot( ) {
            BuildID( START_NODE_ID );
            return new TreeViewItem { id = ROOT_ID, depth = -1, displayName = _exporter.PackageName };
        }
        protected override IList<TreeViewItem> BuildRows( TreeViewItem root ) {
            var rows = GetRows( );
            if ( rows == null ) {
                rows = new List<TreeViewItem>( );
            } else {
                rows.Clear( );
            }
            rows.Add( root );

            AddChildrenRecursive( _root, root, rows );

            SetupDepthsFromParentsAndChildren( root );
            return rows;
        }

        private void AddChildrenRecursive( FileListNode node, TreeViewItem item, IList<TreeViewItem> rows ) {
            foreach ( var child in node.childrenTable.Values ) {
                int id = table_node_id[child];
                var childItem = new TreeViewItem { id = id, displayName = child.path };
                item.AddChild( childItem );
                rows.Add( childItem );
                if ( child.ChildCount >= 1 ) {
                    if ( IsExpanded( id ) ) {
                        AddChildrenRecursive( child, childItem, rows );
                    } else {
                        childItem.children = CreateChildListForCollapsedParent( );
                    }
                }
            }
        }

        protected override IList<int> GetAncestors( int id ) {
            var node = table_id_node[id];
            List<int> result = new List<int>( );
            while ( node.parent != null ) {
                result.Add( table_node_id[node.parent] );
                node = node.parent;
            }
            result.Add( ROOT_ID );
            return result;
        }
        List<int> GetDescendantsID( List<int> list, FileListNode node ) {
            foreach ( var item in node.childrenTable.Values ) {
                list.Add( table_node_id[item] );
                list = GetDescendantsID( list, item );
            }
            return list;
        }
        protected override IList<int> GetDescendantsThatHaveChildren( int id ) {
            if ( id == ROOT_ID ) {
                return GetDescendantsID( new List<int>( ), table_id_node[START_NODE_ID] );
            } else {
                return GetDescendantsID( new List<int>( ), table_id_node[id] );
            }
        }

        protected override void RowGUI( RowGUIArgs args ) {
            var temp_contentColor = GUI.contentColor;
            var path = args.item.displayName;
            string label;
            if ( viewFullPath ) {
                label = path;
            } else {
                label = Path.GetFileName( path );
            }
            Texture icon;
            if ( args.item.id == 0 ) {
                icon = AssetDatabase.GetCachedIcon( AssetDatabase.GetAssetPath( _exporter ) );
            } else {
                if ( Path.GetExtension( path ).Length != 0 ) {
                    if ( File.Exists( path ) ) {
                        icon = AssetDatabase.GetCachedIcon( path );
                    } else {
                        label = ExporterTexts.t_ExportLog_NotFoundPathPrefix + label;
                        icon = EditorGUIUtility.IconContent( "Error" ).image;
                        GUI.contentColor = temp_contentColor + ( Color.red * 0.2f );
                    }
                } else if ( Directory.Exists( path ) ) {
                    icon = AssetDatabase.GetCachedIcon( path );
                    GUI.contentColor = temp_contentColor * 0.85f;
                } else {
                    label = ExporterTexts.t_ExportLog_NotFoundPathPrefix + label;
                    icon = EditorGUIUtility.IconContent( "Error" ).image;
                    GUI.contentColor = temp_contentColor + ( Color.red * 0.2f );
                }
            }
            args.label = label;

            Rect iconRect = args.rowRect;
            iconRect.x += GetContentIndent( args.item );
            iconRect.width = 16f;
            GUI.DrawTexture( iconRect, icon );

            extraSpaceBeforeIconAndLabel = iconRect.width + 2f;
            base.RowGUI( args );
            GUI.contentColor = temp_contentColor;
        }
    }
}
#endif
