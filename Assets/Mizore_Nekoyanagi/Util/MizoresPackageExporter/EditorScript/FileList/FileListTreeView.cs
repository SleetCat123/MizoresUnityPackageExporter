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
        FileListNode _root;
        Dictionary<int, FileListNode> table_id_node = new Dictionary<int, FileListNode>( );
        Dictionary<FileListNode, int> table_node_id = new Dictionary<FileListNode, int>( );
        public FileListNode GetNode( int id ) {
            return table_id_node[id];
        }
        public bool viewFullPath;
        public bool hierarchyView;

        GUIStyle _style;
        GUIStyle _boldStyle;

        const int ROOT_ID = 0;
        const int START_NODE_ID = ROOT_ID + 1;
        const float ICON_MARGIN = 2;
        const float ICON_MARGIN_HALF = ICON_MARGIN * 0.5f;

        public FileListTreeView( TreeViewState treeViewState, FileListNode root ) : base( treeViewState ) {
            _root = root;
            useScrollView = true;
            int height = 20;
            rowHeight = height;

            _style = new GUIStyle( EditorStyles.label );
            _style.fontSize = 12;

            _boldStyle = new GUIStyle( EditorStyles.boldLabel );
            _boldStyle.fontSize = 12;
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
            BuildID( START_NODE_ID );
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
                bool hierarchyView = this.hierarchyView;
                if ( child.type == NodeType.NotFound && child.path != ExporterConsts.PATH_PREFIX_NOTFOUND ) {
                    hierarchyView = false;
                }
                if ( hierarchyView ) {
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
                } else {
                    TreeViewItem currentItem = item;
                    if ( child.ChildCount == 0 || child.parent == _root ) {
                        currentItem = new TreeViewItem { id = id, displayName = child.path };
                        item.AddChild( currentItem );
                        rows.Add( currentItem );
                    }
                    if ( child.ChildCount >= 1 ) {
                        if ( IsExpanded( id ) ) {
                            if ( child.parent == _root ) {
                                AddChildrenRecursive( child, currentItem, rows );
                            } else {
                                AddChildrenRecursive( child, item, rows );
                            }
                        } else {
                            currentItem.children = CreateChildListForCollapsedParent( );
                        }
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
            bool isRoot = node.parent == _root;
            string subLabel = null;
            Color subLabelColor = Color.white;
            if ( isRoot ) {
                icon = IconCache.UnityLogoIcon;
            } else if ( path[0] == '[' ) {
                icon = node.icon;
                switch ( path ) {
                    case ExporterConsts.PATH_PREFIX_NOTFOUND:
                        path = ExporterTexts.t_FileListCategoryNotFound;
                        break;
                    case ExporterConsts.PATH_PREFIX_REFERENCED:
                        path = ExporterTexts.t_FileListCategoryReferences;
                        break;
                    case ExporterConsts.PATH_PREFIX_EXCLUDES:
                        path = ExporterTexts.t_FileListCategoryExcludes;
                        break;
                }
            } else {
                icon = node.icon;
                switch ( node.iconResult ) {
                    case ExporterUtils.GetIconResult.ExistsFile:
                        break;
                    case ExporterUtils.GetIconResult.ExistsFolder:
                        GUI.contentColor = temp_contentColor * 0.9f;
                        break;
                    default:
                    case ExporterUtils.GetIconResult.NotExistsFile:
                    case ExporterUtils.GetIconResult.NotExistsFolder:
                        GUI.contentColor = temp_contentColor + ( Color.red * 0.2f );
                        break;
                }
                switch ( node.type ) {
                    case NodeType.NotFound:
                        subLabel = ExporterTexts.t_FileListNotFoundPathPrefix;
                        subLabelColor = new Color( 1, 0.25f, 0.25f );
                        break;
                    case NodeType.Excludes:
                        if ( node.ChildCount == 0 ) {
                            subLabel = ExporterTexts.t_FileListExcludesPathPrefix;
                        }
                        break;
                    case NodeType.References:
                        if ( node.args != null && node.args.Count != 0 ) {
                            subLabel = ExporterTexts.t_FileListReferencesPathPrefix;
                        }
                        break;
                }
            }

            Rect iconRect = args.rowRect;
            iconRect.x += GetContentIndent( args.item );
            iconRect.y += ICON_MARGIN_HALF;
            iconRect.width = rowHeight - ICON_MARGIN;
            iconRect.height -= ICON_MARGIN;
            if ( icon == null ) {
                icon = IconCache.HelpIcon;
            }
            GUI.DrawTexture( iconRect, icon );

            Rect spaceRect = args.rowRect;
            spaceRect.x = iconRect.x;
            spaceRect.width = iconRect.width + 2;

            Rect labelRect1 = spaceRect;
            if ( subLabel != null ) {
                var c = GUI.contentColor;
                GUI.contentColor = subLabelColor;
                labelRect1 = DrawLabel( labelRect1, subLabel, _boldStyle );
                GUI.contentColor = c;
            }

            Rect labelRect2 = labelRect1;
            if ( viewFullPath || isRoot || args.selected || node.type == NodeType.NotFound ) {
                var c = GUI.contentColor;
                if ( !isRoot ) {
                    GUI.contentColor = GUI.contentColor * 0.9f;
                }
                var directoryName = Path.GetDirectoryName( path );
                if ( directoryName.Length != 0 ) {
                    directoryName = directoryName + "\\";
                    labelRect2 = DrawLabel( labelRect2, directoryName, _style );
                }
                GUI.contentColor = c;
            }

            Rect labelRect3 = DrawLabel( labelRect2, Path.GetFileName( path ), _style );

            GUI.contentColor = temp_contentColor;
        }
        static Rect DrawLabel( Rect prevRect, string label, GUIStyle style ) {
            Rect result = prevRect;
            result.x = prevRect.x + prevRect.width;
            //if ( width > 0 ) {
            //    result.width = width;
            //} else {
            result.width = style.CalcSize( new GUIContent( label ) ).x;
            //}
            EditorGUI.LabelField( result, label, style );
            return result;
        }
    }
}
#endif
