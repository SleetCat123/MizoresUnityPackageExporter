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

            // 追加コピーパス
            EditorGUILayout.Space();
            EditorGUILayout.LabelField( ExporterTexts.PostExportAdditionalCopyPaths, EditorStyles.boldLabel );
            if ( multiple ) {
                EditorGUILayout.HelpBox( ExporterTexts.EditOnlySingle( ExporterTexts.PostExportAdditionalCopyPaths ), MessageType.Info );
            } else {
                DrawAdditionalCopyPaths( t );
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

                if ( !t.organizeInFolder ) {
                    EditorGUILayout.HelpBox( ExporterTexts.PostExportZipRequiresOrganize, MessageType.Warning );
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

                    // ソースパス
                    EditorGUI.BeginChangeCheck();
                    var sourceObj = item.sourcePath?.GetObject( t, string.Empty );
                    var newSourceObj = EditorGUILayout.ObjectField(
                        new GUIContent( ExporterTexts.PostExportSourcePath ),
                        sourceObj,
                        typeof( Object ),
                        false
                    );
                    if ( EditorGUI.EndChangeCheck() ) {
                        Undo.RecordObject( t, "Change SourcePath" );
                        if ( item.sourcePath == null ) {
                            item.sourcePath = new ObjectRefElement();
                        }
                        item.sourcePath.SetPathAutoDetect( t, newSourceObj );
                        EditorUtility.SetDirty( t );
                    }

                    // パス文字列表示
                    if ( item.sourcePath != null && !string.IsNullOrEmpty( item.sourcePath.Path ) ) {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.LabelField( item.sourcePath.Path, EditorStyles.miniLabel );
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
