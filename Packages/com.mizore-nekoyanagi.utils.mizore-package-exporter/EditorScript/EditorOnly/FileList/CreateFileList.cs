using MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor;
using System;
using System.Collections.Generic;
using System.IO;
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
                    ExporterUtils.DebugLog( item.name );
                    Dictionary<string, FilePathList> table = null;
                    await item.GetAllPath_Batch( filter, ( t, max, currentPath, finished ) => {
                        var text = ExporterTexts.ProgressBarInfo_CreateFileList( item.name, currentPath );
                        var progress = t.Count / (float)max;
                        EditorUtility.DisplayProgressBar( ExporterTexts.AssetName, text, progress );
                        if ( finished ) {
                            table = t;
                        }
                    } );
                    ExporterUtils.DebugLog( "GetAllPath_Batch finished" );
                    foreach ( var kvp in table ) {
                        await Task.Delay( 1 );
                        string exportPath = kvp.Key;
                        var list = kvp.Value;
                        if ( root.Contains( exportPath ) ) {
                            Debug.Log( "skip: " + exportPath );
                            //_action?.filelist_postprocessing?.Invoke( item, i );
                            continue;
                        }
                        packages.Add( exportPath );
                        ExporterUtils.DebugLog( exportPath );

                        FileListNode node = new FileListNode( );
                        node.AddOrGetCategoryNode( NodeType.Default );
                        node.AddOrGetCategoryNode( NodeType.References );
                        node.AddOrGetCategoryNode( NodeType.Excludes );
                        foreach ( var path in list.paths ) {
                            node.Add( path, NodeType.Default );
                        }

                        var referencedPaths = list.referencedPaths;
                        foreach ( var path in list.excludePaths ) {
                            // 除外リストにあるものは参照リストから削除
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

                        // 追加コピーパスの追加（フォルダの場合は中身を展開）
                        if ( list.additionalCopyPaths != null ) {
                            foreach ( var copyPath in list.additionalCopyPaths ) {
                                var sourcePath = copyPath.sourcePath;
                                var destName = copyPath.destName;

                                if ( File.Exists( sourcePath ) ) {
                                    // ファイルの場合
                                    string outputName;
                                    if ( string.IsNullOrEmpty( destName ) ) {
                                        outputName = Path.GetFileName( sourcePath );
                                    } else if ( destName.EndsWith( "/" ) || destName.EndsWith( "\\" ) ) {
                                        // フォルダ指定の場合は元のファイル名を追加
                                        outputName = destName + Path.GetFileName( sourcePath );
                                    } else {
                                        outputName = destName;
                                    }
                                    node.Add( sourcePath, NodeType.AdditionalCopy, new string[] { outputName } );
                                } else if ( Directory.Exists( sourcePath ) ) {
                                    // フォルダの場合は中身を展開
                                    var files = Directory.GetFiles( sourcePath, "*", SearchOption.AllDirectories );
                                    foreach ( var file in files ) {
                                        // .metaファイルはスキップ
                                        if ( Path.GetExtension( file ) == ".meta" ) {
                                            continue;
                                        }
                                        var relativePath = file.Substring( sourcePath.Length + 1 ).Replace( '\\', '/' );
                                        string outputPath;
                                        if ( !string.IsNullOrEmpty( destName ) ) {
                                            outputPath = destName + "/" + relativePath;
                                        } else {
                                            outputPath = relativePath;
                                        }
                                        node.Add( file, NodeType.AdditionalCopy, new string[] { outputPath } );
                                    }
                                } else {
                                    // 存在しない場合もそのまま追加（NotFoundとして表示される）
                                    var args = string.IsNullOrEmpty( destName ) ? null : new string[] { destName };
                                    node.Add( sourcePath, NodeType.AdditionalCopy, args );
                                }
                            }
                        }

                        node.id = exportPath;
                        node.path = exportPath;
                        root.Add( node );
                    }
                }
                callback?.Invoke( new FileListData( root, packages ) );
            } catch ( Exception e ) {
                Debug.LogError( e );
            } finally {
                MizoresPackageExporter.LockEditor = false;
                EditorUtility.ClearProgressBar( );
            }
        }
    }
}
