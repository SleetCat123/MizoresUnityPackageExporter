using UnityEngine;
using System.Linq;
using static MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterUtils;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.MizoresPackageExporterConsts;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter
{
#if UNITY_EDITOR
    public static class MultipleEditor
    {
        /// <summary>
        /// 複数オブジェクトの編集
        /// </summary>
        public static void EditMultiple( UnityPackageExporterEditor ed ) {
            var targets = ed.targets;
            var t = ed.t;

            if ( targets.Length <= 1 ) return;
            UnityPackageExporterEditor.scroll = EditorGUILayout.BeginScrollView( UnityPackageExporterEditor.scroll );
            Undo.RecordObjects( targets, ExporterTexts.t_Undo );

            var targetlist = targets.Select( v => v as MizoresPackageExporter );

            // Targets
            GUI.enabled = false;
            foreach ( var item in targetlist ) {
                EditorGUILayout.ObjectField( item, typeof( MizoresPackageExporter ), false );
            }
            GUI.enabled = true;

            // ↓ Objects
            EditorGUILayout.Separator( );
            if ( ExporterUtils.EditorPrefFoldout( Const.EDITOR_PREF_FOLDOUT_OBJECT, ExporterTexts.t_Objects ) ) {
                MinMax objects_count = MinMax.Create( targetlist, v => v.objects.Count );
                for ( int i = 0; i < objects_count.max; i++ ) {
                    using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                        // （複数インスタンス選択時）全てのオブジェクトの値が同じか
                        bool samevalue_in_all = i < objects_count.min && targetlist.All( v => t.objects[i].Object == v.objects[i].Object );

                        ExporterUtils.Indent( 1 );
                        if ( samevalue_in_all ) {
                            EditorGUILayout.LabelField( i.ToString( ), GUILayout.Width( 30 ) );
                        } else {
                            // 一部オブジェクトの値が異なっていたらTextFieldの左に?を表示
                            EditorGUILayout.LabelField( new GUIContent( ExporterTexts.t_Diff_Label, ExporterTexts.t_Diff_Tooltip ), GUILayout.Width( 30 ) );
                        }

                        EditorGUI.BeginChangeCheck( );
                        Object obj;
                        if ( samevalue_in_all ) {
                            obj = EditorGUILayout.ObjectField( t.objects[i].Object, typeof( Object ), false );
                        } else {
                            obj = EditorGUILayout.ObjectField( null, typeof( Object ), false );
                        }
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            foreach ( var item in targetlist ) {
                                // 全ての選択中インスタンスに対してオブジェクトを設定
                                // DynamicPathの要素数が足りなかったらリサイズ
                                ExporterUtils.ResizeList( item.objects, Mathf.Max( i + 1, item.objects.Count ), ( ) => new PackagePrefsElement( ) );
                                item.objects[i].Object = obj;
                                objects_count = MinMax.Create( targetlist, v => v.objects.Count );
                                EditorUtility.SetDirty( item );
                            }
                        }

                        EditorGUI.BeginChangeCheck( );
                        string path;
                        if ( samevalue_in_all ) {
                            path = EditorGUILayout.TextField( t.objects[i].Path );
                        } else {
                            path = EditorGUILayout.TextField( string.Empty );
                        }
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            // パスが変更されたらオブジェクトを置き換える
                            Object o = AssetDatabase.LoadAssetAtPath<Object>( path );
                            if ( o != null ) {
                                foreach ( var item in targetlist ) {
                                    // 全ての選択中インスタンスに対してオブジェクトを設定
                                    // DynamicPathの要素数が足りなかったらリサイズ
                                    ExporterUtils.ResizeList( item.objects, Mathf.Max( i + 1, item.objects.Count ), ( ) => new PackagePrefsElement( ) );
                                    item.objects[i].Object = o;
                                    objects_count = MinMax.Create( targetlist, v => v.objects.Count );
                                    EditorUtility.SetDirty( item );
                                }
                            }
                        }

                        // Button
                        int index_after = ExporterUtils.UpDownButton( i, objects_count.max );
                        if ( i != index_after ) {
                            foreach ( var item in targetlist ) {
                                if ( item.objects.Count <= index_after ) {
                                    ExporterUtils.ResizeList( item.objects, index_after + 1, ( ) => new PackagePrefsElement( ) );
                                }
                                item.objects.Swap( i, index_after );
                                EditorUtility.SetDirty( item );
                            }
                        }
                        EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 10 ) );
                        if ( GUILayout.Button( "-", GUILayout.Width( 15 ) ) ) {
                            foreach ( var item in targetlist ) {
                                ExporterUtils.ResizeList( item.objects, Mathf.Max( i + 1, item.objects.Count ), ( ) => new PackagePrefsElement( ) );
                                item.objects.RemoveAt( i );
                                objects_count = MinMax.Create( targetlist, v => v.objects.Count );
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
                            ExporterUtils.ResizeList( item.objects, objects_count.max + 1 );
                            EditorUtility.SetDirty( item );
                        }
                    }
                }
            }
            // ↑ Objects

            var dpath_count = MinMax.Create( targetlist, v => v.dynamicpath.Count );
            // ↓ Dynamic Path
            EditorGUILayout.Separator( );
            if ( ExporterUtils.EditorPrefFoldout( Const.EDITOR_PREF_FOLDOUT_DYNAMICPATH, ExporterTexts.t_DynamicPath ) ) {
                for ( int i = 0; i < dpath_count.max; i++ ) {
                    using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                        // 全てのオブジェクトの値が同じか
                        bool samevalue_in_all = i < dpath_count.min && targetlist.All( v => t.dynamicpath[i] == v.dynamicpath[i] );

                        ExporterUtils.Indent( 1 );
                        if ( samevalue_in_all ) {
                            EditorGUILayout.LabelField( i.ToString( ), GUILayout.Width( 30 ) );
                        } else {
                            // 一部オブジェクトの値が異なっていたらTextFieldの左に?を表示
                            EditorGUILayout.LabelField( new GUIContent( ExporterTexts.t_Diff_Label, ExporterTexts.t_Diff_Tooltip ), GUILayout.Width( 30 ) );
                        }

                        EditorGUI.BeginChangeCheck( );
                        string path;
                        if ( samevalue_in_all ) {
                            path = EditorGUILayout.TextField( t.dynamicpath[i] );
                            path = ed.BrowseButtons( path );
                        } else {
                            path = EditorGUILayout.TextField( string.Empty );
                            path = ed.BrowseButtons( Application.dataPath );
                        }
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            foreach ( var item in targetlist ) {
                                ExporterUtils.ResizeList( item.dynamicpath, Mathf.Max( i + 1, item.dynamicpath.Count ) );
                                item.dynamicpath[i] = path;
                                dpath_count = MinMax.Create( targetlist, v => v.dynamicpath.Count );
                                EditorUtility.SetDirty( item );
                            }
                        }

                        // Button
                        int index_after = ExporterUtils.UpDownButton( i, dpath_count.max );
                        if ( i != index_after ) {
                            foreach ( var item in targetlist ) {
                                if ( item.dynamicpath.Count <= index_after ) {
                                    ExporterUtils.ResizeList( item.dynamicpath, index_after + 1 );
                                }
                                item.dynamicpath.Swap( i, index_after );
                                EditorUtility.SetDirty( item );
                            }
                        }
                        EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 10 ) );
                        if ( GUILayout.Button( "-", GUILayout.Width( 15 ) ) ) {
                            foreach ( var item in targetlist ) {
                                ExporterUtils.ResizeList( item.dynamicpath, Mathf.Max( i + 1, item.dynamicpath.Count ) );
                                item.dynamicpath.RemoveAt( i );
                                dpath_count = MinMax.Create( targetlist, v => v.dynamicpath.Count );
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
                            ExporterUtils.ResizeList( item.dynamicpath, dpath_count.max + 1, ( ) => string.Empty );
                            EditorUtility.SetDirty( item );
                        }
                    }
                }
            }
            // ↑ Dynamic Path

            // ↓ Dynamic Path Preview
            EditorGUILayout.Separator( );
            if ( ExporterUtils.EditorPrefFoldout( Const.EDITOR_PREF_FOLDOUT_DYNAMICPATH_PREVIEW, ExporterTexts.t_DynamicPathPreview ) ) {
                bool first = true;
                foreach ( var item in targetlist ) {
                    if ( first == false ) EditorGUILayout.Separator( );
                    first = false;
                    using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                        GUI.enabled = false;
                        ExporterUtils.Indent( 1 );
                        EditorGUILayout.ObjectField( item, typeof( MizoresPackageExporter ), false );
                        GUI.enabled = true;
                    }
                    for ( int i = 0; i < dpath_count.max; i++ ) {
                        using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                            ExporterUtils.Indent( 2 );
                            EditorGUILayout.LabelField( i.ToString( ), GUILayout.Width( 30 ) );
                            if ( i < item.dynamicpath.Count ) {
                                string previewpath = item.ConvertDynamicPath( item.dynamicpath[i] );
                                EditorGUILayout.LabelField( new GUIContent( previewpath, previewpath ) );
                            } else {
                                EditorGUILayout.LabelField( "-" );
                            }
                        }
                    }
                }
            }
            // ↑ Dynamic Path Preview

            // ↓ Version File
            EditorGUILayout.Separator( );
            EditorGUILayout.LabelField( ExporterTexts.t_VersionFile, EditorStyles.boldLabel );
            using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                var samevalue_in_all = targetlist.All( v => t.versionFile.Object == v.versionFile.Object );

                EditorGUI.BeginChangeCheck( );
                Object obj;
                if ( samevalue_in_all ) {
                    obj = EditorGUILayout.ObjectField( t.versionFile.Object, typeof( TextAsset ), false );
                } else {
                    obj = EditorGUILayout.ObjectField( null, typeof( TextAsset ), false );
                }
                if ( EditorGUI.EndChangeCheck( ) ) {
                    foreach ( var item in targetlist ) {
                        item.versionFile.Object = obj;
                        item.UpdateExportVersion( );
                        EditorUtility.SetDirty( item );
                    }
                }

                EditorGUI.BeginChangeCheck( );
                string path;
                if ( samevalue_in_all ) {
                    path = EditorGUILayout.TextField( t.versionFile.Path );
                } else {
                    path = EditorGUILayout.TextField( string.Empty );
                }
                if ( EditorGUI.EndChangeCheck( ) ) {
                    // パスが変更されたらオブジェクトを置き換える
                    Object o = AssetDatabase.LoadAssetAtPath<TextAsset>( path );
                    if ( o != null ) {
                        foreach ( var item in targetlist ) {
                            item.versionFile.Object = o;
                            item.UpdateExportVersion( );
                            EditorUtility.SetDirty( item );
                        }
                    }
                }
            }
            // ↑ Version File

            EditorGUILayout.EndScrollView( );
            EditorGUILayout.Separator( );

            EditorGUILayout.LabelField( ExporterTexts.t_Label_ExportPackage, EditorStyles.boldLabel );
            // Check Button
            if ( GUILayout.Button( ExporterTexts.t_Button_Check ) ) {
                UnityPackageExporterEditor.HelpBoxText = string.Empty;
                foreach ( var item in targetlist ) {
                    item.AllFileExists( );
                }
            }

            // Export Button
            if ( GUILayout.Button( ExporterTexts.t_Button_ExportPackages ) ) {
                UnityPackageExporterEditor.HelpBoxText = string.Empty;
                foreach ( var item in targetlist ) {
                    item.Export( );
                }
            }

            // 出力先一覧
            EditorGUILayout.Separator( );
            foreach ( var item in targets ) {
                var path = ( item as MizoresPackageExporter ).ExportPath;
                EditorGUILayout.LabelField( new GUIContent( path, path ) );
            }
            if ( GUILayout.Button( ExporterTexts.TEXT_BUTTON_OPEN, GUILayout.Width( 60 ) ) ) {
                EditorUtility.RevealInFinder( Const.EXPORT_FOLDER_PATH );
            }
        }
    }
#endif
}
