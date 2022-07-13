using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.MizoresPackageExporterConsts;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter
{
    [CreateAssetMenu( menuName = "MizoreNekoyanagi/UnityPackageExporter" )]
    public class MizoresPackageExporter : ScriptableObject, ISerializationCallbackReceiver
    {
        [System.Serializable]
        private class VersionJson
        {
            public string version;
        }

        public bool debugmode = false;
        public List<PackagePrefsElement> objects = new List<PackagePrefsElement>( );
        public List<string> dynamicpath = new List<string>( );
        [SerializeField]
        DynamicPathVariable[] s_variables;
        [System.NonSerialized]
        public Dictionary<string, string> variables = new Dictionary<string, string>( );

        public List<SearchPath> excludes = new List<SearchPath>( );
        public List<PackagePrefsElement> references = new List<PackagePrefsElement>( );

        public PackagePrefsElement versionFile;
        public string versionPrefix = "-";

        public string ExportPath {
            get {
                var version = ExportVersion;
                if ( string.IsNullOrWhiteSpace( version ) ) {
                    return Const.EXPORT_FOLDER_PATH + this.name + ".unitypackage";
                } else {
                    return Const.EXPORT_FOLDER_PATH + this.name + versionPrefix + version + ".unitypackage";
                }
            }
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
        const float UPDATE_INTERVAL = 2.5f;
        double lastUpdate;
        string _exportVersion;
        public string ExportVersion {
            get {
#if UNITY_EDITOR
                // 短時間に連続してファイルを読めないようにする
                if ( UPDATE_INTERVAL < EditorApplication.timeSinceStartup - lastUpdate ) {
                    lastUpdate = EditorApplication.timeSinceStartup;
                    UpdateExportVersion( );
                }
#endif
                return ( string.IsNullOrWhiteSpace( _exportVersion ) ) ? string.Empty : _exportVersion;
            }
        }
        public void UpdateExportVersion( ) {
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

        public void DEBUGLOG( string value ) {
            if ( debugmode ) {
                Debug.Log( "[DEBUG] " + value );
            }
        }

        public string ConvertDynamicPath( string path ) {
            if ( string.IsNullOrWhiteSpace( path ) ) return string.Empty;
            foreach ( var kvp in variables ) {
                path = path.Replace( string.Format( "%{0}%", kvp.Key ), kvp.Value );
            }
            path = path.Replace( "%name%", name );
            path = path.Replace( "%version%", ExportVersion );
            path = path.Replace( "%versionprefix%", versionPrefix );
            return path;
        }
        public IEnumerable<string> GetAllPath( ) {
            var list1 = objects.Where( v => !string.IsNullOrWhiteSpace( v.Path ) ).Select( v => v.Path );
            var list2 = dynamicpath.Where( v => !string.IsNullOrWhiteSpace( v ) ).Select( v => ConvertDynamicPath( v ) );
            var result = list1.Concat( list2 );
            return result;
        }
        IEnumerable<string> GetReferencesPath( ) {
            var list1 = references.
                Where( v => !string.IsNullOrWhiteSpace( v.Path ) && Directory.Exists( v.Path ) ).
                SelectMany( v => Directory.GetFiles( v.Path, "*", SearchOption.AllDirectories ) );
            var result = list1;
            // バックスラッシュをスラッシュに統一（Unityのファイル処理ではスラッシュ推奨らしい？）
            result = result.Select( v => v.Replace( '\\', '/' ) );
            return result;
        }
        public IEnumerable<string> GetAllPath_Full( bool decorate = false ) {
#if UNITY_EDITOR
            var references_path = GetReferencesPath( );
            DEBUGLOG( "References: \n" + string.Join( "\n", references_path ) );
            bool useReference = references_path.Any( );

            var list = GetAllPath( );
            var result = new HashSet<string>( );
            foreach ( var item in list ) {
                if ( Directory.Exists( item ) ) {
                    // サブファイル・フォルダを取得
                    result.Add( item );
                    var subdirs = Directory.GetFileSystemEntries( item, "*", SearchOption.AllDirectories );
                    foreach ( var sub in subdirs ) {
                        result.Add( sub );
                    }
                }
            }
            foreach ( var item in list ) {
                if ( Path.GetExtension( item ).Length != 0 ) {
                    if ( useReference ) {
                        var dependencies = AssetDatabase.GetDependencies( item, true );
                        foreach ( var dp in dependencies ) {
                            if ( dp == item ) {
                                result.Add( dp );
                            } else if ( references_path.Contains( dp ) ) {
                                // 依存AssetがReferencesに含まれていたらエクスポート対象に追加
                                if ( decorate ) {
                                    if ( result.Contains( dp ) == false ) {
                                        result.Add( ExporterTexts.t_ExportLog_DependencyPathPrefix + dp );
                                    }
                                } else {
                                    result.Add( dp );
                                }
                                DEBUGLOG( "Dependency: " + dp );
                            } else {
                                DEBUGLOG( "Ignore Dependency: " + dp );
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
            foreach ( var item in excludes ) {
                var exclude = new SearchPath( item.searchType, ConvertDynamicPath( item.value ) );
                result_enumerable = exclude.Filter( result_enumerable, exclude: true, includeSubfiles: true );
            }
            if ( debugmode ) {
                DEBUGLOG( "Excludes: \n" + string.Join( "\n", result.Except( result_enumerable ) ) );
            }
            return result_enumerable;
#else
            return new string[0];
#endif
        }
        public bool AllFileExists( ) {
            // ファイルが存在するか確認
            bool result = true;
#if UNITY_EDITOR
            var list_full = GetAllPath_Full( decorate: true ).ToList( );
            Debug.Log( string.Join( "\n", list_full ) );
            // 依存Assetやサブフォルダは確実に存在するのでチェックは不要
            var list = GetAllPath( );
            foreach ( var item in list ) {
                if ( Path.GetExtension( item ).Length != 0 ) {
                    if ( File.Exists( item ) == false ) {
                        var text = string.Format( ExporterTexts.t_ExportLog_NotFound, item );
                        UnityPackageExporterEditor.HelpBoxText += text;
                        UnityPackageExporterEditor.HelpBoxMessageType = MessageType.Error;
                        Debug.LogError( text );
                        result = false;

                        int index = list_full.IndexOf( item );
                        list_full[index] = ExporterTexts.t_ExportLog_NotFoundPathPrefix + list_full[index];
                    }
                } else if ( Directory.Exists( item ) == false ) {
                    var text = string.Format( ExporterTexts.t_ExportLog_NotFound, item );
                    UnityPackageExporterEditor.HelpBoxText += text;
                    UnityPackageExporterEditor.HelpBoxMessageType = MessageType.Error;
                    Debug.LogError( text );
                    result = false;

                    int index = list_full.IndexOf( item );
                    list_full[index] = ExporterTexts.t_ExportLog_NotFoundPathPrefix + list_full[index];
                }
            }
            if ( result ) {
                var text = ExporterTexts.t_ExportLog_AllFileExists;
                UnityPackageExporterEditor.HelpBoxText += text;
                UnityPackageExporterEditor.HelpBoxMessageType = MessageType.Info;
                Debug.Log( text );
            }
            UnityPackageExporterEditor.HelpBoxText += "----------\n" + string.Join( "\n", list_full ) + "\n----------\n";
#endif
            return result;
        }
        public void Export( ) {
#if UNITY_EDITOR
            UpdateExportVersion( );
            var list = GetAllPath_Full( );
            Debug.Log( "Start Export: " + string.Join( "/n", list ) );
            // ファイルが存在するか確認
            bool exists = AllFileExists( );
            if ( exists == false ) {
                UnityPackageExporterEditor.HelpBoxText += ExporterTexts.t_ExportLog_Failed;
                UnityPackageExporterEditor.HelpBoxMessageType = MessageType.Error;
                return;
            }

            string[] pathNames = list.ToArray( );
            string exportPath = ExportPath;
            if ( Directory.Exists( exportPath ) == false ) {
                Directory.CreateDirectory( Path.GetDirectoryName( exportPath ) );
            }
            AssetDatabase.ExportPackage( pathNames, exportPath, ExportPackageOptions.Default );
            EditorUtility.RevealInFinder( exportPath );

            UnityPackageExporterEditor.HelpBoxText += string.Format( ExporterTexts.t_ExportLog_Success, exportPath );
            UnityPackageExporterEditor.HelpBoxMessageType = MessageType.Info;
            Debug.Log( exportPath + "をエクスポートしました。" );
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
