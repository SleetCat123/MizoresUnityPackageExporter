using System.Collections.Generic;
using UnityEngine;
using static MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterUtils;
using System.Linq;
using UnityEditorInternal;
using System.Reflection;
using System;
using System.Linq.Expressions;




#if UNITY_EDITOR
using UnityEditor;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor {
    public class GUIElement_PackagePrefsElementList<T> where T : UnityEngine.Object {
        FieldInfo _fieldInfo;

        public GUIElement_PackagePrefsElementList( FieldInfo fieldInfo ) {
            _fieldInfo = fieldInfo;
        }
        public GUIElement_PackagePrefsElementList( Expression<List<T>> func ) {
            var me = func.Body as MemberExpression;
            _fieldInfo = me.Member as FieldInfo;
        }

        public List<PackagePrefsElement> GetList( MizoresPackageExporter t ) {
            return _fieldInfo.GetValue( t ) as List<PackagePrefsElement>;
        }

        public void Draw( MizoresPackageExporter t, MizoresPackageExporter[] targetlist ) {
            using ( new VerticalBoxScope( ) ) {
                MinMax objects_count = MinMax.Create( targetlist, v => GetList( v ).Count );
                bool multiple = targetlist.Length > 1;
                for ( int i = 0; i < objects_count.max; i++ ) {
                    using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
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
                        PackagePrefsElementInspector.Draw<T>( element );
                        EditorGUI.showMixedValue = false;
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            var obj = element.Object;
                            foreach ( var item in targetlist ) {
                                // 全ての選択中インスタンスに対してオブジェクトを設定
                                // 要素数が足りなかったらリサイズ
                                ExporterUtils.ResizeList( GetList( item ), Mathf.Max( i + 1, GetList( item ).Count ), ( ) => new PackagePrefsElement( ) );
                                GetList( item )[i].Object = obj;
                                EditorUtility.SetDirty( item );
                            }
                            objects_count = MinMax.Create( targetlist, v => GetList( v ).Count );
                        }

                        // Button
                        int index_after = ExporterUtils.UpDownButton( i, objects_count.max );
                        if ( i != index_after ) {
                            foreach ( var item in targetlist ) {
                                if ( GetList( item ).Count <= index_after ) {
                                    ExporterUtils.ResizeList( GetList( item ), index_after + 1, ( ) => new PackagePrefsElement( ) );
                                }
                                GetList( item ).Swap( i, index_after );
                                EditorUtility.SetDirty( item );
                            }
                        }
                        EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 10 ) );
                        if ( ExporterUtils.MinusButton( ) ) {
                            foreach ( var item in targetlist ) {
                                ExporterUtils.ResizeList( GetList( item ), Mathf.Max( i + 1, GetList( item ).Count ), ( ) => new PackagePrefsElement( ) );
                                GetList( item ).RemoveAt( i );
                                EditorUtility.SetDirty( item );
                            }
                            objects_count = MinMax.Create( targetlist, v => GetList( v ).Count );
                            i--;
                        }
                    }
                }
                EditorGUI.indentLevel++;
                if ( ExporterUtils.PlusButton( ) ) {
                    foreach ( var item in targetlist ) {
                        ExporterUtils.ResizeList( GetList( item ), objects_count.max + 1, ( ) => new PackagePrefsElement( ) );
                        EditorUtility.SetDirty( item );
                    }
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}
#endif
