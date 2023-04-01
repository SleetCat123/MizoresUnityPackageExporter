using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor
{
#if UNITY_EDITOR
    public static class GUI_BatchExporter
    {
        public static void Draw( MizoresPackageExporterEditor ed, MizoresPackageExporter t, MizoresPackageExporter[] targetlist ) {
            EditorGUI.BeginChangeCheck( );
            t.batchExportMode = (BatchExportMode)EditorGUILayout.EnumPopup( t.batchExportMode );
            if ( EditorGUI.EndChangeCheck( ) ) {
                EditorUtility.SetDirty( t );
            }

            switch ( t.batchExportMode ) {
                default:
                case BatchExportMode.None:
                    break;
                case BatchExportMode.Texts:
                    break;
                case BatchExportMode.Folders: {

                    EditorGUI.BeginChangeCheck( );
                    using ( new EditorGUILayout.HorizontalScope( ) ) {
                        EditorGUILayout.LabelField( "Folder", GUILayout.Width( 60 ) );
                        PackagePrefsElementInspector.Draw<DefaultAsset>( t.batchExportFolderRoot );
                    }
                    t.batchExportFolderRegex = EditorGUILayout.TextField( "Regex", t.batchExportFolderRegex );
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        t.UpdateBatchExportKeys( );
                        EditorUtility.SetDirty( t );
                    }
                    try {
                        Regex.Match( string.Empty, t.batchExportFolderRegex );
                    } catch ( System.ArgumentException e ) {
                        EditorGUILayout.HelpBox( "Regex Error:\n" + e.Message, MessageType.Error );
                    }

                    var list = t.BatchExportKeys;
                    for ( int i = 0; i < list.Length; i++ ) {
                        using ( new EditorGUILayout.HorizontalScope( ) ) {
                            ExporterUtils.Indent( 1 );
                            EditorGUILayout.LabelField( i.ToString( ), GUILayout.Width( 30 ) );
                            var item = list[i];
                            EditorGUILayout.LabelField( item );
                        }
                    }
                    break;
                }
            }
        }
    }
#endif
}