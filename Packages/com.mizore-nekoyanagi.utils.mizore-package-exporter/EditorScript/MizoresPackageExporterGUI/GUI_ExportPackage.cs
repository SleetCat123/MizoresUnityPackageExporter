using UnityEngine;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterConsts;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor {
    public static class GUI_ExportPackage {
        public static void Draw( MizoresPackageExporterEditor ed, MizoresPackageExporter[] targetlist ) {
            var t = targetlist[0];
            bool multiple = targetlist.Length > 1;
            EditorGUILayout.LabelField( ExporterTexts.LabelExportPackage, EditorStyles.boldLabel );
            // Check Button
            //if ( GUILayout.Button( ExporterTexts.t_ButtonCheck ) ) {
            //    ed.logs.Clear( );
            //    foreach ( var item in targetlist ) {
            //        item.AllFileExists( ed.logs );
            //    }
            //}

            string[][] fileList = new string[targetlist.Length][];
            bool any = false;
            for ( int i = 0; i < targetlist.Length; i++ ) {
                var files = targetlist[i].GetAllExportFileName( );
                fileList[i] = files;
                any |= files.Length != 0;
            }

            // List Button
            using ( new EditorGUI.DisabledGroupScope( !any ) ) {
                if ( GUILayout.Button( ExporterTexts.ButtonExportPackages, GUILayout.Height( 50 ) ) ) {
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
                            EditorGUI.indentLevel++;
                            EditorGUILayout.LabelField( j.ToString( ), GUILayout.Width( 30 ) );
                            EditorGUI.indentLevel--;
                        }
                        var path = files[j];
                        EditorGUILayout.LabelField( new GUIContent( path, path ) );
                    }
                }
            }
            if ( !any ) {
                EditorGUILayout.HelpBox( ExporterTexts.ExportListEmpty, MessageType.Error );
            }
            if ( GUILayout.Button( ExporterTexts.ButtonOpen, GUILayout.Width( 60 ) ) ) {
                if ( File.Exists( ed.t.GetExportPath( ) ) ) {
                    EditorUtility.RevealInFinder( ed.t.GetExportPath( ) );
                } else {
                    if ( !Directory.Exists( Const.EXPORT_FOLDER_PATH ) ) {
                        Directory.CreateDirectory( Const.EXPORT_FOLDER_PATH );
                    }
                    EditorUtility.RevealInFinder( Const.EXPORT_FOLDER_PATH );
                }
            }
        }
    }
}
