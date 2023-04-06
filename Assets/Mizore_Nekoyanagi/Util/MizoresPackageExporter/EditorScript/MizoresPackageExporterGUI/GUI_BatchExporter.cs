using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterConsts_Editor;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor
{
#if UNITY_EDITOR
    public static class GUI_BatchExporter
    {
        public static void Draw( MizoresPackageExporterEditor ed, MizoresPackageExporter t, MizoresPackageExporter[] targetlist ) {
            string foldoutLabel;
            if ( t.batchExportMode == BatchExportMode.None ) {
                foldoutLabel = ExporterTexts.t_FoldoutBatchExportDisabled;
            } else {
                foldoutLabel = ExporterTexts.t_FoldoutBatchExportEnabled;
            }
            if ( !ExporterUtils.EditorPrefFoldout(
    Const.EDITOR_PREF_FOLDOUT_BATCHEXPORT, foldoutLabel ) ) {
                return;
            }
            bool multiple = targetlist.Length > 1;
            // var samevalue_in_all_mode = targetlist.All( v => t.batchExportMode == v.batchExportMode );

            EditorGUI.BeginChangeCheck( );
            using ( new EditorGUILayout.HorizontalScope( ) ) {
                ExporterUtils.Indent( 1 );
                t.batchExportMode = (BatchExportMode)EditorGUILayout.EnumPopup( ExporterTexts.t_BatchExportMode, t.batchExportMode );
            }
            if ( EditorGUI.EndChangeCheck( ) ) {
                t.UpdateBatchExportKeys( );
                EditorUtility.SetDirty( t );
            }

            switch ( t.batchExportMode ) {
                default:
                case BatchExportMode.None:
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
                                    t.UpdateBatchExportKeys( );
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
                                    t.UpdateBatchExportKeys( );
                                    EditorUtility.SetDirty( item );
                                }
                            }
                            EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 10 ) );
                            if ( GUILayout.Button( "-", GUILayout.Width( 15 ) ) ) {
                                foreach ( var item in targetlist ) {
                                    ExporterUtils.ResizeList( item.batchExportTexts, Mathf.Max( i + 1, item.batchExportTexts.Count ) );
                                    item.batchExportTexts.RemoveAt( i );
                                    texts_count = MinMax.Create( targetlist, v => v.batchExportTexts.Count );
                                    t.UpdateBatchExportKeys( );
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
                                t.UpdateBatchExportKeys( );
                                EditorUtility.SetDirty( item );
                            }
                        }
                    }
                    break;
                }
                case BatchExportMode.Folders: {
                    EditorGUI.BeginChangeCheck( );
                    using ( new EditorGUILayout.HorizontalScope( ) ) {
                        ExporterUtils.Indent( 1 );
                        EditorGUILayout.LabelField( "Folder", GUILayout.Width( 60 ) );
                        PackagePrefsElementInspector.Draw<DefaultAsset>( t.batchExportFolderRoot );
                    }
                    using ( new EditorGUILayout.HorizontalScope( ) ) {
                        ExporterUtils.Indent( 1 );
                        t.batchExportFolderRegex = EditorGUILayout.TextField( ExporterTexts.t_BatchExportRegex, t.batchExportFolderRegex );
                    }
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        t.UpdateBatchExportKeys( );
                        EditorUtility.SetDirty( t );
                    }
                    try {
                        Regex.Match( string.Empty, t.batchExportFolderRegex );
                    } catch ( System.ArgumentException e ) {
                        using ( new EditorGUILayout.HorizontalScope( ) ) {
                            ExporterUtils.Indent( 1 );
                            EditorGUILayout.HelpBox( "Regex Error:\n" + e.Message, MessageType.Error );
                        }
                    }
                    break;
                }
                case BatchExportMode.ListFile:
                    EditorGUI.BeginChangeCheck( );
                    using ( new EditorGUILayout.HorizontalScope( ) ) {
                        ExporterUtils.Indent( 1 );
                        EditorGUILayout.LabelField( "File", GUILayout.Width( 60 ) );
                        PackagePrefsElementInspector.Draw<TextAsset>( t.batchExportListFile );
                    }
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        t.UpdateBatchExportKeys( );
                        EditorUtility.SetDirty( t );
                    }
                    break;
            }
            switch ( t.batchExportMode ) {
                case BatchExportMode.Texts:
                case BatchExportMode.Folders:
                case BatchExportMode.ListFile:
                    var list = t.BatchExportKeysConverted;
                    for ( int i = 0; i < list.Length; i++ ) {
                        using ( new EditorGUILayout.HorizontalScope( ) ) {
                            ExporterUtils.Indent( 2 );
                            EditorGUILayout.LabelField( i.ToString( ), GUILayout.Width( 30 ) );
                            var item = list[i];
                            EditorGUILayout.LabelField( item );
                        }
                    }
                    break;
            }
        }
    }
#endif
}