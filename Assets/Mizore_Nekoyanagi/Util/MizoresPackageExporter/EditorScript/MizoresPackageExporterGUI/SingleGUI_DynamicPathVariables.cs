using System.Collections.Generic;
using UnityEngine;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterConsts_Editor;
using Const_Keys = MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterConsts_Keys;
#if UNITY_EDITOR
using UnityEditor;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor
{
    public static class SingleGUI_DynamicPathVariables
    {
        const int SPACE_WIDTH = 30;
        const int BUTTON_WIDTH = 15;
        static void DrawBuiltInVariable( string key, string value ) {
            using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                var space_width = GUILayout.Width( SPACE_WIDTH );
                var button_width = GUILayout.Width( BUTTON_WIDTH );
                EditorGUILayout.LabelField( string.Empty, space_width );
                // key = key.Replace( "%", string.Empty );
                EditorGUILayout.TextField( key );
                EditorGUILayout.TextField( value );
                EditorGUILayout.LabelField( string.Empty, space_width );
                EditorGUILayout.LabelField( string.Empty, button_width );
            }
        }
        static string KeyTextField( string key ) {
            if ( key.Length != 0 ) {
                key = "%" + key + "%";
            }
            key = EditorGUILayout.DelayedTextField( key );
            key = key.Replace( "%", string.Empty );
            return key;
        }
        public static void Draw( MizoresPackageExporterEditor ed, MizoresPackageExporter t ) {
            if ( ExporterUtils.EditorPrefFoldout(
                Const.EDITOR_PREF_FOLDOUT_DYNAMICPATH_VARIABLES,
                string.Format( ExporterTexts.t_DynamicPathVariables, t.variables.Count )
                ) ) {
                var space_width = GUILayout.Width( SPACE_WIDTH );
                var button_width = GUILayout.Width( BUTTON_WIDTH );
                GUI.enabled = false;
                DrawBuiltInVariable( Const_Keys.KEY_NAME, t.name );
                DrawBuiltInVariable( Const_Keys.KEY_VERSION, t.ExportVersion );
                DrawBuiltInVariable( Const_Keys.KEY_FORMATTED_VERSION, t.FormattedVersion );
                DrawBuiltInVariable( Const_Keys.KEY_PACKAGE_NAME, t.PackageName );
                DrawBuiltInVariable( Const_Keys.KEY_SAMPLE_DATE, MizoresPackageExporter.ReplaceDate( Const_Keys.KEY_SAMPLE_DATE ) );
                GUI.enabled = true;
                List<string> keys = new List<string>( t.variables.Keys );
                for ( int i = 0; i < keys.Count; i++ ) {
                    string key = keys[i];
                    using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                        EditorGUILayout.LabelField( string.Empty, space_width );

                        // キー名変更
                        EditorGUI.BeginChangeCheck( );
                        string temp_key = KeyTextField( key );
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
                    ed._variableKeyTemp = KeyTextField( ed._variableKeyTemp );
                    EditorGUILayout.LabelField( string.Empty );
                    string temp = ed._variableKeyTemp;
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        if ( string.IsNullOrWhiteSpace( temp ) || t.variables.ContainsKey( temp ) ) {
                            ed._variableKeyTemp = string.Empty;
                        } else {
                            // キー追加
                            t.variables.Add( temp, string.Empty );
                            ed._variableKeyTemp = string.Empty;
                            EditorUtility.SetDirty( t );
                        }
                    }
                }
            }
        }
    }
}
#endif
