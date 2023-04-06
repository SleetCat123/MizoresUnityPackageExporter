using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterConsts_Editor;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor
{
#if UNITY_EDITOR
    public static class GUI_BatchExporter
    {
        public static void Draw( MizoresPackageExporterEditor ed, MizoresPackageExporter t, MizoresPackageExporter[] targetlist ) {
            string foldoutLabel;
            if ( t.batchExportMode == BatchExportMode.None ) {
                foldoutLabel = ExporterTexts.t_FoldoutBatchExportDisabled;
            } else {
                foldoutLabel = ExporterTexts.t_FoldoutBatchExportEnabled;
            }
            if ( !ExporterUtils.EditorPrefFoldout(
    Const.EDITOR_PREF_FOLDOUT_BATCHEXPORT, foldoutLabel ) ) {
                return;
            }
            // var samevalue_in_all_mode = targetlist.All( v => t.batchExportMode == v.batchExportMode );

            EditorGUI.BeginChangeCheck( );
            using ( new EditorGUILayout.HorizontalScope( ) ) {
                ExporterUtils.Indent( 1 );
                t.batchExportMode = (BatchExportMode)EditorGUILayout.EnumPopup( ExporterTexts.t_BatchExportMode, t.batchExportMode );
            }
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
                        ExporterUtils.Indent( 1 );
                        EditorGUILayout.LabelField( "Folder", GUILayout.Width( 60 ) );
                        PackagePrefsElementInspector.Draw<DefaultAsset>( t.batchExportFolderRoot );
                    }
                    using ( new EditorGUILayout.HorizontalScope( ) ) {
                        ExporterUtils.Indent( 1 );
                        t.batchExportFolderRegex = EditorGUILayout.TextField( ExporterTexts.t_BatchExportRegex, t.batchExportFolderRegex );
                    }
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        t.UpdateBatchExportKeys( );
                        EditorUtility.SetDirty( t );
                    }
                    try {
                        Regex.Match( string.Empty, t.batchExportFolderRegex );
                    } catch ( System.ArgumentException e ) {
                        using ( new EditorGUILayout.HorizontalScope( ) ) {
                            ExporterUtils.Indent( 1 );
                            EditorGUILayout.HelpBox( "Regex Error:\n" + e.Message, MessageType.Error );
                        }
                    }

                    var list = t.BatchExportKeys;
                    for ( int i = 0; i < list.Length; i++ ) {
                        using ( new EditorGUILayout.HorizontalScope( ) ) {
                            ExporterUtils.Indent( 2 );
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