using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Const_Keys = MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterConsts_Keys;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    [System.Serializable]
    public class PackageNameSettings {
        /// <summary>jsonのdeserialize用</summary>
        [System.Serializable]
        private class VersionJson {
            public string version;
        }

        public VersionSource versionSource;
        public PackagePrefsElement versionFile;
        public string versionString;
        public string versionFormat = $"-{Const_Keys.KEY_VERSION}";
        public string batchFormat = $"_{Const_Keys.KEY_BATCH_EXPORTER}";
        public string packageName = $"{Const_Keys.KEY_NAME}{Const_Keys.KEY_FORMATTED_BATCH_EXPORTER}{Const_Keys.KEY_FORMATTED_VERSION}";

        public bool useOverride_version = true;
        public bool useOverride_versionFormat;
        public bool useOverride_batchFormat;
        public bool useOverride_packageName;

        public PackageNameSettings( ) { }
        public PackageNameSettings( PackageNameSettings source ) {
            this.versionSource = source.versionSource;
            this.versionFile = source.versionFile;
            this.versionString = source.versionString;
            this.versionFormat = source.versionFormat;
            this.batchFormat = source.batchFormat;
            this.packageName = source.packageName;
            this.lastUpdate_ExportVersion = source.lastUpdate_ExportVersion;
            this._exportVersion = source._exportVersion;
        }

        [NonSerialized] public double lastUpdate_ExportVersion;
        [NonSerialized] string _exportVersion;
        [NonSerialized] public string debug_id;

        public string GetExportVersion( ) {
#if UNITY_EDITOR
            if ( CanUpdateVersion( ) ) {
                // 短時間に連続してファイルを読めないようにする
                UpdateExportVersion( );
            }
#endif
            return ( string.IsNullOrWhiteSpace( _exportVersion ) ) ? string.Empty : _exportVersion;
        }

        public bool CanUpdateVersion( ) {
#if UNITY_EDITOR
            return 5f < EditorApplication.timeSinceStartup - lastUpdate_ExportVersion;
#else
            return false;
#endif
        }

        public void UpdateExportVersion( ) {
#if UNITY_EDITOR
            Debug.Log( $"UpdateExportVersion: \n{debug_id}\n{lastUpdate_ExportVersion}" );
            lastUpdate_ExportVersion = EditorApplication.timeSinceStartup;
            if ( versionSource == VersionSource.String ) {
                _exportVersion = versionString;
                return;
            }
            if ( versionFile == null || string.IsNullOrEmpty( versionFile.Path ) ) {
                _exportVersion = string.Empty;
            } else {
                try {
                    string path = versionFile.Path;
                    using ( StreamReader sr = new StreamReader( path ) ) {
                        string ext = Path.GetExtension( path );
                        if ( ext == ".json" ) {
                            _exportVersion = JsonUtility.FromJson<VersionJson>( sr.ReadToEnd( ) ).version;
                        } else {
                            string line;
                            // versionfileの空白ではない最初の行をバージョンとして扱う
                            while ( ( line = sr.ReadLine( ) ) != null && string.IsNullOrWhiteSpace( line ) ) { }
                            _exportVersion = line;
                        }
                        if ( string.IsNullOrWhiteSpace( _exportVersion ) ) {
                            _exportVersion = string.Empty;
                        } else {
                            _exportVersion = _exportVersion.Trim( );
                            // ファイル名に使用できない文字を_に置き換え
                            _exportVersion = ExporterUtils.InvalidFileCharsRegex.Replace( _exportVersion, "_" );
                        }
                    }
                } catch ( System.Exception e ) {
                    throw e;
                }
            }
#endif
        }

        public void SetBase( PackageNameSettings baseSettings ) {
            if ( !useOverride_version ) {
                this.versionSource = baseSettings.versionSource;
                this.versionFile = baseSettings.versionFile;
                this.versionString = baseSettings.versionString;
                UpdateExportVersion( );
            }
            if ( !useOverride_versionFormat ) {
                this.versionFormat = baseSettings.versionFormat;
            }
            if ( !useOverride_batchFormat ) {
                this.batchFormat = baseSettings.batchFormat;
            }
            if ( !useOverride_packageName ) {
                this.packageName = baseSettings.packageName;
            }
        }
    }
}
