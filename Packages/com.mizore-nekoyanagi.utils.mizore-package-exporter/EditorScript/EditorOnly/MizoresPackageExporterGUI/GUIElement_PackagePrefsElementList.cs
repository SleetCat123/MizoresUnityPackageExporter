using System.Collections.Generic;
using UnityEngine;
using static MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterUtils;
using System.Linq;
using System.Runtime.InteropServices;


#if UNITY_EDITOR
using UnityEditor;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor {
    public class GUIElement_PackagePrefsElementList<T, TElement> where T : Object where TElement : ObjectRefElement, new() {
        System.Func<MizoresPackageExporter, List<TElement>> getList;

        public GUIElement_PackagePrefsElementList( System.Func<MizoresPackageExporter, List<TElement>> getList ) {
            this.getList = getList;
        }

        public List<TElement> GetList( MizoresPackageExporter t ) {
            return getList( t );
        }

        public void Draw( MizoresPackageExporter t, MizoresPackageExporter[] targetlist ) {
            VerticalBoxScope.BeginVerticalBox( );
            MinMax objects_count = MinMax.Create( targetlist, v => GetList( v ).Count );
            bool multiple = targetlist.Length > 1;
            for ( int i = 0; i < objects_count.max; i++ ) {
                EditorGUILayout.BeginHorizontal( );
                // （複数インスタンス選択時）全てのオブジェクトの値が同じか
                bool samevalue_in_all = true;
                if ( multiple ) {
                    samevalue_in_all = i < objects_count.min && targetlist.All( v => {
                        var el1 = GetList( t )[i];
                        el1.exporter = t;
                        var el2 = GetList( v )[i];
                        el2.exporter = v;
                        return el1.Object == el2.Object;
                        } );
                }

                EditorGUI.indentLevel++;
                if ( samevalue_in_all ) {
                    EditorGUILayout.LabelField( i.ToString( ), GUILayout.Width( 30 ) );
                } else {
                    // 一部オブジェクトの値が異なっていたらTextFieldの左に?を表示
                    DiffLabel( );
                }
                EditorGUI.indentLevel--;

                EditorGUI.showMixedValue = !samevalue_in_all;
                EditorGUI.BeginChangeCheck( );
                ObjectRefElement element;
                if ( samevalue_in_all ) {
                    element = GetList( t )[i];
                } else {
                    element = new ObjectRefElement( );
                }
                bool browse = PackagePrefsElementInspector.Draw<T>( t, element );
                EditorGUI.showMixedValue = false;
                if ( EditorGUI.EndChangeCheck( ) ) {
                    var path = element.Path;
                    foreach ( var item in targetlist ) {
                        // 全ての選択中インスタンスに対してパスを設定
                        // 要素数が足りなかったらリサイズ
                        ExporterUtils.ResizeList( GetList( item ), Mathf.Max( i + 1, GetList( item ).Count ), ( ) => new TElement( ) );
                        GetList( item )[i].Path = path;
                        EditorUtility.SetDirty( item );
                    }
                    objects_count = MinMax.Create( targetlist, v => GetList( v ).Count );
                }
                if ( browse ) {
                    // OpenFilePanelなどを使用した場合に以下のエラーが出るのでreturnして回避
                    // 'EndLayoutGroup: BeginLayoutGroup must be called first.'
                    return;
                }

                // Button
                int index_after = GUIElement_Utils.UpDownButton( i, objects_count.max );
                if ( i != index_after ) {
                    foreach ( var item in targetlist ) {
                        if ( GetList( item ).Count <= index_after ) {
                            ExporterUtils.ResizeList( GetList( item ), index_after + 1, ( ) => new TElement( ) );
                        }
                        GetList( item ).Swap( i, index_after );
                        EditorUtility.SetDirty( item );
                    }
                }
                EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 10 ) );
                if ( GUIElement_Utils.MinusButton( ) ) {
                    foreach ( var item in targetlist ) {
                        ExporterUtils.ResizeList( GetList( item ), Mathf.Max( i + 1, GetList( item ).Count ), ( ) => new TElement( ) );
                        GetList( item ).RemoveAt( i );
                        EditorUtility.SetDirty( item );
                    }
                    objects_count = MinMax.Create( targetlist, v => GetList( v ).Count );
                    i--;
                }
                EditorGUILayout.EndHorizontal( );

                // ExportTargetObjectElementの場合
                var exportTargetObjectElement = element as ExportTargetObjectElement;
                if ( exportTargetObjectElement != null ) {
                    var useReferences = targetlist.Any( v => v.references.Count != 0 );
                    using ( new EditorGUI.DisabledScope( !useReferences ) ) {
                        // Search Reference
                        EditorGUI.indentLevel += 2;
                        var samevalue_searchReference = true;
                        if ( multiple ) {
                            samevalue_searchReference = i < objects_count.min && targetlist.All( v => t.objects[i].searchReference == v.objects[i].searchReference );
                        }
                        EditorGUI.BeginChangeCheck( );
                        EditorGUI.showMixedValue = !samevalue_searchReference;
                        var content = new GUIContent( ExporterTexts.SearchReference, ExporterTexts.SearchReferenceTooltip );
                        bool searchReference = EditorGUILayout.Toggle(content, t.objects[i].searchReference );
                        EditorGUI.showMixedValue = false;
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            foreach ( var item in targetlist ) {
                                ExporterUtils.ResizeList( item.objects, Mathf.Max( i + 1, item.objects.Count ), ( ) => new ExportTargetObjectElement( ) );
                                item.objects[i].searchReference = searchReference;
                                EditorUtility.SetDirty( item );
                            }
                            objects_count = MinMax.Create( targetlist, v => v.objects.Count );
                        }
                        EditorGUI.indentLevel -= 2;
                    }
                }

                // プレビュー
                for ( int j = 0; j < targetlist.Length; j++ ) {
                    var item = targetlist[j];
                    var el = GetList( item )[i];
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
                    ExporterUtils.ResizeList( GetList( item ), objects_count.max + 1, ( ) => new TElement( ) );
                    EditorUtility.SetDirty( item );
                }
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical( );
        }
    }
}
#endif
