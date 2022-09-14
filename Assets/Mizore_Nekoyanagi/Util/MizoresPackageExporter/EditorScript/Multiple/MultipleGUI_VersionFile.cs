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
                    ExporterUtils.Indent( 1 );
                    var samevalue_in_all_obj = targetlist.All( v => t.versionFile.Object == v.versionFile.Object );

                    EditorGUI.BeginChangeCheck( );
                    Object obj;
                    if ( samevalue_in_all_obj ) {
                        obj = EditorGUILayout.ObjectField( t.versionFile.Object, typeof( TextAsset ), false );
                    } else {
                        ExporterUtils.DiffLabel( );
                        EditorGUI.showMixedValue = true;
                        obj = EditorGUILayout.ObjectField( null, typeof( TextAsset ), false );
                        EditorGUI.showMixedValue = false;
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
                        EditorGUI.showMixedValue = true;
                        path = EditorGUILayout.TextField( string.Empty );
                        EditorGUI.showMixedValue = false;
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
                // Version Format
                using ( new EditorGUILayout.HorizontalScope( ) ) {
                    ExporterUtils.Indent( 1 );
                    var samevalue_in_all = targetlist.All( v => t.versionFormat == v.versionFormat );
                    EditorGUI.BeginChangeCheck( );
                    string value;
                    if ( samevalue_in_all ) {
                        value = EditorGUILayout.TextField( ExporterTexts.t_VersionFormat, t.versionFormat );
                    } else {
                        ExporterUtils.DiffLabel( );
                        EditorGUI.showMixedValue = true;
                        value = EditorGUILayout.TextField( ExporterTexts.t_VersionFormat, string.Empty );
                        EditorGUI.showMixedValue = false;
                    }
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        foreach ( var item in targetlist ) {
                            item.versionFormat = value;
                            item.UpdateExportVersion( );
                            EditorUtility.SetDirty( item );
                        }
                    }
                }
            }
            // Package Name
            using ( new EditorGUILayout.HorizontalScope( ) ) {
                var samevalue_in_all = targetlist.All( v => t.packageName == v.packageName );
                EditorGUI.BeginChangeCheck( );
                string value;
                if ( samevalue_in_all ) {
                    value = EditorGUILayout.TextField( ExporterTexts.t_PackageName, t.packageName );
                } else {
                    ExporterUtils.DiffLabel( );
                    EditorGUI.showMixedValue = true;
                    value = EditorGUILayout.TextField( ExporterTexts.t_PackageName, string.Empty );
                    EditorGUI.showMixedValue = false;
                }
                if ( EditorGUI.EndChangeCheck( ) ) {
                    foreach ( var item in targetlist ) {
                        item.packageName = value;
                        item.UpdateExportVersion( );
                        EditorUtility.SetDirty( item );
                    }
                }
            }
        }
    }
#endif
}
