#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.FileList
{
    public class FileListTreeView : TreeView
    {
        MizoresPackageExporter[] _exporters;
        PrePostPostProcessing _action;
        FileListNode _root;
        Dictionary<int, FileListNode> table_id_node = new Dictionary<int, FileListNode>( );
        Dictionary<FileListNode, int> table_node_id = new Dictionary<FileListNode, int>( );
        Dictionary<FileListNode, MizoresPackageExporter> table_root = new Dictionary<FileListNode, MizoresPackageExporter>( );
        public bool viewFullPath;

        const int ROOT_ID = 0;
        const int START_NODE_ID = ROOT_ID + 1;

        public FileListTreeView( TreeViewState treeViewState, MizoresPackageExporter[] exporters, PrePostPostProcessing action ) : base( treeViewState ) {
            _exporters = exporters;
            _action = action;
        }
        public void UpdateList( ) {
            var closedPaths = table_id_node.Keys.Where( v => !IsExpanded( v ) );
            Reload( );
            ExpandAll( );
            foreach ( var item in closedPaths ) {
                SetExpanded( item, false );
            }
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
            _root = new FileListNode( );
            for ( int i = 0; i < _exporters.Length; i++ ) {
                var item = _exporters[i];
                _action?.filelist_preprocessing?.Invoke( item, i );
                var exportName = item.ExportFileName;
                if ( _root.Contains( exportName ) ) {
                    Debug.Log( "skip: " + exportName );
                    //_action?.filelist_postprocessing?.Invoke( item, i );
                    continue;
                }
                Debug.Log( exportName );
                var node = FileList.FileListNode.CreateList( item.GetAllPath_Full( ) );
                node.id = exportName;
                _root.Add( node );
                table_root.Add( node, item );
            }
            BuildID( START_NODE_ID );
            //if ( _action != null && _action.filelist_postprocessing != null ) {
            //    for ( int i = 0; i < _exporters.Length; i++ ) {
            //        _action.filelist_postprocessing.Invoke( _exporters[i], i );
            //    }
            //}

            return new TreeViewItem { id = ROOT_ID, depth = -1 };
        }
        protected override IList<TreeViewItem> BuildRows( TreeViewItem root ) {
            var rows = GetRows( );
            if ( rows == null ) {
                rows = new List<TreeViewItem>( );
            } else {
                rows.Clear( );
            }

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
            var node = table_id_node[args.item.id];
            Texture icon;
            string label;
            if ( node.parent == _root ) {
                var exporter = table_root[node];
                label = exporter.ExportFileName;
                icon = IconCache.UnityLogoIcon;
            } else {
                if ( viewFullPath ) {
                    label = path;
                } else {
                    label = Path.GetFileName( path );
                }
                var result = ExporterUtils.TryGetIcon( path, out icon );
                switch ( result ) {
                    case ExporterUtils.GetIconResult.ExistsFile:
                        break;
                    case ExporterUtils.GetIconResult.ExistsFolder:
                        GUI.contentColor = temp_contentColor * 0.85f;
                        break;
                    default:
                    case ExporterUtils.GetIconResult.NotExistsFile:
                    case ExporterUtils.GetIconResult.NotExistsFolder:
                        label = ExporterTexts.t_ExportLog_NotFoundPathPrefix + label;
                        GUI.contentColor = temp_contentColor + ( Color.red * 0.2f );
                        break;
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
