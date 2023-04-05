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
            bool multiple = targetlist.Length > 1;
            EditorGUILayout.LabelField( ExporterTexts.t_LabelExportPackage, EditorStyles.boldLabel );
            // Check Button
            if ( GUILayout.Button( ExporterTexts.t_ButtonCheck ) ) {
                ed.logs.Clear( );
                foreach ( var item in targetlist ) {
                    item.AllFileExists( ed.logs );
                }
            }

            string[][] fileList = new string[targetlist.Length][];
            bool any = false;
            for ( int i = 0; i < targetlist.Length; i++ ) {
                var files = targetlist[i].GetAllExportFileName( );
                fileList[i] = files;
                any |= files.Length != 0;
            }

            // List Button
            using ( new EditorGUI.DisabledGroupScope( !any ) ) {
                if ( GUILayout.Button( ExporterTexts.t_ButtonExportPackages ) ) {
                    FileList.FileListWindow.Show( ed.logs, targetlist.ToArray( ) );
                }
            }

            // 出力先一覧
            for ( int i = 0; i < targetlist.Length; i++ ) {
                var obj = targetlist[i];
                var files = fileList[i];
                if ( multiple ) {
                    EditorGUI.BeginDisabledGroup( true );
                    EditorGUILayout.ObjectField( obj, typeof( MizoresPackageExporter ), false );
                    EditorGUI.EndDisabledGroup( );
                }
                for ( int j = 0; j < files.Length; j++ ) {
                    using ( new EditorGUILayout.HorizontalScope( ) ) {
                        if ( multiple ) {
                            ExporterUtils.Indent( 1 );
                            EditorGUILayout.LabelField( j.ToString( ), GUILayout.Width( 30 ) );
                        }
                        var path = files[j];
                        EditorGUILayout.LabelField( new GUIContent( path, path ) );
                    }
                }
            }
            if ( !any ) {
                EditorGUILayout.HelpBox( "Export List is empty.", MessageType.Error );
            }
            if ( GUILayout.Button( ExporterTexts.t_ButtonOpen, GUILayout.Width( 60 ) ) ) {
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
