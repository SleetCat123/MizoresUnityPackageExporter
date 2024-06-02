using MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.FileList {
    public class CreateFileList {
        public class FileListData {
            public FileListNode rootNode;
            public List<string> packages;

            public FileListData( FileListNode rootNode, List<string> packages ) {
                this.rootNode = rootNode;
                this.packages = packages;
            }
        }
        public static async Task Create( MizoresPackageExporter[] exporters, IEnumerable<string> filter, Action<FileListData> callback ) {
            try {
                var root = new FileListNode( );
                var packages = new List<string>();
                // progressbar
                for ( int i = 0; i < exporters.Length; i++ ) {
                    var item = exporters[i];
                    MizoresPackageExporter.LockEditor = true;
                    Dictionary<string, FilePathList> table = null;
                    await item.GetAllPath_Batch( filter, ( t, max, currentPath, finished ) => {
                        var text = ExporterTexts.ProgressBarInfo_CreateFileList( item.name, currentPath );
                        var progress = t.Count / (float)max;
                        EditorUtility.DisplayProgressBar( ExporterTexts.AssetName, text, progress );
                        if ( finished ) {
                            table = t;
                        }
                    } );
                    foreach ( var kvp in table ) {
                        await Task.Delay( 1 );
                        string exportPath = kvp.Key;
                        if ( root.Contains( exportPath ) ) {
                            Debug.Log( "skip: " + exportPath );
                            //_action?.filelist_postprocessing?.Invoke( item, i );
                            continue;
                        }
                        packages.Add( exportPath );
                        ExporterUtils.DebugLog( exportPath );
                        var list = kvp.Value;

                        FileListNode node = new FileListNode( );
                        node.AddOrGetCategoryNode( NodeType.Default );
                        node.AddOrGetCategoryNode( NodeType.References );
                        node.AddOrGetCategoryNode( NodeType.Excludes );
                        foreach ( var path in list.paths ) {
                            node.Add( path, NodeType.Default );
                        }

                        var referencedPaths = list.referencedPaths;
                        foreach ( var path in list.excludePaths ) {
                            referencedPaths.Remove( path );
                        }
                        foreach ( var refkvp in referencedPaths ) {
                            var path = refkvp.Key;
                            var referenceFrom = refkvp.Value;
                            node.Add( path, NodeType.References, referenceFrom );
                        }

                        foreach ( var path in list.excludePaths ) {
                            node.Add( path, NodeType.Excludes );
                        }
                        node.id = exportPath;
                        node.path = exportPath;
                        root.Add( node );
                    }
                }
                callback?.Invoke( new FileListData( root, packages ) );
            } finally {
                MizoresPackageExporter.LockEditor = false;
                EditorUtility.ClearProgressBar( );
            }
        }
    }
}
