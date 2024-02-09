using System.Collections.Generic;
using UnityEngine;
using static MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterUtils;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor {
    public class GUIElement_PackagePrefsElementList<T, TElement> where T : Object where TElement : PackagePrefsElement, new() {
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
                    samevalue_in_all = i < objects_count.min && targetlist.All( v => GetList( t )[i].Object == GetList( v )[i].Object );
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
                PackagePrefsElement element;
                if ( samevalue_in_all ) {
                    element = GetList( t )[i];
                } else {
                    element = new PackagePrefsElement( );
                }
                bool browse = PackagePrefsElementInspector.Draw<T>( t, element );
                EditorGUI.showMixedValue = false;
                if ( EditorGUI.EndChangeCheck( ) ) {
                    var obj = element.Object;
                    foreach ( var item in targetlist ) {
                        // 全ての選択中インスタンスに対してオブジェクトを設定
                        // 要素数が足りなかったらリサイズ
                        ExporterUtils.ResizeList( GetList( item ), Mathf.Max( i + 1, GetList( item ).Count ), ( ) => new TElement( ) );
                        GetList( item )[i].Object = obj;
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
                    var useReferences = targetlist.Any( v => v.references2.Count != 0 );
                    using ( new EditorGUI.DisabledScope( !useReferences ) ) {
                        // Search Reference
                        using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 30 ) );
                            EditorGUI.indentLevel--;
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
                        }
                    }
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
