using UnityEngine;
using System.Linq;
using MizoreNekoyanagi.PublishUtil.PackageExporterV1;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public static class MizoresPackageExporterUpdator {
        public static bool ConvertToLatest( ScriptableObject obj ) {
            bool converted = false;
#pragma warning disable 612
            var v1 = obj as MizoresPackageExporterV1;
            // 初回実行時
            if ( v1 != null && v1.packageExporterVersion == MizoresPackageExporter.INITIAL_PACKAGE_EXPORTER_OBJECT_VERSION ) {
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
                converted = true;
            }
            if ( packageExporterVersion < 2 ) {
                ExporterUtils.DebugLog( "Convert: references" );
                // referencesの場所変更
                references2 = references.Select( v => new ReferenceElement( v, ReferenceMode.Include ) ).ToList( );
                references.Clear( );

                // dynamicpathの場所変更
                dynamicpath2 = dynamicpath.Select( v => new DynamicPathElement( v ) ).ToList( );
                dynamicpath.Clear( );

                converted = true;
            }
            if ( converted ) {
                Debug.Log( $"Convert version: {packageExporterVersion} -> {CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION}" );
                packageExporterVersion = CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION;
            }
            return converted;
#pragma warning restore 612
        }
    }
}
