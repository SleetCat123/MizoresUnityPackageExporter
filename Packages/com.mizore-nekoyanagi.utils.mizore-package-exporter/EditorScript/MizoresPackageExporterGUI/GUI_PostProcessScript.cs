#if UNITY_EDITOR
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

        static Dictionary<string, System.Type> scriptTypeTable;
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
                // 選択していないものはFullNameで表示
                if ( i == index ) {
                    popupValues[i] = new GUIContent( fullName.Split( '.' ).Last( ), fullName );
                } else {
                    popupValues[i] = new GUIContent( fullName, fullName );
                }
            }
            var content = new GUIContent( ExporterTexts.PostProcessScript, popupValues[index].tooltip );
            index = EditorGUILayout.Popup( content, index, popupValues );
            if ( index == 0 ) {
                return string.Empty;
            } else {
                // tooltipのFullNameを返す
                return popupValues[index].tooltip;
            }
        }
        static void UpdateScriptNameList( ) {
            // IExportPostProcessを実装したクラスを取得
            scriptTypeTable = new Dictionary<string, System.Type>( );
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies( );
            foreach ( var assembly in assemblies ) {
                var types = assembly.GetTypes( );
                foreach ( var type in types ) {
                    if ( type.GetInterfaces( ).Contains( typeof( IExportPostProcess ) ) ) {
                        scriptTypeTable[type.FullName] = type;
                    }
                }
            }
            // 0は空・1は現在のスクリプト名用に開けておく
            var count = scriptTypeTable.Count + 2;
            popupValues = new GUIContent[count];
            popupValues[0] = new GUIContent( "None" );
            popupValues[1] = new GUIContent( string.Empty );
            var keys = scriptTypeTable.Keys.ToList( );
            for ( int i = 0; i < keys.Count; i++ ) {
                var key = keys[i];
                // tooltipはFullName
                popupValues[i + 2] = new GUIContent( key, key );
            }
        }
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

            if ( string.IsNullOrEmpty( t.postProcessScriptTypeName ) ) {
                return;
            }
            System.Type postProcessScriptType;
            scriptTypeTable.TryGetValue( t.postProcessScriptTypeName, out postProcessScriptType );
            // Fieldの初期値取得用にインスタンス化しておく
            if ( postProcessScriptType != null ) {
                IExportPostProcess postProcessTemp = System.Activator.CreateInstance( postProcessScriptType ) as IExportPostProcess;
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
            System.Type postProcessScriptType;
            scriptTypeTable.TryGetValue( t.postProcessScriptTypeName, out postProcessScriptType );
            if ( postProcessScriptType != null ) {
                var fieldInfos = postProcessScriptType.GetFields( );
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
        public static void Draw( MizoresPackageExporterEditor ed, MizoresPackageExporter t, MizoresPackageExporter[] targetlist ) {
            if ( !ExporterUtils.EditorPrefFoldout(
                ExporterEditorPrefs.FOLDOUT_POST_PROCESS_SCRIPT,
                ExporterTexts.FoldoutPostProcessScript
                ) ) {
                return;
            }
            if ( scriptTypeTable == null ) {
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
#endif