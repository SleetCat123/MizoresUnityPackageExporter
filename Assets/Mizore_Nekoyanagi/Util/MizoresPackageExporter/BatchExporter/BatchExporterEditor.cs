using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.BatchExport
{
#if UNITY_EDITOR
    [CustomEditor( typeof( BatchExporter ) )]
    public class BatchExporterEditor : Editor
    {
        public BatchExporter t;
        public ExporterEditorLogs logs = new ExporterEditorLogs( );

        string[] _list;

        private void OnEnable( ) {
            t = target as BatchExporter;
        }
        void CreateList( PackagePrefsElement element ) {
            _list = new string[0];
            if ( element == null || element.Object == null ) {
                return;
            }
            string path = AssetDatabase.GetAssetPath( element.Object );
            var regex = new Regex( t.regex );
            _list = Directory.GetDirectories( path ).Select( v => Path.GetFileName( v ) ).Where( v => regex.IsMatch( v ) ).ToArray( );
        }
        public override void OnInspectorGUI( ) {
            EditorGUI.BeginChangeCheck( );
            t.target = EditorGUILayout.ObjectField( "Setting", t.target, typeof( MizoresPackageExporter ), false ) as MizoresPackageExporter;
            using ( new EditorGUILayout.HorizontalScope( ) ) {
                EditorGUILayout.LabelField( "Folder", GUILayout.Width( 60 ) );
                PackagePrefsElementInspector.Draw<DefaultAsset>( t.root );
            }
            t.regex = EditorGUILayout.TextField( "Regex", t.regex );
            if ( EditorGUI.EndChangeCheck( ) ) {
                CreateList( t.root );
                EditorUtility.SetDirty( t );
            }
            if ( _list == null ) {
                CreateList( t.root );
            }
            for ( int i = 0; i < _list.Length; i++ ) {
                var item = _list[i];
                EditorGUILayout.LabelField( item );
            }
            bool error = false;
            if ( !t.target.packageName.Contains( MizoresPackageExporterConsts_Keys.KEY_BATCH_EXPORTER ) ) {
                error = true;
                EditorGUILayout.HelpBox( "error", MessageType.Error );
            }
            using ( new EditorGUI.DisabledGroupScope( error ) ) {
                if ( GUILayout.Button( ExporterTexts.t_Button_ExportPackages ) ) {
                    var action = new PrePostPostProcessing( );
                    var temp = _list.Clone( ) as string[];
                    action.export_preprocessing = ( v, index ) => {
                        v.variablesOverride = new Dictionary<string, string>( ) {
                        {  MizoresPackageExporterConsts_Keys.KEY_BATCH_EXPORTER, temp[index]}
                    };
                    };
                    action.filelist_preprocessing = action.export_preprocessing;

                    action.export_postprocessing = ( v, index ) => {
                        v.variablesOverride = null;
                    };
                    // action.filelist_postprocessing = action.export_postprocessing;

                    var targetlist = new MizoresPackageExporter[_list.Length];
                    for ( int i = 0; i < targetlist.Length; i++ ) {
                        targetlist[i] = t.target;
                    }
                    FileList.FileListWindow.Show( logs, targetlist, action );
                }
            }
        }
    }
#endif
}