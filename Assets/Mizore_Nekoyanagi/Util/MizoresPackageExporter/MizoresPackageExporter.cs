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
        private class VersionJson {
            public string version;
        }

        public List<PackagePrefsElement> objects = new List<PackagePrefsElement>( );
        public List<string> dynamicpath = new List<string>( );
        [SerializeField]
        DynamicPathVariable[] s_variables;
        [System.NonSerialized]
        public Dictionary<string, string> variables = new Dictionary<string, string>( );
        public PackagePrefsElement versionFile;
        public string versionPrefix="-";

        public string ExportPath { get { return Const.EXPORT_FOLDER_PATH + this.name + versionPrefix + ExportVersion + ".unitypackage"; } }
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
                            _exportVersion = JsonUtility.FromJson<VersionJson>( sr.ReadToEnd() ).version;
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
            var list = objects.Where( v => !string.IsNullOrWhiteSpace( v.Path ) ).Select( v => v.Path );
            list = list.Concat( dynamicpath.Where( v => !string.IsNullOrWhiteSpace( v ) ).Select( v => ConvertDynamicPath( v ) ) );
            return list;
        }
        public bool AllFileExists( ) {
            // ファイルが存在するか確認
            bool result = true;
#if UNITY_EDITOR
            var list = GetAllPath( );
            foreach ( var item in list ) {
                if ( Path.GetExtension( item ).Length != 0 ) {
                    if ( File.Exists( item ) == false ) {
                        var text = string.Format( ExporterTexts.t_ExportLog_NotFound, item );
                        UnityPackageExporterEditor.HelpBoxText += text;
                        UnityPackageExporterEditor.HelpBoxMessageType = MessageType.Error;
                        Debug.LogError( text );
                        result = false;
                    }
                } else if ( Directory.Exists( item ) == false ) {
                    var text = string.Format( ExporterTexts.t_ExportLog_NotFound, item );
                    UnityPackageExporterEditor.HelpBoxText += text;
                    UnityPackageExporterEditor.HelpBoxMessageType = MessageType.Error;
                    Debug.LogError( text );
                    result = false;
                }
            }
            if ( result ) {
                var text = ExporterTexts.t_ExportLog_AllFileExists;
                UnityPackageExporterEditor.HelpBoxText += text;
                UnityPackageExporterEditor.HelpBoxMessageType = MessageType.Info;
                Debug.Log( text );
            }
#endif
            return result;
        }
        public void Export( ) {
#if UNITY_EDITOR
            UpdateExportVersion( );
            var list = GetAllPath( );

            // console
            StringBuilder sb = new StringBuilder( );
            sb.Append( "Start Export: " ).Append( name );
            sb.AppendLine( );
            foreach ( var item in list ) {
                sb.AppendLine( item );
            }
            Debug.Log( sb );
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
            AssetDatabase.ExportPackage( pathNames, exportPath, ExportPackageOptions.Recurse );
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
