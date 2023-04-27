using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor
{
#if UNITY_EDITOR
    public static class GUI_BatchExporter
    {
        static void Main( MizoresPackageExporter t, MizoresPackageExporter[] targetlist, bool samevalue_in_all_mode ) {
            bool multiple = targetlist.Length > 1;

            EditorGUI.BeginChangeCheck( );
            EditorGUI.showMixedValue = !samevalue_in_all_mode;
            using ( new EditorGUILayout.HorizontalScope( ) ) {
                ExporterUtils.Indent( 1 );
                t.batchExportMode = (BatchExportMode)EditorGUILayout.EnumPopup( ExporterTexts.BatchExportMode, t.batchExportMode );
            }
            EditorGUI.showMixedValue = false;
            if ( EditorGUI.EndChangeCheck( ) ) {
                var mode = t.batchExportMode;
                foreach ( var item in targetlist ) {
                    item.batchExportMode = mode;
                    item.UpdateBatchExportKeys( );
                    EditorUtility.SetDirty( item );
                }
            }

            if ( samevalue_in_all_mode ) {
                switch ( t.batchExportMode ) {
                    default:
                    case BatchExportMode.Disable:
                        break;
                    case BatchExportMode.Texts: {
                        var texts_count = MinMax.Create( targetlist, v => v.batchExportTexts.Count );
                        for ( int i = 0; i < texts_count.max; i++ ) {
                            using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                                // 全てのオブジェクトの値が同じか
                                bool samevalue_in_all = true;
                                if ( multiple ) {
                                    samevalue_in_all = i < texts_count.min && targetlist.All( v => t.batchExportTexts[i] == v.batchExportTexts[i] );
                                }

                                ExporterUtils.Indent( 2 );
                                if ( samevalue_in_all ) {
                                    EditorGUILayout.LabelField( i.ToString( ), GUILayout.Width( 30 ) );
                                } else {
                                    // 一部オブジェクトの値が異なっていたらTextFieldの左に?を表示
                                    ExporterUtils.DiffLabel( );
                                }

                                EditorGUI.BeginChangeCheck( );
                                Rect textrect = EditorGUILayout.GetControlRect( );
                                string path;
                                if ( samevalue_in_all ) {
                                    path = EditorGUI.TextField( textrect, t.batchExportTexts[i] );
                                } else {
                                    EditorGUI.showMixedValue = true;
                                    path = EditorGUI.TextField( textrect, string.Empty );
                                    EditorGUI.showMixedValue = false;
                                }

                                if ( EditorGUI.EndChangeCheck( ) ) {
                                    foreach ( var item in targetlist ) {
                                        ExporterUtils.ResizeList( item.batchExportTexts, Mathf.Max( i + 1, item.batchExportTexts.Count ) );
                                        item.batchExportTexts[i] = path;
                                        texts_count = MinMax.Create( targetlist, v => v.batchExportTexts.Count );
                                        item.UpdateBatchExportKeys( );
                                        EditorUtility.SetDirty( item );
                                    }
                                }

                                // Button
                                int index_after = ExporterUtils.UpDownButton( i, texts_count.max );
                                if ( i != index_after ) {
                                    foreach ( var item in targetlist ) {
                                        if ( item.batchExportTexts.Count <= index_after ) {
                                            ExporterUtils.ResizeList( item.batchExportTexts, index_after + 1 );
                                        }
                                        item.batchExportTexts.Swap( i, index_after );
                                        item.UpdateBatchExportKeys( );
                                        EditorUtility.SetDirty( item );
                                    }
                                }
                                EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 10 ) );
                                if ( GUILayout.Button( "-", GUILayout.Width( 15 ) ) ) {
                                    foreach ( var item in targetlist ) {
                                        ExporterUtils.ResizeList( item.batchExportTexts, Mathf.Max( i + 1, item.batchExportTexts.Count ) );
                                        item.batchExportTexts.RemoveAt( i );
                                        texts_count = MinMax.Create( targetlist, v => v.batchExportTexts.Count );
                                        item.UpdateBatchExportKeys( );
                                        EditorUtility.SetDirty( item );
                                    }
                                    i--;
                                }
                            }
                        }
                        using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                            ExporterUtils.Indent( 1 );
                            if ( GUILayout.Button( "+", GUILayout.Width( 60 ) ) ) {
                                foreach ( var item in targetlist ) {
                                    ExporterUtils.ResizeList( item.batchExportTexts, texts_count.max + 1, ( ) => string.Empty );
                                    item.UpdateBatchExportKeys( );
                                    EditorUtility.SetDirty( item );
                                }
                            }
                        }
                        break;
                    }
                    case BatchExportMode.Folders: {
                        var samevalue_in_all_obj = targetlist.All( v => t.batchExportFolderRoot.Object == v.batchExportFolderRoot.Object );

                        if ( !samevalue_in_all_obj ) {
                            ExporterUtils.DiffLabel( );
                        }
                        EditorGUI.showMixedValue = !samevalue_in_all_obj;
                        EditorGUI.BeginChangeCheck( );
                        using ( new EditorGUILayout.HorizontalScope( ) ) {
                            ExporterUtils.Indent( 1 );
                            EditorGUILayout.LabelField( "Folder", GUILayout.Width( 60 ) );
                            PackagePrefsElementInspector.Draw<DefaultAsset>( t.batchExportFolderRoot );
                        }
                        EditorGUI.showMixedValue = false;
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            var obj = t.batchExportFolderRoot.Object;
                            foreach ( var item in targetlist ) {
                                item.batchExportFolderRoot.Object = obj;
                                item.UpdateBatchExportKeys( );
                                EditorUtility.SetDirty( item );
                            }
                        }

                        var samevalue_in_all_regex = targetlist.All( v => t.batchExportFolderRegex == v.batchExportFolderRegex );
                        EditorGUI.BeginChangeCheck( );
                        EditorGUI.showMixedValue = !samevalue_in_all_regex;
                        using ( new EditorGUILayout.HorizontalScope( ) ) {
                            ExporterUtils.Indent( 1 );
                            t.batchExportFolderRegex = EditorGUILayout.TextField( ExporterTexts.BatchExportRegex, t.batchExportFolderRegex );
                        }
                        EditorGUI.showMixedValue = false;
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            string regex = t.batchExportFolderRegex;
                            foreach ( var item in targetlist ) {
                                item.batchExportFolderRegex = regex;
                                item.UpdateBatchExportKeys( );
                                EditorUtility.SetDirty( item );
                            }
                        }
                        break;
                    }
                    case BatchExportMode.ListFile: {
                        var samevalue_in_all_obj = targetlist.All( v => t.batchExportListFile.Object == v.batchExportListFile.Object );

                        if ( !samevalue_in_all_obj ) {
                            ExporterUtils.DiffLabel( );
                        }
                        EditorGUI.showMixedValue = !samevalue_in_all_obj;
                        EditorGUI.BeginChangeCheck( );
                        using ( new EditorGUILayout.HorizontalScope( ) ) {
                            ExporterUtils.Indent( 1 );
                            EditorGUILayout.LabelField( "File", GUILayout.Width( 60 ) );
                            PackagePrefsElementInspector.Draw<TextAsset>( t.batchExportListFile );
                        }
                        EditorGUI.showMixedValue = false;
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            var obj = t.batchExportListFile.Object;
                            foreach ( var item in targetlist ) {
                                item.batchExportListFile.Object = obj;
                                item.UpdateBatchExportKeys( );
                                EditorUtility.SetDirty( item );
                            }
                        }
                        break;
                    }
                }
            }
        }
        static void Preview( MizoresPackageExporter[] targetlist ) {
            bool multiple = targetlist.Length > 1;
            bool first = true;
            foreach ( var item in targetlist ) {
                if ( item.batchExportMode == BatchExportMode.Disable ) {
                    continue;
                }
                if ( first == false ) {
                    EditorGUILayout.Separator( );
                }
                first = false;
                if ( multiple ) {
                    using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                        GUI.enabled = false;
                        ExporterUtils.Indent( 1 );
                        EditorGUILayout.ObjectField( item, typeof( MizoresPackageExporter ), false );
                        GUI.enabled = true;
                    }
                }
                var list = item.BatchExportKeysConverted;
                for ( int i = 0; i < list.Length; i++ ) {
                    using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                        if ( multiple ) {
                            ExporterUtils.Indent( 2 );
                        } else {
                            ExporterUtils.Indent( 1 );
                        }
                        EditorGUILayout.LabelField( i.ToString( ), GUILayout.Width( 30 ) );
                        EditorGUILayout.LabelField( list[i] );
                    }
                }
            }
        }
        public static void Draw( MizoresPackageExporterEditor ed, MizoresPackageExporter t, MizoresPackageExporter[] targetlist ) {
            var samevalue_in_all_mode = targetlist.All( v => t.batchExportMode == v.batchExportMode );
            string foldoutLabel;
            if ( samevalue_in_all_mode ) {
                if ( t.batchExportMode == BatchExportMode.Disable ) {
                    foldoutLabel = ExporterTexts.FoldoutBatchExportDisabled;
                } else {
                    foldoutLabel = ExporterTexts.FoldoutBatchExportEnabled;
                }
            } else {
                foldoutLabel = ExporterTexts.FoldoutBatchExportEnabled;
            }
            if ( ExporterUtils.EditorPrefFoldout(
    ExporterEditorPrefs.FOLDOUT_BATCHEXPORT, foldoutLabel ) ) {
                Main( t, targetlist, samevalue_in_all_mode );
                Preview( targetlist );
            }

            foreach ( var item in targetlist ) {
                if ( item.batchExportMode == BatchExportMode.Folders ) {
                    try {
                        Regex.Match( string.Empty, item.batchExportFolderRegex );
                    } catch ( System.ArgumentException e ) {
                        using ( new EditorGUILayout.HorizontalScope( ) ) {
                            var error = string.Format( ExporterTexts.BatchExportRegexError, item.name, e.Message );
                            EditorGUILayout.HelpBox( error, MessageType.Error );
                        }
                    }
                }
                if ( item.batchExportMode != BatchExportMode.Disable && !item.packageNameSettings.packageName.Contains( ExporterConsts_Keys.KEY_BATCH_EXPORTER ) ) {
                    var error = string.Format( ExporterTexts.BatchExportNoTagError, item.name, ExporterConsts_Keys.KEY_BATCH_EXPORTER );
                    EditorGUILayout.HelpBox( error, MessageType.Error );
                }
            }
        }
    }
#endif
}