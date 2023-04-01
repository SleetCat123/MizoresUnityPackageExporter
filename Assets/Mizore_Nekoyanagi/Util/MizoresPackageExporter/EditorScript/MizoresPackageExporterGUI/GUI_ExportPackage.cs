using UnityEngine;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterConsts;
using System.Collections.Generic;
using System.Linq;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor
{
#if UNITY_EDITOR
    public static class GUI_ExportPackage
    {
        public static void Draw( MizoresPackageExporterEditor ed, MizoresPackageExporter[] targetlist ) {
            EditorGUILayout.LabelField( ExporterTexts.t_Label_ExportPackage, EditorStyles.boldLabel );
            // Check Button
            if ( GUILayout.Button( ExporterTexts.t_Button_Check ) ) {
                ed.logs.Clear( );
                foreach ( var item in targetlist ) {
                    item.AllFileExists( ed.logs );
                }
            }

            // List Button
            if ( GUILayout.Button( ExporterTexts.t_Button_ExportPackages ) ) {
                FileList.FileListWindow.Show( ed.logs, targetlist.ToArray( ) );
            }

            // 出力先一覧
            foreach ( var item in targetlist ) {
                var path = item.ExportPath;
                EditorGUILayout.LabelField( new GUIContent( path, path ) );
            }
            if ( GUILayout.Button( ExporterTexts.TEXT_BUTTON_OPEN, GUILayout.Width( 60 ) ) ) {
                if ( ed.targets.Length == 1 && File.Exists( ed.t.ExportPath ) ) {
                    EditorUtility.RevealInFinder( ed.t.ExportPath );
                } else {
                    EditorUtility.RevealInFinder( Const.EXPORT_FOLDER_PATH );
                }
            }
        }
    }
#endif
}
