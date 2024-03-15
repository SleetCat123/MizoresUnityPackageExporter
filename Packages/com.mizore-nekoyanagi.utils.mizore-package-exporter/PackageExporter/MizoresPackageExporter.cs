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
        [System.Serializable]
        public class StringPair {
            public string key;
            public string value;
            public StringPair( string key, string value ) {
                this.key = key;
                this.value = value;
            }
        }

        public const int INITIAL_PACKAGE_EXPORTER_OBJECT_VERSION = 0;
        public const int CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION = 2;
        [SerializeField]
        private int packageExporterVersion = INITIAL_PACKAGE_EXPORTER_OBJECT_VERSION;
        public int PackageExporterVersion { get => packageExporterVersion; }
        public bool IsCurrentVersion { get => PackageExporterVersion == CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION; }
        public bool IsCompatible { get => PackageExporterVersion <= CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION; }

        public List<ExportTargetObjectElement> objects = new List<ExportTargetObjectElement>( );

        [System.Obsolete, SerializeField]
        List<string> dynamicpath = new List<string>( );
        public List<DynamicPathElement> dynamicpath2 = new List<DynamicPathElement>( );

        [SerializeField]
        DynamicPathVariable[] s_variables;
        [System.NonSerialized]
        public Dictionary<string, string> variables = new Dictionary<string, string>( );

        public List<PackagePrefsElement> excludeObjects = new List<PackagePrefsElement>( );
        public List<SearchPath> excludes = new List<SearchPath>( );

        [System.Obsolete, SerializeField]
        List<PackagePrefsElement> references = new List<PackagePrefsElement>( );

        public List<ReferenceElement> references2 = new List<ReferenceElement>( );


        public string postProcessScriptTypeName;
        [System.NonSerialized]
        public Dictionary<string, string> postProcessScriptFieldValues = new Dictionary<string, string>( );
        [SerializeField]
        StringPair[] s_postProcessScriptFieldValues;

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
            if ( force ) {
                packageExporterVersion = CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION;
                return true;
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
                }
                if ( !string.IsNullOrEmpty( versionFormat ) ) {
                    // versionFormatの場所変更
                    ExporterUtils.DebugLog( "Convert: versionFormat" );
                    packageNameSettings.versionFormat = versionFormat;
                    versionFormat = null;
                }
                if ( !string.IsNullOrEmpty( packageName ) ) {
                    // packageNameの場所変更
                    ExporterUtils.DebugLog( "Convert: packageName" );
                    packageNameSettings.packageName = packageName;
                    packageName = null;
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
        #endregion

        #region BatchExport
        // 互換性のためEnumもSerialize対象にしておく
        public BatchExportMode batchExportMode;
        [SerializeField]string s_batchExportMode;
        // 互換性のためEnumもSerialize対象にしておく
        public BatchExportFolderMode batchExportFolderMode;
        [SerializeField]string s_batchExportFolderMode;
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
            //ExporterUtils.DebugLog( "UpdateBatchExportKeys\n" + name );
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
                    string path;
                    if ( batchExportFolderRoot == null || batchExportFolderRoot.Object == null ) {
                        // Objectが空ならExporterの場所をルートにする
                        path = GetDirectoryPath( );
                    } else {
                        path = AssetDatabase.GetAssetPath( batchExportFolderRoot.Object );
                    }
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
        public string ConvertDynamicPath( string path, bool preview = false ) {
            return ConvertDynamicPath_Main( path, 0, preview );
        }
        string ConvertDynamicPath_Main( string path, int recursiveCount, bool preview ) {
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
            if ( preview ) {
                path = path.Replace( key, "BATCH" );
            } else {
                path = path.Replace( key, temp_batchExportCurrentKey );
            }
            key = Const_Keys.KEY_FORMATTED_BATCH_EXPORTER;
            if ( path.Contains( key ) ) {
                if ( preview ) {
                    path = path.Replace( key, ConvertDynamicPath_Main( CurrentSettings.batchFormat, recursiveCount, preview ) );
                } else {
                    if ( string.IsNullOrWhiteSpace( temp_batchExportCurrentKey ) ) {
                        path = path.Replace( key, string.Empty );
                    } else {
                        path = path.Replace( key, ConvertDynamicPath_Main( CurrentSettings.batchFormat, recursiveCount, preview ) );
                    }
                }
            }

            path = ReplaceDate( path );

            key = Const_Keys.KEY_NAME;
            path = path.Replace( key, name );

            path = ReplaceRelativeName( path );

            key = Const_Keys.KEY_VERSION;
            path = path.Replace( key, CurrentSettings.GetExportVersion( ) );
            key = Const_Keys.KEY_FORMATTED_VERSION;
            if ( path.Contains( key ) ) {
                if ( string.IsNullOrWhiteSpace( CurrentSettings.GetExportVersion( ) ) ) {
                    path = path.Replace( key, string.Empty );
                } else {
                    path = path.Replace( key, ConvertDynamicPath_Main( CurrentSettings.versionFormat, recursiveCount, preview ) );
                }
            }

            key = Const_Keys.KEY_PACKAGE_NAME;
            if ( path.Contains( key ) ) {
                var str = ConvertDynamicPath_Main( CurrentSettings.packageName, recursiveCount, preview );
                // ファイル名に使用できない文字を_に置き換え
                str = ExporterUtils.InvalidFileCharsRegex.Replace( str, "_" );
                path = path.Replace( key, str );
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
        IEnumerable<string> GetReferencesPath( ) {
            List<string> references = new List<string>( );
            List<string> excludeReferences = new List<string>( );
            foreach ( var v in references2 ) {
                var path = v.element.Path;
                if ( string.IsNullOrWhiteSpace( path ) ) {
                    continue;
                }
                List<string> list;
                switch ( v.mode ) {
                    default:
                    case ReferenceMode.Include:
                        list = references;
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
            references = references.Select( v => v.Replace( '\\', '/' ) ).Distinct( ).ToList( );
            excludeReferences = excludeReferences.Select( v => v.Replace( '\\', '/' ) ).Distinct( ).ToList( );
            // includeからexcludeを除外
            return references.Except( excludeReferences );
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
            var referencesPath = GetReferencesPath( );
            ExporterUtils.DebugLog( "References: \n" + string.Join( "\n", referencesPath ) );
            bool useReference = referencesPath.Any( );

            IEnumerable<FilePath> list;
            {
                var list1 = objects.Where( v => !string.IsNullOrWhiteSpace( v.Path ) ).Select( v => new FilePath(v.Path, v.searchReference) );
                var list2 = dynamicpath2
                    .Where( v => !string.IsNullOrWhiteSpace( v.path ) )
                    .Select( v => {
                        var path = ConvertDynamicPath( v.path );
                        if ( PathUtils.IsRelativePath( path ) ) {
                            path = PathUtils.GetProjectAbsolutePath( GetDirectoryPath(), path );
                        }
                        return new FilePath( path, v.searchReference);
                    });
                list = list1.Concat( list2 );
            }

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

            // 除外指定ファイル・フォルダの検索用
            IEnumerable< SearchPath> excludeSearchPaths = excludeObjects.Where( v => v != null && v.Object != null ).Select( v => new SearchPath( SearchPathType.Exact, v.Path ) );
            excludeSearchPaths = excludeSearchPaths.Concat( excludes.Select( v => new SearchPath( v.searchType, ConvertDynamicPath( v.value ) ) ) );

            var result = new HashSet<string>( );
            var result_exclude1 = new HashSet<string>( );
            var referencesResults = new Dictionary<string, HashSet<string>>( );
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
                            } else if ( referencesPath.Contains( dp ) ) {
                                // 依存AssetがReferencesに含まれていたらエクスポート対象に追加
                                result.Add( dp );

                                HashSet<string> referenceFrom;
                                if ( !referencesResults.TryGetValue( dp, out referenceFrom ) ) {
                                    referenceFrom = new HashSet<string>( );
                                    referencesResults.Add( dp, referenceFrom );
                                }
                                referenceFrom.Add( item.path );

                                ExporterUtils.DebugLog( "Dependency: " + dp );
                                ExporterUtils.DebugLog( "Referenced by: " + item );
                            } else {
                                // 依存AssetがReferencesに含まれていない場合は無視
                                ExporterUtils.DebugLog( "Ignore Dependency: " + dp );
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

            // 除外指定されたファイル・フォルダを処理（2回目　Referencesで追加されたファイルを除外するために再度処理）
            ExporterUtils.DebugLog( "Before Exclude: \n" + string.Join( "\n", result ) + "\n" );
            IEnumerable<string> result_enumerable = result;
            foreach ( var exclude in excludeSearchPaths ) {
                result_enumerable = exclude.Filter( result_enumerable, exclude: true, includeSubfiles: true );
            }
            var result_exclude2 = result.Except( result_enumerable );

            // 除外処理1回目と2回目の結果を結合
            var excludeResults =  result_exclude1.Concat( result_exclude2 );
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
        static bool Export_Internal( ExporterEditorLogs logs, string exportPath, IEnumerable<string> list ) {
#if UNITY_EDITOR
            Debug.Log( exportPath + "\n" + "Start Export: " + string.Join( "/n", list ) );
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
                bool exported = Export_Internal( logs, exportPath, list.paths );
                if ( exported ) {
                    CallPostProcessScript( this, exportPath, list, logs );
                }
            }
#endif
        }
        public static void CallPostProcessScript( MizoresPackageExporter p, string exportPath, FilePathList list, ExporterEditorLogs logs ) {
#if UNITY_EDITOR
            if ( !ExporterEditorPrefs.UsePostProcessScript ) {
                return;
            }
            if ( string.IsNullOrEmpty( p.postProcessScriptTypeName ) ) {
                return;
            }
            // 後処理スクリプトを実行
            var type = System.Type.GetType( p.postProcessScriptTypeName );
            if ( type == null ) {
                Debug.LogError( ExporterTexts.PostProcessScriptNotFound( p.postProcessScriptTypeName ) );
            } else {
                if ( !type.GetInterfaces( ).Contains( typeof( IExportPostProcess ) ) ) {
                    Debug.LogError( ExporterTexts.PostProcessScriptNotImplement );
                    return;
                }
                var fields = type.GetFields( System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance );
                // インスタンス化
                var instance = System.Activator.CreateInstance( type ) as IExportPostProcess;
                // フィールドに値を設定
                foreach ( var field in fields ) {
                    string valueStr;
                    if ( p.postProcessScriptFieldValues.TryGetValue( field.Name, out valueStr ) ) {
                        Debug.Log( $"Set field value: {field.Name} = {valueStr} ({field.FieldType})" );
                        object value;
                        if ( ExporterUtils.FromJson( valueStr, field.FieldType, out value ) ) {
                            field.SetValue( instance, value );
                        }
                    }
                }
                Debug.Log( $"Call PostProcessScript: {type.Name}.OnExported" );
                logs.Add( $"Call PostProcessScript: {type.Name}.OnExported" );
                instance.OnExported( p, exportPath, list, logs );
                Debug.Log( $"Finish PostProcessScript: {type.Name}.OnExported" );
                logs.Add( $"Finish PostProcessScript: {type.Name}.OnExported" );
            }
#endif
        }

        public void OnBeforeSerialize( ) {
            s_variables = variables.Select( kvp => new DynamicPathVariable( kvp.Key, kvp.Value ) ).ToArray( );
            s_packageNameSettingsOverride = packageNameSettingsOverride.Select( kvp => new PackageNameSettingsKVP( kvp.Key, kvp.Value ) ).ToArray( );
            s_postProcessScriptFieldValues = postProcessScriptFieldValues.Select( kvp => new StringPair( kvp.Key, kvp.Value ) ).ToArray( );
            s_batchExportMode = batchExportMode.GetString( );
            s_batchExportFolderMode = batchExportFolderMode.GetString( );
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
            if ( string.IsNullOrEmpty( s_batchExportMode ) ) {
                // enumのstring保存が未実装なデータを読み込んだ場合のみ発生
                // enumの順番が変わった場合はここで対応する
            } else {
                batchExportMode = BatchExportModeExtensions.Parse( s_batchExportMode );
            }
            if ( string.IsNullOrEmpty( s_batchExportFolderMode ) ) {
            } else {
                batchExportFolderMode = BatchExportFolderModeExtensions.Parse( s_batchExportFolderMode );
            }
        }

    }
}
