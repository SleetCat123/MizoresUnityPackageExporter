using UnityEngine;
using System.Linq;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor {
    public static class GUI_PostProcessScript {
        static MizoresPackageExporter editInstance;
        static FieldInfo[] fieldInfos;
        static Dictionary<string, object> postProcessTempValues = new Dictionary<string, object>( );
        static List<string> errorField = new List<string>( );
        static void UpdatePostProcessScript( MizoresPackageExporterEditor ed, MizoresPackageExporter[] targetlist ) {
            var t = targetlist[0];
            bool multiple = targetlist.Length > 1;
            editInstance = t;
            postProcessTempValues.Clear( );
            fieldInfos = null;
            errorField.Clear( );
            if ( multiple ) {
                return;
            }

            if ( t.postProcessScript == null ) {
                return;
            }
            // Fieldの初期値取得用にインスタンス化しておく
            var postProcessScript = t.postProcessScript as MonoScript;
            if ( postProcessScript != null ) {
                var type = postProcessScript.GetClass( );
                if ( type != null && type.GetInterfaces( ).Contains( typeof( IExportPostProcess ) ) ) {
                    IExportPostProcess postProcessTemp = System.Activator.CreateInstance( type ) as IExportPostProcess;
                    fieldInfos = type.GetFields( );
                    foreach ( var field in fieldInfos ) {
                        string valueStr;
                        var fieldName = field.Name;
                        if ( t.postProcessScriptFieldValues.TryGetValue( fieldName, out valueStr ) ) {
                            object obj;
                            if ( ExporterUtils.FromJson( valueStr, field.FieldType, out obj ) ) {
                                postProcessTempValues[fieldName] = obj;
                            } else {
                                postProcessTempValues[fieldName] = field.GetValue( postProcessTemp );
                                errorField.Add( fieldName );
                            }
                        } else {
                            // 初期値を取得
                            postProcessTempValues[fieldName] = field.GetValue( postProcessTemp );
                        }
                    }
                } else {
                    Debug.LogError( $"Can't create instance: {t.postProcessScript.name} ({type})" );
                }
            } else {
                Debug.LogError( "Can't create instance: " + t.postProcessScript.name );
            }
        }
        static void CleanUnusedFields( MizoresPackageExporter t ) {
            var postProcessScript = t.postProcessScript as MonoScript;
            if ( postProcessScript != null ) {
                var type = postProcessScript.GetClass( );
                if ( type != null && type.GetInterfaces( ).Contains( typeof( IExportPostProcess ) ) ) {
                    var fieldInfos = type.GetFields( );
                    var fieldNames = fieldInfos.Select( v => v.Name );
                    var keys = new List<string>( t.postProcessScriptFieldValues.Keys );
                    foreach ( var key in keys ) {
                        if ( !fieldNames.Contains( key ) ) {
                            t.postProcessScriptFieldValues.Remove( key );
                            Debug.Log( $"Remove unused field: {key}" );
                        }
                    }
                }
            }
        }
        public static void Draw( MizoresPackageExporterEditor ed, MizoresPackageExporter t, MizoresPackageExporter[] targetlist ) {
            if ( !ExporterUtils.EditorPrefFoldout(
                ExporterEditorPrefs.FOLDOUT_POST_PROCESS_SCRIPT,
                ExporterTexts.FoldoutPostProcessScript
                ) ) {
                return;
            }
            bool multiple = targetlist.Length > 1;
            // PostProcessScript
            var sameValuePostProcess = targetlist.All( v => v.postProcessScript == t.postProcessScript );
            using ( new EditorGUILayout.HorizontalScope( ) ) {
                if ( !sameValuePostProcess ) {
                    ExporterUtils.DiffLabel( );
                    EditorGUI.showMixedValue = true;
                }
                EditorGUI.BeginChangeCheck( );
                var content = new GUIContent( ExporterTexts.PostProcessScript, ExporterTexts.PostProcessScriptTooltip );
                var postProcessScript = EditorGUILayout.ObjectField( ExporterTexts.PostProcessScript, ed.t.postProcessScript, typeof( MonoScript ), false ) as MonoScript;
                if ( EditorGUI.EndChangeCheck( ) ) {
                    bool error = false;
                    if ( postProcessScript == null ) {
                        error = false;
                    } else if ( postProcessScript.GetClass( ) == null ) {
                        Debug.LogError( "Script is not class: " + postProcessScript.name );
                        error = true;
                    } else if ( !postProcessScript.GetClass( ).GetInterfaces( ).Contains( typeof( IExportPostProcess ) ) ) {
                        Debug.LogError( "Script is not inherit IExportPostProcess: " + postProcessScript.name );
                        error = true;
                    } else {
                        error = false;
                    }
                    if ( !error ) {
                        foreach ( var item in targetlist ) {
                            item.postProcessScript = postProcessScript;
                            EditorUtility.SetDirty( item );
                        }
                        editInstance = null;
                    }
                }
                EditorGUI.showMixedValue = false;
            }
            // PostProcessArgs
            if ( multiple ) {
                EditorGUILayout.LabelField( ExporterTexts.PostProcessScriptFields, EditorStyles.boldLabel );
                EditorGUILayout.HelpBox( ExporterTexts.EditOnlySingle( ExporterTexts.PostProcessScriptFields ), MessageType.Info );
                return;
            }
            if ( editInstance != t ) {
                UpdatePostProcessScript( ed, targetlist );
            }
            if ( fieldInfos == null ) {
                return;
            }
            var rect = EditorGUILayout.GetControlRect( );
            EditorGUI.LabelField( rect, ExporterTexts.PostProcessScriptFields, EditorStyles.boldLabel );

            var currentEvent = Event.current;
            if ( currentEvent.type == EventType.ContextClick && rect.Contains( currentEvent.mousePosition ) ) {
                // 全てのFieldをリセット
                var menu = new GenericMenu( );
                menu.AddItem( new GUIContent( ExporterTexts.PostProcessScriptResetAllFields ), false, ( ) => {
                    t.postProcessScriptFieldValues.Clear( );
                    Debug.Log( $"Reset all fields: {t.name}" );
                    UpdatePostProcessScript( ed, targetlist );
                    EditorUtility.SetDirty( t );
                } );
                // 未使用のFieldを削除
                menu.AddItem( new GUIContent( ExporterTexts.PostProcessScriptCleanUnusedFields ), false, ( ) => {
                    CleanUnusedFields( t );
                    Debug.Log( $"Clean unused fields: {t.name}" );
                    UpdatePostProcessScript( ed, targetlist );
                    EditorUtility.SetDirty( t );
                } );
                menu.ShowAsContext( );
                currentEvent.Use( );
            }

            EditorGUI.indentLevel++;
            foreach ( var field in fieldInfos ) {
                object value;
                var fieldName = field.Name;
                if ( !postProcessTempValues.TryGetValue( fieldName, out value ) ) {
                    EditorGUILayout.HelpBox( "Can't get value: " + fieldName, MessageType.Error );
                    continue;
                }
                if ( errorField.Contains( fieldName ) ) {
                    EditorGUILayout.HelpBox( "Can't get value: " + fieldName, MessageType.Error );
                }

                EditorGUI.BeginChangeCheck( );
                Rect fieldRect;
                value = FieldEditor.Field( field, value, out fieldRect );
                if ( EditorGUI.EndChangeCheck( ) ) {
                    postProcessTempValues[fieldName] = value;
                    t.postProcessScriptFieldValues[fieldName] = ExporterUtils.ToJson( field.FieldType, value );
                    EditorUtility.SetDirty( t );
                }

                if ( currentEvent.type == EventType.ContextClick && fieldRect.Contains( currentEvent.mousePosition ) ) {
                    // リセット
                    var menu = new GenericMenu( );
                    menu.AddItem( new GUIContent( ExporterTexts.PostProcessScriptResetField( fieldName ) ), false, ( ) => {
                        t.postProcessScriptFieldValues.Remove( fieldName );
                        Debug.Log( $"Reset field: {fieldName}" );
                        UpdatePostProcessScript( ed, targetlist );
                        EditorUtility.SetDirty( t );
                    } );
                    menu.ShowAsContext( );
                    currentEvent.Use( );
                }
            }
            EditorGUI.indentLevel--;
        }
    }
}
