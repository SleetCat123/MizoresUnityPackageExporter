using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterConsts;
using Const_Keys = MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterConsts_Keys;
using System.Threading.Tasks;
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
        [System.Serializable]
        public class StringPair {
            public string key;
            public string value;
            public StringPair( string key, string value ) {
                this.key = key;
                this.value = value;
            }
        }
#if UNITY_EDITOR
        public static bool LockEditor = false;
#endif

        public const int INITIAL_PACKAGE_EXPORTER_OBJECT_VERSION = 0;
        public const int CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION = 2;
        [SerializeField]
        public int packageExporterVersion = INITIAL_PACKAGE_EXPORTER_OBJECT_VERSION;

        public List<ExportTargetObjectElement> objects = new List<ExportTargetObjectElement>( );

        [SerializeField]
        DynamicPathVariable[] s_variables;
        [System.NonSerialized]
        public Dictionary<string, string> variables = new Dictionary<string, string>( );

        public List<ObjectRefElement> excludeObjects = new List<ObjectRefElement>( );
        public List<SearchPath> excludes = new List<SearchPath>( );

        public List<ReferenceElement> references = new List<ReferenceElement>( );


        public string postProcessScriptTypeName;
        [System.NonSerialized]
        public Dictionary<string, string> postProcessScriptFieldValues = new Dictionary<string, string>( );
        [SerializeField]
        StringPair[] s_postProcessScriptFieldValues;

        #region PackageName
        public PackageNameSettings packageNameSettings = new PackageNameSettings( );

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

        public string GetPackageName( string batchExportKey ) {
            var currentSettings = GetOverridedSettings( batchExportKey );
            return ConvertDynamicPath( currentSettings.packageName, batchExportKey );
        }
        public string GetExportFileName( string batchExportKey ) {
            return GetPackageName( batchExportKey ) + ".unitypackage";
        }
        public string GetExportPath( string batchExportKey ) {
            return Const.EXPORT_FOLDER_PATH + GetExportFileName( batchExportKey );
        }
        public string[] GetAllExportFileName( string batchExportKey ) {
            if ( batchExportMode == BatchExportMode.Single ) {
                return new string[] { GetExportFileName( string.Empty ) };
            } else {
                var texts = GetBatchExportKeysConverted();
                var result = new string[texts.Length];
                for ( int i = 0; i < texts.Length; i++ ) {
                    result[i] = GetExportFileName( texts[i] );
                }
                return result.Distinct( ).ToArray( );
            }
        }
        public string GetFormattedVersion( string batchExportKey ) {
            var currentSettings = GetOverridedSettings( batchExportKey );
            if ( string.IsNullOrWhiteSpace( currentSettings.GetExportVersion( ) ) ) {
                return string.Empty;
            } else {
                return ConvertDynamicPath( currentSettings.versionFormat, batchExportKey );
            }
        }

        public void UpdateAllExportVersions( ) {
            packageNameSettings.UpdateExportVersion( );
            foreach ( var item in packageNameSettingsOverride.Keys ) {
                GetOverridedSettings( item ).UpdateExportVersion( );
            }
        }
        #endregion

        #region BatchExport
        public BatchExportModeData batchExportMode;
        public BatchExportFolderModeData batchExportFolderMode;
        public List<string> batchExportTexts = new List<string>();
        public ObjectRefElement batchExportFolderRoot;
        public ObjectRefElement batchExportListFile;
        public string batchExportFolderRegex;
        [System.NonSerialized]
        string[] temp_batchExportKeys;

        double lastUpdate_BatchExportKeys;
        public string[] GetBatchExportKeysConverted( ) {
            var list = BatchExportKeys;
            for ( int i = 0; i < list.Length; i++ ) {
                list[i] = ConvertDynamicPath( list[i], string.Empty );
            }
            return list;
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
            //ExporterUtils.DebugLog( "UpdateBatchExportKeys\n" + name );
            lastUpdate_BatchExportKeys = EditorApplication.timeSinceStartup;
            switch ( batchExportMode.value ) {
                default:
                case BatchExportMode.Single:
                    temp_batchExportKeys = new string[0];
                    break;
                case BatchExportMode.Texts:
                    temp_batchExportKeys = batchExportTexts.Distinct( ).ToArray( );
                    break;
                case BatchExportMode.Folders:
                    string path;
                    if ( batchExportFolderRoot == null || batchExportFolderRoot.GetObject( this, string.Empty ) == null ) {
                        // Objectが空ならExporterの場所をルートにする
                        path = GetDirectoryPath( );
                    } else {
                        path = AssetDatabase.GetAssetPath( batchExportFolderRoot.GetObject( this, string.Empty ) );
                    }
                    Regex regex;
                    try {
                        regex = new Regex( batchExportFolderRegex );
                    } catch ( System.ArgumentException ) {
                        regex = new Regex( string.Empty );
                    }
                    IEnumerable<string> files;
                    switch ( batchExportFolderMode.value ) {
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
                    if ( batchExportFolderRoot == null || batchExportFolderRoot.GetObject( this, string.Empty ) == null ) {
                        temp_batchExportKeys = new string[0];
                        break;
                    }
                    var list = new List<string>( );
                    var file = batchExportListFile.GetObject( this, string.Empty ) as TextAsset;
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
        public string ReplaceRelativeName( string key ) {
            return Const_Keys.REGEX_RELATIVE_NAME.Replace( key, m => {
                var value = m.Groups[1].Value;
                // 現在のパスから.の個数だけ上の階層にあるフォルダの名前を取得
                var path = GetDirectoryPath( );
                var count = value.Length;
                for ( int i = 0; i < count; i++ ) {
                    if ( path.Length == 0 ) {
                        break;
                    }
                    path = Path.GetDirectoryName( path );
                }
                if ( path.Length == 0 ) {
                    // 上の階層が無い場合はそのまま返す
                    return key;
                }
                return Path.GetFileName( path );
            }
            );
        }
        public static string ReplaceDate( string key ) {
            var date = System.DateTime.Now;
            return Const_Keys.REGEX_DATE_FORMAT.Replace( key, m => date.ToString( m.Groups[1].Value ) );
        }
        public string ConvertDynamicPath( string path, string batchExportKey ) {
            return ConvertDynamicPath_Main( path, 0, batchExportKey );
        }
        string ConvertDynamicPath_Main( string path, int recursiveCount, string batchExportKey ) {
            if ( string.IsNullOrWhiteSpace( path ) ) return string.Empty;
            if ( 2 < recursiveCount ) {
                return path;
            }
            recursiveCount += 1;

            foreach ( var kvp in variables ) {
                var key = string.Format( "%{0}%", kvp.Key );
                path = path.Replace( key, kvp.Value );
            }

            path = path.Replace( Const_Keys.KEY_BATCH_EXPORTER, batchExportKey );

            var key_batchf = Const_Keys.KEY_FORMATTED_BATCH_EXPORTER;
            var currentSettings = GetOverridedSettings( batchExportKey );
            if ( path.Contains( key_batchf ) ) {
                if ( string.IsNullOrWhiteSpace( batchExportKey ) ) {
                    path = path.Replace( key_batchf, string.Empty );
                } else {
                    path = path.Replace( key_batchf, ConvertDynamicPath_Main( currentSettings.batchFormat, recursiveCount, batchExportKey ) );
                }
            }

            path = ReplaceDate( path );

            path = path.Replace( Const_Keys.KEY_NAME, name );

            path = ReplaceRelativeName( path );

            path = path.Replace( Const_Keys.KEY_VERSION, currentSettings.GetExportVersion( ) );
            var key_versionf = Const_Keys.KEY_FORMATTED_VERSION;
            if ( path.Contains( key_versionf ) ) {
                if ( string.IsNullOrWhiteSpace( currentSettings.GetExportVersion( ) ) ) {
                    path = path.Replace( key_versionf, string.Empty );
                } else {
                    path = path.Replace( key_versionf, ConvertDynamicPath_Main( currentSettings.versionFormat, recursiveCount, batchExportKey ) );
                }
            }

            var key_packagename = Const_Keys.KEY_PACKAGE_NAME;
            if ( path.Contains( key_packagename ) ) {
                var str = ConvertDynamicPath_Main( currentSettings.packageName, recursiveCount, batchExportKey );
                // ファイル名に使用できない文字を_に置き換え
                str = ExporterUtils.InvalidFileCharsRegex.Replace( str, "_" );
                path = path.Replace( key_packagename, str );
            }

            return path;
        }
        #endregion

        public string GetDirectoryPath( ) {
#if UNITY_EDITOR
            return Path.GetDirectoryName( AssetDatabase.GetAssetPath( this ) ) + "\\";
#else
            return string.Empty;
#endif
        }

        /// <summary>
        /// Referencesの候補を取得
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetReferencesPath( string batchExportKey ) {
            List<string> referencePaths = new List<string>( );
            List<string> excludeReferences = new List<string>( );
            foreach ( var v in references ) {
                var path = v.element.GetConvertedPath(this, batchExportKey);
                if ( string.IsNullOrWhiteSpace( path ) ) {
                    continue;
                }
                List<string> list;
                switch ( v.mode.value ) {
                    default:
                    case ReferenceMode.Include:
                        list = referencePaths;
                        break;
                    case ReferenceMode.Exclude:
                        list = excludeReferences;
                        break;
                }
                if ( File.Exists( path ) ) {
                    list.Add( path );
                } else if ( Directory.Exists( path ) ) {
                    list.AddRange( Directory.GetFiles( path, "*", SearchOption.AllDirectories ) );
                }
            }
            // バックスラッシュをスラッシュに統一（Unityのファイル処理ではスラッシュ推奨らしい？）
            referencePaths = referencePaths.Select( v => v.Replace( '\\', '/' ) ).Distinct( ).ToList( );
            excludeReferences = excludeReferences.Select( v => v.Replace( '\\', '/' ) ).Distinct( ).ToList( );
            // includeからexcludeを除外
            return referencePaths.Except( excludeReferences );
        }
        public delegate void GetAllPath_BatchCallback( Dictionary<string, FilePathList> result, int maxCount, string currentPath, bool finished );
        public async Task GetAllPath_Batch( GetAllPath_BatchCallback callback ) {
            await GetAllPath_Batch( null, callback );
        }
        public async Task GetAllPath_Batch( IEnumerable<string> filter, GetAllPath_BatchCallback callback ) {
            var result = new Dictionary<string, FilePathList>( );
            if ( batchExportMode == BatchExportMode.Single ) {
                var path = GetExportPath( string.Empty );
                FilePathList list = null;
                await GetAllPath( ( v ) => list = v, string.Empty );
                result.Add( path, list );
                callback?.Invoke( result, 1, path, true );
            } else {
                var texts = GetBatchExportKeysConverted( );
                var maxCount = texts.Length;
                callback?.Invoke( result, maxCount, "", true );
                for ( int i = 0; i < maxCount; i++ ) {
                    var batchExportKey = texts[i];
                    string path = GetExportPath( batchExportKey );
                    ExporterUtils.DebugLog( path );
                    if ( filter != null && !filter.Contains( path ) ) {
                        continue;
                    }
                    if ( !result.ContainsKey( path ) ) {
                        FilePathList list = null;
                        ExporterUtils.DebugLog( "start GetAllPath" );
                        await GetAllPath( ( v ) => list = v, batchExportKey );
                        ExporterUtils.DebugLog( "end GetAllPath" );
                        result.Add( path, list );
                    }
                    callback?.Invoke( result, maxCount, path, false );
                    await Task.Delay( 10 );
                }
                ExporterUtils.DebugLog( "%batch% set empty" );
                callback?.Invoke( result, maxCount, "", true );
            }
        }
        public async Task GetAllPath( System.Action<FilePathList> callback, string batchExportKey ) {
#if UNITY_EDITOR
            var referencesPaths = GetReferencesPath( batchExportKey);
            if ( ExporterEditorPrefs.DebugMode ) {
                Debug.Log( "References: \n" + string.Join( "\n", referencesPaths ) );
            }
            bool useReference = referencesPaths.Any( );

            Debug.Log( "%batch%: " + batchExportKey );

            List<FilePath> list = new List<FilePath>();
            foreach ( var v in objects ) {
                var path = v.GetConvertedPath( this, batchExportKey );
                if ( string.IsNullOrWhiteSpace( path ) ) {
                    continue;
                }
                list.Add( new FilePath( path, v.searchReference ) );
            }

            await Task.Delay( 1 );
            var list_include_sub = new List<FilePath>( );
            foreach ( var item in list ) {
                if ( Directory.Exists( item.path ) ) {
                    list_include_sub.Add( item );
                    // サブファイル・フォルダを取得
                    var subdirs = Directory.GetFileSystemEntries( item.path, "*", SearchOption.AllDirectories );
                    foreach ( var sub in subdirs ) {
                        var path = sub.Replace( '\\', '/' );
                        if ( !list_include_sub.Any( v => v.path == path ) ) {
                            list_include_sub.Add( new FilePath( path, item.searchReference ) );
                        }
                    }
                } else {
                    if ( !list_include_sub.Any( v => v.path == item.path ) ) {
                        list_include_sub.Add( item );
                    }
                }
            }
            // .metaファイルを除外
            list_include_sub = list_include_sub.Where( v => Path.GetExtension( v.path ) != ".meta" ).ToList( );
            if ( ExporterEditorPrefs.DebugMode ) {
                Debug.Log( "Include(Init): \n" + string.Join( "\n", list_include_sub.Select( v => v.path ) ) );
            }
            await Task.Delay( 1 );

            // 除外指定ファイル・フォルダの検索用
            List< SearchPath> excludeSearchPaths = new List<SearchPath>( );
            foreach ( var v in excludeObjects ) {
                if ( v == null || v.GetObject( this, batchExportKey ) == null ) {
                    continue;
                }
                excludeSearchPaths.Add( new SearchPath( SearchPathType.Exact, v.GetConvertedPath( this, batchExportKey ) ) );
            }
            foreach ( var v in excludes ) {
                excludeSearchPaths.Add( new SearchPath( v.searchType, ConvertDynamicPath( v.value, batchExportKey ) ) );
            }

            await Task.Delay( 1 );

            var result = new HashSet<string>( );
            var result_exclude1 = new HashSet<string>( );
            var referencesResults = new Dictionary<string, HashSet<string>>( );
            var logging_dependencies = new HashSet<string>( );
            var logging_ignoreDependencies = new HashSet<string>( );
            foreach ( var item in list_include_sub ) {
                if ( excludeSearchPaths.Any( v => v.IsMatch( item.path ) ) ) {
                    // 除外対象ならスキップ
                    result_exclude1.Add( item.path );
                    continue;
                }
                if ( Path.GetExtension( item.path ).Length != 0 ) {
                    if ( useReference && item.searchReference ) {
                        // 依存Assetを検索
                        var dependencies = AssetDatabase.GetDependencies( item.path, true );
                        foreach ( var dp in dependencies ) {
                            if ( dp == item.path ) {
                                // 自分自身
                                result.Add( dp );
                            } else if ( referencesPaths.Contains( dp ) ) {
                                // 依存AssetがReferencesに含まれていたらエクスポート対象に追加
                                result.Add( dp );

                                HashSet<string> referenceFrom;
                                if ( !referencesResults.TryGetValue( dp, out referenceFrom ) ) {
                                    referenceFrom = new HashSet<string>( );
                                    referencesResults.Add( dp, referenceFrom );
                                }
                                referenceFrom.Add( item.path );

                                //ExporterUtils.DebugLog( "Dependency: " + dp );
                                //ExporterUtils.DebugLog( "Referenced by: " + item );
                                logging_dependencies.Add( $"{dp} (Referenced by: {item.path})" );
                            } else {
                                // 依存AssetがReferencesに含まれていない場合は無視
                                //ExporterUtils.DebugLog( "Ignore Dependency: " + dp );
                                logging_ignoreDependencies.Add( dp );
                            }
                        }
                    } else {
                        // 依存Assetを検索しない場合はそのまま追加
                        result.Add( item.path );
                    }
                } else if ( Directory.Exists( item.path ) ) {
                    // 何もしない
                } else {
                    // 拡張子が無いファイルはそのまま追加
                    result.Add( item.path );
                }
            }
            if ( ExporterEditorPrefs.DebugMode ) {
                Debug.Log( "Dependencies: \n" + string.Join( "\n", logging_dependencies ) );
                Debug.Log( "Ignore Dependencies: \n" + string.Join( "\n", logging_ignoreDependencies ) );
            }

            await Task.Delay( 1 );
            // 除外指定されたファイル・フォルダを処理（2回目　Referencesで追加されたファイルを除外するために再度処理）
            if ( ExporterEditorPrefs.DebugMode ) {
                Debug.Log( "Before Exclude: \n" + string.Join( "\n", result ) );
            }
            IEnumerable<string> result_enumerable = result;
            foreach ( var exclude in excludeSearchPaths ) {
                result_enumerable = exclude.Filter( result_enumerable, exclude: true, includeSubfiles: true );
                await Task.Delay( 1 );
            }
            var result_exclude2 = result.Except( result_enumerable );

            await Task.Delay( 1 );
            // 除外処理1回目と2回目の結果を結合
            var excludeResults =  result_exclude1.Concat( result_exclude2 );
            if ( ExporterEditorPrefs.DebugMode ) {
                if ( excludeResults.Any( ) ) {
                    Debug.Log( "Excludes Result: \n" + string.Join( "\n", excludeResults ) );
                } else {
                    Debug.Log( ExporterTexts.ExcludesWereEmpty );
                }
            }

            result_enumerable = result_enumerable.OrderBy( v => v );
            Debug.Log( "Export Target: \n" + string.Join( "\n", result_enumerable ) );

            var filePathList = new FilePathList( ) {
                batchExportKey = batchExportKey,
                paths = result_enumerable,
                excludePaths = excludeResults,
                referencedPaths = referencesResults,
            };

            // PostProcessScriptによるパスの追加
            var instanceData = ExportPostProcessUtils.CreateInstance( this );
            if ( instanceData != null ) {
                var postprocessPaths = instanceData.instance.GetPathList( this, filePathList );
                if ( postprocessPaths != null ) {
                    filePathList.postprocessPaths = postprocessPaths;
                }
            }

            callback?.Invoke( filePathList );
#else
            await Task.Delay( 1 );
            callback?.Invoke( new FilePathList( ) );
#endif
        }
        public static bool AllFileExists( ExporterEditorLogs logs, IEnumerable<string> list ) {
            // ファイルが存在するか確認
            bool result = true;
#if UNITY_EDITOR
            var list_full = list.ToList( );
            Debug.Log( "Check FileExists: \n" + string.Join( "\n", list_full ) );
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
        static bool Export_Internal( ExporterEditorLogs logs, string exportPath, IEnumerable<string> list ) {
#if UNITY_EDITOR
            // ファイルが存在するか確認
            bool exists = AllFileExists( logs, list );
            if ( exists == false ) {
                string failedText = ExporterTexts.ExportLogFailed( exportPath );
                Debug.LogError( failedText );
                logs.Add( ExporterEditorLogs.LogType.Error, failedText );
                return false;
            }

            if ( Directory.Exists( exportPath ) == false ) {
                Directory.CreateDirectory( Path.GetDirectoryName( exportPath ) );
            }
            if ( list.Any( ) ) {
                string[] pathNames = list.ToArray( );
                Debug.Log( "Start Export: " + exportPath + "\n" + string.Join( "\n", pathNames ) );
                AssetDatabase.ExportPackage( pathNames, exportPath, ExportPackageOptions.Default );
                EditorUtility.RevealInFinder( exportPath );

                logs.Add( ExporterEditorLogs.LogType.Info, ExporterTexts.ExportLogSuccess( exportPath ) );
                Debug.Log( exportPath + "\nをエクスポートしました。" );
                return true;
            } else {
                logs.Add( ExporterEditorLogs.LogType.Error, ExporterTexts.ExportLogFailedTargetEmpty( exportPath ) );
                Debug.LogWarning( exportPath + "\nにエクスポートするファイルが何もありませんでした。" );
                return false;
            }
#else
            return false;
#endif
        }
        public async Task Export( ExporterEditorLogs logs, HashSet<string> exportPaths ) {
#if UNITY_EDITOR
            try {
                logs.Clear( );
                UpdateAllExportVersions( );
                UpdateBatchExportKeys( );

                MizoresPackageExporter.LockEditor = true;
                Dictionary<string, FilePathList> table = null;
                await GetAllPath_Batch( ( t, max, currentPath, finished ) => {
                    var text = ExporterTexts.ProgressBarInfo_Export( name, currentPath );
                    var progress = t.Count / (float)max;
                    EditorUtility.DisplayProgressBar( ExporterTexts.AssetName, text, progress );
                    if ( finished ) {
                        table = t;
                    }
                } );
                foreach ( var kvp in table ) {
                    string exportPath = kvp.Key;
                    var list = kvp.Value;
                    if ( exportPaths.Contains( exportPath ) == false ) {
                        ExporterUtils.DebugLog( "Ignore Export: " + exportPath );
                        continue;
                    }
                    bool exported = Export_Internal( logs, exportPath, list.paths );
                    if ( exported ) {
                        CallPostProcessScript( this, list.batchExportKey, exportPath, list, logs );
                    }
                }
            } finally {
                EditorUtility.ClearProgressBar( );
                MizoresPackageExporter.LockEditor = false;
            }
#endif
        }
        public static void CallPostProcessScript( MizoresPackageExporter p, string batchExportKey, string exportPath, FilePathList list, ExporterEditorLogs logs ) {
#if UNITY_EDITOR
            var instanceData = ExportPostProcessUtils.CreateInstance( p );
            if ( instanceData == null ) {
                return;
            }
            var type = instanceData.type;
            var instance = instanceData.instance;
            var fields = instanceData.fields;
            Debug.Log( $"Call PostProcessScript: {type.Name}.OnExported" );
            logs.Add( $"Call PostProcessScript: {type.Name}.OnExported" );
            instance.OnExported( p, exportPath, list, logs );
            Debug.Log( $"Finish PostProcessScript: {type.Name}.OnExported" );
            logs.Add( $"Finish PostProcessScript: {type.Name}.OnExported" );
#endif
        }

        public void OnBeforeSerialize( ) {
            s_variables = variables.Select( kvp => new DynamicPathVariable( kvp.Key, kvp.Value ) ).ToArray( );
            s_packageNameSettingsOverride = packageNameSettingsOverride.Select( kvp => new PackageNameSettingsKVP( kvp.Key, kvp.Value ) ).ToArray( );
            s_postProcessScriptFieldValues = postProcessScriptFieldValues.Select( kvp => new StringPair( kvp.Key, kvp.Value ) ).ToArray( );
        }

        public void OnAfterDeserialize( ) {
            if ( s_variables != null ) {
                variables = s_variables.ToDictionary( v => v.key, v => v.value );
            }
            if ( s_packageNameSettingsOverride != null ) {
                packageNameSettingsOverride = s_packageNameSettingsOverride.ToDictionary( v => v.key, v => v.value );
            }
            if ( s_postProcessScriptFieldValues != null ) {
                postProcessScriptFieldValues = s_postProcessScriptFieldValues.ToDictionary( v => v.key, v => v.value );
            }
        }

    }
}
