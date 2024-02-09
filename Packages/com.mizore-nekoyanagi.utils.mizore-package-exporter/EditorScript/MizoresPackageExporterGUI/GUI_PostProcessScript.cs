#if UNITY_EDITOR
using UnityEngine;
using System.Linq;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor {
    public static class GUI_PostProcessScript {
        static MizoresPackageExporter editInstance;
        static string typeName;
        static FieldInfo[] fieldInfos;
        static Dictionary<string, object> postProcessTempValues = new Dictionary<string, object>( );
        static List<string> errorField = new List<string>( );

        class ScriptTypeData {
            public System.Type type;
            public string name;
            public string scriptPath;
        }
        static Dictionary<string, ScriptTypeData> scriptDataTable;
        static GUIContent[] popupValues;
        static string ScriptTypePopup( string value ) {
            popupValues[1] = new GUIContent( string.Empty );
            int index = 0;
            if ( !string.IsNullOrEmpty( value ) ) {
                index = System.Array.IndexOf( popupValues.Select( v => v.tooltip ).ToArray( ), value );
                if ( index == -1 ) {
                    popupValues[1] = new GUIContent( ExporterTexts.PostProcessScriptPopupNotFound( value ), value );
                    index = 1;
                } else {
                    popupValues[1] = new GUIContent( );
                }
            }
            for ( int i = 2; i < popupValues.Length; i++ ) {
                var fullName = popupValues[i].tooltip;
                if ( i == index ) {
                    // 選択しているものはNameで表示
                    popupValues[i] = new GUIContent( scriptDataTable[fullName].name, fullName );
                } else {
                    // 選択していないものはFullNameで表示
                    popupValues[i] = new GUIContent( fullName, fullName );
                }
            }
            using ( new EditorGUILayout.HorizontalScope( ) ) {
                var content = new GUIContent( ExporterTexts.PostProcessScript, popupValues[index].tooltip );
                index = EditorGUILayout.Popup( content, index, popupValues );

                using ( new EditorGUI.DisabledScope( true ) ) {
                    MonoScript scriptObj = null;
                    if ( scriptDataTable.TryGetValue( popupValues[index].tooltip, out var scriptData ) ) {
                        scriptObj = AssetDatabase.LoadAssetAtPath<MonoScript>( scriptData.scriptPath );
                    }
                    EditorGUILayout.ObjectField( scriptObj, typeof( MonoScript ), false, GUILayout.Width( 60 ) );
                }
            }

            if ( index == 0 ) {
                return string.Empty;
            } else {
                // tooltipのFullNameを返す
                return popupValues[index].tooltip;
            }
        }
        static void UpdateScriptNameList( ) {
            // IExportPostProcessを実装したcsファイルを探す
            var scriptType = typeof( IExportPostProcess );
            scriptDataTable = new Dictionary<string, ScriptTypeData>( );
            var scripts = AssetDatabase.FindAssets( "t:Script" ).Select( v => AssetDatabase.LoadAssetAtPath<MonoScript>(  AssetDatabase.GUIDToAssetPath( v )));
            foreach ( var script in scripts ) {
                if ( script == null ) {
                    continue;
                }
                // IExportPostProcessを実装しているか
                var type = script.GetClass( );
                if ( type == null || !type.GetInterfaces( ).Contains( typeof( IExportPostProcess ) ) ) {
                    continue;
                }
                scriptDataTable[type.FullName] = new ScriptTypeData {
                    type = type,
                    name = type.Name,
                    scriptPath = AssetDatabase.GetAssetPath( script )
                };
            }
            // 0は空・1は現在のスクリプト名用に開けておく
            var count = scriptDataTable.Count + 2;
            popupValues = new GUIContent[count];
            popupValues[0] = new GUIContent( "None" );
            popupValues[1] = new GUIContent( string.Empty );
            var keys = scriptDataTable.Keys.ToList( );
            for ( int i = 0; i < keys.Count; i++ ) {
                var key = keys[i];
                // tooltipはFullName
                popupValues[i + 2] = new GUIContent( key, key );
            }
        }
        static void Clear( ) {
            editInstance = null;
            typeName = null;
            fieldInfos = null;
            postProcessTempValues.Clear( );
            errorField.Clear( );
        }
        static void UpdatePostProcessScript( MizoresPackageExporterEditor ed, MizoresPackageExporter[] targetlist ) {
            var t = targetlist[0];
            bool multiple = targetlist.Length > 1;
            editInstance = t;
            postProcessTempValues.Clear( );
            typeName = null;
            fieldInfos = null;
            errorField.Clear( );
            if ( multiple ) {
                return;
            }

            if ( string.IsNullOrEmpty( t.postProcessScriptTypeName ) ) {
                return;
            }
            if ( scriptDataTable.TryGetValue( t.postProcessScriptTypeName, out var scriptData ) ) {
                // Fieldの初期値取得用にインスタンス化しておく
                var postProcessScriptType = scriptData.type;
                IExportPostProcess postProcessTemp = System.Activator.CreateInstance( postProcessScriptType ) as IExportPostProcess;
                typeName = postProcessScriptType.FullName;
                fieldInfos = postProcessScriptType.GetFields( );
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
                Debug.LogError( $"Can't create instance: {t.postProcessScriptTypeName}" );
            }
        }
        static void CleanUnusedFields( MizoresPackageExporter t ) {
            if ( scriptDataTable.TryGetValue( t.postProcessScriptTypeName, out var scriptData ) ) {
                var postProcessScriptType = scriptData.type;
                var fieldInfos = postProcessScriptType.GetFields( );
                var fieldNames = fieldInfos.Select( v => v.Name );
                var keys = new List<string>( t.postProcessScriptFieldValues.Keys );
                foreach ( var key in keys ) {
                    if ( !fieldNames.Contains( key ) ) {
                        t.postProcessScriptFieldValues.Remove( key );
                        Debug.Log( $"Remove unused field: {key}" );
                    }
                }
            } else {
                var keys = new List<string>( t.postProcessScriptFieldValues.Keys );
                foreach ( var key in keys ) {
                    Debug.Log( $"Remove unused field: {key}" );
                }
                t.postProcessScriptFieldValues.Clear( );
            }
        }
        public static void Draw( MizoresPackageExporterEditor ed, MizoresPackageExporter t, MizoresPackageExporter[] targetlist ) {
            if ( !CustomFoldout.EditorPrefFoldout(
                ExporterEditorPrefs.FOLDOUT_POST_PROCESS_SCRIPT,
                ExporterTexts.FoldoutPostProcessScript
                ) ) {
                return;
            }
            VerticalBoxScope.BeginVerticalBox( );
            if ( scriptDataTable == null ) {
                UpdateScriptNameList( );
            }
            bool multiple = targetlist.Length > 1;
            // PostProcessScript
            var sameValuePostProcess = targetlist.All( v => v.postProcessScriptTypeName == t.postProcessScriptTypeName );
            using ( new EditorGUILayout.HorizontalScope( ) ) {
                if ( !sameValuePostProcess ) {
                    ExporterUtils.DiffLabel( );
                    EditorGUI.showMixedValue = true;
                }
                EditorGUI.BeginChangeCheck( );
                var postProcessScriptTypeName = ScriptTypePopup( t.postProcessScriptTypeName );
                if ( EditorGUI.EndChangeCheck( ) ) {
                    foreach ( var item in targetlist ) {
                        item.postProcessScriptTypeName = postProcessScriptTypeName;
                        EditorUtility.SetDirty( item );
                    }
                    editInstance = null;
                }
                EditorGUI.showMixedValue = false;
                if ( GUILayout.Button( ExporterTexts.PostProcessScriptUpdate, GUILayout.Width( 60 ) ) ) {
                    UpdateScriptNameList( );
                }
            }
            // PostProcessArgs
            if ( multiple ) {
                EditorGUILayout.LabelField( ExporterTexts.PostProcessScriptFields, EditorStyles.boldLabel );
                EditorGUILayout.HelpBox( ExporterTexts.EditOnlySingle( ExporterTexts.PostProcessScriptFields ), MessageType.Info );
                return;
            }
            if ( !string.IsNullOrEmpty( t.postProcessScriptTypeName ) && !scriptDataTable.ContainsKey( t.postProcessScriptTypeName ) ) {
                EditorGUILayout.HelpBox( ExporterTexts.PostProcessScriptNotFound( t.postProcessScriptTypeName ), MessageType.Error );
                Clear( );
            } else if ( editInstance != t ) {
                UpdatePostProcessScript( ed, targetlist );
            }
            List<string> fieldNames = new List<string>( );
            if ( fieldInfos != null ) {
                EditorGUILayout.LabelField( typeName );
                EditorGUILayout.Separator( );
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
                    fieldNames.Add( fieldName );
                    if ( !postProcessTempValues.TryGetValue( fieldName, out value ) ) {
                        EditorGUILayout.HelpBox( "Can't get value: " + fieldName, MessageType.Error );
                        continue;
                    }
                    if ( errorField.Contains( fieldName ) ) {
                        EditorGUILayout.HelpBox( "Can't get value: " + fieldName, MessageType.Error );
                    }

                    bool valueOverrided = t.postProcessScriptFieldValues.ContainsKey( fieldName );

                    EditorGUI.BeginChangeCheck( );
                    Rect fieldRect;
                    value = FieldEditor.Field( field, value, valueOverrided, out fieldRect );
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
            // 未使用のField
            bool first = true;
            var temp_contentColor = GUI.contentColor;
            var temp_indentLevel = EditorGUI.indentLevel;
            bool clean = false;
            foreach ( var kvp in t.postProcessScriptFieldValues ) {
                if ( !fieldNames.Contains( kvp.Key ) ) {
                    if ( first ) {
                        EditorGUILayout.Separator( );
                        using ( new EditorGUILayout.HorizontalScope( ) ) {
                            var c = new Color( 0.7f, 0.7f, 0.7f );
                            GUI.contentColor = c;
                            EditorGUILayout.LabelField( ExporterTexts.PostProcessScriptUnusedFields, EditorStyles.boldLabel );
                            GUI.contentColor = temp_contentColor;
                            if ( GUILayout.Button( ExporterTexts.PostProcessScriptCleanUnusedFields, GUILayout.Width( 160 ) ) ) {
                                clean = true;
                            }
                            GUI.contentColor = c;
                        }
                        EditorGUI.indentLevel++;
                        first = false;
                    }
                    EditorGUILayout.LabelField( kvp.Key, kvp.Value );
                }
            }
            if ( clean ) {
                CleanUnusedFields( t );
                UpdatePostProcessScript( ed, targetlist );
                EditorUtility.SetDirty( t );
            }
            GUI.contentColor = temp_contentColor;
            EditorGUI.indentLevel = temp_indentLevel;
            VerticalBoxScope.EndVerticalBox( );
        }
    }
}
#endif