#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor {
#if UNITY_EDITOR
    public static class GUI_BatchExporter_OverrideSettings {
        public static void Draw( MizoresPackageExporter t, string key, int indent ) {
            var settings = t.packageNameSettingsOverride[key];
            var temp_indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = indent;
            EditorGUI.BeginChangeCheck( );
            {
                var value = EditorGUILayout.Toggle( ExporterTexts.SettingOverrideVersion, settings.useOverride_version );
                if ( EditorGUI.EndChangeCheck( ) ) {
                    settings.useOverride_version = value;
                    settings.lastUpdate_ExportVersion = 0;
                    EditorUtility.SetDirty( t );
                }
            }
            if ( settings.useOverride_version ) {
                EditorGUI.BeginChangeCheck( );
                var versionSource = ( VersionSource )EditorGUILayout.EnumPopup( ExporterTexts.VersionSource, settings.versionSource );
                if ( EditorGUI.EndChangeCheck( ) ) {
                    settings.versionSource = versionSource;
                    settings.lastUpdate_ExportVersion = 0;
                    EditorUtility.SetDirty( t );
                }
                switch ( settings.versionSource ) {
                    case VersionSource.String: {
                        EditorGUI.BeginChangeCheck( );
                        string versionString = EditorGUILayout.TextField( ExporterTexts.Version, settings.versionString );
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            settings.versionString = versionString;
                            settings.lastUpdate_ExportVersion = 0;
                            EditorUtility.SetDirty( t );
                        }
                        break;
                    }
                    case VersionSource.File: {
                        EditorGUI.BeginChangeCheck( );
                        PackagePrefsElementInspector.Draw<TextAsset>( t, settings.versionFile );
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            settings.lastUpdate_ExportVersion = 0;
                            EditorUtility.SetDirty( t );
                        }
                        break;
                    }
                }
            }
            // Override Version Format
            {
                EditorGUI.BeginChangeCheck( );
                var value = EditorGUILayout.Toggle( ExporterTexts.SettingOverrideVersionFormat, settings.useOverride_versionFormat );
                if ( EditorGUI.EndChangeCheck( ) ) {
                    settings.useOverride_versionFormat = value;
                    EditorUtility.SetDirty( t );
                }
            }
            if ( settings.useOverride_versionFormat ) {
                // Version Format
                EditorGUI.BeginChangeCheck( );
                string value = EditorGUILayout.TextField( ExporterTexts.VersionFormat, settings.versionFormat );
                if ( EditorGUI.EndChangeCheck( ) ) {
                    settings.versionFormat = value;
                    EditorUtility.SetDirty( t );
                }
            }
            // Override Batch Format
            {
                EditorGUI.BeginChangeCheck( );
                var value = EditorGUILayout.Toggle( ExporterTexts.SettingOverrideBatchFormat, settings.useOverride_batchFormat );
                if ( EditorGUI.EndChangeCheck( ) ) {
                    settings.useOverride_batchFormat = value;
                    EditorUtility.SetDirty( t );
                }
            }
            if ( settings.useOverride_batchFormat ) {
                // Batch Format
                EditorGUI.BeginChangeCheck( );
                string value = EditorGUILayout.TextField( ExporterTexts.BatchFormat, settings.batchFormat );
                if ( EditorGUI.EndChangeCheck( ) ) {
                    settings.batchFormat = value;
                    EditorUtility.SetDirty( t );
                }
            }
            // Override Package Name
            {
                EditorGUI.BeginChangeCheck( );
                var value = EditorGUILayout.Toggle( ExporterTexts.SettingOverridePackageName, settings.useOverride_packageName );
                if ( EditorGUI.EndChangeCheck( ) ) {
                    settings.useOverride_packageName = value;
                    EditorUtility.SetDirty( t );
                }
            }
            if ( settings.useOverride_packageName ) {
                // Package Name
                EditorGUI.BeginChangeCheck( );
                string value = EditorGUILayout.TextField( ExporterTexts.PackageName, settings.packageName );
                if ( EditorGUI.EndChangeCheck( ) ) {
                    settings.packageName = value;
                    EditorUtility.SetDirty( t );
                }
            }
            EditorGUI.indentLevel = temp_indent;
        }
    }
#endif
}