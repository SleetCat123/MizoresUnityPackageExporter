using MizoreNekoyanagi.PublishUtil.PackageExporter;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;

namespace MizoreNekoyanagi.Private.ExportPackage {
    public class PostProcess01 : IExportPostProcess {
        [Tooltip( "エクスポート対象のfbxファイルをfbxフォルダにコピーするか" )]
        public bool copyFbx = true;

        [Space]
        public bool createZip = true;
        public System.IO.Compression.CompressionLevel compressionLevel = System.IO.Compression.CompressionLevel.Optimal;
        public string zipFolderName = "_zip";

        [Space]
        [Tooltip( "PackageExporterと同じパスにあるフォルダ" )]
        public string releaseFolderName = "_release";
        [Tooltip( "PackageExporterの1つ上の階層にあるフォルダ" )]
        public string commonFolderName = "_common";
        public void OnExported( string exporterPath, string packagePath, FilePathList list, ExporterEditorLogs logs ) {
            var paths = list.paths;

            Debug.Log( "!!! OnExported: " + packagePath );
            logs.Add( "!!! OnExported: " + packagePath );
            var dir = Path.GetDirectoryName( packagePath );
            Debug.Log( "Directory: " + dir );
            logs.Add( "Directory: " + dir );
            var packageName = Path.GetFileNameWithoutExtension( packagePath );
            var folderPath = Path.Combine( dir, packageName );
            if ( Directory.Exists( folderPath ) ) {
                // 同名のフォルダがある場合は適当に名前を変える
                var lastWriteTime = Directory.GetLastWriteTime( folderPath );
                var timeStr = lastWriteTime.ToString( "yyyyMMdd_HHmmss" );
                var old = folderPath + "_" + timeStr;
                int i = 0;
                while ( Directory.Exists( old ) ) {
                    old = folderPath + "_" + timeStr + "_" + i;
                    i++;
                }
                Directory.Move( folderPath, old );
                Debug.Log( "Rename old folder: " + old );
                logs.Add( "Rename old folder: " + old );
            }
            Directory.CreateDirectory( folderPath );
            // packageをフォルダに移動
            File.Move( packagePath, Path.Combine( folderPath, Path.GetFileName( packagePath ) ) );

            if ( copyFbx ) {
                // 出力されるfbxをfbxフォルダにコピー
                var fbxFiles = paths.Where( v => Path.GetExtension( v ) == ".fbx" );
                var fbxDir = Path.Combine( folderPath, "fbx" );
                if ( fbxFiles.Any( ) ) {
                    if ( !Directory.Exists( fbxDir ) ) {
                        Directory.CreateDirectory( fbxDir );
                    }
                }
                foreach ( var fbx in fbxFiles ) {
                    Debug.Log( "Copy fbx: " + fbx );
                    logs.Add( "Copy fbx: " + fbx );
                    File.Copy( fbx, Path.Combine( fbxDir, Path.GetFileName( fbx ) ) );
                }
            }

            if ( !string.IsNullOrEmpty( releaseFolderName ) ) {
                // packageexporterと同じパスにreleaseFolderNameフォルダがあったら、その中身をコピー
                var releaseFolderPath = Path.Combine( exporterPath,releaseFolderName );
                if ( Directory.Exists( releaseFolderPath ) ) {
                    Debug.Log( "Copy release folder: " + releaseFolderPath );
                    logs.Add( "Copy release folder: " + releaseFolderPath );
                    var files = Directory.GetFiles( releaseFolderPath, "*", SearchOption.AllDirectories );
                    foreach ( var file in files ) {
                        // .metaファイルはコピーしない
                        if ( Path.GetExtension( file ) == ".meta" ) {
                            continue;
                        }
                        // フォルダ構造を維持してコピー
                        var relativePath = file.Substring( releaseFolderPath.Length + 1 );
                        var destPath = Path.Combine( folderPath, relativePath );
                        Directory.CreateDirectory( Path.GetDirectoryName( destPath ) );
                        File.Copy( file, destPath );
                        Debug.Log( "Copy release file: " + file );
                        logs.Add( "Copy release file: " + file );
                    }
                }
            }

            if ( !string.IsNullOrEmpty( commonFolderName ) ) {
                // packageexporterの1つ上の階層にcommonFolderNameフォルダがあったら、その中身をコピー
                var commonFolderPath = Path.Combine( Path.GetDirectoryName( exporterPath ),commonFolderName );
                if ( Directory.Exists( commonFolderPath ) ) {
                    Debug.Log( "Copy common folder: " + commonFolderPath );
                    logs.Add( "Copy common folder: " + commonFolderPath );
                    var files = Directory.GetFiles( commonFolderPath );
                    foreach ( var license in files ) {
                        // .metaファイルはコピーしない
                        if ( Path.GetExtension( license ) == ".meta" ) {
                            continue;
                        }
                        File.Copy( license, Path.Combine( folderPath, Path.GetFileName( license ) ) );
                        Debug.Log( "Copy common file: " + license );
                        logs.Add( "Copy common file: " + license );
                    }
                }
            }

            if ( createZip ) {
                // zip化
                string zipDir;
                if ( string.IsNullOrEmpty( zipFolderName ) ) {
                    zipDir = dir;
                } else {
                    zipDir = Path.Combine( dir, zipFolderName );
                    if ( !Directory.Exists( zipDir ) ) {
                        Directory.CreateDirectory( zipDir );
                    }
                }
                var zipPath = Path.Combine( zipDir, packageName + ".zip" );
                if ( File.Exists( zipPath ) ) {
                    File.Delete( zipPath );
                }
                Debug.Log( "Create zip: " + zipPath );
                ZipFile.CreateFromDirectory( folderPath, zipPath, compressionLevel, false );
                Debug.Log( "zip created: " + zipPath );
                logs.Add( "zip created: " + zipPath );
            }
        }
    }
}