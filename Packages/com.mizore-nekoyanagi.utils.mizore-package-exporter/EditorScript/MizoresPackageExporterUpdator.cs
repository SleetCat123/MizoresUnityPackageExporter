#if UNITY_EDITOR
using UnityEngine;
using System.Linq;
using MizoreNekoyanagi.PublishUtil.PackageExporterV1;
using System.IO;
using System.Collections.Generic;
using UnityEditor;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public static class MizoresPackageExporterUpdator {
        public static bool IsLatest( ScriptableObject obj ) {
            var latest = obj as MizoresPackageExporter;
            if ( latest != null ) {
                return latest.packageExporterVersion == MizoresPackageExporter.CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION;
            }
            return false;
        }
        public static bool IsCompatible( ScriptableObject obj ) {
#pragma warning disable 612
            var v1 = obj as MizoresPackageExporterV1;
            if ( v1 != null ) {
                return v1.packageExporterVersion <= 1;
            }
            var v2 = obj as MizoresPackageExporter;
            if ( v2 != null ) {
                return v2.packageExporterVersion <= MizoresPackageExporter.CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION;
            }
            #pragma warning restore 612
            return false;
        }
        public static ScriptableObject ConvertToLatest( ScriptableObject obj ) {
            if ( IsLatest( obj ) ) {
                return obj;
            }
            var name = obj.name;
#pragma warning disable 612
            MizoresPackageExporter latest = obj as MizoresPackageExporter;
            if ( latest == null ) {
                var v1 = obj as MizoresPackageExporterV1;
                if ( v1 != null ) {
                    // 初回実行時
                    if ( v1.packageExporterVersion == MizoresPackageExporter.INITIAL_PACKAGE_EXPORTER_OBJECT_VERSION ) {
                        ExporterUtils.DebugLog( "Initialize" );
                        // packageExporterVersion実装前のオブジェクトからの変換
                        if ( v1.versionFile != null && !v1.versionFile.IsEmpty( ) ) {
                            // versionFileの場所変更
                            ExporterUtils.DebugLog( "Convert: versionFile" );
                            v1.packageNameSettings.versionSource = MizoresPackageExporterV1.VersionSource.File;
                            v1.packageNameSettings.versionFile = v1.versionFile;
                            v1.versionFile = null;
                            EditorUtility.SetDirty( v1 );
                        }
                        if ( !string.IsNullOrEmpty( v1.versionFormat ) ) {
                            // versionFormatの場所変更

                            ExporterUtils.DebugLog( "Convert: versionFormat" );
                            v1.packageNameSettings.versionFormat = v1.versionFormat;
                            v1.versionFormat = null;
                            EditorUtility.SetDirty( v1 );
                        }
                        if ( !string.IsNullOrEmpty( v1.packageName ) ) {
                            // packageNameの場所変更
                            ExporterUtils.DebugLog( "Convert: packageName" );
                            v1.packageNameSettings.packageName = v1.packageName;
                            v1.packageName = null;
                            EditorUtility.SetDirty( v1 );
                        }
                        Debug.Log( $"Convert version: {MizoresPackageExporter.INITIAL_PACKAGE_EXPORTER_OBJECT_VERSION} -> 1" );
                        v1.packageExporterVersion = MizoresPackageExporter.CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION;
                    }

                    // V1 -> V2
                    Debug.Log( $"Convert Object: {v1.name}   V1 -> V2" );
                    var v2 = MizoresPackageExporter.CreateInstance<MizoresPackageExporter>( );
                    ExporterUtils.DebugLog( "Convert: references" );

                    // V1のobjectsとdynamicpathを結合
                    var objects = v1.objects.Select( v => new ExportTargetObjectElement( v.Path ) );
                    var dynamicpath = v1.dynamicpath.Select( v => new ExportTargetObjectElement( v ) );
                    v2.objects = objects.Concat( dynamicpath ).ToList( );

                    v2.variables = v1.variables.ToDictionary( v => v.Key, v => v.Value );
                    v2.excludeObjects = v1.excludeObjects.Select( v => new ObjectRefElement( v.Path ) ).ToList( );
                    v2.excludes = v1.excludes.Select( v => new SearchPath( (SearchPathType)v.searchType, v.value ) ).ToList( );
                    v2.references = v1.references.Select( v => new ReferenceElement( new ObjectRefElement( v.Path ), ReferenceMode.Include ) ).ToList( );
                    v2.packageNameSettings = ( PackageNameSettings )v1.packageNameSettings;
                    v2.packageNameSettingsOverride = v1.packageNameSettingsOverride.ToDictionary( v => v.Key, v => ( PackageNameSettings )v.Value );
                    v2.batchExportMode = ( BatchExportMode )v1.batchExportMode;
                    v2.batchExportFolderMode = ( BatchExportFolderMode )v1.batchExportFolderMode;
                    v2.batchExportTexts = new List<string>( v1.batchExportTexts );
                    v2.batchExportFolderRoot = new ObjectRefElement( v1.batchExportFolderRoot.Path );
                    v2.batchExportListFile = new ObjectRefElement( v1.batchExportListFile.Path );
                    v2.batchExportFolderRegex = v1.batchExportFolderRegex;
                    v2.packageExporterVersion = MizoresPackageExporter.CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION;
                    latest = v2;
                }
            }
            if ( latest != obj ) {
                // 新規作成時、Assetファイルを上書きする
                // GUIDが変わらないようにFile.Copyで上書きする

                // 既存のAssetを削除して変換後のAssetを作成
                var path = AssetDatabase.GetAssetPath( obj );
                AssetDatabase.DeleteAsset( path );
                AssetDatabase.CreateAsset( latest, path );
                AssetDatabase.SaveAssets( );
                AssetDatabase.Refresh( );
                // オブジェクトを選択
                latest = AssetDatabase.LoadAssetAtPath<MizoresPackageExporter>( path );
                Selection.objects = Selection.objects.Concat( new Object[ ] { latest } ).ToArray( );
            }
            return latest;
#pragma warning restore 612
        }
    }
}
#endif
