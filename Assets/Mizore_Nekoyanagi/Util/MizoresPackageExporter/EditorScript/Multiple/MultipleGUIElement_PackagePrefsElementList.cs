using System.Collections.Generic;
using UnityEngine;
using static MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterUtils;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.MultipleEditor
{
    public static class MultipleGUIElement_PackagePrefsElementList {
        public static void Draw<T>( MizoresPackageExporter t, IEnumerable<MizoresPackageExporter> targetlist, System.Func<MizoresPackageExporter, List<PackagePrefsElement>> getlist ) where T : UnityEngine.Object {
            MinMax objects_count = MinMax.Create( targetlist, v => getlist( v ).Count );
            for ( int i = 0; i < objects_count.max; i++ ) {
                using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                    // （複数インスタンス選択時）全てのオブジェクトの値が同じか
                    bool samevalue_in_all = i < objects_count.min && targetlist.All( v => getlist( t )[i].Object == getlist( v )[i].Object );

                    ExporterUtils.Indent( 1 );
                    if ( samevalue_in_all ) {
                        EditorGUILayout.LabelField( i.ToString( ), GUILayout.Width( 30 ) );
                    } else {
                        // 一部オブジェクトの値が異なっていたらTextFieldの左に?を表示
                        DiffLabel( );
                    }

                    EditorGUI.BeginChangeCheck( );
                    Object obj;
                    if ( samevalue_in_all ) {
                        obj = EditorGUILayout.ObjectField( getlist( t )[i].Object, typeof( T ), false );
                    } else {
                        obj = EditorGUILayout.ObjectField( null, typeof( T ), false );
                    }
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        foreach ( var item in targetlist ) {
                            // 全ての選択中インスタンスに対してオブジェクトを設定
                            // DynamicPathの要素数が足りなかったらリサイズ
                            ExporterUtils.ResizeList( getlist( item ), Mathf.Max( i + 1, getlist( item ).Count ), ( ) => new PackagePrefsElement( ) );
                            getlist( item )[i].Object = obj;
                            objects_count = MinMax.Create( targetlist, v => getlist( v ).Count );
                            EditorUtility.SetDirty( item );
                        }
                    }

                    EditorGUI.BeginChangeCheck( );
                    string path;
                    if ( samevalue_in_all ) {
                        path = EditorGUILayout.TextField( getlist( t )[i].Path );
                    } else {
                        path = EditorGUILayout.TextField( string.Empty );
                    }
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        // パスが変更されたらオブジェクトを置き換える
                        Object o = AssetDatabase.LoadAssetAtPath<T>( path );
                        if ( o != null ) {
                            foreach ( var item in targetlist ) {
                                // 全ての選択中インスタンスに対してオブジェクトを設定
                                // DynamicPathの要素数が足りなかったらリサイズ
                                ExporterUtils.ResizeList( getlist( item ), Mathf.Max( i + 1, getlist( item ).Count ), ( ) => new PackagePrefsElement( ) );
                                getlist( item )[i].Object = o;
                                objects_count = MinMax.Create( targetlist, v => getlist( v ).Count );
                                EditorUtility.SetDirty( item );
                            }
                        }
                    }

                    // Button
                    int index_after = ExporterUtils.UpDownButton( i, objects_count.max );
                    if ( i != index_after ) {
                        foreach ( var item in targetlist ) {
                            if ( getlist( item ).Count <= index_after ) {
                                ExporterUtils.ResizeList( getlist( item ), index_after + 1, ( ) => new PackagePrefsElement( ) );
                            }
                            getlist( item ).Swap( i, index_after );
                            EditorUtility.SetDirty( item );
                        }
                    }
                    EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 10 ) );
                    if ( GUILayout.Button( "-", GUILayout.Width( 15 ) ) ) {
                        foreach ( var item in targetlist ) {
                            ExporterUtils.ResizeList( getlist( item ), Mathf.Max( i + 1, getlist( item ).Count ), ( ) => new PackagePrefsElement( ) );
                            getlist( item ).RemoveAt( i );
                            objects_count = MinMax.Create( targetlist, v => getlist( v ).Count );
                            EditorUtility.SetDirty( item );
                        }
                        i--;
                    }
                }
            }
            using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                ExporterUtils.Indent( 1 );
                if ( GUILayout.Button( "+", GUILayout.Width( 60 ) ) ) {
                    foreach ( var item in targetlist ) {
                        ExporterUtils.ResizeList( getlist( item ), objects_count.max + 1 );
                        EditorUtility.SetDirty( item );
                    }
                }
            }
        }
    }
#endif
}
