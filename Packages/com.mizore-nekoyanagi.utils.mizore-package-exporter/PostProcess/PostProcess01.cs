using MizoreNekoyanagi.PublishUtil.PackageExporter;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MizoreNekoyanagi.Private.ExportPackage {
    public class PostProcess01 : IExportPostProcess {
        public bool zip = true;
        public void OnExported( string exporterPath, string packagePath, FilePathList list ) {
            var paths = list.paths;

            Debug.Log( "!!! OnExported: " + packagePath );
            var dir = Path.GetDirectoryName( packagePath );
            Debug.Log( "Directory: " + dir );
            var packageName = Path.GetFileNameWithoutExtension( packagePath );
            var folderPath = Path.Combine( dir, packageName );
            if ( Directory.Exists( folderPath ) ) {
                // 同名のフォルダがある場合は適当に名前を変える
                var old = folderPath + "_old";
                int i = 0;
                while ( Directory.Exists( old ) ) {
                    old = folderPath + "_old" + i;
                    i++;
                }
                Directory.Move( folderPath, old );
            }
            Directory.CreateDirectory( folderPath );
            // packageをフォルダに移動
            File.Move( packagePath, Path.Combine( folderPath, Path.GetFileName( packagePath ) ) );

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
                File.Copy( fbx, Path.Combine( fbxDir, Path.GetFileName( fbx ) ) );
            }

            // packageexporterと同じパスにreadme.txtがあったらコピー
            var readmePath = Path.Combine( exporterPath, "readme.txt" );
            if ( File.Exists( readmePath ) ) {
                Debug.Log( "Copy readme: " + readmePath );
                File.Copy( readmePath, Path.Combine( folderPath, "readme.txt" ) );
            }

            // packageexporterと同じパスにreadmeフォルダがあったら、その中身をコピー
            var releaseFolderPath = Path.Combine( exporterPath, "release" );
            if ( Directory.Exists( releaseFolderPath ) ) {
                Debug.Log( "Copy release folder: " + releaseFolderPath );
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
                }
            }

            // packageexporterの1つ上の階層に汎用規約フォルダがあったら、その中身をコピー
            var licenseFolderPath = Path.Combine( Path.GetDirectoryName( exporterPath ), "汎用規約" );
            if ( Directory.Exists( licenseFolderPath ) ) {
                Debug.Log( "Copy license: " + licenseFolderPath );
                var licenseFiles = Directory.GetFiles( licenseFolderPath );
                foreach ( var license in licenseFiles ) {
                    // .metaファイルはコピーしない
                    if ( Path.GetExtension( license ) == ".meta" ) {
                        continue;
                    }
                    File.Copy( license, Path.Combine( folderPath, Path.GetFileName( license ) ) );
                }
            }
        }
    }
}