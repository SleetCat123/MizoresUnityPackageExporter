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
        public static void AddObjects( MizoresPackageExporter t, IEnumerable<MizoresPackageExporter> targetlist, Object[] objectReferences ) {
            var add = objectReferences.
                Where( v => EditorUtility.IsPersistent( v ) ).
                Select( v => new DynamicPathElement( AssetDatabase.GetAssetPath( v )) );
            if ( ExporterEditorPrefs.UseRelativePath ) {
                var dir = t.GetDirectoryPath( );
                add = add.Select( v => {
                    v.path = PathUtils.GetRelativePath( dir, v.path );
                    return v;
                } );
            }
            foreach ( var item in targetlist ) {
                item.dynamicpath2.AddRange( add );
                EditorUtility.SetDirty( item );
            }
        }

        public static void Draw( MizoresPackageExporterEditor ed, MizoresPackageExporter t, MizoresPackageExporter[] targetlist ) {
            var dpath_count = MinMax.Create( targetlist, v => v.dynamicpath2.Count );
            bool multiple = targetlist.Length > 1;
            // ↓ Dynamic Path
            if ( ExporterUtils.EditorPrefFoldout(
                ExporterEditorPrefs.FOLDOUT_DYNAMICPATH,
                ExporterTexts.FoldoutDynamicPath( dpath_count.ToString( ) ),
                new ExporterUtils.FoldoutFuncs( ) {
                    canDragDrop = objectReferences => dpath_count.SameValue && ExporterUtils.Filter_HasPersistentObject( objectReferences ),
                    onDragPerform = ( objectReferences ) => AddObjects( t, targetlist, objectReferences ),
                    onRightClick = ( ) => {
                        var menu = new GenericMenu( );
                        menu.AddItem( new GUIContent( ExporterTexts.ConvertAllPathsToAbsolute ), false, ( ) => {
                            foreach ( var item in targetlist ) {
                                for ( int i = 0; i < item.dynamicpath2.Count; i++ ) {
                                    var dir = item.GetDirectoryPath( );
                                    item.dynamicpath2[i].path = PathUtils.GetProjectAbsolutePath( dir, item.dynamicpath2[i].path );
                                }
                                EditorUtility.SetDirty( item );
                            }
                        } );
                        menu.AddItem( new GUIContent( ExporterTexts.ConvertAllPathsToRelative ), false, ( ) => {
                            foreach ( var item in targetlist ) {
                                for ( int i = 0; i < item.dynamicpath2.Count; i++ ) {
                                    var dir = item.GetDirectoryPath( );
                                    item.dynamicpath2[i].path = PathUtils.GetRelativePath( dir, item.dynamicpath2[i].path );
                                }
                                EditorUtility.SetDirty( item );
                            }
                        } );
                        GUIElement_CopyPasteList.OnRightClickFoldout( targetlist, ExporterTexts.FoldoutDynamicPath, ( ex ) => ex.dynamicpath2, ( ex, list ) => ex.dynamicpath2 = list, menu );
                    }
                }
                ) ) {
                for ( int i = 0; i < dpath_count.max; i++ ) {
                    using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                        // 全てのオブジェクトの値が同じか
                        bool samevalue_in_all = true;
                        if ( multiple ) {
                            samevalue_in_all = i < dpath_count.min && targetlist.All( v => t.dynamicpath2[i].path == v.dynamicpath2[i].path );
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
                                if ( PathUtils.IsRelativePath( t.dynamicpath2[i].path ) ) {
                                    menu.AddItem( new GUIContent( ExporterTexts.ConvertToAbsolutePath ), false, ( ) => {
                                        foreach ( var item in targetlist ) {
                                            if ( index < item.dynamicpath2.Count ) {
                                                var dir = item.GetDirectoryPath( );
                                                item.dynamicpath2[index].path = PathUtils.GetProjectAbsolutePath( dir, item.dynamicpath2[index].path );
                                                EditorUtility.SetDirty( item );
                                            }
                                        }
                                        GUI.FocusControl( null );
                                        EditorGUI.FocusTextInControl( null );
                                    } );
                                } else {
                                    menu.AddItem( new GUIContent( ExporterTexts.ConvertToRelativePath ), false, ( ) => {
                                        foreach ( var item in targetlist ) {
                                            if ( index < item.dynamicpath2.Count ) {
                                                var dir = item.GetDirectoryPath( );
                                                item.dynamicpath2[index].path = PathUtils.GetRelativePath( dir, item.dynamicpath2[index].path );
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
                            path = EditorGUI.TextField( textrect, t.dynamicpath2[i].path );
                        } else {
                            EditorGUI.showMixedValue = true;
                            path = EditorGUI.TextField( textrect, string.Empty );
                            EditorGUI.showMixedValue = false;
                        }
                        bool browse = GUIElement_Utils.BrowseButtons( t, path, out string browseResult, GUIElement_Utils.BrowseType.FileAndFolder );
                        if ( browse ) {
                            path = browseResult;
                        }
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
                                ExporterUtils.ResizeList( item.dynamicpath2, Mathf.Max( i + 1, item.dynamicpath2.Count ), ( ) => new DynamicPathElement( ) );
                                item.dynamicpath2[i].path = path;
                                EditorUtility.SetDirty( item );
                            }
                            dpath_count = MinMax.Create( targetlist, v => v.dynamicpath2.Count );
                        }

                        // Button
                        int index_after = GUIElement_Utils.UpDownButton( i, dpath_count.max );
                        if ( i != index_after ) {
                            foreach ( var item in targetlist ) {
                                if ( item.dynamicpath2.Count <= index_after ) {
                                    ExporterUtils.ResizeList( item.dynamicpath2, index_after + 1 );
                                }
                                item.dynamicpath2.Swap( i, index_after );
                                EditorUtility.SetDirty( item );
                            }
                        }
                        EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 10 ) );
                        if ( GUIElement_Utils.MinusButton( ) ) {
                            foreach ( var item in targetlist ) {
                                ExporterUtils.ResizeList( item.dynamicpath2, Mathf.Max( i + 1, item.dynamicpath2.Count ) );
                                item.dynamicpath2.RemoveAt( i );
                                EditorUtility.SetDirty( item );
                            }
                            dpath_count = MinMax.Create( targetlist, v => v.dynamicpath2.Count );
                            i--;
                            if ( i == -1 ) {
                                break;
                            }
                        }
                    }
                    var useReferences = targetlist.Any( v => v.references2.Count != 0 );
                    using ( new EditorGUI.DisabledScope( !useReferences ) ) {
                        // Search Reference
                        using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 30 ) );
                            EditorGUI.indentLevel--;
                            var samevalue_searchReference = true;
                            if ( multiple ) {
                                samevalue_searchReference = i < dpath_count.min && targetlist.All( v => t.dynamicpath2[i].searchReference == v.dynamicpath2[i].searchReference );
                            }
                            EditorGUI.BeginChangeCheck( );
                            EditorGUI.showMixedValue = !samevalue_searchReference;
                            var content = new GUIContent( ExporterTexts.SearchReference, ExporterTexts.SearchReferenceTooltip );
                            bool searchReference = EditorGUILayout.Toggle(content, t.dynamicpath2[i].searchReference );
                            EditorGUI.showMixedValue = false;
                            if ( EditorGUI.EndChangeCheck( ) ) {
                                foreach ( var item in targetlist ) {
                                    ExporterUtils.ResizeList( item.dynamicpath2, Mathf.Max( i + 1, item.dynamicpath2.Count ), ( ) => new DynamicPathElement( ) );
                                    item.dynamicpath2[i].searchReference = searchReference;
                                    EditorUtility.SetDirty( item );
                                }
                                dpath_count = MinMax.Create( targetlist, v => v.dynamicpath2.Count );
                            }
                        }
                    }
                }
                EditorGUI.indentLevel++;
                if ( GUIElement_Utils.PlusButton( ) ) {
                    foreach ( var item in targetlist ) {
                        ExporterUtils.ResizeList( item.dynamicpath2, dpath_count.max + 1, ( ) => new DynamicPathElement( ) );
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
                    using ( new VerticalBoxScope( ) ) {
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
                                if ( i < item.dynamicpath2.Count ) {
                                    string previewpath = item.ConvertDynamicPath( item.dynamicpath2[i].path, true );
                                    if ( PathUtils.IsRelativePath( previewpath ) ) {
                                        var dir = item.GetDirectoryPath( );
                                        previewpath = PathUtils.GetProjectAbsolutePath( dir, previewpath );
                                    }
                                    EditorGUILayout.LabelField( new GUIContent( previewpath, previewpath ) );
                                    // Assetが存在するならObjectFieldで表示
                                    var obj = AssetDatabase.LoadAssetAtPath<Object>( previewpath );
                                    if ( obj != null ) {
                                        using ( new EditorGUI.DisabledScope( true ) ) {
                                            EditorGUILayout.ObjectField( obj, typeof( Object ), false, GUILayout.Width( 120 ) );
                                        }
                                    }
                                } else {
                                    EditorGUILayout.LabelField( "-" );
                                }
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
