using System.Collections.Generic;
using UnityEngine;
using static MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterUtils;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor {
    public static class GUIElement_PackagePrefsElementList<TAsset, TElement> where TAsset : Object where TElement : ObjectRefElement, new() {

        public delegate List<TElement> GetListDelegate( MizoresPackageExporter t );

        public static void Draw( MizoresPackageExporter[] targetlist, GetListDelegate GetList ) {
            var t = targetlist[0];
            var tList = GetList( t );
            VerticalBoxScope.BeginVerticalBox( );
            MinMax objects_count = MinMax.Create( targetlist, v => GetList( v ).Count );
            bool multiple = targetlist.Length > 1;
            for ( int i = 0; i < objects_count.max; i++ ) {
                EditorGUILayout.BeginHorizontal( );
                // （複数インスタンス選択時）全てのオブジェクトの値が同じか
                bool samevalue_in_all = true;
                if ( multiple ) {
                    samevalue_in_all = i < objects_count.min && targetlist.All( v => {
                        var el1 = tList[i];
                        el1.exporter = t;
                        var el2 = GetList( v )[i];
                        el2.exporter = v;
                        return el1.GetObject( ) == el2.GetObject( );
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
                    element = tList[i];
                } else {
                    element = new ObjectRefElement( );
                }
                bool browse = PackagePrefsElementInspector.Draw<TAsset>( t, element );
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
                EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 3 ) );
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
                if ( objects_count.max == 0 ) {
                    break;
                }

                // ExportTargetObjectElementの場合
                var exportTargetObjectElement = element as ExportTargetObjectElement;
                if ( exportTargetObjectElement != null ) {
                    var useReferences = targetlist.Any( v => v.references.Count != 0 );
                    using ( new EditorGUI.DisabledScope( !useReferences ) ) {
                        // Search Reference
                        EditorGUI.indentLevel += 2;
                        var samevalue_searchReference = true;
                        if ( multiple ) {
                            samevalue_searchReference = i < objects_count.min && targetlist.All( v => ( tList[i] as ExportTargetObjectElement ).searchReference == ( GetList( v )[i] as ExportTargetObjectElement ).searchReference );
                        }
                        EditorGUI.BeginChangeCheck( );
                        EditorGUI.showMixedValue = !samevalue_searchReference;
                        var content = new GUIContent( ExporterTexts.SearchReference, ExporterTexts.SearchReferenceTooltip );
                        bool searchReference = EditorGUILayout.Toggle(content, ( tList[i] as ExportTargetObjectElement ).searchReference );
                        EditorGUI.showMixedValue = false;
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            foreach ( var item in targetlist ) {
                                ExporterUtils.ResizeList( tList, Mathf.Max( i + 1, tList.Count ), ( ) => new TElement( ) );
                                ( tList[i] as ExportTargetObjectElement ).searchReference = searchReference;
                                EditorUtility.SetDirty( item );
                            }
                            objects_count = MinMax.Create( targetlist, v => tList.Count );
                        }
                        EditorGUI.indentLevel -= 2;
                    }
                }

                // プレビュー
                for ( int j = 0; j < targetlist.Length; j++ ) {
                    var item = targetlist[j];
                    var list = GetList( item );
                    if ( list.Count <= i ) {
                        continue;
                    }
                    var el = list[i];
                    el.exporter = item;
                    var preview = el.GetConvertedPath();
                    EditorGUI.indentLevel += 2;
                    if ( multiple ) {
                        using ( new EditorGUI.DisabledScope( true ) ) {
                            EditorGUILayout.ObjectField( item, typeof( MizoresPackageExporter ), false );
                        }
                        EditorGUI.indentLevel++;
                    }
                    EditorGUILayout.LabelField( new GUIContent( preview, preview ) );
                    if ( multiple ) {
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
