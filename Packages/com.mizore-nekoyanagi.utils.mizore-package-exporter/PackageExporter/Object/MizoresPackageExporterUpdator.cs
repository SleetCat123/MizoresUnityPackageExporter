using UnityEngine;
using System.Linq;
using MizoreNekoyanagi.PublishUtil.PackageExporterV1;
using System.IO;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public static class MizoresPackageExporterUpdator {
        public static bool IsLatest( ScriptableObject obj ) {
            var latest = obj as MizoresPackageExporter;
            if ( latest != null ) {
                return latest.PackageExporterVersion == MizoresPackageExporter.CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION;
            }
            return false;
        }
        public static ScriptableObject ConvertToLatest( ScriptableObject obj ) {
            if ( IsLatest( obj ) ) {
                return obj;
            }
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
                        }
                        if ( !string.IsNullOrEmpty( v1.versionFormat ) ) {
                            // versionFormatの場所変更
                            ExporterUtils.DebugLog( "Convert: versionFormat" );
                            v1.packageNameSettings.versionFormat = v1.versionFormat;
                            v1.versionFormat = null;
                        }
                        if ( !string.IsNullOrEmpty( v1.packageName ) ) {
                            // packageNameの場所変更
                            ExporterUtils.DebugLog( "Convert: packageName" );
                            v1.packageNameSettings.packageName = v1.packageName;
                            v1.packageName = null;
                        }
                        Debug.Log( $"Convert version: {MizoresPackageExporter.INITIAL_PACKAGE_EXPORTER_OBJECT_VERSION} -> 1" );
                        v1.packageExporterVersion = MizoresPackageExporter.CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION;
                    }
                    var v2 = MizoresPackageExporter.CreateInstance<MizoresPackageExporter>( );
                    ExporterUtils.DebugLog( "Convert: references" );
                    // referencesの場所変更
                    v2.references2 = v1.references.Select( v => new ReferenceElement( new PackagePrefsElement( v.obj ), ReferenceMode.Include ) ).ToList( );
                    // dynamicpathの場所変更
                    v2.dynamicpath2 = v1.dynamicpath.Select( v => new DynamicPathElement( v ) ).ToList( );

                    latest = v2;
                }
            }
            if ( latest != obj ) {
                // 新規作成時、Assetファイルを上書きする
                // GUIDが変わらないようにFile.Copyで上書きする

                // Assetとして保存
                var path = AssetDatabase.GetAssetPath( obj );
                var tempPath = AssetDatabase.GenerateUniqueAssetPath( path );
                AssetDatabase.CreateAsset( latest, tempPath );
                // Assetの上書き
                File.Copy( tempPath, path, true );
                // 一時ファイルの削除
                AssetDatabase.DeleteAsset( tempPath );
                AssetDatabase.SaveAssets( );
                AssetDatabase.Refresh( );
            }
            return latest;
#pragma warning restore 612
        }
    }
}
