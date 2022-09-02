using System.Collections.Generic;
using UnityEngine;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.MizoresPackageExporterConsts;
using Const_Keys = MizoreNekoyanagi.PublishUtil.PackageExporter.MizoresPackageExporterConsts_Keys;
#if UNITY_EDITOR
using UnityEditor;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.SingleEditor
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
                EditorGUILayout.TextField( key.Replace( "%", string.Empty ) );
                EditorGUILayout.TextField( value );
                EditorGUILayout.LabelField( string.Empty, space_width );
                EditorGUILayout.LabelField( string.Empty, button_width );
            }
        }
        public static void Draw( MizoresPackageExporterEditor ed, MizoresPackageExporter t ) {
            if ( ExporterUtils.EditorPrefFoldout(
                Const.EDITOR_PREF_FOLDOUT_DYNAMICPATH_VARIABLES,
                string.Format( ExporterTexts.t_DynamicPath_Variables, t.variables.Count )
                ) ) {
                var space_width = GUILayout.Width( SPACE_WIDTH );
                var button_width = GUILayout.Width( BUTTON_WIDTH );
                GUI.enabled = false;
                DrawBuiltInVariable( Const_Keys.KEY_NAME, t.name );
                DrawBuiltInVariable( Const_Keys.KEY_VERSION, t.ExportVersion );
                DrawBuiltInVariable( Const_Keys.KEY_FORMATTED_VERSION, t.FormattedVersion );
                DrawBuiltInVariable( Const_Keys.KEY_PACKAGE_NAME, t.PackageName );
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
                    ed._variableKeyTemp = EditorGUILayout.DelayedTextField( ed._variableKeyTemp );
                    EditorGUILayout.LabelField( string.Empty );
                    string temp = ed._variableKeyTemp;
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        if ( string.IsNullOrWhiteSpace( temp ) || t.variables.ContainsKey( temp ) ) {
                            ed._variableKeyTemp = null;
                        } else {
                            // キー追加
                            t.variables.Add( temp, string.Empty );
                            ed._variableKeyTemp = null;
                            EditorUtility.SetDirty( t );
                        }
                    }
                }
            }
        }
    }
}
#endif
