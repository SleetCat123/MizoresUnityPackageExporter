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
            var objects = objectReferences.Where( v => EditorUtility.IsPersistent( v ) );
            foreach ( var exporter in targetlist ) {
                var add = objects.Select( v => new ReferenceElement( new ObjectRefElement( exporter, v ), ReferenceMode.Include ) );
                exporter.references.AddRange( add );
                EditorUtility.SetDirty( exporter );
            }
        }
        public static void Draw( MizoresPackageExporterEditor ed, MizoresPackageExporter t, MizoresPackageExporter[] targetlist ) {
            MinMax references_count = MinMax.Create( targetlist, v => v.references.Count );
            if ( CustomFoldout.EditorPrefFoldout(
                ExporterEditorPrefs.FOLDOUT_REFERENCES,
                new GUIContent( ExporterTexts.FoldoutReferences( references_count.ToString( ) ), ExporterTexts.FoldoutReferencesTooltip ),
                new CustomFoldout.FoldoutFuncs( ) {
                    canDragDrop = objectReferences => references_count.SameValue && ExporterUtils.Filter_HasPersistentObject( objectReferences ),
                    onDragPerform = ( objectReferences ) => AddObjects( targetlist, objectReferences ),
                    onRightClick = ( ) => GUIElement_CopyPasteList.OnRightClickFoldout( targetlist, ExporterTexts.FoldoutReferences, ( ex ) => ex.references, ( ex, list ) => ex.references = list )
                }
                ) ) {
                VerticalBoxScope.BeginVerticalBox( );
                DrawList( t, targetlist );
                VerticalBoxScope.EndVerticalBox( );
            }
        }
        static void DrawList( MizoresPackageExporter t, MizoresPackageExporter[] targetlist ) {
            MinMax objects_count = MinMax.Create( targetlist, v => v.references.Count );
            bool multiple = targetlist.Length > 1;
            for ( int i = 0; i < objects_count.max; i++ ) {
                EditorGUILayout.BeginHorizontal( );
                // （複数インスタンス選択時）全てのオブジェクトの値が同じか
                bool samevalue_in_all = true;
                if ( multiple ) {
                    samevalue_in_all = i < objects_count.min && targetlist.All( v => {
                        var el1 = t.references[i].element;
                        el1.exporter = t;
                        var el2 = v.references[i].element;
                        el2.exporter = v;
                        return el1.Object == el2.Object;
                        } );
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
                ObjectRefElement element;
                if ( samevalue_in_all ) {
                    element = t.references[i].element;
                } else {
                    element = new ObjectRefElement( );
                }

                EditorGUI.showMixedValue = !samevalue_in_all;
                bool browse = PackagePrefsElementInspector.Draw<Object>( t, element );
                EditorGUI.showMixedValue = false;
                if ( EditorGUI.EndChangeCheck( ) ) {
                    foreach ( var item in targetlist ) {
                        // 全ての選択中インスタンスに対してオブジェクトを設定
                        // 要素数が足りなかったらリサイズ
                        var refs = item.references;
                        ExporterUtils.ResizeList( refs, Mathf.Max( i + 1, refs.Count ), ( ) => new ReferenceElement( ) );
                        refs[i].element = new ObjectRefElement( element );
                        EditorUtility.SetDirty( item );
                    }
                    objects_count = MinMax.Create( targetlist, v => v.references.Count );
                }
                if ( browse ) {
                    // OpenFilePanelなどを使用した場合に以下のエラーが出るのでreturnして回避
                    // 'EndLayoutGroup: BeginLayoutGroup must be called first.'
                    return;
                }

                var samevalue_in_all_mode = samevalue_in_all && targetlist.All( v => t.references[i].mode == v.references[i].mode );
                ReferenceMode referenceMode;
                if ( samevalue_in_all_mode ) {
                    referenceMode = t.references[i].mode;
                } else {
                    referenceMode = ReferenceMode.Include;
                }
                EditorGUI.showMixedValue = !samevalue_in_all_mode;
                EditorGUI.BeginChangeCheck( );
                referenceMode = ( ReferenceMode )EditorGUILayout.EnumPopup( referenceMode, GUILayout.Width( 80 ) );
                EditorGUI.showMixedValue = false;
                if ( EditorGUI.EndChangeCheck( ) ) {
                    foreach ( var item in targetlist ) {
                        // 全ての選択中インスタンスに対してオブジェクトを設定
                        // 要素数が足りなかったらリサイズ
                        var refs = item.references;
                        ExporterUtils.ResizeList( refs, Mathf.Max( i + 1, refs.Count ), ( ) => new ReferenceElement( ) );
                        refs[i].mode = referenceMode;
                        EditorUtility.SetDirty( item );
                    }
                }

                // Button
                int index_after = GUIElement_Utils.UpDownButton( i, objects_count.max );
                if ( i != index_after ) {
                    foreach ( var item in targetlist ) {
                        var refs = item.references;
                        if ( refs.Count <= index_after ) {
                            ExporterUtils.ResizeList( refs, index_after + 1, ( ) => new ReferenceElement( ) );
                        }
                        refs.Swap( i, index_after );
                        EditorUtility.SetDirty( item );
                    }
                }
                EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 10 ) );
                if ( GUIElement_Utils.MinusButton( ) ) {
                    foreach ( var item in targetlist ) {
                        var refs = item.references;
                        ExporterUtils.ResizeList( refs, Mathf.Max( i + 1, refs.Count ), ( ) => new ReferenceElement( ) );
                        refs.RemoveAt( i );
                        EditorUtility.SetDirty( item );
                    }
                    objects_count = MinMax.Create( targetlist, v => v.references.Count );
                    i--;
                }
                EditorGUILayout.EndHorizontal( );

                // プレビュー
                for ( int j = 0; j < targetlist.Length; j++ ) {
                    var item = targetlist[j];
                    var el = item.references[i].element;
                    el.exporter = item;
                    var preview = el.Path;
                    if ( PathUtils.IsDynamicPath( preview ) ) {
                        preview = t.ConvertDynamicPath( preview );
                    }
                    if ( PathUtils.IsRelativePath( preview ) ) {
                        preview = PathUtils.GetProjectAbsolutePath( t.GetDirectoryPath( ), preview );
                    }
                    EditorGUI.indentLevel += 2;
                    if ( targetlist.Length > 1 ) {
                        using ( new EditorGUI.DisabledScope( true ) ) {
                            EditorGUILayout.ObjectField( item, typeof( MizoresPackageExporter ), false );
                        }
                        EditorGUI.indentLevel++;
                    }
                    EditorGUILayout.LabelField( new GUIContent( preview, preview ) );
                    if ( targetlist.Length > 1 ) {
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel -= 2;
                }
            }
            EditorGUI.indentLevel++;
            if ( GUIElement_Utils.PlusButton( ) ) {
                foreach ( var item in targetlist ) {
                    ExporterUtils.ResizeList( item.references, objects_count.max + 1, ( ) => new ReferenceElement( ) );
                    EditorUtility.SetDirty( item );
                }
            }
            EditorGUI.indentLevel--;
        }
    }
#endif
}