using UnityEngine;
using System.Linq;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor {
#if UNITY_EDITOR
    public static class GUI_VersionFile {
        public static void DrawMain( PackageNameSettings[] settings, MizoresPackageExporter[] targetlist, int indent ) {
            var t = targetlist[0];
            var s = settings[0];
            var same_versionSource_valueInAllObj = settings.All( v => s.versionSource == v.versionSource );
            var temp_indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = indent;

            EditorGUI.BeginChangeCheck( );
            EditorGUI.showMixedValue = !same_versionSource_valueInAllObj;
            var versionSource = ( VersionSource )EditorGUILayout.EnumPopup( ExporterTexts.VersionSource, s.versionSource );
            EditorGUI.showMixedValue = false;
            if ( EditorGUI.EndChangeCheck( ) ) {
                for ( int i = 0; i < targetlist.Length; i++ ) {
                    settings[i].versionSource = versionSource;
                    targetlist[i].CurrentSettings.UpdateExportVersion( );
                    EditorUtility.SetDirty( targetlist[i] );
                }
            }

            if ( same_versionSource_valueInAllObj ) {
                switch ( s.versionSource ) {
                    case VersionSource.String: {
                        var samevalue_in_all_obj = settings.All( v => s.versionString == v.versionString );
                        EditorGUI.BeginChangeCheck( );
                        string versionString;

                        EditorGUI.showMixedValue = !samevalue_in_all_obj;
                        versionString = EditorGUILayout.TextField( ExporterTexts.Version, s.versionString );
                        EditorGUI.showMixedValue = false;

                        if ( EditorGUI.EndChangeCheck( ) ) {
                            for ( int i = 0; i < targetlist.Length; i++ ) {
                                settings[i].versionString = versionString;
                                targetlist[i].CurrentSettings.UpdateExportVersion( );
                                EditorUtility.SetDirty( targetlist[i] );
                            }
                        }
                        break;
                    }
                    case VersionSource.File: {
                        var samevalue_in_all_obj = settings.All( v => s.versionFile.Object == v.versionFile.Object );

                        if ( !samevalue_in_all_obj ) {
                            ExporterUtils.DiffLabel( );
                        }
                        EditorGUI.showMixedValue = !samevalue_in_all_obj;
                        EditorGUI.BeginChangeCheck( );
                        PackagePrefsElementInspector.Draw<TextAsset>( s.versionFile );
                        EditorGUI.showMixedValue = false;
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            var obj = s.versionFile.Object;
                            for ( int i = 0; i < targetlist.Length; i++ ) {
                                settings[i].versionFile.Object = obj;
                                targetlist[i].CurrentSettings.UpdateExportVersion( );
                                EditorUtility.SetDirty( targetlist[i] );
                            }
                        }
                        break;
                    }
                }
            }

            // Version Format
            {
                var samevalue_in_all = settings.All( v => s.versionFormat == v.versionFormat );
                EditorGUI.BeginChangeCheck( );
                string value;
                if ( !samevalue_in_all ) {
                    ExporterUtils.DiffLabel( );
                    EditorGUI.showMixedValue = true;
                }
                value = EditorGUILayout.TextField( ExporterTexts.VersionFormat, s.versionFormat );
                EditorGUI.showMixedValue = false;
                if ( EditorGUI.EndChangeCheck( ) ) {
                    for ( int i = 0; i < targetlist.Length; i++ ) {
                        settings[i].versionFormat = value;
                        // targetlist[i].UpdateExportVersion( );
                        EditorUtility.SetDirty( targetlist[i] );
                    }
                }
            }
            // Batch Format
            {
                if ( targetlist.Any( v => v.batchExportMode != BatchExportMode.Single ) ) {
                    var samevalue_in_all = settings.All( v => s.batchFormat == v.batchFormat );
                    EditorGUI.BeginChangeCheck( );
                    string value;
                    if ( !samevalue_in_all ) {
                        ExporterUtils.DiffLabel( );
                        EditorGUI.showMixedValue = true;
                    }
                    value = EditorGUILayout.TextField( ExporterTexts.BatchFormat, s.batchFormat );
                    EditorGUI.showMixedValue = false;
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        for ( int i = 0; i < targetlist.Length; i++ ) {
                            settings[i].batchFormat = value;
                            // targetlist[i].UpdateExportVersion( );
                            EditorUtility.SetDirty( targetlist[i] );
                        }
                    }
                }
            }
            // Package Name
            {
                var samevalue_in_all = settings.All( v => s.packageName == v.packageName );
                EditorGUI.BeginChangeCheck( );
                string value;
                if ( samevalue_in_all ) {
                    value = EditorGUILayout.TextField( ExporterTexts.PackageName, s.packageName );
                } else {
                    ExporterUtils.DiffLabel( );
                    EditorGUI.showMixedValue = true;
                    value = EditorGUILayout.TextField( ExporterTexts.PackageName, string.Empty );
                    EditorGUI.showMixedValue = false;
                }
                if ( EditorGUI.EndChangeCheck( ) ) {
                    for ( int i = 0; i < targetlist.Length; i++ ) {
                        settings[i].packageName = value;
                        targetlist[i].CurrentSettings.UpdateExportVersion( );
                        EditorUtility.SetDirty( targetlist[i] );
                    }
                }
            }

            EditorGUI.indentLevel = temp_indentLevel;
        }
    }
#endif
}
