using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.MizoresPackageExporterConsts;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor
{
#if UNITY_EDITOR
    public static class GUI_VersionFile
    {
        public static void Draw( MizoresPackageExporter t, IEnumerable<MizoresPackageExporter> targetlist ) {
            if ( ExporterUtils.EditorPrefFoldout(
                Const.EDITOR_PREF_FOLDOUT_VERSIONFILE, ExporterTexts.t_Version ) ) {
                var same_versionSource_valueInAllObj = targetlist.All( v => t.versionSource == v.versionSource );
                using ( new EditorGUILayout.HorizontalScope( ) ) {
                    ExporterUtils.Indent( 1 );
                    EditorGUI.BeginChangeCheck( );
                    MizoresPackageExporter.VersionSource versionSource;

                    EditorGUI.showMixedValue = !same_versionSource_valueInAllObj;
                    versionSource = (MizoresPackageExporter.VersionSource)EditorGUILayout.EnumPopup( ExporterTexts.t_VersionSource, t.versionSource );
                    EditorGUI.showMixedValue = false;

                    if ( EditorGUI.EndChangeCheck( ) ) {
                        foreach ( var item in targetlist ) {
                            item.versionSource = versionSource;
                            item.UpdateExportVersion( );
                            EditorUtility.SetDirty( item );
                        }
                    }
                }
                if ( same_versionSource_valueInAllObj ) {
                    using ( new EditorGUILayout.HorizontalScope( ) ) {
                        ExporterUtils.Indent( 1 );
                        switch ( t.versionSource ) {
                            case MizoresPackageExporter.VersionSource.String: {
                                var samevalue_in_all_obj = targetlist.All( v => t.versionString == v.versionString );
                                EditorGUI.BeginChangeCheck( );
                                string versionString;

                                EditorGUI.showMixedValue = !samevalue_in_all_obj;
                                versionString = EditorGUILayout.TextField( ExporterTexts.t_Version, t.versionString );
                                EditorGUI.showMixedValue = false;

                                if ( EditorGUI.EndChangeCheck( ) ) {
                                    foreach ( var item in targetlist ) {
                                        item.versionString = versionString;
                                        item.UpdateExportVersion( );
                                        EditorUtility.SetDirty( item );
                                    }
                                }
                                break;
                            }
                            case MizoresPackageExporter.VersionSource.File: {
                                var samevalue_in_all_obj = targetlist.All( v => t.versionFile.Object == v.versionFile.Object );

                                if ( !samevalue_in_all_obj ) {
                                    ExporterUtils.DiffLabel( );
                                }
                                EditorGUI.showMixedValue = !samevalue_in_all_obj;
                                EditorGUI.BeginChangeCheck( );
                                PackagePrefsElementInspector.Draw<TextAsset>( t.versionFile );
                                EditorGUI.showMixedValue = false;
                                if ( EditorGUI.EndChangeCheck( ) ) {
                                    var obj = t.versionFile.Object;
                                    foreach ( var item in targetlist ) {
                                        item.versionFile.Object = obj;
                                        item.UpdateExportVersion( );
                                        EditorUtility.SetDirty( item );
                                    }
                                }
                                break;
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
                    if ( !samevalue_in_all ) {
                        ExporterUtils.DiffLabel( );
                        EditorGUI.showMixedValue = true;
                    }
                    value = EditorGUILayout.TextField( ExporterTexts.t_VersionFormat, t.versionFormat );
                    EditorGUI.showMixedValue = false;
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
