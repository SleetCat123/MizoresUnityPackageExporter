using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter
{
    [CreateAssetMenu( menuName = "MizoreNekoyanagi/UnityPackageExporter" )]
    public class MizoresPackageExporter : ScriptableObject, ISerializationCallbackReceiver
    {
        public List<PackagePrefsElement> objects = new List<PackagePrefsElement>( );
        public List<string> dynamicpath = new List<string>( );
        [SerializeField]
        DynamicPathVariable[] s_variables;
        [System.NonSerialized]
        public Dictionary<string, string> variables = new Dictionary<string, string>( );
        public PackagePrefsElement versionFile;

        public const string EXPORT_FOLDER_PATH = "MizorePackageExporter/";
        public const string EXPORT_LOG_NOT_FOUND = "[{0}] is not exists. The export has been cancelled.\n[{0}]は存在しません。エクスポートは中断されました。\n";
        public string ExportPath { get { return EXPORT_FOLDER_PATH + this.name + ExportVersion + ".unitypackage"; } }
        public string ExportVersion { get { return versionFile == null || string.IsNullOrEmpty( versionFile.Path ) ? "" : "-" + File.ReadAllText( versionFile.Path ).Trim( ); } }

        public string ConvertDynamicPath( string path ) {
            foreach ( var kvp in variables ) {
                path = path.Replace( string.Format( "%{0}%", kvp.Key ), kvp.Value );
            }
            path = path.Replace( "%name%", name );
            path = path.Replace( "%version%", ExportVersion );
            return path;
        }
        public void Export( ) {
#if UNITY_EDITOR
            var list = objects.Where( v => !string.IsNullOrWhiteSpace( v.Path ) ).Select( v => v.Path );
            list = list.Concat( dynamicpath.Where( v => !string.IsNullOrWhiteSpace( v ) ).Select( v => ConvertDynamicPath( v ) ) );

            // console
            StringBuilder sb = new StringBuilder( );
            sb.Append( "Start Export: " ).Append( name );
            sb.AppendLine( );
            foreach ( var item in list ) {
                sb.AppendLine( item );
            }
            Debug.Log( sb );
            // ファイルが存在するか確認
            foreach ( var item in list ) {
                if ( Path.GetExtension( item ).Length != 0 ) {
                    if ( File.Exists( item ) == false ) {
                        var text = string.Format( EXPORT_LOG_NOT_FOUND, item );
                        UnityPackageExporterEditor.HelpBoxText += text;
                        UnityPackageExporterEditor.HelpBoxMessageType = MessageType.Error;
                        Debug.LogError( text );
                        return;
                    }
                } else if ( Directory.Exists( item ) == false ) {
                    var text = string.Format( EXPORT_LOG_NOT_FOUND, item );
                    UnityPackageExporterEditor.HelpBoxText += text;
                    UnityPackageExporterEditor.HelpBoxMessageType = MessageType.Error;
                    Debug.LogError( text );
                    return;
                }
            }
            //

            string[] pathNames = list.ToArray( );
            string exportPath = ExportPath;
            if ( Directory.Exists( exportPath ) == false ) {
                Directory.CreateDirectory( Path.GetDirectoryName( exportPath ) );
            }
            AssetDatabase.ExportPackage( pathNames, exportPath, ExportPackageOptions.Recurse );
            EditorUtility.RevealInFinder( exportPath );
            Debug.Log( this.name + "をエクスポートしました。" );
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
