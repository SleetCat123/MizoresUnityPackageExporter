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
                    var list = kvp.Value;
                    if ( root.Contains( exportPath ) ) {
                        Debug.Log( "skip: " + exportPath );
                        //_action?.filelist_postprocessing?.Invoke( item, i );
                        continue;
                    }
                    Debug.Log( exportPath );
                    var node = FileList.FileListNode.CreateList( list );
                    node.id = exportPath;
                    node.path = exportPath;
                    root.Add( node );
                }
            }
            return root;
        }
    }
}
