using System.Linq;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor {
#if UNITY_EDITOR
    public static class GUI_BatchExporter {
        static MizoresPackageExporter selected;
        static string selectedKey;
        static void Main( MizoresPackageExporter t, MizoresPackageExporter[] targetlist, bool samevalue_in_all_mode ) {
            bool multiple = targetlist.Length > 1;

            EditorGUI.BeginChangeCheck( );
            EditorGUI.showMixedValue = !samevalue_in_all_mode;
            EditorGUI.indentLevel++;
            t.batchExportMode = ( BatchExportMode )EditorGUILayout.EnumPopup( ExporterTexts.BatchExportMode, t.batchExportMode );
            EditorGUI.indentLevel--;
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
                    case BatchExportMode.Single:
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

                                EditorGUI.indentLevel += 2;
                                if ( samevalue_in_all ) {
                                    EditorGUILayout.LabelField( i.ToString( ), GUILayout.Width( 30 ) );
                                } else {
                                    // 一部オブジェクトの値が異なっていたらTextFieldの左に?を表示
                                    ExporterUtils.DiffLabel( );
                                }
                                EditorGUI.indentLevel -= 2;

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
                        EditorGUI.indentLevel++;
                        if ( GUILayout.Button( "+", GUILayout.Width( 60 ) ) ) {
                            foreach ( var item in targetlist ) {
                                ExporterUtils.ResizeList( item.batchExportTexts, texts_count.max + 1, ( ) => string.Empty );
                                item.UpdateBatchExportKeys( );
                                EditorUtility.SetDirty( item );
                            }
                        }
                        EditorGUI.indentLevel--;
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
                            EditorGUI.indentLevel++;
                            EditorGUILayout.LabelField( ExporterTexts.BatchExportFolder, GUILayout.Width( 60 ) );
                            EditorGUI.indentLevel--;
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

                        var samevalue_in_all_foldermode = targetlist.All( v => t.batchExportFolderMode == v.batchExportFolderMode );
                        EditorGUI.BeginChangeCheck( );
                        EditorGUI.showMixedValue = !samevalue_in_all_foldermode;
                        EditorGUI.indentLevel++;
                        t.batchExportFolderMode = ( BatchExportFolderMode )EditorGUILayout.EnumPopup( ExporterTexts.BatchExportFolderMode, t.batchExportFolderMode );
                        EditorGUI.indentLevel--;
                        EditorGUI.showMixedValue = false;
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            var mode = t.batchExportFolderMode;
                            foreach ( var item in targetlist ) {
                                item.batchExportFolderMode = mode;
                                item.UpdateBatchExportKeys( );
                                EditorUtility.SetDirty( item );
                            }
                        }

                        var samevalue_in_all_regex = targetlist.All( v => t.batchExportFolderRegex == v.batchExportFolderRegex );
                        EditorGUI.BeginChangeCheck( );
                        EditorGUI.showMixedValue = !samevalue_in_all_regex;
                        EditorGUI.indentLevel++;
                        t.batchExportFolderRegex = EditorGUILayout.TextField( ExporterTexts.BatchExportRegex, t.batchExportFolderRegex );
                        EditorGUI.indentLevel--;
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
                        EditorGUI.indentLevel++;
                        EditorGUILayout.LabelField( ExporterTexts.BatchExportListFile, GUILayout.Width( 60 ) );
                        PackagePrefsElementInspector.Draw<TextAsset>( t.batchExportListFile );
                        EditorGUI.indentLevel--;
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
        static void DrawList( MizoresPackageExporter[] targetlist ) {
            var t = targetlist[0];
            bool multiple = targetlist.Length > 1;
            bool first = true;
            foreach ( var item in targetlist ) {
                if ( item.batchExportMode == BatchExportMode.Single ) {
                    continue;
                }
                if ( first ) {
                    EditorGUI.indentLevel++;
                    ExporterUtils.SeparateLine( );
                    EditorGUI.indentLevel--;
                    EditorGUILayout.LabelField( ExporterTexts.BatchExportListLabel, EditorStyles.boldLabel );
                } else {
                    EditorGUILayout.Separator( );
                }
                first = false;
                if ( multiple ) {
                    using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                        GUI.enabled = false;
                        EditorGUI.indentLevel++;
                        EditorGUILayout.ObjectField( item, typeof( MizoresPackageExporter ), false );
                        EditorGUI.indentLevel--;
                        GUI.enabled = true;
                    }
                }
                var list = item.BatchExportKeysConverted;
                for ( int i = 0; i < list.Length; i++ ) {
                    string key = list[i];
                    bool isSelected = false;
                    bool hasOverride = item.packageNameSettingsOverride.ContainsKey( key );
                    using ( new EditorGUILayout.HorizontalScope( ) ) {
                        EditorGUI.indentLevel += 2;
                        Rect rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect( ) );
                        EditorGUI.indentLevel -= 2;
                        var label = i.ToString( ) + "   " + key;
                        GUIStyle style;
                        if ( hasOverride ) {
                            style = EditorStyles.foldoutHeader;
                        } else {
                            style = EditorStyles.label;
                        }
                        isSelected = selected == item && selectedKey == key;
                        bool foldout = EditorGUI.BeginFoldoutHeaderGroup( rect, isSelected, label, style );
                        string buttonLabel;
                        if ( hasOverride ) {
                            buttonLabel = ExporterTexts.ButtonRemoveNameOverride;
                        } else {
                            buttonLabel = ExporterTexts.ButtonAddNameOverride;
                        }
                        if ( GUILayout.Button( buttonLabel, GUILayout.Width( 120 ) ) ) {
                            if ( hasOverride ) {
                                foldout = false;
                                item.packageNameSettingsOverride.Remove( key );
                                hasOverride = false;
                            } else {
                                foldout = true;
                                var settings = new PackageNameSettings( item.packageNameSettings );
                                item.packageNameSettingsOverride.Add( key, settings );
                                hasOverride = true;
                            }
                        }
                        if ( foldout && hasOverride ) {
                            selected = item;
                            selectedKey = key;
                            isSelected = true;
                        } else if ( isSelected ) {
                            selected = null;
                            selectedKey = null;
                            isSelected = false;
                        }
                        EditorGUI.EndFoldoutHeaderGroup( );
                    }
                    if ( isSelected && hasOverride ) {
                        GUI_BatchExporter_OverrideSettings.Draw( item, key, 2 );
                    }
                }

                EditorGUI.indentLevel++;
                var unusedOverrides = item.packageNameSettingsOverride.Keys.Except( list );
                EditorGUI.BeginDisabledGroup( !unusedOverrides.Any( ) );
                Rect buttonrect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect( ) );
                if ( GUI.Button( buttonrect, ExporterTexts.ButtonCleanNameOverride ) ) {
                    foreach ( var remove in unusedOverrides ) {
                        Debug.Log( "Override Removed: \n" + remove );
                        item.packageNameSettingsOverride.Remove( remove );
                    }
                    Debug.Log( ExporterTexts.LogCleanNameOverride( unusedOverrides.Count( ) ) );
                }
                EditorGUI.EndDisabledGroup( );
                EditorGUI.indentLevel--;
            }
        }
        public static void Draw( MizoresPackageExporterEditor ed, MizoresPackageExporter t, MizoresPackageExporter[] targetlist ) {
            var samevalue_in_all_mode = targetlist.All( v => t.batchExportMode == v.batchExportMode );
            string foldoutLabel;
            if ( samevalue_in_all_mode ) {
                if ( t.batchExportMode == BatchExportMode.Single ) {
                    foldoutLabel = ExporterTexts.FoldoutBatchExportDisabled;
                } else {
                    foldoutLabel = ExporterTexts.FoldoutBatchExportEnabled;
                }
            } else {
                foldoutLabel = ExporterTexts.FoldoutBatchExportEnabled;
            }
            if ( ExporterUtils.EditorPrefFoldout( ExporterEditorPrefs.FOLDOUT_EXPORT_SETTING, foldoutLabel ) ) {
                Main( t, targetlist, samevalue_in_all_mode );

                EditorGUILayout.Separator( );
                GUI_VersionFile.DrawMain( targetlist.Select( v => v.packageNameSettings ).ToArray( ), targetlist, 1 );

                DrawList( targetlist );
            }

            foreach ( var item in targetlist ) {
                if ( item.batchExportMode == BatchExportMode.Folders ) {
                    try {
                        Regex.Match( string.Empty, item.batchExportFolderRegex );
                    } catch ( System.ArgumentException e ) {
                        using ( new EditorGUILayout.HorizontalScope( ) ) {
                            var error = ExporterTexts.BatchExportRegexError( item.name, e.Message );
                            EditorGUILayout.HelpBox( error, MessageType.Error );
                        }
                    }
                }
                if ( item.batchExportMode != BatchExportMode.Single ) {
                    var packageName = item.packageNameSettings.packageName;
                    if ( !packageName.Contains( ExporterConsts_Keys.KEY_BATCH_EXPORTER ) && !packageName.Contains( ExporterConsts_Keys.KEY_FORMATTED_BATCH_EXPORTER ) ) {
                        var error = ExporterTexts.BatchExportNoTagError( item.name );
                        EditorGUILayout.HelpBox( error, MessageType.Error );
                    }
                }
            }
        }
    }
#endif
}