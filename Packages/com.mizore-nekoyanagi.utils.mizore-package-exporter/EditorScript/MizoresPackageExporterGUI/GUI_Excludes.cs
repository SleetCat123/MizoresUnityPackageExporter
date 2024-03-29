﻿using UnityEngine;
using System.Linq;
using static MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterUtils;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor {
#if UNITY_EDITOR
    public static class GUI_Excludes {
        public static void AddObjects( IEnumerable<MizoresPackageExporter> targetlist, System.Func<MizoresPackageExporter, List<SearchPath>> getList, Object[] objectReferences ) {
            var add = objectReferences.
                Where( v => EditorUtility.IsPersistent( v ) ).
                Select( v => new SearchPath( SearchPathType.Exact, AssetDatabase.GetAssetPath( v ) ) );
            foreach ( var item in targetlist ) {
                getList( item ).AddRange( add );
                EditorUtility.SetDirty( item );
            }
        }
        public static void Draw( MizoresPackageExporterEditor ed, MizoresPackageExporter t, MizoresPackageExporter[] targetlist ) {
            var minmax_count = MinMax.Create( targetlist, v => v.excludes.Count );
            bool multiple = targetlist.Length > 1;
            // ↓ Excludes
            if ( ExporterUtils.EditorPrefFoldout(
                ExporterEditorPrefs.FOLDOUT_EXCLUDES,
                ExporterTexts.FoldoutExcludes( minmax_count.ToString( ) ),
                new FoldoutFuncs( ) {
                    canDragDrop = objectReferences => minmax_count.SameValue && ExporterUtils.Filter_HasPersistentObject( objectReferences ),
                    onDragPerform = ( objectReferences ) => AddObjects( targetlist, v => v.excludes, objectReferences ),
                    onRightClick = ( ) => GUIElement_CopyPasteList.OnRightClickFoldout<SearchPath>( targetlist, ExporterTexts.FoldoutExcludes, ( ex ) => ex.excludes, ( ex, list ) => ex.excludes = list )
                }
                ) ) {
                Event currentEvent = Event.current;
                for ( int i = 0; i < minmax_count.max; i++ ) {
                    using ( var scope = new EditorGUILayout.HorizontalScope( ) ) {
                        // 全てのオブジェクトの値が同じか
                        bool samevalue_in_all_value = true;
                        bool samevalue_in_all_type = true;
                        if ( multiple ) {
                            samevalue_in_all_value = i < minmax_count.min && targetlist.All( v => t.excludes[i].value == v.excludes[i].value );
                            samevalue_in_all_type = i < minmax_count.min && targetlist.All( v => t.excludes[i].searchType == v.excludes[i].searchType );
                        }

                        EditorGUI.indentLevel++;
                        if ( samevalue_in_all_value && samevalue_in_all_type ) {
                            EditorGUILayout.LabelField( i.ToString( ), GUILayout.Width( 30 ) );
                        } else {
                            // 一部オブジェクトの値が異なっていたらTextFieldの左に?を表示
                            DiffLabel( );
                        }
                        EditorGUI.indentLevel--;

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
                                searchType = ( SearchPathType )EditorGUILayout.EnumPopup( t.excludes[i].searchType, GUILayout.Width( 70 ) );
                            } else {
                                EditorGUI.showMixedValue = true;
                                searchType = ( SearchPathType )EditorGUILayout.EnumPopup( SearchPathType.Exact, GUILayout.Width( 70 ) );
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
                                string label = ExporterTexts.CopyTargetWithValue( item.GetType( ).Name, i );
                                menu.AddItem( new GUIContent( label ), false, CopyCache.Copy, item );
                            } else {
                                menu.AddDisabledItem( new GUIContent( ExporterTexts.CopyTargetNoValue ) );
                            }
                            // Paste
                            if ( CopyCache.CanPaste<SearchPath>( ) ) {
                                var item = t.excludes[i];
                                string label = ExporterTexts.PasteTargetWithValue( item.GetType( ).Name, item.ToString( ) );
                                int index = i;
                                menu.AddItem( new GUIContent( label ), false, ( ) => {
                                    var paste = CopyCache.GetCache<SearchPath>( );
                                    foreach ( var ex in targetlist ) {
                                        ex.excludes[index] = paste;
                                        EditorUtility.SetDirty( ex );
                                    }
                                } );
                            } else {
                                menu.AddDisabledItem( new GUIContent( ExporterTexts.PasteTargetNoValue ) );
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
                EditorGUI.indentLevel++;
                if ( GUILayout.Button( "+", GUILayout.Width( 60 ) ) ) {
                    foreach ( var item in targetlist ) {
                        ExporterUtils.ResizeList( item.excludes, minmax_count.max + 1, ( ) => new SearchPath( ) );
                        EditorUtility.SetDirty( item );
                    }
                }
                EditorGUI.indentLevel--;
            }
            // ↑ Excludes

            // ↓ Excludes Preview
            if ( ExporterUtils.EditorPrefFoldout( ExporterEditorPrefs.FOLDOUT_EXCLUDES_PREVIEW, ExporterTexts.FoldoutExcludesPreview ) ) {
                bool first = true;
                foreach ( var item in targetlist ) {
                    if ( first == false ) EditorGUILayout.Separator( );
                    first = false;
                    if ( multiple ) {
                        GUI.enabled = false;
                        EditorGUI.indentLevel++;
                        EditorGUILayout.ObjectField( item, typeof( MizoresPackageExporter ), false );
                        EditorGUI.indentLevel--;
                        GUI.enabled = true;
                    }
                    for ( int i = 0; i < minmax_count.max; i++ ) {
                        if ( multiple ) {
                            EditorGUI.indentLevel += 2;
                        } else {
                            EditorGUI.indentLevel++;
                        }
                        EditorGUILayout.LabelField( i.ToString( ), GUILayout.Width( 30 ) );
                        if ( multiple ) {
                            EditorGUI.indentLevel -= 2;
                        } else {
                            EditorGUI.indentLevel--;
                        }

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
            // ↑ Excludes Preview
        }
    }
#endif
}
