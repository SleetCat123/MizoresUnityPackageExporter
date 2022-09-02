﻿using System;
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
        public FileListNode parent;
        public string id;
        public string path;
        public Dictionary<string, FileListNode> childrenTable = new Dictionary<string, FileListNode>( );

        public int ChildCount { get => childrenTable.Count; }

        public void Add( string id, FileListNode node ) {
            childrenTable.Add( node.id, node );
        }
        public void Add( string path ) {
            path = path.Replace( '\\', '/' );
            var splittedPath = path.Split( '/' );
            FileListNode node = this;
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
                    child.parent = node;
                    child.path = string.Join( "/", tempPath );
                    child.id = filename;
                    node.Add( filename, child );
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
    }
}
