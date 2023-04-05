﻿using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterConsts;
using Const_Keys = MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterConsts_Keys;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter
{
    [CreateAssetMenu( menuName = "MizoreNekoyanagi/UnityPackageExporter" )]
    public class MizoresPackageExporter : ScriptableObject, ISerializationCallbackReceiver
    {
        /// <summary>jsonのdeserialize用</summary>
        [System.Serializable]
        private class VersionJson
        {
            public string version;
        }

        public const int CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION = 1;
        [SerializeField]
        private int packageExporterVersion = CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION;
        public int PackageExporterVersion { get => packageExporterVersion; }
        public bool IsCurrentVersion { get => PackageExporterVersion == CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION; }
        public bool IsCompatible { get => PackageExporterVersion <= CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION; }

        public List<PackagePrefsElement> objects = new List<PackagePrefsElement>( );
        public List<string> dynamicpath = new List<string>( );
        [SerializeField]
        DynamicPathVariable[] s_variables;
        [System.NonSerialized]
        public Dictionary<string, string> variables = new Dictionary<string, string>( );

        public List<PackagePrefsElement> excludeObjects = new List<PackagePrefsElement>( );
        public List<SearchPath> excludes = new List<SearchPath>( );
        public List<PackagePrefsElement> references = new List<PackagePrefsElement>( );

        const float UPDATE_INTERVAL = 3f;
        static bool CanUpdate( double lastUpdate ) {
#if UNITY_EDITOR
            return UPDATE_INTERVAL < EditorApplication.timeSinceStartup - lastUpdate;
#else
            return false;
#endif
        }

        #region PackageName
        public VersionSource versionSource;
        public PackagePrefsElement versionFile;
        public string versionString;
        public string versionFormat = $"-{Const_Keys.KEY_VERSION}";
        public string packageName = $"{Const_Keys.KEY_NAME}{Const_Keys.KEY_FORMATTED_VERSION}";

        public string PackageName {
            get {
                return ConvertDynamicPath( packageName );
            }
        }
        public string ExportFileName {
            get {
                return PackageName + ".unitypackage";
            }
        }
        public string ExportPath {
            get {
                return Const.EXPORT_FOLDER_PATH + ExportFileName;
            }
        }
        public string[] GetAllExportFileName( ) {
            if ( batchExportMode == BatchExportMode.None ) {
                temp_batchExportCurrentKey = string.Empty;
                return new string[] { ExportFileName };
            }
            var texts = BatchExportKeys;
            var result = new string[texts.Length];
            for ( int i = 0; i < texts.Length; i++ ) {
                temp_batchExportCurrentKey = texts[i];
                result[i] = ExportFileName;
            }
            return result.Distinct( ).ToArray( );
        }
        static Regex _invalidCharsRegex;
        /// <summary>
        /// ファイル名に使用できない文字の判定用
        /// </summary>
        static Regex InvalidCharsRegex {
            get {
                if ( _invalidCharsRegex == null ) {
                    string invalid = "." + new string( Path.GetInvalidFileNameChars( ) );
                    string pattern = string.Format( "[{0}]", Regex.Escape( invalid ) );
                    _invalidCharsRegex = new Regex( pattern );
                }
                return _invalidCharsRegex;
            }
        }
        [System.NonSerialized]
        double lastUpdate_ExportVersion;
        string _exportVersion;
        public string ExportVersion {
            get {
#if UNITY_EDITOR
                // 短時間に連続してファイルを読めないようにする
                if ( CanUpdate( lastUpdate_ExportVersion ) ) {
                    UpdateExportVersion( );
                }
#endif
                return ( string.IsNullOrWhiteSpace( _exportVersion ) ) ? string.Empty : _exportVersion;
            }
        }
        public string FormattedVersion {
            get {
                if ( string.IsNullOrWhiteSpace( ExportVersion ) ) {
                    return string.Empty;
                } else {
                    return ConvertDynamicPath( versionFormat );
                }
            }
        }

        public void UpdateExportVersion( ) {
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
                            _exportVersion = InvalidCharsRegex.Replace( _exportVersion, "_" );
                        }
                    }
                } catch ( System.Exception e ) {
                    throw e;
                }
            }
        }

        public void ConvertToCurrentVersion( bool force = false ) {
            if ( !force && !IsCompatible ) {
                throw new System.Exception( "互換性のないバージョンで作成されたオブジェクトです。\nThis object is not compatible." );
            }
            if ( IsCurrentVersion ) {
                return;
            }
            if ( force ) {
                packageExporterVersion = CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION;
                return;
            }

            switch ( packageExporterVersion ) {
                case 0:
                    versionSource = VersionSource.File;
                    break;
            }

            Debug.Log( $"Convert version: {packageExporterVersion} -> {CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION}" );
            packageExporterVersion = CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION;
        }
        #endregion

        #region BatchExport
        public BatchExportMode batchExportMode;
        public List<string> batchExportTexts;
        public PackagePrefsElement batchExportFolderRoot;
        public string batchExportFolderRegex;
        [System.NonSerialized]
        string[] temp_batchExportKeys;
        [System.NonSerialized]
        string temp_batchExportCurrentKey;

        double lastUpdate_BatchExportKeys;
        public string[] BatchExportKeys {
            get {
#if UNITY_EDITOR
                // 短時間に連続してファイルを読めないようにする
                if ( CanUpdate( lastUpdate_BatchExportKeys ) ) {
                    UpdateBatchExportKeys( );
                }
#endif
                return temp_batchExportKeys;
            }
        }
        public void UpdateBatchExportKeys( ) {
            lastUpdate_BatchExportKeys = EditorApplication.timeSinceStartup;
            switch ( batchExportMode ) {
                default:
                case BatchExportMode.None:
                    temp_batchExportKeys = new string[0];
                    break;
                case BatchExportMode.Texts:
                    temp_batchExportKeys = batchExportTexts.Distinct( ).ToArray( );
                    break;
                case BatchExportMode.Folders:
                    temp_batchExportKeys = new string[0];
                    if ( batchExportFolderRoot == null || batchExportFolderRoot.Object == null ) {
                        break;
                    }
                    string path = AssetDatabase.GetAssetPath( batchExportFolderRoot.Object );
                    Regex regex;
                    try {
                        regex = new Regex( batchExportFolderRegex );
                    } catch ( System.ArgumentException ) {
                        regex = new Regex( string.Empty );
                    }
                    temp_batchExportKeys = Directory.GetDirectories( path ).Select( v => Path.GetFileName( v ) ).Where( v => regex.IsMatch( v ) ).Distinct( ).ToArray( );
                    break;
            }
        }
        #endregion

        #region DynamicPath
        static Regex dateFormatRegex = new Regex( "%date:([^%]+)%" );
        public static string ReplaceDate( string key ) {
            var date = System.DateTime.Now;
            return dateFormatRegex.Replace( key, m => date.ToString( m.Groups[1].Value ) );
        }
        public string ConvertDynamicPath( string path ) {
            return ConvertDynamicPath_Main( path, 0 );
        }
        string ConvertDynamicPath_Main( string path, int recursiveCount ) {
            if ( string.IsNullOrWhiteSpace( path ) ) return string.Empty;
            if ( 2 < recursiveCount ) {
                return path;
            }
            recursiveCount += 1;
            string key = null;
            foreach ( var kvp in variables ) {
                key = string.Format( "%{0}%", kvp.Key );
                path = path.Replace( key, kvp.Value );
            }

            key = Const_Keys.KEY_BATCH_EXPORTER;
            path = path.Replace( key, temp_batchExportCurrentKey );

            path = ReplaceDate( path );

            key = Const_Keys.KEY_NAME;
            path = path.Replace( key, name );

            key = Const_Keys.KEY_VERSION;
            path = path.Replace( key, ExportVersion );

            key = Const_Keys.KEY_FORMATTED_VERSION;
            if ( path.Contains( key ) ) {
                if ( string.IsNullOrWhiteSpace( ExportVersion ) ) {
                    path = path.Replace( key, string.Empty );
                } else {
                    path = path.Replace( key, ConvertDynamicPath_Main( versionFormat, recursiveCount ) );
                }
            }

            key = Const_Keys.KEY_PACKAGE_NAME;
            if ( path.Contains( key ) ) {
                var str = ConvertDynamicPath_Main( packageName, recursiveCount );
                // ファイル名に使用できない文字を_に置き換え
                str = InvalidCharsRegex.Replace( str, "_" );
                path = path.Replace( key, str );
            }

            return path;
        }
        #endregion
        IEnumerable<string> GetReferencesPath( ) {
            List<string> result = new List<string>( );
            foreach ( var v in references ) {
                if ( string.IsNullOrWhiteSpace( v.Path ) ) continue;
                if ( File.Exists( v.Path ) ) {
                    result.Add( v.Path );
                } else if ( Directory.Exists( v.Path ) ) {
                    result.AddRange( Directory.GetFiles( v.Path, "*", SearchOption.AllDirectories ) );
                }
            }
            // バックスラッシュをスラッシュに統一（Unityのファイル処理ではスラッシュ推奨らしい？）
            return result.Select( v => v.Replace( '\\', '/' ) );
        }
        public Dictionary<string, IEnumerable<string>> GetAllPath_Batch( ) {
            var result = new Dictionary<string, IEnumerable<string>>( );
            if ( batchExportMode == BatchExportMode.None ) {
                temp_batchExportCurrentKey = string.Empty;
                result.Add( ExportPath, GetAllPath( ) );
                return result;
            }

            var texts = BatchExportKeys;
            for ( int i = 0; i < texts.Length; i++ ) {
                temp_batchExportCurrentKey = texts[i];
                var path = ExportPath;
                if ( !result.ContainsKey( path ) ) {
                    result.Add( ExportPath, GetAllPath( ) );
                }
            }
            return result;
        }
        public IEnumerable<string> GetAllPath( ) {
#if UNITY_EDITOR
            var references_path = GetReferencesPath( );
            ExporterUtils.DebugLog( "References: \n" + string.Join( "\n", references_path ) );
            bool useReference = references_path.Any( );

            IEnumerable<string> list;
            {
                var list1 = objects.Where( v => !string.IsNullOrWhiteSpace( v.Path ) ).Select( v => v.Path );
                var list2 = dynamicpath.Where( v => !string.IsNullOrWhiteSpace( v ) ).Select( v => ConvertDynamicPath( v ) );
                list = list1.Concat( list2 );
                list = list.Select( v => v.Replace( '\\', '/' ) );
            }

            var list_include_sub = new HashSet<string>( );
            foreach ( var item in list ) {
                if ( Directory.Exists( item ) ) {
                    list_include_sub.Add( item );
                    // サブファイル・フォルダを取得
                    var subdirs = Directory.GetFileSystemEntries( item, "*", SearchOption.AllDirectories );
                    foreach ( var sub in subdirs ) {
                        var path = sub.Replace( '\\', '/' );
                        list_include_sub.Add( path );
                    }
                } else {
                    list_include_sub.Add( item );
                }
            }
            // .metaファイルを除外
            list_include_sub = new HashSet<string>( list_include_sub.Where( v => Path.GetExtension( v ) != ".meta" ) );

            var result = new HashSet<string>( );

            foreach ( var item in list_include_sub ) {
                if ( Path.GetExtension( item ).Length != 0 ) {
                    if ( useReference ) {
                        var dependencies = AssetDatabase.GetDependencies( item, true );
                        foreach ( var dp in dependencies ) {
                            if ( dp == item ) {
                                result.Add( dp );
                            } else if ( references_path.Contains( dp ) ) {
                                // 依存AssetがReferencesに含まれていたらエクスポート対象に追加
                                result.Add( dp );
                                Debug.Log( "Dependency: " + dp );
                            } else {
                                ExporterUtils.DebugLog( "Ignore Dependency: " + dp );
                            }
                        }
                    } else {
                        result.Add( item );
                    }
                } else if ( Directory.Exists( item ) ) {
                    // 何もしない
                } else {
                    result.Add( item );
                }
            }

            // 除外指定されたファイル・フォルダを処理
            IEnumerable<string> result_enumerable = result;
            foreach ( var item in excludeObjects ) {
                if ( item == null || item.Object == null ) continue;
                var exclude = new SearchPath( SearchPathType.Exact, ConvertDynamicPath( item.Path ) );
                result_enumerable = exclude.Filter( result_enumerable, exclude: true, includeSubfiles: true );
            }
            foreach ( var item in excludes ) {
                var exclude = new SearchPath( item.searchType, ConvertDynamicPath( item.value ) );
                result_enumerable = exclude.Filter( result_enumerable, exclude: true, includeSubfiles: true );
            }
            var excludeResults = result.Except( result_enumerable );
            if ( excludeResults.Any( ) ) {
                Debug.Log( "Excludes: \n" + string.Join( "\n", excludeResults ) + "\n" );
            } else {
                Debug.Log( ExporterTexts.t_ExcludesWereEmpty );
            }
            return result_enumerable;
#else
            return new string[0];
#endif
        }
        public bool AllFileExists( ExporterEditorLogs logs ) {
            // ファイルが存在するか確認
            bool result = true;
#if UNITY_EDITOR
            var list_full = GetAllPath( ).ToList( );
            Debug.Log( string.Join( "\n", list_full ) );
            for ( int i = 0; i < list_full.Count; i++ ) {
                var path = list_full[i];
                if ( Path.GetExtension( path ).Length != 0 ) {
                    if ( File.Exists( path ) == false ) {
                        var text = string.Format( ExporterTexts.t_ExportLogNotFound, path );
                        logs.Add( ExporterEditorLogs.LogType.Error, text );
                        Debug.LogError( text );
                        result = false;

                        list_full[i] = ExporterTexts.t_ExportLogNotFoundPathPrefix + path;
                    }
                } else if ( Directory.Exists( path ) == false ) {
                    var text = string.Format( ExporterTexts.t_ExportLogNotFound, path );
                    logs.Add( ExporterEditorLogs.LogType.Error, text );
                    Debug.LogError( text );
                    result = false;

                    list_full[i] = ExporterTexts.t_ExportLogNotFoundPathPrefix + path;
                }
            }
            if ( result ) {
                var text = ExporterTexts.t_ExportLogAllFileExists;
                logs.Add( ExporterEditorLogs.LogType.Info, text );
                Debug.Log( text );
            }
            logs.AddSeparator( );
            foreach ( var item in list_full ) {
                Texture icon;
                var r = ExporterUtils.TryGetIcon( item, out icon );
                if ( r.IsExists( ) ) {
                    logs.Add( ExporterEditorLogs.LogType.Info, icon, item );
                } else {
                    logs.Add( ExporterEditorLogs.LogType.Error, item );
                }
            }
            logs.AddSeparator( );
#endif
            return result;
        }
        void Export_Internal( ExporterEditorLogs logs, string exportPath, IEnumerable<string> list ) {
            Debug.Log( "Start Export: " + string.Join( "/n", list ) );
            // ファイルが存在するか確認
            bool exists = AllFileExists( logs );
            if ( exists == false ) {
                logs.Add( ExporterEditorLogs.LogType.Error, ExporterTexts.t_ExportLogFailed );
                return;
            }

            string[] pathNames = list.ToArray( );
            if ( Directory.Exists( exportPath ) == false ) {
                Directory.CreateDirectory( Path.GetDirectoryName( exportPath ) );
            }
            AssetDatabase.ExportPackage( pathNames, exportPath, ExportPackageOptions.Default );
            EditorUtility.RevealInFinder( exportPath );

            logs.Add( ExporterEditorLogs.LogType.Info, ExporterTexts.t_ExportLogSuccess );
            Debug.Log( exportPath + "をエクスポートしました。" );
        }
        public void Export( ExporterEditorLogs logs ) {
#if UNITY_EDITOR
            logs.Clear( );
            UpdateExportVersion( );
            UpdateBatchExportKeys( );

            var table = GetAllPath_Batch( );
            foreach ( var kvp in table ) {
                string exportPath = kvp.Key;
                var list = kvp.Value;
                Export_Internal( logs, exportPath, list );
            }
#endif
        }

        public void OnBeforeSerialize( ) {
            s_variables = variables.Select( kvp => new DynamicPathVariable( kvp.Key, kvp.Value ) ).ToArray( );
        }

        public void OnAfterDeserialize( ) {
            if ( s_variables != null ) {
                variables = s_variables.ToDictionary( v => v.key, v => v.value );
            }
        }

    }
}