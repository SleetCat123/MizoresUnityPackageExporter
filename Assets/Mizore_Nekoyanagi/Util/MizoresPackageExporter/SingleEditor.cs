using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.MizoresPackageExporterConsts;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter
{
#if UNITY_EDITOR
    public static class SingleEditor
    {
        public static void EditSingle( UnityPackageExporterEditor ed ) {
            var t = ed.t;

            using ( var s = new EditorGUI.DisabledGroupScope( true ) ) {
                EditorGUILayout.ObjectField( t, typeof( MizoresPackageExporter ), false );
            }
            UnityPackageExporterEditor.scroll = EditorGUILayout.BeginScrollView( UnityPackageExporterEditor.scroll );
            Undo.RecordObject( t, ExporterTexts.t_Undo );

            // ↓ Objects
            if ( ExporterUtils.EditorPrefFoldout( Const.EDITOR_PREF_FOLDOUT_OBJECT, ExporterTexts.t_Objects ) ) {
                for ( int i = 0; i < t.objects.Count; i++ ) {
                    using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                        PackagePrefsElement item = t.objects[i];
                        ExporterUtils.Indent( 1 );
                        EditorGUILayout.LabelField( i.ToString( ), GUILayout.Width( 30 ) );

                        EditorGUI.BeginChangeCheck( );
                        item.Object = EditorGUILayout.ObjectField( item.Object, typeof( Object ), false );
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            EditorUtility.SetDirty( t );
                        }

                        EditorGUI.BeginChangeCheck( );
                        string path = item.Path;
                        path = EditorGUILayout.TextField( path );
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            // パスが変更されたらオブジェクトを置き換える
                            Object o = AssetDatabase.LoadAssetAtPath<Object>( path );
                            if ( o != null ) {
                                item.Object = o;
                            }
                            EditorUtility.SetDirty( t );
                        }

                        int index_after = ExporterUtils.UpDownButton( i, t.objects.Count );
                        if ( i != index_after ) {
                            t.objects.Swap( i, index_after );
                            EditorUtility.SetDirty( t );
                        }
                        EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 10 ) );
                        if ( GUILayout.Button( "-", GUILayout.Width( 15 ) ) ) {
                            t.objects.RemoveAt( i );
                            i--;
                            EditorUtility.SetDirty( t );
                        }
                    }
                }
                using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                    ExporterUtils.Indent( 1 );
                    if ( GUILayout.Button( "+", GUILayout.Width( 60 ) ) ) {
                        t.objects.Add( new PackagePrefsElement( ) );
                        EditorUtility.SetDirty( t );
                    }
                }
            }
            // ↑ Objects

            // ↓ Dynamic Path
            EditorGUILayout.Separator( );
            if ( ExporterUtils.EditorPrefFoldout( Const.EDITOR_PREF_FOLDOUT_DYNAMICPATH, ExporterTexts.t_DynamicPath ) ) {
                for ( int i = 0; i < t.dynamicpath.Count; i++ ) {
                    using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                        ExporterUtils.Indent( 1 );
                        EditorGUILayout.LabelField( i.ToString( ), GUILayout.Width( 30 ) );

                        // 値編集
                        EditorGUI.BeginChangeCheck( );
                        t.dynamicpath[i] = EditorGUILayout.TextField( t.dynamicpath[i] );
                        t.dynamicpath[i] = ed.BrowseButtons( t.dynamicpath[i] );
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            EditorUtility.SetDirty( t );
                        }

                        // ボタン
                        int index_after = ExporterUtils.UpDownButton( i, t.dynamicpath.Count );
                        if ( i != index_after ) {
                            t.dynamicpath.Swap( i, index_after );
                            EditorUtility.SetDirty( t );
                        }
                        EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 10 ) );
                        if ( GUILayout.Button( "-", GUILayout.Width( 15 ) ) ) {
                            t.dynamicpath.RemoveAt( i );
                            i--;
                            EditorUtility.SetDirty( t );
                        }
                    }
                }
                using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                    ExporterUtils.Indent( 1 );
                    if ( GUILayout.Button( "+", GUILayout.Width( 60 ) ) ) {
                        t.dynamicpath.Add( string.Empty );
                        EditorUtility.SetDirty( t );
                    }
                }
            }
            // ↑ Dynamic Path

            // ↓ Dynamic Path Preview
            EditorGUILayout.Separator( );
            if ( ExporterUtils.EditorPrefFoldout( Const.EDITOR_PREF_FOLDOUT_DYNAMICPATH_PREVIEW, ExporterTexts.t_DynamicPathPreview ) ) {
                for ( int i = 0; i < t.dynamicpath.Count; i++ ) {
                    using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                        // プレビュー
                        string previewpath = t.ConvertDynamicPath( t.dynamicpath[i] );
                        ExporterUtils.Indent( 1 );
                        EditorGUILayout.LabelField( i.ToString( ), GUILayout.Width( 30 ) );
                        EditorGUILayout.LabelField( new GUIContent( previewpath, previewpath ) );
                    }
                }
            }
            // ↑ Dynamic Path Preview

            // ↓ Dynamic Path Variables
            EditorGUILayout.Separator( );
            if ( ExporterUtils.EditorPrefFoldout( Const.EDITOR_PREF_FOLDOUT_DYNAMICPATH_VARIABLES, ExporterTexts.t_DynamicPath_Variables ) ) {
                var space_width = GUILayout.Width( 30 );
                var button_width = GUILayout.Width( 15 );
                GUI.enabled = false;
                using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                    EditorGUILayout.LabelField( string.Empty, space_width );
                    EditorGUILayout.TextField( "name" );
                    EditorGUILayout.TextField( t.name );
                    EditorGUILayout.LabelField( string.Empty, space_width );
                    EditorGUILayout.LabelField( string.Empty, button_width );
                }
                using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                    EditorGUILayout.LabelField( string.Empty, space_width );
                    EditorGUILayout.TextField( "version" );
                    EditorGUILayout.TextField( t.ExportVersion );
                    EditorGUILayout.LabelField( string.Empty, space_width );
                    EditorGUILayout.LabelField( string.Empty, button_width );
                }
                using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                    EditorGUILayout.LabelField( string.Empty, space_width );
                    EditorGUILayout.TextField( "versionprefix" );
                    EditorGUILayout.TextField( t.versionPrefix );
                    EditorGUILayout.LabelField( string.Empty, space_width );
                    EditorGUILayout.LabelField( string.Empty, button_width );
                }
                GUI.enabled = true;
                List<string> keys = new List<string>( t.variables.Keys );
                for ( int i = 0; i < keys.Count; i++ ) {
                    string key = keys[i];
                    using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                        EditorGUILayout.LabelField( string.Empty, space_width );

                        // キー名変更
                        EditorGUI.BeginChangeCheck( );
                        string temp_key = EditorGUILayout.DelayedTextField( key );
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            if ( string.IsNullOrWhiteSpace( temp_key ) || t.variables.ContainsKey( temp_key ) ) {

                            } else {
                                t.variables.Add( temp_key, t.variables[key] );
                                t.variables.Remove( key );
                                key = temp_key;
                                EditorUtility.SetDirty( t );
                            }
                        }

                        // 値変更
                        EditorGUI.BeginChangeCheck( );
                        t.variables[key] = EditorGUILayout.TextField( t.variables[key] );
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            EditorUtility.SetDirty( t );
                        }

                        // ボタン
                        EditorGUILayout.LabelField( string.Empty, space_width );
                        if ( GUILayout.Button( "-", button_width ) ) {
                            t.variables.Remove( key );
                            EditorUtility.SetDirty( t );
                        }
                    }
                }
                using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                    // 新規キー追加
                    EditorGUILayout.LabelField( string.Empty, space_width );
                    EditorGUI.BeginChangeCheck( );
                    UnityPackageExporterEditor.variable_key_temp = EditorGUILayout.DelayedTextField( UnityPackageExporterEditor.variable_key_temp );
                    EditorGUILayout.LabelField( string.Empty );
                    string temp = UnityPackageExporterEditor.variable_key_temp;
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        if ( string.IsNullOrWhiteSpace( temp ) || t.variables.ContainsKey( temp ) ) {
                            UnityPackageExporterEditor.variable_key_temp = null;
                        } else {
                            // キー追加
                            t.variables.Add( temp, string.Empty );
                            UnityPackageExporterEditor.variable_key_temp = null;
                            EditorUtility.SetDirty( t );
                        }
                    }
                }
            }
            // ↑ Dynamic Path Variables

            // ↓ References
            if ( ExporterUtils.EditorPrefFoldout( Const.EDITOR_PREF_FOLDOUT_REFERENCES, ExporterTexts.t_References ) ) {
                for ( int i = 0; i < t.references.Count; i++ ) {
                    using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                        PackagePrefsElement item = t.references[i];
                        ExporterUtils.Indent( 1 );
                        EditorGUILayout.LabelField( i.ToString( ), GUILayout.Width( 30 ) );

                        EditorGUI.BeginChangeCheck( );
                        item.Object = EditorGUILayout.ObjectField( item.Object, typeof( DefaultAsset ), false );
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            EditorUtility.SetDirty( t );
                        }

                        EditorGUI.BeginChangeCheck( );
                        string path = item.Path;
                        path = EditorGUILayout.TextField( path );
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            // パスが変更されたらオブジェクトを置き換える
                            Object o = AssetDatabase.LoadAssetAtPath<Object>( path );
                            if ( o != null ) {
                                item.Object = o;
                            }
                            EditorUtility.SetDirty( t );
                        }

                        int index_after = ExporterUtils.UpDownButton( i, t.references.Count );
                        if ( i != index_after ) {
                            t.references.Swap( i, index_after );
                            EditorUtility.SetDirty( t );
                        }
                        EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 10 ) );
                        if ( GUILayout.Button( "-", GUILayout.Width( 15 ) ) ) {
                            t.references.RemoveAt( i );
                            i--;
                            EditorUtility.SetDirty( t );
                        }
                    }
                }
                using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                    ExporterUtils.Indent( 1 );
                    if ( GUILayout.Button( "+", GUILayout.Width( 60 ) ) ) {
                        t.references.Add( new PackagePrefsElement( ) );
                        EditorUtility.SetDirty( t );
                    }
                }
            }
            // ↑ References

            // ↓ Excludes
            if ( ExporterUtils.EditorPrefFoldout( Const.EDITOR_PREF_FOLDOUT_EXCLUDES, ExporterTexts.t_Excludes ) ) {
                for ( int i = 0; i < t.excludes.Count; i++ ) {
                    using ( new EditorGUILayout.HorizontalScope( ) ) {
                        SearchPath item = t.excludes[i];
                        ExporterUtils.Indent( 1 );
                        EditorGUILayout.LabelField( i.ToString( ), GUILayout.Width( 30 ) );

                        EditorGUI.BeginChangeCheck( );
                        using ( new EditorGUILayout.HorizontalScope( ) ) {
                            item.value = EditorGUILayout.TextField( item.value );
                            item.searchType = (SearchPathType)EditorGUILayout.EnumPopup( item.searchType, GUILayout.Width(60) );
                        }
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            EditorUtility.SetDirty( t );
                        }

                        // ボタン
                        int index_after = ExporterUtils.UpDownButton( i, t.excludes.Count );
                        if ( i != index_after ) {
                            t.excludes.Swap( i, index_after );
                            EditorUtility.SetDirty( t );
                        }
                        EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 10 ) );
                        if ( GUILayout.Button( "-", GUILayout.Width( 15 ) ) ) {
                            t.excludes.RemoveAt( i );
                            i--;
                            EditorUtility.SetDirty( t );
                        }
                    }
                }
                using ( new EditorGUILayout.HorizontalScope( ) ) {
                    ExporterUtils.Indent( 1 );
                    if ( GUILayout.Button( "+", GUILayout.Width( 60 ) ) ) {
                        t.excludes.Add( new SearchPath( ) );
                        EditorUtility.SetDirty( t );
                    }
                }
            }
            // ↑ Excludes

            // ↓ Version File
            EditorGUILayout.Separator( );
            EditorGUILayout.LabelField( ExporterTexts.t_VersionFile, EditorStyles.boldLabel );
            using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                EditorGUI.BeginChangeCheck( );
                t.versionFile.Object = EditorGUILayout.ObjectField( t.versionFile.Object, typeof( TextAsset ), false );
                if ( EditorGUI.EndChangeCheck( ) ) {
                    t.UpdateExportVersion( );
                    EditorUtility.SetDirty( t );
                }
                EditorGUI.BeginChangeCheck( );
                string path = t.versionFile.Path;
                path = EditorGUILayout.TextField( path );
                if ( EditorGUI.EndChangeCheck( ) ) {
                    // パスが変更されたらオブジェクトを置き換える
                    Object o = AssetDatabase.LoadAssetAtPath<TextAsset>( path );
                    if ( o != null ) {
                        t.versionFile.Object = o;
                        t.UpdateExportVersion( );
                    }
                    EditorUtility.SetDirty( t );
                }
            }
            EditorGUI.BeginChangeCheck( );
            t.versionPrefix = EditorGUILayout.TextField( "Prefix", t.versionPrefix );
            if ( EditorGUI.EndChangeCheck( ) ) {
                EditorUtility.SetDirty( t );
            }
            // ↑ Version File

            EditorGUILayout.EndScrollView( );
            EditorGUILayout.Separator( );


            EditorGUILayout.LabelField( ExporterTexts.t_Label_ExportPackage, EditorStyles.boldLabel );
            // Check Button
            if ( GUILayout.Button( ExporterTexts.t_Button_Check ) ) {
                UnityPackageExporterEditor.HelpBoxText = string.Empty;
                t.AllFileExists( );
            }

            // Export Button
            if ( GUILayout.Button( ExporterTexts.t_Button_ExportPackage ) ) {
                UnityPackageExporterEditor.HelpBoxText = string.Empty;
                t.Export( );
            }

            // Open Button
            using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                EditorGUILayout.LabelField( new GUIContent( t.ExportPath, t.ExportPath ) );
                if ( GUILayout.Button( ExporterTexts.TEXT_BUTTON_OPEN, GUILayout.Width( 60 ) ) ) {
                    if ( File.Exists( t.ExportPath ) ) {
                        EditorUtility.RevealInFinder( t.ExportPath );
                    } else {
                        EditorUtility.RevealInFinder( Const.EXPORT_FOLDER_PATH );
                    }
                }
            }
        }
    }
#endif
}
