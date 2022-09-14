using UnityEngine;
using System.Linq;
using static MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterUtils;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.MizoresPackageExporterConsts;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.MultipleEditor
{
#if UNITY_EDITOR
    public static class MultipleGUI_Excludes
    {
        public static void Draw( MizoresPackageExporterEditor ed, MizoresPackageExporter t, IEnumerable<MizoresPackageExporter> targetlist ) {
            var minmax_count = MinMax.Create( targetlist, v => v.excludes.Count );
            // ↓ Excludes
            if ( ExporterUtils.EditorPrefFoldout(
                Const.EDITOR_PREF_FOLDOUT_EXCLUDES,
                string.Format( ExporterTexts.t_Excludes, minmax_count.GetRangeString( ) ),
                new FoldoutFuncs( ) {
                    onRightClick = ( ) => MultipleGUIElement_CopyPaste.OnRightClickFoldout<SearchPath>( targetlist, ExporterTexts.t_Excludes, ( ex, list ) => ex.excludes = list )
                }
                ) ) {
                for ( int i = 0; i < minmax_count.max; i++ ) {
                    using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                        // 全てのオブジェクトの値が同じか
                        bool samevalue_in_all_value = i < minmax_count.min && targetlist.All( v => t.excludes[i].value == v.excludes[i].value );
                        bool samevalue_in_all_type = i < minmax_count.min && targetlist.All( v => t.excludes[i].searchType == v.excludes[i].searchType );

                        ExporterUtils.Indent( 1 );
                        if ( samevalue_in_all_value && samevalue_in_all_type ) {
                            EditorGUILayout.LabelField( i.ToString( ), GUILayout.Width( 30 ) );
                        } else {
                            // 一部オブジェクトの値が異なっていたらTextFieldの左に?を表示
                            DiffLabel( );
                        }


                        using ( new EditorGUILayout.HorizontalScope( ) ) {
                            EditorGUI.BeginChangeCheck( );
                            Rect textrect = EditorGUILayout.GetControlRect( );
                            string value;
                            if ( samevalue_in_all_value ) {
                                value = EditorGUI.TextField( textrect, t.excludes[i].value );
                            } else {
                                value = EditorGUI.TextField( textrect, string.Empty );
                            }
                            if ( ExporterUtils.DragDrop( textrect, ExporterUtils.Filter_HasPersistentObject ) ) {
                                GUI.changed = true;
                                value = AssetDatabase.GetAssetPath( DragAndDrop.objectReferences[0] );
                            }
                            if ( EditorGUI.EndChangeCheck( ) ) {
                                foreach ( var item in targetlist ) {
                                    ExporterUtils.ResizeList( item.excludes, Mathf.Max( i + 1, item.excludes.Count ), ( ) => new SearchPath( ) );
                                    item.excludes[i].value = value;
                                    minmax_count = MinMax.Create( targetlist, v => v.excludes.Count );
                                    EditorUtility.SetDirty( item );
                                }
                            }

                            EditorGUI.BeginChangeCheck( );
                            SearchPathType searchType;
                            if ( samevalue_in_all_type ) {
                                searchType = (SearchPathType)EditorGUILayout.EnumPopup( t.excludes[i].searchType, GUILayout.Width( 70 ) );
                            } else {
                                searchType = (SearchPathType)EditorGUILayout.EnumPopup( SearchPath.DUMMY_TYPE, GUILayout.Width( 70 ) );
                            }
                            if ( EditorGUI.EndChangeCheck( ) ) {
                                if ( searchType == SearchPath.DUMMY_TYPE ) {
                                    searchType = SearchPathType.Exact;
                                }
                                foreach ( var item in targetlist ) {
                                    ExporterUtils.ResizeList( item.excludes, Mathf.Max( i + 1, item.excludes.Count ), ( ) => new SearchPath( ) );
                                    item.excludes[i].searchType = searchType;
                                    minmax_count = MinMax.Create( targetlist, v => v.excludes.Count );
                                    EditorUtility.SetDirty( item );
                                }
                            }
                        }

                        // Button
                        int index_after = ExporterUtils.UpDownButton( i, minmax_count.max );
                        if ( i != index_after ) {
                            foreach ( var item in targetlist ) {
                                if ( item.excludes.Count <= index_after ) {
                                    ExporterUtils.ResizeList( item.excludes, index_after + 1, ( ) => new SearchPath( ) );
                                }
                                item.excludes.Swap( i, index_after );
                                EditorUtility.SetDirty( item );
                            }
                        }
                        EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 10 ) );
                        if ( GUILayout.Button( "-", GUILayout.Width( 15 ) ) ) {
                            foreach ( var item in targetlist ) {
                                ExporterUtils.ResizeList( item.excludes, Mathf.Max( i + 1, item.excludes.Count ), ( ) => new SearchPath( ) );
                                item.excludes.RemoveAt( i );
                                minmax_count = MinMax.Create( targetlist, v => v.excludes.Count );
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
                            ExporterUtils.ResizeList( item.excludes, minmax_count.max + 1, ( ) => new SearchPath( ) );
                            EditorUtility.SetDirty( item );
                        }
                    }
                }
            }
            // ↑ Excludes

            // ↓ Excludes Preview
            if ( ExporterUtils.EditorPrefFoldout( Const.EDITOR_PREF_FOLDOUT_EXCLUDES_PREVIEW, ExporterTexts.t_ExcludesPreview ) ) {
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
                    for ( int i = 0; i < minmax_count.max; i++ ) {
                        using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                            ExporterUtils.Indent( 2 );
                            EditorGUILayout.LabelField( i.ToString( ), GUILayout.Width( 30 ) );

                            if ( i < item.excludes.Count ) {
                                string previewpath = item.ConvertDynamicPath( item.excludes[i].value );
                                EditorGUILayout.LabelField( new GUIContent( previewpath, previewpath ) );
                                using ( new EditorGUI.DisabledGroupScope( true ) ) {
                                    EditorGUILayout.EnumPopup( item.excludes[i].searchType, GUILayout.Width( 140 ) );
                                }
                            } else {
                                EditorGUILayout.LabelField( "-" );
                            }
                        }
                    }
                }
            }
            // ↑ Excludes Preview
        }
    }
#endif
}
