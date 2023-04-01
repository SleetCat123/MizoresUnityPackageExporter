using UnityEngine;
using System.Linq;
using static MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterUtils;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.MizoresPackageExporterConsts;
using System.Collections.Generic;
using MizoreNekoyanagi.PublishUtil.PackageExporter.MultipleEditor;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor
{
#if UNITY_EDITOR
    public static class GUI_Excludes
    {
        public static void AddObjects( IEnumerable<MizoresPackageExporter> targetlist, System.Func<MizoresPackageExporter, List<SearchPath>> getList, Object[] objectReferences ) {
            var add = objectReferences.
                Where( v => EditorUtility.IsPersistent( v ) ).
                Select( v => new SearchPath( SearchPathType.Exact, AssetDatabase.GetAssetPath( v ) ) );
            foreach ( var item in targetlist ) {
                getList( item ).AddRange( add );
                EditorUtility.SetDirty( item );
            }
        }
        public static void Draw( MizoresPackageExporterEditor ed, MizoresPackageExporter t, IEnumerable<MizoresPackageExporter> targetlist ) {
            var minmax_count = MinMax.Create( targetlist, v => v.excludes.Count );
            // ↓ Excludes
            if ( ExporterUtils.EditorPrefFoldout(
                Const.EDITOR_PREF_FOLDOUT_EXCLUDES,
                string.Format( ExporterTexts.t_Excludes, minmax_count.GetRangeString( ) ),
                new FoldoutFuncs( ) {
                    canDragDrop = objectReferences => minmax_count.SameValue && ExporterUtils.Filter_HasPersistentObject( objectReferences ),
                    onDragPerform = ( objectReferences ) => AddObjects( targetlist, v => v.excludes, objectReferences ),
                    onRightClick = ( ) => MultipleGUIElement_CopyPasteList.OnRightClickFoldout<SearchPath>( targetlist, ExporterTexts.t_Excludes, ( ex ) => ex.excludes, ( ex, list ) => ex.excludes = list )
                }
                ) ) {
                Event currentEvent = Event.current;
                for ( int i = 0; i < minmax_count.max; i++ ) {
                    using ( var scope = new EditorGUILayout.HorizontalScope( ) ) {
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
                                EditorGUI.showMixedValue = true;
                                value = EditorGUI.TextField( textrect, string.Empty );
                                EditorGUI.showMixedValue = false;
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
                                EditorGUI.showMixedValue = true;
                                searchType = (SearchPathType)EditorGUILayout.EnumPopup( SearchPathType.Exact, GUILayout.Width( 70 ) );
                                EditorGUI.showMixedValue = false;
                            }
                            if ( EditorGUI.EndChangeCheck( ) ) {
                                foreach ( var item in targetlist ) {
                                    ExporterUtils.ResizeList( item.excludes, Mathf.Max( i + 1, item.excludes.Count ), ( ) => new SearchPath( ) );
                                    item.excludes[i].searchType = searchType;
                                    minmax_count = MinMax.Create( targetlist, v => v.excludes.Count );
                                    EditorUtility.SetDirty( item );
                                }
                            }
                        }

                        // Copy&Paste
                        if ( currentEvent.type == EventType.ContextClick && scope.rect.Contains( currentEvent.mousePosition ) ) {
                            GenericMenu menu = new GenericMenu( );
                            // Copy
                            if ( samevalue_in_all_type && samevalue_in_all_value ) {
                                var item = t.excludes[i];
                                string label = string.Format( ExporterTexts.t_CopyTargetWithValue, item.GetType( ).Name, i.ToString( ) );
                                menu.AddItem( new GUIContent( label ), false, CopyCache.Copy, item );
                            } else {
                                menu.AddDisabledItem( new GUIContent( ExporterTexts.t_CopyTargetWithValue ) );
                            }
                            // Paste
                            if ( CopyCache.CanPaste<SearchPath>( ) ) {
                                var item = t.excludes[i];
                                string label = string.Format( ExporterTexts.t_PasteTargetWithValue, item.GetType( ).Name, item.ToString( ) );
                                int index = i;
                                menu.AddItem( new GUIContent( label ), false, ( ) => {
                                    var paste = CopyCache.GetCache<SearchPath>( );
                                    foreach ( var ex in targetlist ) {
                                        ex.excludes[index] = paste;
                                        EditorUtility.SetDirty( ex );
                                    }
                                } );
                            } else {
                                menu.AddDisabledItem( new GUIContent( ExporterTexts.t_PasteTargetNoValue ) );
                            }
                            menu.ShowAsContext( );

                            currentEvent.Use( );
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
                bool multiple = targetlist.Count( ) > 1;
                foreach ( var item in targetlist ) {
                    if ( first == false ) EditorGUILayout.Separator( );
                    first = false;
                    if ( multiple ) {
                        using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                            GUI.enabled = false;
                            ExporterUtils.Indent( 1 );
                            EditorGUILayout.ObjectField( item, typeof( MizoresPackageExporter ), false );
                            GUI.enabled = true;
                        }
                    }
                    for ( int i = 0; i < minmax_count.max; i++ ) {
                        using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                            if ( multiple ) {
                                ExporterUtils.Indent( 2 );
                            } else {
                                ExporterUtils.Indent( 1 );
                            }
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
