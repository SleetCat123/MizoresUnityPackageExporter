using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor {
#if UNITY_EDITOR
    public static class GUI_ReferencesObjects {
        public static void AddObjects( IEnumerable<MizoresPackageExporter> targetlist, Object[] objectReferences ) {
            var add = objectReferences.
                Where( v => EditorUtility.IsPersistent( v ) ).
                Select( v => new ReferenceElement( new PackagePrefsElement( v ), ReferenceMode.Include ) );
            foreach ( var item in targetlist ) {
                item.references2.AddRange( add );
                EditorUtility.SetDirty( item );
            }
        }
        public static void Draw( MizoresPackageExporterEditor ed, MizoresPackageExporter t, MizoresPackageExporter[] targetlist ) {
            MinMax references_count = MinMax.Create( targetlist, v => v.references2.Count );
            if ( ExporterUtils.EditorPrefFoldout(
                ExporterEditorPrefs.FOLDOUT_REFERENCES,
                new GUIContent( ExporterTexts.FoldoutReferences( references_count.ToString( ) ), ExporterTexts.FoldoutReferencesTooltip ),
                new ExporterUtils.FoldoutFuncs( ) {
                    canDragDrop = objectReferences => references_count.SameValue && ExporterUtils.Filter_HasPersistentObject( objectReferences ),
                    onDragPerform = ( objectReferences ) => AddObjects( targetlist, objectReferences ),
                    onRightClick = ( ) => GUIElement_CopyPasteList.OnRightClickFoldout( targetlist, ExporterTexts.FoldoutReferences, ( ex ) => ex.references2, ( ex, list ) => ex.references2 = list )
                }
                ) ) {
                DrawList( t, targetlist );
            }
        }
        static void DrawList( MizoresPackageExporter t, MizoresPackageExporter[] targetlist ) {
            MinMax objects_count = MinMax.Create( targetlist, v => v.references2.Count );
            bool multiple = targetlist.Length > 1;
            for ( int i = 0; i < objects_count.max; i++ ) {
                using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                    // （複数インスタンス選択時）全てのオブジェクトの値が同じか
                    bool samevalue_in_all = true;
                    if ( multiple ) {
                        samevalue_in_all = i < objects_count.min && targetlist.All( v => t.references2[i].element.Object == v.references2[i].element.Object );
                    }

                    EditorGUI.indentLevel++;
                    if ( samevalue_in_all ) {
                        EditorGUILayout.LabelField( i.ToString( ), GUILayout.Width( 30 ) );
                    } else {
                        // 一部オブジェクトの値が異なっていたらTextFieldの左に?を表示
                        ExporterUtils.DiffLabel( );
                    }
                    EditorGUI.indentLevel--;

                    EditorGUI.BeginChangeCheck( );
                    ReferenceElement element;
                    if ( samevalue_in_all ) {
                        element = t.references2[i];
                    } else {
                        element = new ReferenceElement( );
                    }

                    EditorGUI.showMixedValue = !samevalue_in_all;
                    PackagePrefsElementInspector.Draw<Object>( element.element );
                    EditorGUI.showMixedValue = false;

                    var samevalue_in_all_mode = samevalue_in_all && targetlist.All( v => t.references2[i].mode == v.references2[i].mode );
                    EditorGUI.showMixedValue = !samevalue_in_all_mode;
                    EditorGUI.BeginChangeCheck( );
                    var mode = ( ReferenceMode )EditorGUILayout.EnumPopup( t.references2[i].mode, GUILayout.Width( 80 ) );
                    EditorGUI.showMixedValue = false;
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        foreach ( var item in targetlist ) {
                            // 全ての選択中インスタンスに対してオブジェクトを設定
                            // 要素数が足りなかったらリサイズ
                            var refs = item.references2;
                            ExporterUtils.ResizeList( refs, Mathf.Max( i + 1, refs.Count ), ( ) => new ReferenceElement( ) );
                            refs[i].mode = mode;
                            EditorUtility.SetDirty( item );
                        }
                    }

                    if ( EditorGUI.EndChangeCheck( ) ) {
                        foreach ( var item in targetlist ) {
                            // 全ての選択中インスタンスに対してオブジェクトを設定
                            // 要素数が足りなかったらリサイズ
                            var refs = item.references2;
                            ExporterUtils.ResizeList( refs, Mathf.Max( i + 1, refs.Count ), ( ) => new ReferenceElement( ) );
                            refs[i] = new ReferenceElement( element );
                            objects_count = MinMax.Create( targetlist, v => v.references2.Count );
                            EditorUtility.SetDirty( item );
                        }
                    }

                    // Button
                    int index_after = ExporterUtils.UpDownButton( i, objects_count.max );
                    if ( i != index_after ) {
                        foreach ( var item in targetlist ) {
                            var refs = item.references2;
                            if ( refs.Count <= index_after ) {
                                ExporterUtils.ResizeList( refs, index_after + 1, ( ) => new ReferenceElement( ) );
                            }
                            refs.Swap( i, index_after );
                            EditorUtility.SetDirty( item );
                        }
                    }
                    EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 10 ) );
                    if ( GUILayout.Button( "-", GUILayout.Width( 15 ) ) ) {
                        foreach ( var item in targetlist ) {
                            var refs = item.references2;
                            ExporterUtils.ResizeList( refs, Mathf.Max( i + 1, refs.Count ), ( ) => new ReferenceElement( ) );
                            refs.RemoveAt( i );
                            objects_count = MinMax.Create( targetlist, v => v.references2.Count );
                            EditorUtility.SetDirty( item );
                        }
                        i--;
                    }
                }
            }
            EditorGUI.indentLevel++;
            if ( GUILayout.Button( "+", GUILayout.Width( 60 ) ) ) {
                foreach ( var item in targetlist ) {
                    ExporterUtils.ResizeList( item.references2, objects_count.max + 1, ( ) => new ReferenceElement( ) );
                    EditorUtility.SetDirty( item );
                }
            }
            EditorGUI.indentLevel--;
        }
    }
#endif
}