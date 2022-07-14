using System.Collections.Generic;
using UnityEngine;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.MizoresPackageExporterConsts;
#if UNITY_EDITOR
using UnityEditor;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.SingleEditor
{
    public static class SingleGUI_DynamicPathVariables
    {
        public static void Draw( MizoresPackageExporter t ) {
            if ( ExporterUtils.EditorPrefFoldout( Const.EDITOR_PREF_FOLDOUT_DYNAMICPATH_VARIABLES, ExporterTexts.t_DynamicPath_Variables ) ) {
                var space_width = GUILayout.Width( 30 );
                var button_width = GUILayout.Width( 15 );
                GUI.enabled = false;
                using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                    EditorGUILayout.LabelField( string.Empty, space_width );
                    EditorGUILayout.TextField( "name" );
                    EditorGUILayout.TextField( t.name );
                    EditorGUILayout.LabelField( string.Empty, space_width );
                    EditorGUILayout.LabelField( string.Empty, button_width );
                }
                using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                    EditorGUILayout.LabelField( string.Empty, space_width );
                    EditorGUILayout.TextField( "version" );
                    EditorGUILayout.TextField( t.ExportVersion );
                    EditorGUILayout.LabelField( string.Empty, space_width );
                    EditorGUILayout.LabelField( string.Empty, button_width );
                }
                using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                    EditorGUILayout.LabelField( string.Empty, space_width );
                    EditorGUILayout.TextField( "versionprefix" );
                    EditorGUILayout.TextField( t.versionPrefix );
                    EditorGUILayout.LabelField( string.Empty, space_width );
                    EditorGUILayout.LabelField( string.Empty, button_width );
                }
                GUI.enabled = true;
                List<string> keys = new List<string>( t.variables.Keys );
                for ( int i = 0; i < keys.Count; i++ ) {
                    string key = keys[i];
                    using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                        EditorGUILayout.LabelField( string.Empty, space_width );

                        // キー名変更
                        EditorGUI.BeginChangeCheck( );
                        string temp_key = EditorGUILayout.DelayedTextField( key );
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            if ( string.IsNullOrWhiteSpace( temp_key ) || t.variables.ContainsKey( temp_key ) ) {

                            } else {
                                t.variables.Add( temp_key, t.variables[key] );
                                t.variables.Remove( key );
                                key = temp_key;
                                EditorUtility.SetDirty( t );
                            }
                        }

                        // 値変更
                        EditorGUI.BeginChangeCheck( );
                        t.variables[key] = EditorGUILayout.TextField( t.variables[key] );
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            EditorUtility.SetDirty( t );
                        }

                        // ボタン
                        EditorGUILayout.LabelField( string.Empty, space_width );
                        if ( GUILayout.Button( "-", button_width ) ) {
                            t.variables.Remove( key );
                            EditorUtility.SetDirty( t );
                        }
                    }
                }
                using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                    // 新規キー追加
                    EditorGUILayout.LabelField( string.Empty, space_width );
                    EditorGUI.BeginChangeCheck( );
                    UnityPackageExporterEditor.variable_key_temp = EditorGUILayout.DelayedTextField( UnityPackageExporterEditor.variable_key_temp );
                    EditorGUILayout.LabelField( string.Empty );
                    string temp = UnityPackageExporterEditor.variable_key_temp;
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        if ( string.IsNullOrWhiteSpace( temp ) || t.variables.ContainsKey( temp ) ) {
                            UnityPackageExporterEditor.variable_key_temp = null;
                        } else {
                            // キー追加
                            t.variables.Add( temp, string.Empty );
                            UnityPackageExporterEditor.variable_key_temp = null;
                            EditorUtility.SetDirty( t );
                        }
                    }
                }
            }
        }
    }
#endif
}
