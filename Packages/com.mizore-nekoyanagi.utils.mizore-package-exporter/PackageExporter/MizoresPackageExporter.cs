using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterConsts;
using Const_Keys = MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterConsts_Keys;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    [CreateAssetMenu( menuName = "MizoreNekoyanagi/UnityPackageExporter" )]
    public class MizoresPackageExporter : ScriptableObject, ISerializationCallbackReceiver {
        [System.Serializable]
        private class PackageNameSettingsKVP {
            public string key;
            public PackageNameSettings value;

            public PackageNameSettingsKVP( string key, PackageNameSettings value ) {
                this.key = key;
                this.value = value;
            }
        }

        public const int INITIAL_PACKAGE_EXPORTER_OBJECT_VERSION = 0;
        public const int CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION = 1;
        [SerializeField]
        private int packageExporterVersion = INITIAL_PACKAGE_EXPORTER_OBJECT_VERSION;
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

        #region PackageName
        /// <summary>互換性のため残しておく。今後はpackageNameSettings.versionFileを使用</summary>
        [System.Obsolete, SerializeField]
        PackagePrefsElement versionFile;

        /// <summary>互換性のため残しておく。今後はpackageNameSettings.versionFormatを使用</summary>
        [System.Obsolete, SerializeField]
        string versionFormat = null;

        /// <summary>互換性のため残しておく。今後はpackageNameSettings.packageNameを使用</summary>
        [System.Obsolete, SerializeField]
        string packageName = null;

        public PackageNameSettings packageNameSettings = new PackageNameSettings( );

        public PackageNameSettings CurrentSettings => GetOverridedSettings( temp_batchExportCurrentKey );

        [SerializeField]
        PackageNameSettingsKVP[] s_packageNameSettingsOverride;
        [System.NonSerialized]
        public Dictionary<string, PackageNameSettings> packageNameSettingsOverride = new Dictionary<string, PackageNameSettings>( );
        public PackageNameSettings GetOverridedSettings( string batchExportKey ) {
            if ( string.IsNullOrEmpty( batchExportKey ) ) {
                return packageNameSettings;
            }
            PackageNameSettings ov;
            if ( packageNameSettingsOverride.TryGetValue( batchExportKey, out ov ) ) {
                ov.SetBase( packageNameSettings );
                ov.debug_id = batchExportKey;
                return ov;
            } else {
                return packageNameSettings;
            }
        }

        public string GetPackageName( ) {
            return ConvertDynamicPath( CurrentSettings.packageName );
        }
        public string GetExportFileName( ) {
            return GetPackageName( ) + ".unitypackage";
        }
        public string GetExportPath( ) {
            return Const.EXPORT_FOLDER_PATH + GetExportFileName( );
        }
        public string[] GetAllExportFileName( ) {
            if ( batchExportMode == BatchExportMode.Single ) {
                temp_batchExportCurrentKey = string.Empty;
                return new string[] { GetExportFileName( ) };
            } else {
                var texts = BatchExportKeysConverted;
                var result = new string[texts.Length];
                for ( int i = 0; i < texts.Length; i++ ) {
                    temp_batchExportCurrentKey = texts[i];
                    result[i] = GetExportFileName( );
                }
                temp_batchExportCurrentKey = string.Empty;
                return result.Distinct( ).ToArray( );
            }
        }
        public string GetFormattedVersion( ) {
            if ( string.IsNullOrWhiteSpace( CurrentSettings.GetExportVersion( ) ) ) {
                return string.Empty;
            } else {
                return ConvertDynamicPath( CurrentSettings.versionFormat );
            }
        }
        public string FormattedBatch {
            get {
                if ( string.IsNullOrWhiteSpace( temp_batchExportCurrentKey ) ) {
                    return string.Empty;
                } else {
                    return ConvertDynamicPath( CurrentSettings.batchFormat );
                }
            }
        }

        public void UpdateAllExportVersions( ) {
            packageNameSettings.UpdateExportVersion( );
            foreach ( var item in packageNameSettingsOverride.Keys ) {
                GetOverridedSettings( item ).UpdateExportVersion( );
            }
        }

        public bool ConvertToCurrentVersion( bool force = false ) {
            if ( !force && !IsCompatible ) {
                throw new System.Exception( "互換性のないバージョンで作成されたオブジェクトです。\nThis object is not compatible." );
            }

            bool converted = false;
            // 初回実行時
            if ( packageExporterVersion == INITIAL_PACKAGE_EXPORTER_OBJECT_VERSION ) {
                ExporterUtils.DebugLog( "Initialize" );
#pragma warning disable 612
                // packageExporterVersion実装前のオブジェクトからの変換
                if ( versionFile != null && !string.IsNullOrEmpty( versionFile.Path ) ) {
                    // versionFileの場所変更
                    ExporterUtils.DebugLog( "Convert: versionFile" );
                    packageNameSettings.versionSource = VersionSource.File;
                    packageNameSettings.versionFile = versionFile;
                    versionFile = null;
                    converted = true;
                }
                if ( !string.IsNullOrEmpty( versionFormat ) ) {
                    // versionFormatの場所変更
                    ExporterUtils.DebugLog( "Convert: versionFormat" );
                    packageNameSettings.versionFormat = versionFormat;
                    versionFormat = null;
                    converted = true;
                }
                if ( !string.IsNullOrEmpty( packageName ) ) {
                    // packageNameの場所変更
                    ExporterUtils.DebugLog( "Convert: packageName" );
                    packageNameSettings.packageName = packageName;
                    packageName = null;
                    converted = true;
                }
#pragma warning restore 612
                packageExporterVersion = CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION;
            }

            if ( IsCurrentVersion ) {
                return converted;
            }
            if ( force ) {
                packageExporterVersion = CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION;
                return converted;
            }

            switch ( packageExporterVersion ) {
                case 0:
                case 1:
                    break;
            }

            Debug.Log( $"Convert version: {packageExporterVersion} -> {CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION}" );
            packageExporterVersion = CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION;
            return converted;
        }
        #endregion

        #region BatchExport
        public BatchExportMode batchExportMode;
        public BatchExportFolderMode batchExportFolderMode;
        public List<string> batchExportTexts = new List<string>();
        public PackagePrefsElement batchExportFolderRoot;
        public PackagePrefsElement batchExportListFile;
        public string batchExportFolderRegex;
        [System.NonSerialized]
        string[] temp_batchExportKeys;

        [System.NonSerialized]
        string temp_batchExportCurrentKey;

        double lastUpdate_BatchExportKeys;
        public string[] BatchExportKeysConverted {
            get {
                var list = BatchExportKeys;
                for ( int i = 0; i < list.Length; i++ ) {
                    list[i] = ConvertDynamicPath( list[i] );
                }
                return list;
            }
        }
        public bool CanUpdateBatchExportKeys( ) {
#if UNITY_EDITOR
            return 5f < EditorApplication.timeSinceStartup - lastUpdate_BatchExportKeys;
#else
            return false;
#endif
        }
        public string[] BatchExportKeys {
            get {
#if UNITY_EDITOR
                // 短時間に連続してファイルを読めないようにする
                if ( CanUpdateBatchExportKeys( ) || temp_batchExportKeys == null ) {
                    UpdateBatchExportKeys( );
                }
#endif
                return temp_batchExportKeys;
            }
        }
        public void UpdateBatchExportKeys( ) {
#if UNITY_EDITOR
            ExporterUtils.DebugLog( "UpdateBatchExportKeys\n" + name );
            lastUpdate_BatchExportKeys = EditorApplication.timeSinceStartup;
            switch ( batchExportMode ) {
                default:
                case BatchExportMode.Single:
                    temp_batchExportKeys = new string[0];
                    break;
                case BatchExportMode.Texts:
                    temp_batchExportKeys = batchExportTexts.Distinct( ).ToArray( );
                    break;
                case BatchExportMode.Folders:
                    if ( batchExportFolderRoot == null || batchExportFolderRoot.Object == null ) {
                        temp_batchExportKeys = new string[0];
                        break;
                    }
                    string path = AssetDatabase.GetAssetPath( batchExportFolderRoot.Object );
                    Regex regex;
                    try {
                        regex = new Regex( batchExportFolderRegex );
                    } catch ( System.ArgumentException ) {
                        regex = new Regex( string.Empty );
                    }
                    IEnumerable<string> files;
                    switch ( batchExportFolderMode ) {
                        default:
                        case BatchExportFolderMode.All:
                            files = Directory.GetFileSystemEntries( path );
                            break;
                        case BatchExportFolderMode.Files:
                            files = Directory.GetFiles( path );
                            break;
                        case BatchExportFolderMode.Folders:
                            files = Directory.GetDirectories( path );
                            break;
                    }
                    files = files.Where( v => Path.GetExtension( v ) != ".meta" ).Select( v => Path.GetFileName( v ) );
                    files = files.Select( v => Path.GetFileNameWithoutExtension( v ) ).Where( v => regex.IsMatch( v ) );
                    temp_batchExportKeys = files.Distinct( ).ToArray( );
                    break;
                case BatchExportMode.ListFile:
                    if ( batchExportFolderRoot == null || batchExportFolderRoot.Object == null ) {
                        temp_batchExportKeys = new string[0];
                        break;
                    }
                    var list = new List<string>( );
                    var file = batchExportListFile.Object as TextAsset;
                    if ( file != null ) {
                        using ( var reader = new StringReader( file.text ) ) {
                            while ( reader.Peek( ) > -1 ) {
                                string line = reader.ReadLine( );
                                //if ( string.IsNullOrWhiteSpace( line ) ) {
                                //    continue;
                                //}
                                list.Add( line );
                            }
                        }
                    }
                    temp_batchExportKeys = list.Distinct( ).ToArray( );
                    break;
            }
#endif
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
            key = Const_Keys.KEY_FORMATTED_BATCH_EXPORTER;
            if ( path.Contains( key ) ) {
                if ( string.IsNullOrWhiteSpace( temp_batchExportCurrentKey ) ) {
                    path = path.Replace( key, string.Empty );
                } else {
                    path = path.Replace( key, ConvertDynamicPath_Main( CurrentSettings.batchFormat, recursiveCount ) );
                }
            }

            path = ReplaceDate( path );

            key = Const_Keys.KEY_NAME;
            path = path.Replace( key, name );

            key = Const_Keys.KEY_VERSION;
            path = path.Replace( key, CurrentSettings.GetExportVersion( ) );
            key = Const_Keys.KEY_FORMATTED_VERSION;
            if ( path.Contains( key ) ) {
                if ( string.IsNullOrWhiteSpace( CurrentSettings.GetExportVersion( ) ) ) {
                    path = path.Replace( key, string.Empty );
                } else {
                    path = path.Replace( key, ConvertDynamicPath_Main( CurrentSettings.versionFormat, recursiveCount ) );
                }
            }

            key = Const_Keys.KEY_PACKAGE_NAME;
            if ( path.Contains( key ) ) {
                var str = ConvertDynamicPath_Main( CurrentSettings.packageName, recursiveCount );
                // ファイル名に使用できない文字を_に置き換え
                str = ExporterUtils.InvalidFileCharsRegex.Replace( str, "_" );
                path = path.Replace( key, str );
            }

            return path;
        }
        #endregion

        public string GetDirectoryPath( ) {
            return Path.GetDirectoryName( AssetDatabase.GetAssetPath( this ) ) + "/";
        }

        /// <summary>
        /// Referencesの候補を取得
        /// </summary>
        /// <returns></returns>
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
        public class FilePathList {
            public IEnumerable<string> paths;
            public IEnumerable<string> excludePaths;
            public Dictionary<string, HashSet<string>> referencedPaths;
        }
        public Dictionary<string, FilePathList> GetAllPath_Batch( ) {
            var result = new Dictionary<string, FilePathList>( );
            if ( batchExportMode == BatchExportMode.Single ) {
                temp_batchExportCurrentKey = string.Empty;
                result.Add( GetExportPath( ), GetAllPath( ) );
                return result;
            } else {
                var texts = BatchExportKeysConverted;
                for ( int i = 0; i < texts.Length; i++ ) {
                    temp_batchExportCurrentKey = texts[i];
                    string path = GetExportPath( );
                    if ( !result.ContainsKey( path ) ) {
                        result.Add( path, GetAllPath( ) );
                    }
                }
                temp_batchExportCurrentKey = string.Empty;
                return result;
            }
        }
        public FilePathList GetAllPath( ) {
#if UNITY_EDITOR
            var references_path = GetReferencesPath( );
            ExporterUtils.DebugLog( "References: \n" + string.Join( "\n", references_path ) );
            bool useReference = references_path.Any( );

            IEnumerable<string> list;
            {
                var list1 = objects.Where( v => !string.IsNullOrWhiteSpace( v.Path ) ).Select( v => v.Path );
                var list2 = dynamicpath
                    .Where( v => !string.IsNullOrWhiteSpace( v ) )
                    .Select( v => {
                        v = ConvertDynamicPath( v );
                        if ( PathUtils.IsRelativePath( v ) ) {
                            v = PathUtils.GetProjectAbsolutePath( GetDirectoryPath(), v );
                        }
                        return v;
                    });
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
            var referencesResults = new Dictionary<string, HashSet<string>>( );
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

                                HashSet<string> referenceFrom;
                                if ( !referencesResults.TryGetValue( dp, out referenceFrom ) ) {
                                    referenceFrom = new HashSet<string>( );
                                    referencesResults.Add( dp, referenceFrom );
                                }
                                referenceFrom.Add( item );

                                ExporterUtils.DebugLog( "Dependency: " + dp );
                                ExporterUtils.DebugLog( "Referenced by: " + item );
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
            ExporterUtils.DebugLog( "Before Exclude: \n" + string.Join( "\n", result ) + "\n" );
            IEnumerable<string> result_enumerable = result;
            foreach ( var item in excludeObjects ) {
                if ( item == null || item.Object == null ) continue;
                var exclude = new SearchPath( SearchPathType.Exact, item.Path );
                result_enumerable = exclude.Filter( result_enumerable, exclude: true, includeSubfiles: true );
            }
            foreach ( var item in excludes ) {
                var exclude = new SearchPath( item.searchType, ConvertDynamicPath( item.value ) );
                result_enumerable = exclude.Filter( result_enumerable, exclude: true, includeSubfiles: true );
            }
            var excludeResults = result.Except( result_enumerable );
            if ( excludeResults.Any( ) ) {
                ExporterUtils.DebugLog( "Excludes Result: \n" + string.Join( "\n", excludeResults ) + "\n" );
            } else {
                ExporterUtils.DebugLog( ExporterTexts.ExcludesWereEmpty );
            }
            return new FilePathList( ) {
                paths = result_enumerable,
                excludePaths = excludeResults,
                referencedPaths = referencesResults
            };
#else
            return new FilePathList( );
#endif
        }
        public static bool AllFileExists( ExporterEditorLogs logs, IEnumerable<string> list ) {
            // ファイルが存在するか確認
            bool result = true;
#if UNITY_EDITOR
            var list_full = list.ToList( );
            Debug.Log( string.Join( "\n", list_full ) );
            for ( int i = 0; i < list_full.Count; i++ ) {
                var path = list_full[i];
                if ( Path.GetExtension( path ).Length != 0 ) {
                    if ( File.Exists( path ) == false ) {
                        var text = ExporterTexts.ExportLogNotFound( path );
                        logs.Add( ExporterEditorLogs.LogType.Error, text );
                        Debug.LogError( text );
                        result = false;

                        list_full[i] = ExporterTexts.FileListNotFoundPathPrefix + path;
                    }
                } else if ( Directory.Exists( path ) == false ) {
                    var text = ExporterTexts.ExportLogNotFound( path );
                    logs.Add( ExporterEditorLogs.LogType.Error, text );
                    Debug.LogError( text );
                    result = false;

                    list_full[i] = ExporterTexts.FileListNotFoundPathPrefix + path;
                }
            }
            if ( result ) {
                var text = ExporterTexts.ExportLogAllFileExists;
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
        static void Export_Internal( ExporterEditorLogs logs, string exportPath, IEnumerable<string> list ) {
#if UNITY_EDITOR
            Debug.Log( exportPath + "\n" + "Start Export: " + string.Join( "/n", list ) );
            // ファイルが存在するか確認
            bool exists = AllFileExists( logs, list );
            if ( exists == false ) {
                string failedText = ExporterTexts.ExportLogFailed( exportPath );
                Debug.LogError( failedText );
                logs.Add( ExporterEditorLogs.LogType.Error, failedText );
                return;
            }

            if ( Directory.Exists( exportPath ) == false ) {
                Directory.CreateDirectory( Path.GetDirectoryName( exportPath ) );
            }
            if ( list.Any( ) ) {
                string[] pathNames = list.ToArray( );
                AssetDatabase.ExportPackage( pathNames, exportPath, ExportPackageOptions.Default );
                EditorUtility.RevealInFinder( exportPath );

                logs.Add( ExporterEditorLogs.LogType.Info, ExporterTexts.ExportLogSuccess( exportPath ) );
                Debug.Log( exportPath + "\nをエクスポートしました。" );
            } else {
                logs.Add( ExporterEditorLogs.LogType.Error, ExporterTexts.ExportLogFailedTargetEmpty( exportPath ) );
                Debug.LogWarning( exportPath + "\nにエクスポートするファイルが何もありませんでした。" );
            }
#endif
        }
        public void Export( ExporterEditorLogs logs, HashSet<string> ignorePaths ) {
#if UNITY_EDITOR
            logs.Clear( );
            UpdateAllExportVersions( );
            UpdateBatchExportKeys( );

            var table = GetAllPath_Batch( );
            foreach ( var kvp in table ) {
                string exportPath = kvp.Key;
                if ( ignorePaths.Contains( exportPath ) ) {
                    ExporterUtils.DebugLog( "Ignore Export: " + exportPath );
                    continue;
                }
                var list = kvp.Value;
                Export_Internal( logs, exportPath, list.paths );
            }
#endif
        }

        public void OnBeforeSerialize( ) {
            s_variables = variables.Select( kvp => new DynamicPathVariable( kvp.Key, kvp.Value ) ).ToArray( );
            s_packageNameSettingsOverride = packageNameSettingsOverride.Select( kvp => new PackageNameSettingsKVP( kvp.Key, kvp.Value ) ).ToArray( );
        }

        public void OnAfterDeserialize( ) {
            if ( s_variables != null ) {
                variables = s_variables.ToDictionary( v => v.key, v => v.value );
            }
            if ( s_packageNameSettingsOverride != null ) {
                packageNameSettingsOverride = s_packageNameSettingsOverride.ToDictionary( v => v.key, v => v.value );
            }
        }

    }
}
