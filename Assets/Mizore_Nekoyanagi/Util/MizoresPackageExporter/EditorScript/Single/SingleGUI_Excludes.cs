using UnityEngine;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.MizoresPackageExporterConsts;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.SingleEditor
{
    public static class SingleGUI_Excludes
    {
        static void AddObjects( MizoresPackageExporter t, List<SearchPath> list, Object[] objectReferences ) {
            list.AddRange(
                objectReferences.
                Where( v => EditorUtility.IsPersistent( v ) ).
                Select( v => new SearchPath( SearchPathType.Exact, AssetDatabase.GetAssetPath( v ) ) )
                );
            EditorUtility.SetDirty( t );
        }
        public static void Draw( MizoresPackageExporter t ) {
            // ↓ Excludes
            if ( ExporterUtils.EditorPrefFoldout(
                Const.EDITOR_PREF_FOLDOUT_EXCLUDES,
                string.Format( ExporterTexts.t_Excludes, t.excludes.Count ),
                new ExporterUtils.FoldoutFuncs( ) {
                    canDragDrop = ExporterUtils.Filter_HasPersistentObject,
                    onDragPerform = ( objectReferences ) => AddObjects( t, t.excludes, objectReferences ),
                    onRightClick = ( ) => SingleGUIElement_CopyPaste.OnRightClickFoldout( t, ExporterTexts.t_Excludes, t.excludes, ( list ) => t.excludes = list )
                }
                ) ) {
                Event currentEvent = Event.current;
                for ( int i = 0; i < t.excludes.Count; i++ ) {
                    using ( var scope = new EditorGUILayout.HorizontalScope( ) ) {
                        SearchPath item = t.excludes[i];
                        ExporterUtils.Indent( 1 );
                        EditorGUILayout.LabelField( i.ToString( ), GUILayout.Width( 30 ) );

                        EditorGUI.BeginChangeCheck( );
                        using ( new EditorGUILayout.HorizontalScope( ) ) {
                            Rect textrect = EditorGUILayout.GetControlRect( );
                            item.value = EditorGUI.TextField( textrect, item.value );
                            item.searchType = (SearchPathType)EditorGUILayout.EnumPopup( item.searchType, GUILayout.Width( 70 ) );
                            if ( ExporterUtils.DragDrop( textrect, ExporterUtils.Filter_HasPersistentObject ) ) {
                                GUI.changed = true;
                                item.value = AssetDatabase.GetAssetPath( DragAndDrop.objectReferences[0] );
                            }
                        }
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            EditorUtility.SetDirty( t );
                        }

                        // Copy&Paste
                        if ( currentEvent.type == EventType.ContextClick && scope.rect.Contains( currentEvent.mousePosition ) ) {
                            GenericMenu menu = new GenericMenu( );
                            {
                                string label = string.Format( ExporterTexts.t_CopyTargetWithValue, item.GetType( ).Name, item.ToString( ) );
                                menu.AddItem( new GUIContent( label ), false, CopyCache.Copy, item );
                            }
                            if ( CopyCache.CanPaste<SearchPath>( ) ) {
                                string label = string.Format( ExporterTexts.t_PasteTargetWithValue, item.GetType( ).Name, item.ToString( ) );
                                int index = i;
                                menu.AddItem( new GUIContent( label ), false, ( ) => {
                                    item = CopyCache.GetCache<SearchPath>( );
                                    t.excludes[index] = item;
                                    EditorUtility.SetDirty( t );
                                } );
                            } else {
                                menu.AddDisabledItem( new GUIContent( ExporterTexts.t_PasteTargetNoValue ) );
                            }
                            menu.ShowAsContext( );

                            currentEvent.Use( );
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

            // ↓ Excludes Preview
            if ( ExporterUtils.EditorPrefFoldout( Const.EDITOR_PREF_FOLDOUT_EXCLUDES_PREVIEW, ExporterTexts.t_ExcludesPreview ) ) {
                for ( int i = 0; i < t.excludes.Count; i++ ) {
                    using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                        // プレビュー
                        string previewpath = t.ConvertDynamicPath( t.excludes[i].value );
                        ExporterUtils.Indent( 1 );
                        EditorGUILayout.LabelField( i.ToString( ), GUILayout.Width( 30 ) );

                        EditorGUILayout.LabelField( new GUIContent( previewpath, previewpath ) );
                        using ( new EditorGUI.DisabledGroupScope( true ) ) {
                            EditorGUILayout.EnumPopup( t.excludes[i].searchType, GUILayout.Width( 140 ) );
                        }
                    }
                }
            }
            // ↑ Excludes Preview
        }
    }
}
#endif
