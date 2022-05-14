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
        public IEnumerable<string> GetAllPath( ) {
            var list = objects.Where( v => !string.IsNullOrWhiteSpace( v.Path ) ).Select( v => v.Path );
            list = list.Concat( dynamicpath.Where( v => !string.IsNullOrWhiteSpace( v ) ).Select( v => ConvertDynamicPath( v ) ) );
            return list;
        }
        public bool AllFileExists( ) {
            // ファイルが存在するか確認
            bool result = true;
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
            return result;
        }
        public void Export( ) {
#if UNITY_EDITOR
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
                return;
            }

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
