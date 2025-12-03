using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterConsts;
using Const_Keys = MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterConsts_Keys;
using System.Threading.Tasks;
using System.Text;
#if UNITY_2022_1_OR_NEWER
using System.IO.Compression;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter
{
    [CreateAssetMenu(menuName = "MizoreNekoyanagi/UnityPackageExporter")]
    public class MizoresPackageExporter : ScriptableObject, ISerializationCallbackReceiver
    {
        [System.Serializable]
        private class PackageNameSettingsKVP
        {
            public string key;
            public PackageNameSettings value;

            public PackageNameSettingsKVP(string key, PackageNameSettings value)
            {
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

        public List<ExportTargetObjectElement> objects = new List<ExportTargetObjectElement>();

        [SerializeField]
        DynamicPathVariable[] s_variables;
        [System.NonSerialized]
        public Dictionary<string, string> variables = new Dictionary<string, string>();

        public List<ObjectRefElement> excludeObjects = new List<ObjectRefElement>();
        public List<SearchPath> excludes = new List<SearchPath>();

        public List<ReferenceElement> references = new List<ReferenceElement>();

        #region PostExport
        [Tooltip( "エクスポート後にパッケージを同名フォルダに整理する" )]
        public bool organizeInFolder = false;

        [Tooltip( "追加でコピーするファイル/フォルダ" )]
        public List<AdditionalCopyPath> additionalCopyPaths = new List<AdditionalCopyPath>();

        [Tooltip( "zip化する" )]
        public bool createZip = false;

#if UNITY_2022_1_OR_NEWER
        [Tooltip( "zip圧縮レベル" )]
        public CompressionLevel compressionLevel = CompressionLevel.Optimal;
#endif

        [Tooltip( "zipを出力するフォルダ名（空の場合はパッケージと同じ場所）" )]
        public string zipFolderName = "_zip";
        #endregion

        #region PackageName
        public PackageNameSettings packageNameSettings = new PackageNameSettings();

        [SerializeField]
        PackageNameSettingsKVP[] s_packageNameSettingsOverride;
        [System.NonSerialized]
        public Dictionary<string, PackageNameSettings> packageNameSettingsOverride = new Dictionary<string, PackageNameSettings>();
        public PackageNameSettings GetOverridedSettings(string batchExportKey)
        {
            if (string.IsNullOrEmpty(batchExportKey))
            {
                return packageNameSettings;
            }
            PackageNameSettings ov;
            if (packageNameSettingsOverride.TryGetValue(batchExportKey, out ov))
            {
                ov.SetBase(packageNameSettings);
                ov.debug_id = batchExportKey;
                return ov;
            }
            else
            {
                return packageNameSettings;
            }
        }

        public string GetPackageName(string batchExportKey)
        {
            var currentSettings = GetOverridedSettings(batchExportKey);
            return ConvertDynamicPath(currentSettings.packageName, batchExportKey);
        }
        public string GetExportFileName(string batchExportKey)
        {
            return GetPackageName(batchExportKey) + ".unitypackage";
        }
        public string GetExportPath(string batchExportKey)
        {
            return Const.EXPORT_FOLDER_PATH + GetExportFileName(batchExportKey);
        }
        public string[] GetAllExportFileName(string batchExportKey)
        {
            if (batchExportMode == BatchExportMode.Single)
            {
                return new string[] { GetExportFileName(string.Empty) };
            }
            else
            {
                var texts = GetBatchExportKeysConverted();
                var result = new string[texts.Length];
                for (int i = 0; i < texts.Length; i++)
                {
                    result[i] = GetExportFileName(texts[i]);
                }
                return result.Distinct().ToArray();
            }
        }
        public string GetFormattedVersion(string batchExportKey)
        {
            var currentSettings = GetOverridedSettings(batchExportKey);
            if (string.IsNullOrWhiteSpace(currentSettings.GetExportVersion()))
            {
                return string.Empty;
            }
            else
            {
                return ConvertDynamicPath(currentSettings.versionFormat, batchExportKey);
            }
        }

        public void UpdateAllExportVersions()
        {
            packageNameSettings.UpdateExportVersion();
            foreach (var item in packageNameSettingsOverride.Keys)
            {
                GetOverridedSettings(item).UpdateExportVersion();
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
        public string[] GetBatchExportKeysConverted()
        {
            var list = BatchExportKeys;
            for (int i = 0; i < list.Length; i++)
            {
                list[i] = ConvertDynamicPath(list[i], string.Empty);
            }
            return list;
        }
        public bool CanUpdateBatchExportKeys()
        {
#if UNITY_EDITOR
            return 5f < EditorApplication.timeSinceStartup - lastUpdate_BatchExportKeys;
#else
            return false;
#endif
        }
        public string[] BatchExportKeys
        {
            get
            {
#if UNITY_EDITOR
                // 短時間に連続してファイルを読めないようにする
                if ( CanUpdateBatchExportKeys( ) || temp_batchExportKeys == null ) {
                    UpdateBatchExportKeys( );
                }
#endif
                return temp_batchExportKeys;
            }
        }
        public void UpdateBatchExportKeys()
        {
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
                    // ファイルの場合は拡張子を除去、フォルダの場合はそのまま使用
                    files = files.Select( v => {
                        string fullPath = Path.Combine( path, v );
                        return File.Exists( fullPath ) ? Path.GetFileNameWithoutExtension( v ) : v;
                    } ).Where( v => regex.IsMatch( v ) );
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
        public string ReplaceRelativeName(string key)
        {
            return Const_Keys.REGEX_RELATIVE_NAME.Replace(key, m =>
            {
                var value = m.Groups[1].Value;
                // 現在のパスから.の個数だけ上の階層にあるフォルダの名前を取得
                var path = GetDirectoryPath();
                var count = value.Length;
                for (int i = 0; i < count; i++)
                {
                    if (path.Length == 0)
                    {
                        break;
                    }
                    path = Path.GetDirectoryName(path);
                }
                if (path.Length == 0)
                {
                    // 上の階層が無い場合はそのまま返す
                    return key;
                }
                return Path.GetFileName(path);
            }
            );
        }
        public static string ReplaceDate(string key)
        {
            var date = System.DateTime.Now;
            return Const_Keys.REGEX_DATE_FORMAT.Replace(key, m => date.ToString(m.Groups[1].Value));
        }
        public string ConvertDynamicPath(string path, string batchExportKey)
        {
            return ConvertDynamicPath_Main(path, 0, batchExportKey);
        }
        string ConvertDynamicPath_Main(string path, int recursiveCount, string batchExportKey)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;
            if (2 < recursiveCount)
            {
                return path;
            }
            recursiveCount += 1;

            foreach (var kvp in variables)
            {
                var key = string.Format("%{0}%", kvp.Key);
                path = path.Replace(key, kvp.Value);
            }

            path = path.Replace(Const_Keys.KEY_BATCH_EXPORTER, batchExportKey);

            var key_batchf = Const_Keys.KEY_FORMATTED_BATCH_EXPORTER;
            var currentSettings = GetOverridedSettings(batchExportKey);
            if (path.Contains(key_batchf))
            {
                if (string.IsNullOrWhiteSpace(batchExportKey))
                {
                    path = path.Replace(key_batchf, string.Empty);
                }
                else
                {
                    path = path.Replace(key_batchf, ConvertDynamicPath_Main(currentSettings.batchFormat, recursiveCount, batchExportKey));
                }
            }

            path = ReplaceDate(path);

            path = path.Replace(Const_Keys.KEY_NAME, name);

            path = ReplaceRelativeName(path);

            path = path.Replace(Const_Keys.KEY_VERSION, currentSettings.GetExportVersion());
            var key_versionf = Const_Keys.KEY_FORMATTED_VERSION;
            if (path.Contains(key_versionf))
            {
                if (string.IsNullOrWhiteSpace(currentSettings.GetExportVersion()))
                {
                    path = path.Replace(key_versionf, string.Empty);
                }
                else
                {
                    path = path.Replace(key_versionf, ConvertDynamicPath_Main(currentSettings.versionFormat, recursiveCount, batchExportKey));
                }
            }

            var key_packagename = Const_Keys.KEY_PACKAGE_NAME;
            if (path.Contains(key_packagename))
            {
                var str = ConvertDynamicPath_Main(currentSettings.packageName, recursiveCount, batchExportKey);
                // ファイル名に使用できない文字を_に置き換え
                str = ExporterUtils.InvalidFileCharsRegex.Replace(str, "_");
                path = path.Replace(key_packagename, str);
            }

            return path;
        }
        #endregion

        public string GetDirectoryPath()
        {
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
        IEnumerable<string> GetReferencesPath(string batchExportKey)
        {
            List<string> referencePaths = new List<string>();
            List<string> excludeReferences = new List<string>();
            foreach (var v in references)
            {
                var path = v.element.GetConvertedPath(this, batchExportKey);
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }
                List<string> list;
                switch (v.mode.value)
                {
                    default:
                    case ReferenceMode.Include:
                        list = referencePaths;
                        break;
                    case ReferenceMode.Exclude:
                        list = excludeReferences;
                        break;
                }
                if (File.Exists(path))
                {
                    list.Add(path);
                }
                else if (Directory.Exists(path))
                {
                    list.AddRange(Directory.GetFiles(path, "*", SearchOption.AllDirectories));
                }
            }
            // バックスラッシュをスラッシュに統一（Unityのファイル処理ではスラッシュ推奨らしい？）
            referencePaths = referencePaths.Select(v => v.Replace('\\', '/')).Distinct().ToList();
            excludeReferences = excludeReferences.Select(v => v.Replace('\\', '/')).Distinct().ToList();
            // includeからexcludeを除外
            return referencePaths.Except(excludeReferences);
        }
        public delegate void GetAllPath_BatchCallback(Dictionary<string, FilePathList> result, int maxCount, string currentPath, bool finished);
        public async Task GetAllPath_Batch(GetAllPath_BatchCallback callback)
        {
            await GetAllPath_Batch(null, callback);
        }
        public async Task GetAllPath_Batch(IEnumerable<string> filter, GetAllPath_BatchCallback callback)
        {
            var result = new Dictionary<string, FilePathList>();
            if (batchExportMode == BatchExportMode.Single)
            {
                var path = GetExportPath(string.Empty);
                FilePathList list = null;
                await GetAllPath((v) => list = v, string.Empty);
                result.Add(path, list);
                callback?.Invoke(result, 1, path, true);
            }
            else
            {
                var texts = GetBatchExportKeysConverted();
                var maxCount = texts.Length;
                callback?.Invoke(result, maxCount, "", true);
                for (int i = 0; i < maxCount; i++)
                {
                    var batchExportKey = texts[i];
                    string path = GetExportPath(batchExportKey);
                    ExporterUtils.DebugLog(path);
                    if (filter != null && !filter.Contains(path))
                    {
                        continue;
                    }
                    if (!result.ContainsKey(path))
                    {
                        FilePathList list = null;
                        ExporterUtils.DebugLog("start GetAllPath");
                        await GetAllPath((v) => list = v, batchExportKey);
                        ExporterUtils.DebugLog("end GetAllPath");
                        result.Add(path, list);
                    }
                    callback?.Invoke(result, maxCount, path, false);
                    await Task.Delay(10);
                }
                ExporterUtils.DebugLog("%batch% set empty");
                callback?.Invoke(result, maxCount, "", true);
            }
        }
        public async Task GetAllPath(System.Action<FilePathList> callback, string batchExportKey)
        {
#if UNITY_EDITOR
            var referencesPaths = GetReferencesPath( batchExportKey );
            if ( ExporterEditorPrefs.DebugMode ) {
                Debug.Log( "References: \n" + string.Join( "\n", referencesPaths ) );
            }
            bool useReference = referencesPaths.Any( );

            Debug.Log( "%batch%: " + batchExportKey );

            List<FilePath> list = new List<FilePath>( );
            foreach ( var obj in objects ) {
                var path = obj.GetConvertedPath( this, batchExportKey );
                if ( string.IsNullOrWhiteSpace( path ) ) {
                    continue;
                }
                var element = new FilePath( path, obj.searchReference );
                list.Add( element );
                if ( Directory.Exists( element.path ) ) {
                    list.Add( element );
                    // サブファイル・フォルダを取得
                    var subdirs = Directory.GetFileSystemEntries( element.path, "*", SearchOption.AllDirectories );
                    foreach ( var sub in subdirs ) {
                        var subpath = sub.Replace( '\\', '/' );
                        if ( !list.Any( v => v.path == subpath ) ) {
                            list.Add( new FilePath( subpath, element.searchReference ) );
                        }
                    }
                } else {
                    if ( !list.Any( v => v.path == element.path ) ) {
                        list.Add( element );
                    }
                }
            }

            await Task.Delay( 1 );

            // .metaファイルを除外
            list = list.Where( v => Path.GetExtension( v.path ) != ".meta" ).ToList( );
            if ( ExporterEditorPrefs.DebugMode ) {
                Debug.Log( "Include(Init): \n" + string.Join( "\n", list.Select( v => v.path ) ) );
            }
            await Task.Delay( 1 );

            // 除外指定ファイル・フォルダの検索用
            List<SearchPath> excludeSearchPaths = new List<SearchPath>( );
            foreach ( var v in excludeObjects ) {
                if ( v == null || v.GetObject( this, batchExportKey ) == null ) {
                    continue;
                }
                var convertedPath = v.GetConvertedPath( this, batchExportKey );
                // フォルダの場合はStartsWithで前方一致検索し、フォルダ内のファイルも除外対象にする
                var searchType = Directory.Exists( convertedPath ) ? SearchPathType.StartsWith : SearchPathType.Exact;
                excludeSearchPaths.Add( new SearchPath( searchType, true, convertedPath ) );
            }
            foreach ( var v in excludes ) {
                excludeSearchPaths.Add( new SearchPath( v.searchType, v.PreserveCase, ConvertDynamicPath( v.Value, batchExportKey ) ) );
            }

            await Task.Delay( 1 );

            var paths = new HashSet<string>( );
            var excludePaths = new HashSet<string>( );
            var referencesResults = new Dictionary<string, HashSet<string>>( );
            var logging_dependencies = new StringBuilder( "Dependencies: \n" );
            var logging_ignoreDependencies = new StringBuilder( "Ignore Dependencies: \n" );
            foreach ( var item in list ) {
                if ( excludeSearchPaths.Any( v => v.IsMatch( item.path ) ) ) {
                    // 除外対象ならスキップ
                    excludePaths.Add( item.path );
                    continue;
                }
                if ( Path.GetExtension( item.path ).Length != 0 ) {
                    if ( useReference && item.searchReference ) {
                        // 依存Assetを検索
                        var dependencies = AssetDatabase.GetDependencies( item.path, true );
                        foreach ( var dp in dependencies ) {
                            if ( dp == item.path ) {
                                // 自分自身
                                paths.Add( dp );
                            } else if ( referencesPaths.Contains( dp ) ) {
                                // 依存AssetがReferencesに含まれていたらエクスポート対象に追加
                                paths.Add( dp );

                                HashSet<string> referenceFrom;
                                if ( !referencesResults.TryGetValue( dp, out referenceFrom ) ) {
                                    referenceFrom = new HashSet<string>( );
                                    referencesResults.Add( dp, referenceFrom );
                                }
                                referenceFrom.Add( item.path );

                                //ExporterUtils.DebugLog( "Dependency: " + dp );
                                //ExporterUtils.DebugLog( "Referenced by: " + item );
                                logging_dependencies.AppendLine( $"{dp} (Referenced by: {item.path})" );
                            } else {
                                // 依存AssetがReferencesに含まれていない場合は無視
                                //ExporterUtils.DebugLog( "Ignore Dependency: " + dp );
                                logging_ignoreDependencies.AppendLine( dp );
                            }
                        }
                    } else {
                        // 依存Assetを検索しない場合はそのまま追加
                        paths.Add( item.path );
                    }
                } else if ( Directory.Exists( item.path ) ) {
                    // 何もしない
                } else {
                    // 拡張子が無いファイルはそのまま追加
                    paths.Add( item.path );
                }
            }
            if ( ExporterEditorPrefs.DebugMode ) {
                Debug.Log( logging_dependencies.ToString( ) );
                Debug.Log( logging_ignoreDependencies.ToString( ) );
            }

            await Task.Delay( 1 );
            // 除外指定されたファイル・フォルダを処理（2回目　Referencesで追加されたファイルを除外するために再度処理）
            if ( ExporterEditorPrefs.DebugMode ) {
                Debug.Log( "Before Exclude: \n" + string.Join( "\n", paths ) );
            }
            IEnumerable<string> result_enumerable = paths;
            foreach ( var exclude in excludeSearchPaths ) {
                var matchPaths = exclude.GetMatchPaths( result_enumerable, includeSubfiles: true );
                result_enumerable = result_enumerable.Except( matchPaths );
                await Task.Delay( 1 );
            }
            await Task.Delay( 1 );
            // 除外処理前後の差分をとる
            excludePaths.UnionWith( paths.Except( result_enumerable ) );
            await Task.Delay( 1 );
            if ( ExporterEditorPrefs.DebugMode ) {
                if ( excludePaths.Any( ) ) {
                    Debug.Log( "Excludes Result: \n" + string.Join( "\n", excludePaths ) );
                } else {
                    Debug.Log( ExporterTexts.ExcludesWereEmpty );
                }
            }

            result_enumerable = result_enumerable.OrderBy( v => v );
            Debug.Log( "Export Target: \n" + string.Join( "\n", result_enumerable ) );

            var filePathList = new FilePathList( ) {
                batchExportKey = batchExportKey,
                paths = result_enumerable,
                excludePaths = excludePaths,
                referencedPaths = referencesResults,
            };

            callback?.Invoke( filePathList );
#else
            await Task.Delay(1);
            callback?.Invoke(new FilePathList());
#endif
        }
        public static bool AllFileExists(ExporterEditorLogs logs, IEnumerable<string> list)
        {
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
        static bool Export_Internal(ExporterEditorLogs logs, string exportPath, IEnumerable<string> list)
        {
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
        public async Task Export(ExporterEditorLogs logs, HashSet<string> exportPaths)
        {
#if UNITY_EDITOR
            try {
                logs.Clear( );
                UpdateAllExportVersions( );
                UpdateBatchExportKeys( );

                MizoresPackageExporter.LockEditor = true;
                Dictionary<string, FilePathList> table = null;
                await GetAllPath_Batch( ( t, max, currentPath, finished ) => {
                    var text = ExporterTexts.ProgressBarInfo_Export( name, currentPath );
                    var progress = t.Count / ( float )max;
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
                        ExecutePostExport( this, list.batchExportKey, exportPath, list, logs );
                    }
                }
            } finally {
                EditorUtility.ClearProgressBar( );
                MizoresPackageExporter.LockEditor = false;
            }
#endif
        }

        /// <summary>
        /// エクスポート後の処理を実行（フォルダ整理、追加コピー、zip化）
        /// </summary>
        public static void ExecutePostExport( MizoresPackageExporter p, string batchExportKey, string exportPath, FilePathList list, ExporterEditorLogs logs )
        {
#if UNITY_EDITOR
            // フォルダ整理、追加コピー、zip化のいずれかが有効な場合のみ処理
            if ( !p.organizeInFolder && p.additionalCopyPaths.Count == 0 && !p.createZip ) {
                return;
            }

            var dir = Path.GetDirectoryName( exportPath );
            var packageName = Path.GetFileNameWithoutExtension( exportPath );
            string folderPath;

            if ( p.organizeInFolder ) {
                // パッケージを同名フォルダに整理
                folderPath = Path.Combine( dir, packageName );
                if ( Directory.Exists( folderPath ) ) {
                    // 同名のフォルダがある場合はタイムスタンプを付加してリネーム
                    var lastWriteTime = Directory.GetLastWriteTime( folderPath );
                    var timeStr = lastWriteTime.ToString( "yyyyMMdd_HHmmss" );
                    var old = folderPath + "_" + timeStr;
                    int i = 0;
                    while ( Directory.Exists( old ) ) {
                        old = folderPath + "_" + timeStr + "_" + i;
                        i++;
                    }
                    Directory.Move( folderPath, old );
                    Debug.Log( "Rename old folder: " + old );
                    logs.Add( "Rename old folder: " + old );
                }
                Directory.CreateDirectory( folderPath );
                // パッケージをフォルダに移動
                var newPackagePath = Path.Combine( folderPath, Path.GetFileName( exportPath ) );
                File.Move( exportPath, newPackagePath );
                Debug.Log( "Move package to folder: " + newPackagePath );
                logs.Add( "Move package to folder: " + newPackagePath );
            } else {
                folderPath = dir;
            }

            // 追加ファイル/フォルダのコピー
            foreach ( var copyPath in p.additionalCopyPaths ) {
                if ( copyPath == null || copyPath.sourcePath == null ) {
                    continue;
                }
                var convertedPath = copyPath.GetConvertedSourcePath( p, batchExportKey );
                if ( string.IsNullOrWhiteSpace( convertedPath ) ) {
                    Debug.LogWarning( "Invalid path: " + copyPath.sourcePath );
                    continue;
                }
                var destName = copyPath.GetConvertedDestName( p, batchExportKey );

                if ( File.Exists( convertedPath ) ) {
                    // ファイルの場合はコピー
                    var fileName = destName ?? Path.GetFileName( convertedPath );
                    var destPath = Path.Combine( folderPath, fileName );
                    Debug.Log( "Copy File: " + convertedPath + " -> " + destPath );
                    logs.Add( "Copy File: " + convertedPath + " -> " + destPath );
                    Directory.CreateDirectory( Path.GetDirectoryName( destPath ) );
                    if ( File.Exists( destPath ) ) {
                        File.Delete( destPath );
                    }
                    File.Copy( convertedPath, destPath );
                } else if ( Directory.Exists( convertedPath ) ) {
                    // フォルダの場合は中身をコピー
                    var destFolderName = destName;
                    Debug.Log( "Copy Folder: " + convertedPath + ( destFolderName != null ? " -> " + destFolderName : "" ) );
                    logs.Add( "Copy Folder: " + convertedPath + ( destFolderName != null ? " -> " + destFolderName : "" ) );
                    var files = Directory.GetFiles( convertedPath, "*", SearchOption.AllDirectories );
                    foreach ( var file in files ) {
                        // .metaファイルはコピーしない
                        if ( Path.GetExtension( file ) == ".meta" ) {
                            continue;
                        }
                        // フォルダ構造を維持してコピー
                        var relativePath = file.Substring( convertedPath.Length + 1 );
                        string destPath;
                        if ( destFolderName != null ) {
                            destPath = Path.Combine( folderPath, destFolderName, relativePath );
                        } else {
                            destPath = Path.Combine( folderPath, relativePath );
                        }
                        Directory.CreateDirectory( Path.GetDirectoryName( destPath ) );
                        if ( File.Exists( destPath ) ) {
                            File.Delete( destPath );
                        }
                        File.Copy( file, destPath );
                        Debug.Log( "Copy File: " + file + " -> " + destPath );
                        logs.Add( "Copy File: " + file + " -> " + destPath );
                    }
                } else {
                    Debug.LogWarning( "Path not found: " + convertedPath );
                    logs.Add( ExporterEditorLogs.LogType.Warning, "Path not found: " + convertedPath );
                }
            }

#if UNITY_2022_1_OR_NEWER
            // zip化
            if ( p.createZip && p.organizeInFolder ) {
                string zipDir;
                if ( string.IsNullOrEmpty( p.zipFolderName ) ) {
                    zipDir = dir;
                } else {
                    zipDir = Path.Combine( dir, p.zipFolderName );
                    if ( !Directory.Exists( zipDir ) ) {
                        Directory.CreateDirectory( zipDir );
                    }
                }
                var zipPath = Path.Combine( zipDir, packageName + ".zip" );
                if ( File.Exists( zipPath ) ) {
                    File.Delete( zipPath );
                }
                Debug.Log( "Create zip: " + zipPath );
                logs.Add( "Create zip: " + zipPath );
                ZipFile.CreateFromDirectory( folderPath, zipPath, p.compressionLevel, false );
                Debug.Log( "zip created: " + zipPath );
                logs.Add( "zip created: " + zipPath );
            } else if ( p.createZip && !p.organizeInFolder ) {
                Debug.LogWarning( "createZip requires organizeInFolder to be enabled" );
                logs.Add( ExporterEditorLogs.LogType.Warning, "createZip requires organizeInFolder to be enabled" );
            }
#else
            if ( p.createZip ) {
                Debug.LogError( "createZip is not supported on this version of Unity (requires 2022.1 or newer)" );
                logs.Add( ExporterEditorLogs.LogType.Error, "createZip is not supported on this version of Unity (requires 2022.1 or newer)" );
            }
#endif
#endif
        }

        public void OnBeforeSerialize()
        {
            s_variables = variables.Select(kvp => new DynamicPathVariable(kvp.Key, kvp.Value)).ToArray();
            s_packageNameSettingsOverride = packageNameSettingsOverride.Select(kvp => new PackageNameSettingsKVP(kvp.Key, kvp.Value)).ToArray();
        }

        public void OnAfterDeserialize()
        {
            if (s_variables != null)
            {
                variables = s_variables.ToDictionary(v => v.key, v => v.value);
            }
            if (s_packageNameSettingsOverride != null)
            {
                packageNameSettingsOverride = s_packageNameSettingsOverride.ToDictionary(v => v.key, v => v.value);
            }
        }
    }
}
