using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor {

#if UNITY_EDITOR
    public static class GUI_DynamicPath {
        public static void AddObjects( MizoresPackageExporter t, IEnumerable<MizoresPackageExporter> targetlist, System.Func<MizoresPackageExporter, List<string>> getList, Object[] objectReferences ) {
            var add = objectReferences.
                Where( v => EditorUtility.IsPersistent( v ) ).
                Select( v => AssetDatabase.GetAssetPath( v ) );
            if ( ExporterEditorPrefs.UseRelativePath ) {
                var dir = t.GetDirectoryPath( );
                add = add.Select( v => PathUtils.GetRelativePath( dir, v ) );
            }
            foreach ( var item in targetlist ) {
                getList( item ).AddRange( add );
                EditorUtility.SetDirty( item );
            }
        }

        public static void Draw( MizoresPackageExporterEditor ed, MizoresPackageExporter t, MizoresPackageExporter[] targetlist ) {
            var dpath_count = MinMax.Create( targetlist, v => v.dynamicpath.Count );
            bool multiple = targetlist.Length > 1;
            // ↓ Dynamic Path
            if ( ExporterUtils.EditorPrefFoldout(
                ExporterEditorPrefs.FOLDOUT_DYNAMICPATH,
                ExporterTexts.FoldoutDynamicPath( dpath_count.ToString( ) ),
                new ExporterUtils.FoldoutFuncs( ) {
                    canDragDrop = objectReferences => dpath_count.SameValue && ExporterUtils.Filter_HasPersistentObject( objectReferences ),
                    onDragPerform = ( objectReferences ) => AddObjects( t, targetlist, v => v.dynamicpath, objectReferences ),
                    onRightClick = ( ) => {
                        var menu = new GenericMenu( );
                        menu.AddItem( new GUIContent( ExporterTexts.ConvertAllPathsToAbsolute ), false, ( ) => {
                            foreach ( var item in targetlist ) {
                                for ( int i = 0; i < item.dynamicpath.Count; i++ ) {
                                    var dir = item.GetDirectoryPath( );
                                    item.dynamicpath[i] = PathUtils.GetProjectAbsolutePath( dir, item.dynamicpath[i] );
                                }
                                EditorUtility.SetDirty( item );
                            }
                        } );
                        menu.AddItem( new GUIContent( ExporterTexts.ConvertAllPathsToRelative ), false, ( ) => {
                            foreach ( var item in targetlist ) {
                                for ( int i = 0; i < item.dynamicpath.Count; i++ ) {
                                    var dir = item.GetDirectoryPath( );
                                    item.dynamicpath[i] = PathUtils.GetRelativePath( dir, item.dynamicpath[i] );
                                }
                                EditorUtility.SetDirty( item );
                            }
                        } );
                        GUIElement_CopyPasteList.OnRightClickFoldout<string>( targetlist, ExporterTexts.FoldoutDynamicPath, ( ex ) => ex.dynamicpath, ( ex, list ) => ex.dynamicpath = list, menu );
                        }
                }
                ) ) {
                for ( int i = 0; i < dpath_count.max; i++ ) {
                    using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                        // 全てのオブジェクトの値が同じか
                        bool samevalue_in_all = true;
                        if ( multiple ) {
                            samevalue_in_all = i < dpath_count.min && targetlist.All( v => t.dynamicpath[i] == v.dynamicpath[i] );
                        }

                        EditorGUI.indentLevel++;
                        if ( samevalue_in_all ) {
                            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width( 30 ) );
                            EditorGUI.LabelField( rect, i.ToString( ) );

                            Event currentEvent = Event.current;
                            if ( currentEvent.type == EventType.ContextClick && rect.Contains( currentEvent.mousePosition ) ) {
                                // 絶対パス／相対パスの変換
                                GenericMenu menu = new GenericMenu( );
                                int index= i;
                                if ( PathUtils.IsRelativePath( t.dynamicpath[i] ) ) {
                                    menu.AddItem( new GUIContent( ExporterTexts.ConvertToAbsolutePath ), false, ( ) => {
                                        foreach ( var item in targetlist ) {
                                            if ( index < item.dynamicpath.Count ) {
                                                var dir = item.GetDirectoryPath( );
                                                item.dynamicpath[index] = PathUtils.GetProjectAbsolutePath( dir, item.dynamicpath[index] );
                                                EditorUtility.SetDirty( item );
                                            }
                                        }
                                        GUI.FocusControl( null );
                                        EditorGUI.FocusTextInControl( null );
                                    } );
                                } else {
                                    menu.AddItem( new GUIContent( ExporterTexts.ConvertToRelativePath ), false, ( ) => {
                                        foreach ( var item in targetlist ) {
                                            if ( index < item.dynamicpath.Count ) {
                                                var dir = item.GetDirectoryPath( );
                                                item.dynamicpath[index] = PathUtils.GetRelativePath( dir, item.dynamicpath[index] );
                                                EditorUtility.SetDirty( item );
                                            }
                                        }
                                        GUI.FocusControl( null );
                                        EditorGUI.FocusTextInControl( null );
                                    } );
                                }
                                menu.ShowAsContext( );
                                currentEvent.Use( );
                            }
                        } else {
                            // 一部オブジェクトの値が異なっていたらTextFieldの左に?を表示
                            ExporterUtils.DiffLabel( );
                        }
                        EditorGUI.indentLevel--;

                        EditorGUI.BeginChangeCheck( );
                        Rect textrect = EditorGUILayout.GetControlRect( );
                        string path;
                        if ( samevalue_in_all ) {
                            path = EditorGUI.TextField( textrect, t.dynamicpath[i] );
                        } else {
                            EditorGUI.showMixedValue = true;
                            path = EditorGUI.TextField( textrect, string.Empty );
                            EditorGUI.showMixedValue = false;
                        }
                        path = GUIElement_Utils.BrowseButtons( t, path );
                        if ( ExporterUtils.DragDrop( textrect, ExporterUtils.Filter_HasPersistentObject ) ) {
                            GUI.changed = true;
                            path = AssetDatabase.GetAssetPath( DragAndDrop.objectReferences[0] );
                            if ( ExporterEditorPrefs.UseRelativePath ) {
                                var dir = t.GetDirectoryPath( );
                                path = PathUtils.GetRelativePath( dir, path );
                            }
                        }

                        if ( EditorGUI.EndChangeCheck( ) ) {
                            path = PathUtils.ToValidPath( path );
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
                EditorGUI.indentLevel++;
                if ( GUILayout.Button( "+", GUILayout.Width( 60 ) ) ) {
                    foreach ( var item in targetlist ) {
                        ExporterUtils.ResizeList( item.dynamicpath, dpath_count.max + 1, ( ) => string.Empty );
                        EditorUtility.SetDirty( item );
                    }
                }
                EditorGUI.indentLevel--;
            }
            // ↑ Dynamic Path

            // ↓ Dynamic Path Preview
            if ( ExporterUtils.EditorPrefFoldout( ExporterEditorPrefs.FOLDOUT_DYNAMICPATH_PREVIEW, ExporterTexts.FoldoutDynamicPathPreview ) ) {
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
                    for ( int i = 0; i < dpath_count.max; i++ ) {
                        using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                            var indent = EditorGUI.indentLevel;
                            if ( multiple ) {
                                EditorGUI.indentLevel += 2;
                            } else {
                                EditorGUI.indentLevel++;
                            }
                            EditorGUILayout.LabelField( i.ToString( ), GUILayout.Width( 30 ) );
                            EditorGUI.indentLevel = indent;
                            if ( i < item.dynamicpath.Count ) {
                                string previewpath = item.ConvertDynamicPath( item.dynamicpath[i], true );
                                if ( PathUtils.IsRelativePath( previewpath ) ) {
                                    var dir = item.GetDirectoryPath( );
                                    previewpath = PathUtils.GetProjectAbsolutePath( dir, previewpath );
                                }
                                EditorGUILayout.LabelField( new GUIContent( previewpath, previewpath ) );
                            } else {
                                EditorGUILayout.LabelField( "-" );
                            }
                        }
                    }
                }
            }
            // ↑ Dynamic Path Preview
        }
    }
#endif
}
