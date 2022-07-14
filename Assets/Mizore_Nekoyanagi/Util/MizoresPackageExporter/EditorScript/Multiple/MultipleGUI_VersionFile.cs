using UnityEngine;
using System.Linq;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.MultipleEditor
{
#if UNITY_EDITOR
    public static class MultipleGUI_VersionFile
    {
        public static void Draw( MizoresPackageExporter t, IEnumerable<MizoresPackageExporter> targetlist ) {
            EditorGUILayout.LabelField( ExporterTexts.t_VersionFile, EditorStyles.boldLabel );
            using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                var samevalue_in_all = targetlist.All( v => t.versionFile.Object == v.versionFile.Object );

                EditorGUI.BeginChangeCheck( );
                Object obj;
                if ( samevalue_in_all ) {
                    obj = EditorGUILayout.ObjectField( t.versionFile.Object, typeof( TextAsset ), false );
                } else {
                    obj = EditorGUILayout.ObjectField( null, typeof( TextAsset ), false );
                }
                if ( EditorGUI.EndChangeCheck( ) ) {
                    foreach ( var item in targetlist ) {
                        item.versionFile.Object = obj;
                        item.UpdateExportVersion( );
                        EditorUtility.SetDirty( item );
                    }
                }

                EditorGUI.BeginChangeCheck( );
                string path;
                if ( samevalue_in_all ) {
                    path = EditorGUILayout.TextField( t.versionFile.Path );
                } else {
                    path = EditorGUILayout.TextField( string.Empty );
                }
                if ( EditorGUI.EndChangeCheck( ) ) {
                    // パスが変更されたらオブジェクトを置き換える
                    Object o = AssetDatabase.LoadAssetAtPath<TextAsset>( path );
                    if ( o != null ) {
                        foreach ( var item in targetlist ) {
                            item.versionFile.Object = o;
                            item.UpdateExportVersion( );
                            EditorUtility.SetDirty( item );
                        }
                    }
                }
            }
        }
    }
#endif
}
