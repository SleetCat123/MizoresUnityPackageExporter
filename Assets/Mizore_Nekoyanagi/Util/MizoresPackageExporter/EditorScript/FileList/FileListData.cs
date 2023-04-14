using System.Collections.Generic;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.FileList
{
    public class CreateFileList
    {
        public static FileListNode Create( MizoresPackageExporter[] exporters ) {
            var root = new FileListNode( );
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
                    Debug.Log( exportPath );
                    var list = kvp.Value;

                    FileListNode node = new FileListNode( );
                    foreach ( var path in list.paths ) {
                        node.Add( path, NodeType.Default );
                    }
                    foreach ( var refkvp in list.referencedPaths ) {
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
            return root;
        }
    }
}
