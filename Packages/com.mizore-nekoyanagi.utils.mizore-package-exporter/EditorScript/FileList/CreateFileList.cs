using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.FileList
{
    public class CreateFileList
    {
        public class FileListData {
            public FileListNode rootNode;
            public List<string> packages;

            public FileListData( FileListNode rootNode, List<string> packages ) {
                this.rootNode = rootNode;
                this.packages = packages;
            }
        }
        public static FileListData Create( MizoresPackageExporter[] exporters ) {
            var root = new FileListNode( );
            var packages = new List<string>();
            for ( int i = 0; i < exporters.Length; i++ ) {
                var item = exporters[i];
                var table = item.GetAllPath_Batch( );
                foreach ( var kvp in table ) {
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
            return new FileListData( root, packages);
        }
    }
}
