#if UNITY_EDITOR
using UnityEngine;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterConsts;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            string formatError = null;
            bool any = false;
            // 同一 Exporter 内のバッチキー間重複を収集する
            var intraDuplicates = new List<string>( );
            var hasIntraDuplicate = new bool[targetlist.Length];
            for ( int i = 0; i < targetlist.Length; i++ ) {
                var files = targetlist[i].GetAllExportFileName( string.Empty, out var err, out var dups );
                if ( err != null ) formatError = err;
                fileList[i] = files;
                any |= files.Length != 0;
                intraDuplicates.AddRange( dups );
                hasIntraDuplicate[i] = dups.Length > 0;
            }

            // 出力先パスの重複チェック（Exporter 間重複）
            var allFiles = new List<string>( );
            for ( int i = 0; i < targetlist.Length; i++ ) {
                allFiles.AddRange( fileList[i] );
            }
            // Exporter 内バッチ重複 + Exporter 間重複を合算して一覧化
            var duplicates = intraDuplicates.Concat( ExporterUtils.FindDuplicates( allFiles ) ).Distinct( System.StringComparer.OrdinalIgnoreCase ).ToList( );
            bool hasDuplicate = duplicates.Count > 0;

            // List Button
            using ( new EditorGUI.DisabledGroupScope( !any || hasDuplicate ) ) {
                if ( GUILayout.Button( ExporterTexts.ButtonExportPackages, GUILayout.Height( 50 ) ) ) {
                    var task = FileList.FileListWindow.Show( ed.logs, targetlist.ToArray( ) );
                }
            }

            // 出力先一覧
            for ( int i = 0; i < targetlist.Length; i++ ) {
                VerticalBoxScope.BeginVerticalBox( );
                var obj = targetlist[i];
                var files = fileList[i];
                if ( multiple ) {
                    using ( new EditorGUILayout.HorizontalScope( ) ) {
                        EditorGUI.BeginDisabledGroup( true );
                        EditorGUILayout.ObjectField( obj, typeof( MizoresPackageExporter ), false );
                        EditorGUI.EndDisabledGroup( );
                        EditorGUI.BeginDisabledGroup( hasIntraDuplicate[i] );
                        if ( GUILayout.Button( ExporterTexts.ButtonExportSinglePackage, GUILayout.Width( 60 ) ) ) {
                            var task = FileList.FileListWindow.Show( ed.logs, new MizoresPackageExporter[] { obj } );
                        }
                        EditorGUI.EndDisabledGroup( );
                    }
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
                        if ( GUILayout.Button( ExporterTexts.ButtonExportSinglePackage, GUILayout.Width( 60 ) ) ) {
                            Debug.Log( "Export: " + path );
                            var task = FileList.FileListWindow.Show( 
                                ed.logs,
                                new MizoresPackageExporter[] { obj }, 
                                new List<string> { Const.EXPORT_FOLDER_PATH + path } 
                                );
                        }
                    }
                }
                VerticalBoxScope.EndVerticalBox( );
            }
            if ( formatError != null ) {
                ExporterUtils.FormatErrorHelpBox( ExporterTexts.DateFormatError( formatError ) );
            }
            if ( hasDuplicate ) {
                EditorGUILayout.HelpBox( ExporterTexts.ExportDuplicatePathError( string.Join( "\n", duplicates ) ), MessageType.Error );
            }
            if ( !any ) {
                EditorGUILayout.HelpBox( ExporterTexts.ExportListEmpty, MessageType.Error );
            }
            if ( GUILayout.Button( ExporterTexts.ButtonOpen, GUILayout.Width( 60 ) ) ) {
                if ( File.Exists( ed.t.GetExportPath( string.Empty ) ) ) {
                    EditorUtility.RevealInFinder( ed.t.GetExportPath( string.Empty ) );
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
#endif