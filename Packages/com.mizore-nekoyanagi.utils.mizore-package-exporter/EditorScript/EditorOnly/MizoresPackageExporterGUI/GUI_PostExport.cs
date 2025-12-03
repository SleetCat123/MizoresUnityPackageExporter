#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor {
    public static class GUI_PostExport {
        public static void Draw( MizoresPackageExporterEditor ed, MizoresPackageExporter t, MizoresPackageExporter[] targetlist ) {
            MinMax copyPaths_count = MinMax.Create( targetlist, v => v.additionalCopyPaths.Count );
            if ( !CustomFoldout.EditorPrefFoldout(
                ExporterEditorPrefs.FOLDOUT_POST_EXPORT,
                ExporterTexts.FoldoutPostExport( copyPaths_count.ToString() )
                ) ) {
                return;
            }
            VerticalBoxScope.BeginVerticalBox();

            bool multiple = targetlist.Length > 1;

            // フォルダ整理
            var sameOrganize = targetlist.All( v => v.organizeInFolder == t.organizeInFolder );
            using ( new EditorGUILayout.HorizontalScope() ) {
                if ( !sameOrganize ) {
                    ExporterUtils.DiffLabel();
                    EditorGUI.showMixedValue = true;
                }
                EditorGUI.BeginChangeCheck();
                var organizeInFolder = EditorGUILayout.Toggle(
                    new GUIContent( ExporterTexts.PostExportOrganizeInFolder, ExporterTexts.PostExportOrganizeInFolderTooltip ),
                    t.organizeInFolder
                );
                if ( EditorGUI.EndChangeCheck() ) {
                    foreach ( var item in targetlist ) {
                        Undo.RecordObject( item, "Change OrganizeInFolder" );
                        item.organizeInFolder = organizeInFolder;
                        EditorUtility.SetDirty( item );
                    }
                }
                EditorGUI.showMixedValue = false;
            }

            // フォルダ整理が有効な時のみ表示
            if ( t.organizeInFolder ) {
                EditorGUI.indentLevel++;

                // フォルダ名
                var sameFolderName = targetlist.All( v => v.organizeFolderName == t.organizeFolderName );
                using ( new EditorGUILayout.HorizontalScope() ) {
                    if ( !sameFolderName ) {
                        ExporterUtils.DiffLabel();
                        EditorGUI.showMixedValue = true;
                    }
                    EditorGUI.BeginChangeCheck();
                    var folderName = EditorGUILayout.TextField(
                        new GUIContent( ExporterTexts.PostExportOrganizeFolderName, ExporterTexts.PostExportOrganizeFolderNameTooltip ),
                        t.organizeFolderName
                    );
                    if ( EditorGUI.EndChangeCheck() ) {
                        foreach ( var item in targetlist ) {
                            Undo.RecordObject( item, "Change OrganizeFolderName" );
                            item.organizeFolderName = folderName;
                            EditorUtility.SetDirty( item );
                        }
                    }
                    EditorGUI.showMixedValue = false;
                }

                // フォルダ名プレビュー
                if ( !string.IsNullOrEmpty( t.organizeFolderName ) ) {
                    EditorGUI.indentLevel++;
                    var preview = t.ConvertDynamicPath( t.organizeFolderName, string.Empty );
                    EditorGUILayout.LabelField( new GUIContent( preview, preview ), EditorStyles.miniLabel );
                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;

                // 追加コピーパス
                EditorGUILayout.Space();
                EditorGUILayout.LabelField( ExporterTexts.PostExportAdditionalCopyPaths, EditorStyles.boldLabel );
                if ( multiple ) {
                    EditorGUILayout.HelpBox( ExporterTexts.EditOnlySingle( ExporterTexts.PostExportAdditionalCopyPaths ), MessageType.Info );
                } else {
                    DrawAdditionalCopyPaths( t );
                }
            }

            // zip設定
            EditorGUILayout.Space();
            var sameCreateZip = targetlist.All( v => v.createZip == t.createZip );
            using ( new EditorGUILayout.HorizontalScope() ) {
                if ( !sameCreateZip ) {
                    ExporterUtils.DiffLabel();
                    EditorGUI.showMixedValue = true;
                }
                EditorGUI.BeginChangeCheck();
                var createZip = EditorGUILayout.Toggle(
                    new GUIContent( ExporterTexts.PostExportCreateZip, ExporterTexts.PostExportCreateZipTooltip ),
                    t.createZip
                );
                if ( EditorGUI.EndChangeCheck() ) {
                    foreach ( var item in targetlist ) {
                        Undo.RecordObject( item, "Change CreateZip" );
                        item.createZip = createZip;
                        EditorUtility.SetDirty( item );
                    }
                }
                EditorGUI.showMixedValue = false;
            }

            if ( t.createZip ) {
                EditorGUI.indentLevel++;

#if UNITY_2022_1_OR_NEWER
                // 圧縮レベル
                var sameCompression = targetlist.All( v => v.compressionLevel == t.compressionLevel );
                using ( new EditorGUILayout.HorizontalScope() ) {
                    if ( !sameCompression ) {
                        ExporterUtils.DiffLabel();
                        EditorGUI.showMixedValue = true;
                    }
                    EditorGUI.BeginChangeCheck();
                    var compressionLevel = ( System.IO.Compression.CompressionLevel )EditorGUILayout.EnumPopup(
                        new GUIContent( ExporterTexts.PostExportCompressionLevel ),
                        t.compressionLevel
                    );
                    if ( EditorGUI.EndChangeCheck() ) {
                        foreach ( var item in targetlist ) {
                            Undo.RecordObject( item, "Change CompressionLevel" );
                            item.compressionLevel = compressionLevel;
                            EditorUtility.SetDirty( item );
                        }
                    }
                    EditorGUI.showMixedValue = false;
                }
#else
                EditorGUILayout.HelpBox( ExporterTexts.PostExportZipNotSupported, MessageType.Warning );
#endif

                // zipフォルダ名
                var sameZipFolder = targetlist.All( v => v.zipFolderName == t.zipFolderName );
                using ( new EditorGUILayout.HorizontalScope() ) {
                    if ( !sameZipFolder ) {
                        ExporterUtils.DiffLabel();
                        EditorGUI.showMixedValue = true;
                    }
                    EditorGUI.BeginChangeCheck();
                    var zipFolderName = EditorGUILayout.TextField(
                        new GUIContent( ExporterTexts.PostExportZipFolderName, ExporterTexts.PostExportZipFolderNameTooltip ),
                        t.zipFolderName
                    );
                    if ( EditorGUI.EndChangeCheck() ) {
                        foreach ( var item in targetlist ) {
                            Undo.RecordObject( item, "Change ZipFolderName" );
                            item.zipFolderName = zipFolderName;
                            EditorUtility.SetDirty( item );
                        }
                    }
                    EditorGUI.showMixedValue = false;
                }

                EditorGUI.indentLevel--;
            }

            VerticalBoxScope.EndVerticalBox();
        }

        static void DrawAdditionalCopyPaths( MizoresPackageExporter t ) {
            var list = t.additionalCopyPaths;
            for ( int i = 0; i < list.Count; i++ ) {
                var item = list[i];
                if ( item == null ) {
                    item = new AdditionalCopyPath();
                    list[i] = item;
                }

                using ( new EditorGUILayout.VerticalScope( EditorStyles.helpBox ) ) {
                    using ( new EditorGUILayout.HorizontalScope() ) {
                        EditorGUILayout.LabelField( $"[{i}]", GUILayout.Width( 30 ) );

                        int index_after = GUIElement_Utils.UpDownButton( i, list.Count );
                        if ( i != index_after ) {
                            Undo.RecordObject( t, "Reorder AdditionalCopyPath" );
                            var temp = list[i];
                            list[i] = list[index_after];
                            list[index_after] = temp;
                            EditorUtility.SetDirty( t );
                        }

                        if ( GUILayout.Button( "-", GUILayout.Width( 20 ) ) ) {
                            Undo.RecordObject( t, "Remove AdditionalCopyPath" );
                            list.RemoveAt( i );
                            EditorUtility.SetDirty( t );
                            i--;
                            continue;
                        }
                    }

                    // ソースパス（PackagePrefsElementInspectorを使用して変数入力対応）
                    EditorGUILayout.LabelField( ExporterTexts.PostExportSourcePath );
                    if ( item.sourcePath == null ) {
                        item.sourcePath = new ObjectRefElement();
                    }
                    EditorGUI.BeginChangeCheck();
                    bool browse = PackagePrefsElementInspector.Draw<Object>( t, item.sourcePath );
                    if ( EditorGUI.EndChangeCheck() ) {
                        Undo.RecordObject( t, "Change SourcePath" );
                        EditorUtility.SetDirty( t );
                    }
                    if ( browse ) {
                        return;
                    }

                    // ソースパスプレビュー
                    if ( item.sourcePath != null && !string.IsNullOrEmpty( item.sourcePath.Path ) ) {
                        EditorGUI.indentLevel++;
                        var sourcePreview = item.GetConvertedSourcePath( t, string.Empty );
                        EditorGUILayout.LabelField( new GUIContent( sourcePreview, sourcePreview ), EditorStyles.miniLabel );
                        EditorGUI.indentLevel--;
                    }

                    // 出力先名
                    EditorGUI.BeginChangeCheck();
                    var destName = EditorGUILayout.TextField(
                        new GUIContent( ExporterTexts.PostExportDestName, ExporterTexts.PostExportDestNameTooltip ),
                        item.destName
                    );
                    if ( EditorGUI.EndChangeCheck() ) {
                        Undo.RecordObject( t, "Change DestName" );
                        item.destName = destName;
                        EditorUtility.SetDirty( t );
                    }

                    // 出力先名プレビュー
                    if ( !string.IsNullOrEmpty( item.destName ) ) {
                        EditorGUI.indentLevel++;
                        var destPreview = item.GetConvertedDestName( t, string.Empty );
                        if ( !string.IsNullOrEmpty( destPreview ) ) {
                            EditorGUILayout.LabelField( new GUIContent( "→ " + destPreview, destPreview ), EditorStyles.miniLabel );
                        }
                        EditorGUI.indentLevel--;
                    }
                }
            }

            // 追加ボタン
            if ( GUILayout.Button( "+", GUILayout.Width( 30 ) ) ) {
                Undo.RecordObject( t, "Add AdditionalCopyPath" );
                list.Add( new AdditionalCopyPath() );
                EditorUtility.SetDirty( t );
            }
        }
    }
}
#endif
