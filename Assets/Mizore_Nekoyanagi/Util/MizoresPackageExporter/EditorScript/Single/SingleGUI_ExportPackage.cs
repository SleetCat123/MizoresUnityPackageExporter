using UnityEngine;
using System.IO;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.MizoresPackageExporterConsts;
#if UNITY_EDITOR
using UnityEditor;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.SingleEditor
{
    public static class SingleGUI_ExportPackage
    {
        public static void Draw( MizoresPackageExporterEditor ed, MizoresPackageExporter t ) {
            EditorGUILayout.LabelField( ExporterTexts.t_Label_ExportPackage, EditorStyles.boldLabel );
            // Check Button
            if ( GUILayout.Button( ExporterTexts.t_Button_Check ) ) {
                ed.logs.Clear( );
                t.AllFileExists( ed.logs );
            }

            // Check Button
            if ( GUILayout.Button( ExporterTexts.t_Button_Check ) ) {
                FileList.FileListWindow.Show( ed );
            }

            // Export Button
            if ( GUILayout.Button( ExporterTexts.t_Button_ExportPackage ) ) {
                ed.Export( );
            }

            // Open Button
            using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                EditorGUILayout.LabelField( new GUIContent( t.ExportPath, t.ExportPath ) );
                if ( GUILayout.Button( ExporterTexts.TEXT_BUTTON_OPEN, GUILayout.Width( 60 ) ) ) {
                    if ( File.Exists( t.ExportPath ) ) {
                        EditorUtility.RevealInFinder( t.ExportPath );
                    } else {
                        EditorUtility.RevealInFinder( Const.EXPORT_FOLDER_PATH );
                    }
                }
            }
        }
    }
}
#endif
