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
        static void UpdatePostProcessScript( MizoresPackageExporterEditor ed, MizoresPackageExporter[] targetlist ) {
            var t = targetlist[0];
            bool multiple = targetlist.Length > 1;
            editInstance = t;
            postProcessTempValues.Clear( );
            fieldInfos = null;
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
                            if ( field.FieldType.IsPrimitive ) {
                                // 基本型の場合は文字列から変換
                                try {
                                    postProcessTempValues[fieldName] = System.Convert.ChangeType( valueStr, field.FieldType );
                                } catch ( System.Exception ) {
                                    Debug.LogError( $"Can't convert value: {fieldName} ({valueStr})" );
                                    // 変換に失敗したら初期値を取得
                                    postProcessTempValues[fieldName] = field.GetValue( postProcessTemp );
                                }
                            } else if ( field.FieldType == typeof( string ) ) {
                                // string型の場合はそのまま
                                postProcessTempValues[fieldName] = valueStr;
                            } else {
                                // その他の場合はJsonから変換
                                postProcessTempValues[fieldName] = JsonUtility.FromJson( valueStr, field.FieldType );
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
                    break;
                }
                EditorGUI.BeginChangeCheck( );
                if ( field.FieldType == typeof( string ) ) {
                    value = EditorGUILayout.TextField( fieldName, ( string )value );
                } else if ( field.FieldType == typeof( bool ) ) {
                    value = EditorGUILayout.Toggle( fieldName, ( bool )value );
                } else if ( field.FieldType == typeof( int ) ) {
                    var slider = field.GetCustomAttributes( typeof( RangeAttribute ), false ).FirstOrDefault( ) as RangeAttribute;
                    if ( slider == null ) {
                        value = EditorGUILayout.IntField( fieldName, ( int )value );
                    } else {
                        value = EditorGUILayout.IntSlider( fieldName, ( int )value, ( int )slider.min, ( int )slider.max );
                    }
                } else if ( field.FieldType == typeof( float ) ) {
                    var slider = field.GetCustomAttributes( typeof( RangeAttribute ), false ).FirstOrDefault( ) as RangeAttribute;
                    if ( slider == null ) {
                        value = EditorGUILayout.FloatField( fieldName, ( float )value );
                    } else {
                        value = EditorGUILayout.Slider( fieldName, ( float )value, slider.min, slider.max );
                    }
                } else if ( field.FieldType == typeof( Vector2 ) ) {
                    value = EditorGUILayout.Vector2Field( fieldName, ( Vector2 )value );
                } else if ( field.FieldType == typeof( Vector3 ) ) {
                    value = EditorGUILayout.Vector3Field( fieldName, ( Vector3 )value );
                } else if ( field.FieldType == typeof( Vector4 ) ) {
                    value = EditorGUILayout.Vector4Field( fieldName, ( Vector4 )value );
                } else if ( field.FieldType == typeof( Color ) ) {
                    value = EditorGUILayout.ColorField( fieldName, ( Color )value );
                } else if ( field.FieldType == typeof( AnimationCurve ) ) {
                    value = EditorGUILayout.CurveField( fieldName, ( AnimationCurve )value );
                } else if ( field.FieldType == typeof( Bounds ) ) {
                    value = EditorGUILayout.BoundsField( fieldName, ( Bounds )value );
                } else if ( field.FieldType == typeof( Rect ) ) {
                    value = EditorGUILayout.RectField( fieldName, ( Rect )value );
                } else if ( field.FieldType == typeof( LayerMask ) ) {
                    value = EditorGUILayout.LayerField( fieldName, ( LayerMask )value );
                } else if ( field.FieldType == typeof( System.Enum ) ) {
                    value = EditorGUILayout.EnumPopup( fieldName, ( System.Enum )value );
                } else if ( field.FieldType == typeof( Object ) ) {
                    value = EditorGUILayout.ObjectField( fieldName, ( Object )value, typeof( Object ), true );
                } else {
                    EditorGUILayout.LabelField( fieldName, "Unsupported Type: " + field.FieldType );
                }
                if ( EditorGUI.EndChangeCheck( ) ) {
                    postProcessTempValues[fieldName] = value;
                    if ( field.FieldType.IsPrimitive ) {
                        // 基本型の場合は文字列に変換
                        t.postProcessScriptFieldValues[fieldName] = value.ToString( );
                    } else {
                        // その他の場合はJsonに変換
                        t.postProcessScriptFieldValues[fieldName] = JsonUtility.ToJson( value );
                    }
                    EditorUtility.SetDirty( t );
                }
            }
            EditorGUI.indentLevel--;
        }
    }
}
