using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter
{
#if UNITY_EDITOR
    public static class SingleEditor
    {
        public static void EditSingle( UnityPackageExporterEditor ed ) {
            var t = ed.t;

            using ( var s = new EditorGUI.DisabledGroupScope( true ) ) {
                EditorGUILayout.ObjectField( t, typeof( MizoresPackageExporter ), false );
            }
            UnityPackageExporterEditor.scroll = EditorGUILayout.BeginScrollView( UnityPackageExporterEditor.scroll );
            Undo.RecordObject( t, ExporterTexts.t_Undo );

            // ↓ Objects
            EditorGUILayout.LabelField( ExporterTexts.t_Objects, EditorStyles.boldLabel );
            for ( int i = 0; i < t.objects.Count; i++ ) {
                using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                    PackagePrefsElement item = t.objects[i];
                    EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 30 ) );

                    EditorGUI.BeginChangeCheck( );
                    item.Object = EditorGUILayout.ObjectField( item.Object, typeof( Object ), false );
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        EditorUtility.SetDirty( t );
                    }

                    EditorGUI.BeginChangeCheck( );
                    string path = item.Path;
                    path = EditorGUILayout.TextField( path );
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        // パスが変更されたらオブジェクトを置き換える
                        Object o = AssetDatabase.LoadAssetAtPath<Object>( path );
                        if ( o != null ) {
                            item.Object = o;
                        }
                        EditorUtility.SetDirty( t );
                    }

                    if ( GUILayout.Button( "-", GUILayout.Width( 15 ) ) ) {
                        t.objects.RemoveAt( i );
                        i--;
                        EditorUtility.SetDirty( t );
                    }
                }
            }
            if ( GUILayout.Button( "+", GUILayout.Width( 60 ) ) ) {
                t.objects.Add( new PackagePrefsElement( ) );
                EditorUtility.SetDirty( t );
            }
            // ↑ Objects

            // ↓ Dynamic Path
            EditorGUILayout.Separator( );
            EditorGUILayout.LabelField( ExporterTexts.t_DynamicPath, EditorStyles.boldLabel );
            for ( int i = 0; i < t.dynamicpath.Count; i++ ) {
                using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                    EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 30 ) );

                    EditorGUI.BeginChangeCheck( );
                    t.dynamicpath[i] = EditorGUILayout.TextField( t.dynamicpath[i] );
                    t.dynamicpath[i] = ed.BrowseButtons( t.dynamicpath[i] );
                    EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 30 ) );
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        EditorUtility.SetDirty( t );
                    }

                    if ( GUILayout.Button( "-", GUILayout.Width( 15 ) ) ) {
                        t.dynamicpath.RemoveAt( i );
                        i--;
                        EditorUtility.SetDirty( t );
                    }
                }
                using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                    // プレビュー
                    string previewpath = t.ConvertDynamicPath( t.dynamicpath[i] );
                    EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 30 ) );
                    EditorGUILayout.LabelField( new GUIContent( previewpath, previewpath ) );
                }
            }
            if ( GUILayout.Button( "+", GUILayout.Width( 60 ) ) ) {
                t.dynamicpath.Add( string.Empty );
                EditorUtility.SetDirty( t );
            }
            // ↑ Dynamic Path

            // ↓ Dynamic Path Variables
            EditorGUILayout.Separator( );
            EditorGUILayout.LabelField( ExporterTexts.t_DynamicPath_Variables, EditorStyles.boldLabel );
            GUI.enabled = false;
            using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 30 ) );
                EditorGUILayout.TextField( "%name%" );
                EditorGUILayout.TextField( t.ConvertDynamicPath( "%name%" ) );
            }
            using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 30 ) );
                EditorGUILayout.TextField( "%version%" );
                EditorGUILayout.TextField( t.ConvertDynamicPath( "%version%" ) );
            }
            GUI.enabled = true;
            List<string> keys = new List<string>( t.variables.Keys );
            foreach ( var key in keys ) {
                using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                    EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 30 ) );

                    EditorGUI.BeginChangeCheck( );
                    t.variables[key] = EditorGUILayout.TextField( t.variables[key] );
                    EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 30 ) );
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        EditorUtility.SetDirty( t );
                    }

                    if ( GUILayout.Button( "-", GUILayout.Width( 15 ) ) ) {
                        t.variables.Remove( key );
                        EditorUtility.SetDirty( t );
                    }
                }
            }
            using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {

            }
            // ↑ Dynamic Path Variables

            // ↓ Version File
            EditorGUILayout.Separator( );
            EditorGUILayout.LabelField( ExporterTexts.t_VersionFile, EditorStyles.boldLabel );
            using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                EditorGUI.BeginChangeCheck( );
                t.versionFile.Object = EditorGUILayout.ObjectField( t.versionFile.Object, typeof( Object ), false );
                if ( EditorGUI.EndChangeCheck( ) ) {
                    EditorUtility.SetDirty( t );
                }
                EditorGUI.BeginChangeCheck( );
                string path = t.versionFile.Path;
                path = EditorGUILayout.TextField( path );
                if ( EditorGUI.EndChangeCheck( ) ) {
                    // パスが変更されたらオブジェクトを置き換える
                    Object o = AssetDatabase.LoadAssetAtPath<Object>( path );
                    if ( o != null ) {
                        t.versionFile.Object = o;
                    }
                    EditorUtility.SetDirty( t );
                }
            }
            // ↑ Version File

            // Export Button
            EditorGUILayout.EndScrollView( );
            EditorGUILayout.Separator( );
            if ( GUILayout.Button( ExporterTexts.t_Button_ExportPackage ) ) {
                UnityPackageExporterEditor.HelpBoxText = string.Empty;
                t.Export( );
            }

            // Open Button
            using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                EditorGUILayout.LabelField( new GUIContent( t.ExportPath, t.ExportPath ) );
                if ( GUILayout.Button( ExporterTexts.TEXT_BUTTON_OPEN, GUILayout.Width( 60 ) ) ) {
                    if ( File.Exists( t.ExportPath ) ) {
                        EditorUtility.RevealInFinder( t.ExportPath );
                    } else {
                        EditorUtility.RevealInFinder( MizoresPackageExporter.EXPORT_FOLDER_PATH );
                    }
                }
            }
        }
    }
#endif
}
