#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.FileList {
    public class FileListTreeView : TreeView {
        FileListNode _root;
        Dictionary<int, FileListNode> table_id_node = new Dictionary<int, FileListNode>( );
        Dictionary<FileListNode, int> table_node_id = new Dictionary<FileListNode, int>( );
        public FileListNode GetNode( int id ) {
            return table_id_node[id];
        }
        /// <summary>
        /// エクスポート対象から除外するパス
        /// </summary>
        public HashSet<string> ignorePaths = new HashSet<string>();
        public bool viewFullPath;
        public bool viewExcludeFiles;
        public bool viewReferencedFiles;
        public bool hierarchyView;

        public bool HasExcludeFile { private set; get; }
        public bool HasReferencedFile { private set; get; }

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
            HasExcludeFile = false;
            HasReferencedFile = false;

            AddChildrenRecursive( _root, root, rows );

            SetupDepthsFromParentsAndChildren( root );
            return rows;
        }

        private void AddChildrenRecursive( FileListNode node, TreeViewItem item, IList<TreeViewItem> rows ) {
            foreach ( var child in node.childrenTable.Values ) {
                switch ( child.type ) {
                    case NodeType.Excludes:
                        HasExcludeFile = true;
                        if ( !viewExcludeFiles ) {
                            continue;
                        }
                        break;
                    case NodeType.References:
                        HasReferencedFile = true;
                        if ( !viewReferencedFiles ) {
                            continue;
                        }
                        break;
                }
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
            if ( !hierarchyView && node.type == NodeType.Excludes ) {
                subLabelColor = Color.white * 0.7f;
                GUI.contentColor = temp_contentColor * 0.8f;
            }
            if ( isRoot ) {
                icon = IconCache.UnityLogoIcon;
            } else if ( path[0] == '[' ) {
                icon = node.icon;
                switch ( path ) {
                    case ExporterConsts.PATH_PREFIX_NOTFOUND:
                        path = ExporterTexts.FileListCategoryNotFound;
                        break;
                    case ExporterConsts.PATH_PREFIX_REFERENCED:
                        path = ExporterTexts.FileListCategoryReferences;
                        break;
                    case ExporterConsts.PATH_PREFIX_EXCLUDES:
                        path = ExporterTexts.FileListCategoryExcludes;
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
                        subLabel = ExporterTexts.FileListNotFoundPathPrefix;
                        subLabelColor = new Color( 1, 0.25f, 0.25f );
                        break;
                    case NodeType.Excludes:
                        if ( node.ChildCount == 0 ) {
                            subLabel = ExporterTexts.FileListExcludesPathPrefix;
                        }
                        break;
                    case NodeType.References:
                        if ( node.args != null && node.args.Count != 0 ) {
                            subLabel = ExporterTexts.FileListReferencesPathPrefix;
                        }
                        break;
                }
            }

            Rect rect = args.rowRect;
            rect.x += GetContentIndent( args.item );
            if ( isRoot ) {
                rect.width = 20;
                EditorGUI.BeginChangeCheck( );
                bool export = EditorGUI.Toggle( rect, !ignorePaths.Contains(path) );
                if (EditorGUI.EndChangeCheck()) {
                    if ( export ) {
                        ignorePaths.Remove(path);
                    } else {
                        ignorePaths.Add(path);
                    }
                }
            } else {
                rect.width = 0;
            }

            rect.x = rect.xMax;
            rect.width = rowHeight - ICON_MARGIN;
            rect.y += ICON_MARGIN_HALF;
            rect.height -= ICON_MARGIN;
            if ( icon == null ) {
                icon = IconCache.HelpIcon;
            }
            GUI.DrawTexture( rect, icon );
            rect.y -= ICON_MARGIN_HALF;
            rect.height += ICON_MARGIN;

            if ( subLabel != null ) {
                var c = GUI.contentColor;
                GUI.contentColor = subLabelColor;
                rect.x = rect.xMax;
                rect = DrawLabel( rect, subLabel, _boldStyle );
                GUI.contentColor = c;
            }

            if ( viewFullPath || isRoot || args.selected || node.type == NodeType.NotFound ) {
                var c = GUI.contentColor;
                if ( !isRoot ) {
                    GUI.contentColor = GUI.contentColor * 0.9f;
                }
                var directoryName = Path.GetDirectoryName( path );
                if ( directoryName.Length != 0 ) {
                    directoryName = directoryName + "\\";
                    rect.x = rect.xMax;
                    rect = DrawLabel( rect, directoryName, _style );
                }
                GUI.contentColor = c;
            }

            rect.x = rect.xMax;
            DrawLabel( rect, Path.GetFileName( path ), _style );

            GUI.contentColor = temp_contentColor;
        }
        static Rect DrawLabel( Rect rect, string label, GUIStyle style ) {
            Rect result = rect;
            result.width = style.CalcSize( new GUIContent( label ) ).x;
            EditorGUI.LabelField( result, label, style );
            return result;
        }
    }
}
#endif
