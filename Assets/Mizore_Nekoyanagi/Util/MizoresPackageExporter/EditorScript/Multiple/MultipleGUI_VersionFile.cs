using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.MizoresPackageExporterConsts;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.MultipleEditor
{
#if UNITY_EDITOR
    public static class MultipleGUI_VersionFile
    {
        public static void Draw( MizoresPackageExporter t, IEnumerable<MizoresPackageExporter> targetlist ) {
            if ( ExporterUtils.EditorPrefFoldout(
                Const.EDITOR_PREF_FOLDOUT_VERSIONFILE, ExporterTexts.t_VersionFile ) ) {
                using ( new EditorGUILayout.HorizontalScope( ) ) {
                    var samevalue_in_all_obj = targetlist.All( v => t.versionFile.Object == v.versionFile.Object );

                    EditorGUI.BeginChangeCheck( );
                    Object obj;
                    if ( samevalue_in_all_obj ) {
                        obj = EditorGUILayout.ObjectField( t.versionFile.Object, typeof( TextAsset ), false );
                    } else {
                        ExporterUtils.DiffLabel( );
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
                    if ( samevalue_in_all_obj ) {
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
                // Version Prefix
                using ( new EditorGUILayout.HorizontalScope( ) ) {
                    var samevalue_in_all_prefix = targetlist.All( v => t.versionPrefix == v.versionPrefix );
                    EditorGUI.BeginChangeCheck( );
                    string versionPrefix;
                    if ( samevalue_in_all_prefix ) {
                        versionPrefix = EditorGUILayout.TextField( ExporterTexts.t_VersionPrefix, t.versionPrefix );
                    } else {
                        ExporterUtils.DiffLabel( );
                        versionPrefix = EditorGUILayout.TextField( ExporterTexts.t_VersionPrefix, string.Empty );
                    }
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        foreach ( var item in targetlist ) {
                            item.versionPrefix = versionPrefix;
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
