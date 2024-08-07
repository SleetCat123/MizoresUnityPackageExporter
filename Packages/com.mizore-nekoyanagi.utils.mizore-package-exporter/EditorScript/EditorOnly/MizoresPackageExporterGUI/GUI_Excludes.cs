using UnityEngine;
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
            if ( CustomFoldout.EditorPrefFoldout(
                ExporterEditorPrefs.FOLDOUT_EXCLUDES,
                ExporterTexts.FoldoutExcludes( minmax_count.ToString( ) ),
                new CustomFoldout.FoldoutFuncs( ) {
                    canDragDrop = objectReferences => minmax_count.SameValue && ExporterUtils.Filter_HasPersistentObject( objectReferences ),
                    onDragPerform = ( objectReferences ) => AddObjects( targetlist, v => v.excludes, objectReferences ),
                    onRightClick = ( ) => GUIElement_CopyPasteList.OnRightClickFoldout<SearchPath>( targetlist, ExporterTexts.FoldoutExcludes, ( ex ) => ex.excludes, ( ex, list ) => ex.excludes = list )
                }
                ) ) {
                VerticalBoxScope.BeginVerticalBox( );
                Event currentEvent = Event.current;
                for ( int i = 0; i < minmax_count.max; i++ ) {
                    var scopeRect = EditorGUILayout.BeginHorizontal( );
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

                    EditorGUI.BeginChangeCheck( );
                    Rect textrect = EditorGUILayout.GetControlRect( GUILayout.MinWidth( 30 ) );
                    string filter;
                    if ( samevalue_in_all_value ) {
                        filter = EditorGUI.TextField( textrect, t.excludes[i].value );
                    } else {
                        EditorGUI.showMixedValue = true;
                        filter = EditorGUI.TextField( textrect, string.Empty );
                        EditorGUI.showMixedValue = false;
                    }
                    var element = new ObjectRefElement( filter );
                    bool browse = GUIElement_Utils.BrowseButtons( t, ref element, GUIElement_Utils.BrowseType.FileAndFolder );
                    if ( browse ) {
                        if ( element.IsGUID ) {
                            filter = AssetDatabase.GUIDToAssetPath( element.Path );
                        } else {
                            filter = element.Path;
                        }
                    }
                    if ( ExporterUtils.DragDrop( textrect, ExporterUtils.Filter_HasPersistentObject ) ) {
                        filter = AssetDatabase.GetAssetPath( DragAndDrop.objectReferences[0] );
                        switch ( ExporterEditorPrefs.DefaultPathType ) {
                            case PathType.GUID:
                            case PathType.Absolute:
                                break;
                            case PathType.Relative:
                                var dir = t.GetDirectoryPath( );
                                filter = PathUtils.GetRelativePath( dir, filter );
                                break;
                        }
                        GUI.changed = true;
                    }
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        filter = PathUtils.ToValidPath( filter );
                        foreach ( var item in targetlist ) {
                            ExporterUtils.ResizeList( item.excludes, Mathf.Max( i + 1, item.excludes.Count ), ( ) => new SearchPath( ) );
                            item.excludes[i].value = filter;
                            EditorUtility.SetDirty( item );
                        }
                        minmax_count = MinMax.Create( targetlist, v => v.excludes.Count );
                    }
                    if ( browse ) {
                        // OpenFilePanelなどを使用した場合に以下のエラーが出るのでreturnして回避
                        // 'EndLayoutGroup: BeginLayoutGroup must be called first.'
                        return;
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
                            EditorUtility.SetDirty( item );
                        }
                        minmax_count = MinMax.Create( targetlist, v => v.excludes.Count );
                    }

                    // Copy&Paste
                    if ( currentEvent.type == EventType.ContextClick && scopeRect.Contains( currentEvent.mousePosition ) ) {
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
                    int index_after = GUIElement_Utils.UpDownButton( i, minmax_count.max );
                    if ( i != index_after ) {
                        foreach ( var item in targetlist ) {
                            if ( item.excludes.Count <= index_after ) {
                                ExporterUtils.ResizeList( item.excludes, index_after + 1, ( ) => new SearchPath( ) );
                            }
                            item.excludes.Swap( i, index_after );
                            EditorUtility.SetDirty( item );
                        }
                    }
                    EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 3 ) );
                    if ( GUIElement_Utils.MinusButton( ) ) {
                        foreach ( var item in targetlist ) {
                            ExporterUtils.ResizeList( item.excludes, Mathf.Max( i + 1, item.excludes.Count ), ( ) => new SearchPath( ) );
                            item.excludes.RemoveAt( i );
                            EditorUtility.SetDirty( item );
                        }
                        minmax_count = MinMax.Create( targetlist, v => v.excludes.Count );
                        i--;
                    }
                    EditorGUILayout.EndHorizontal( );

                    // プレビュー
                    for ( int j = 0; j < targetlist.Length; j++ ) {
                        var item = targetlist[j];
                        if ( item.excludes.Count <= i ) {
                            continue;
                        }
                        var el = item.excludes[i];
                        EditorGUI.indentLevel += 2;
                        if ( multiple ) {
                            EditorGUI.BeginDisabledGroup( true );
                            EditorGUILayout.ObjectField( item, typeof( MizoresPackageExporter ), false );
                            EditorGUI.EndDisabledGroup( );
                            EditorGUI.indentLevel++;
                        }
                        EditorGUILayout.BeginHorizontal( );
                        string previewpath = item.ConvertDynamicPath( item.excludes[i].value, ExporterConsts.BATCH_EXPORT_KEY_DUMMY );
                        EditorGUILayout.LabelField( new GUIContent( previewpath, previewpath ) );
                        if ( multiple ) {
                            EditorGUI.indentLevel--;
                        }
                        EditorGUI.indentLevel -= 2;
                        if ( multiple ) {
                            using ( new EditorGUI.DisabledGroupScope( true ) ) {
                                EditorGUILayout.EnumPopup( item.excludes[i].searchType, GUILayout.Width( 140 ) );
                            }
                        }
                        EditorGUILayout.EndHorizontal( );
                    }
                }
                EditorGUI.indentLevel++;
                if ( GUIElement_Utils.PlusButton( ) ) {
                    foreach ( var item in targetlist ) {
                        ExporterUtils.ResizeList( item.excludes, minmax_count.max + 1, ( ) => new SearchPath( ) );
                        EditorUtility.SetDirty( item );
                    }
                }
                EditorGUI.indentLevel--;
                VerticalBoxScope.EndVerticalBox( );
            }
        }
    }
#endif
}
