using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace MizoreNekoyanagi.PublishUtil.PackageExporter.FileList {
    public enum NodeType {
        Default, NotFound, Excludes, References
    }
    public class FileListNode {
        public FileListNode parent;
        public string id;
        public string path;
        public Dictionary<string, FileListNode> childrenTable = new Dictionary<string, FileListNode>( );
        public Texture icon;
        public ExporterUtils.GetIconResult iconResult;
        public NodeType type;
        public List<string> args;

        public int ChildCount { get => childrenTable.Count; }

        public bool Contains( string path ) {
            return childrenTable.ContainsKey( path );
        }
        public FileListNode AddOrGetCategoryNode( NodeType type ) {
            string prefix;
            switch ( type ) {
                default:
                case NodeType.Default:
                    prefix = ExporterConsts.PATH_PREFIX_EXPORT;
                    break;
                case NodeType.NotFound:
                    prefix = ExporterConsts.PATH_PREFIX_NOTFOUND;
                    break;
                case NodeType.References:
                    prefix = ExporterConsts.PATH_PREFIX_REFERENCED;
                    break;
                case NodeType.Excludes:
                    prefix = ExporterConsts.PATH_PREFIX_EXCLUDES;
                    break;
            }
            FileListNode categoryNode;
            if ( !childrenTable.TryGetValue( prefix, out categoryNode ) ) {
                categoryNode = new FileListNode( );
                categoryNode.path = prefix;
                categoryNode.id = prefix;
                categoryNode.iconResult = ExporterUtils.GetIconResult.Dummy;
                switch ( type ) {
                    default:
                    case NodeType.Default:
                        categoryNode.icon = IconCache.UnityLogoIcon;
                        break;
                    case NodeType.NotFound:
                        categoryNode.icon = IconCache.ErrorIcon;
                        break;
                    case NodeType.References:
                        categoryNode.icon = IconCache.AddIcon;
                        break;
                    case NodeType.Excludes:
                        categoryNode.icon = IconCache.RemoveIcon;
                        break;
                }
                categoryNode.type = type;
                Add( categoryNode );
            }
            return categoryNode;
        }
        public void Add( FileListNode node ) {
            node.parent = this;
            childrenTable.Add( node.id, node );
        }
        public void Add( string path, NodeType type, IEnumerable<string> args = null ) {
            FileListNode node = this;
            node.iconResult = ExporterUtils.TryGetIcon( path, out node.icon );
            if ( !node.iconResult.IsExists( ) ) {
                type = NodeType.NotFound;
            }
            node = AddOrGetCategoryNode( type );

            path = path.Replace( '\\', '/' );
            var splittedPath = path.Split( '/' );
            string tempPath = null;
            for ( int i = 0; i < splittedPath.Length; i++ ) {
                var filename = splittedPath[i];
                if ( tempPath == null ) {
                    tempPath = filename;
                } else {
                    tempPath = tempPath + "/" + filename;
                }
                FileListNode child;
                if ( !node.childrenTable.TryGetValue( filename, out child ) ) {
                    child = new FileListNode( );
                    child.path = string.Join( "/", tempPath );
                    child.id = filename;
                    child.iconResult = ExporterUtils.TryGetIcon( child.path, out child.icon );
                    child.type = type;
                    node.Add( child );
                }
                node = child;
            }
            if ( args != null ) {
                node.args = args.ToList( );
            }
        }
    }
}
