#if UNITY_EDITOR
using UnityEngine;
using System.Linq;
using UnityEditor;
using System.Reflection;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor {
    public static class FieldEditor {
        public static object Field( FieldInfo field, object value, bool edited, out Rect fieldRect ) {
            var space = field. GetCustomAttributes( typeof( SpaceAttribute ), true ).FirstOrDefault( ) as SpaceAttribute;
            if ( space != null ) {
                EditorGUILayout.Space( );
            }

            GUIContent content;
            string label;
            if ( edited ) {
                label = $"* {field.Name}";
            } else {
                label = field.Name;
            }
            var tooltip = field.GetCustomAttributes( typeof( TooltipAttribute ), false ).FirstOrDefault( ) as TooltipAttribute;
            if ( tooltip == null ) {
                content = new GUIContent( label );
            } else {
                content = new GUIContent( label, tooltip.tooltip );
            }

            var type = field.FieldType;

            RangeAttribute rangeAttribute = null;
            if ( type == typeof( int ) || type == typeof( float ) ) {
                rangeAttribute = field.GetCustomAttributes( typeof( RangeAttribute ), false ).FirstOrDefault( ) as RangeAttribute;
            }

            return Field( content, value, type, rangeAttribute, out fieldRect );
        }
        public static object Field( GUIContent content, object value, System.Type type, RangeAttribute rangeAttribute, out Rect fieldRect ) {
            EditorGUI.BeginChangeCheck( );
            if ( type == typeof( string ) ) {
                value = EditorGUILayout.TextField( content, ( string )value );
                fieldRect = GUILayoutUtility.GetLastRect( );
            } else if ( type == typeof( bool ) ) {
                value = EditorGUILayout.Toggle( content, ( bool )value );
                fieldRect = GUILayoutUtility.GetLastRect( );
            } else if ( type == typeof( int ) ) {
                if ( rangeAttribute == null ) {
                    value = EditorGUILayout.IntField( content, ( int )value );
                } else {
                    value = EditorGUILayout.IntSlider( content, ( int )value, ( int )rangeAttribute.min, ( int )rangeAttribute.max );
                }
                fieldRect = GUILayoutUtility.GetLastRect( );
            } else if ( type == typeof( float ) ) {
                if ( rangeAttribute == null ) {
                    value = EditorGUILayout.FloatField( content, ( float )value );
                } else {
                    value = EditorGUILayout.Slider( content, ( float )value, rangeAttribute.min, rangeAttribute.max );
                }
                fieldRect = GUILayoutUtility.GetLastRect( );
            } else if ( type == typeof( Vector2 ) ) {
                value = EditorGUILayout.Vector2Field( content, ( Vector2 )value );
                fieldRect = GUILayoutUtility.GetLastRect( );
            } else if ( type == typeof( Vector3 ) ) {
                value = EditorGUILayout.Vector3Field( content, ( Vector3 )value );
                fieldRect = GUILayoutUtility.GetLastRect( );
            } else if ( type == typeof( Vector4 ) ) {
                value = EditorGUILayout.Vector4Field( content, ( Vector4 )value );
                fieldRect = GUILayoutUtility.GetLastRect( );
            } else if ( type == typeof( Color ) ) {
                value = EditorGUILayout.ColorField( content, ( Color )value );
                fieldRect = GUILayoutUtility.GetLastRect( );
            } else if ( type == typeof( AnimationCurve ) ) {
                value = EditorGUILayout.CurveField( content, ( AnimationCurve )value );
                fieldRect = GUILayoutUtility.GetLastRect( );
            } else if ( type == typeof( Bounds ) ) {
                value = EditorGUILayout.BoundsField( content, ( Bounds )value );
                fieldRect = GUILayoutUtility.GetLastRect( );
            } else if ( type == typeof( Rect ) ) {
                value = EditorGUILayout.RectField( content, ( Rect )value );
                fieldRect = GUILayoutUtility.GetLastRect( );
            } else if ( type == typeof( LayerMask ) ) {
                value = EditorGUILayout.LayerField( content, ( LayerMask )value );
                fieldRect = GUILayoutUtility.GetLastRect( );
            } else if ( type.IsEnum ) {
                value = EditorGUILayout.EnumPopup( content, ( System.Enum )value );
                fieldRect = GUILayoutUtility.GetLastRect( );
            } else if ( type.IsSubclassOf( typeof( Object ) ) ) {
                value = EditorGUILayout.ObjectField( content, ( Object )value, type, true );
                fieldRect = GUILayoutUtility.GetLastRect( );
            } else if ( type.IsArray ) {
                fieldRect = EditorGUILayout.GetControlRect( );
                EditorGUI.LabelField( fieldRect, content, EditorStyles.boldLabel );
                var array = value as System.Array;
                var elementType = type.GetElementType( );
                for ( int i = 0; i < array.Length; i++ ) {
                    using ( new EditorGUILayout.HorizontalScope( ) ) {
                        EditorGUI.BeginChangeCheck( );
                        EditorGUI.indentLevel++;
                        Rect dummy;
                        var v = Field(new GUIContent(i.ToString()), array.GetValue( i ),elementType, rangeAttribute, out dummy);
                        EditorGUI.indentLevel--;
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            array.SetValue( v, i );
                            GUI.changed = true;
                        }

                        int index_after = GUIElement_Utils.UpDownButton( i, array.Length );
                        if ( i != index_after ) {
                            array.Swap( i, index_after );
                            GUI.changed = true;
                        }
                        EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 10 ) );
                        if ( GUILayout.Button( "-", GUILayout.Width( 20 ) ) ) {
                            var newArray = System.Array.CreateInstance(elementType, array.Length - 1 );
                            for ( int j = 0; j < array.Length; j++ ) {
                                if ( j < i ) {
                                    newArray.SetValue( array.GetValue( j ), j );
                                } else if ( j > i ) {
                                    newArray.SetValue( array.GetValue( j ), j - 1 );
                                }
                            }
                            array = newArray;
                            value = array;
                            GUI.changed = true;
                            i--;
                        }
                    }
                }
                EditorGUI.indentLevel++;
                var rect = EditorGUILayout.GetControlRect( GUILayout.Width( 20 ) );
                rect = EditorGUI.IndentedRect( rect );
                rect.width = 20;
                EditorGUI.indentLevel--;
                if ( GUI.Button( rect, "+" ) ) {
                    var newArray = System.Array.CreateInstance( type.GetElementType( ), array.Length + 1 );
                    for ( int j = 0; j < array.Length; j++ ) {
                        newArray.SetValue( array.GetValue( j ), j );
                    }
                    newArray.SetValue( array.GetValue( array.Length - 1 ), array.Length );
                    array = newArray;
                    value = array;
                    GUI.changed = true;
                }
            } else if ( type.IsGenericType && type.GetGenericTypeDefinition( ) == typeof( System.Collections.Generic.List<> ) ) {
                fieldRect = EditorGUILayout.GetControlRect( );
                EditorGUI.LabelField( fieldRect, content, EditorStyles.boldLabel );
                var list = value as System.Collections.IList;
                var elementType = type.GetGenericArguments( )[0];
                for ( int i = 0; i < list.Count; i++ ) {
                    using ( new EditorGUILayout.HorizontalScope( ) ) {
                        EditorGUI.BeginChangeCheck( );
                        EditorGUI.indentLevel++;
                        Rect dummy;
                        var v = Field(new GUIContent(i.ToString()), list[i], elementType, rangeAttribute, out dummy);
                        EditorGUI.indentLevel--;
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            list[i] = v;
                            GUI.changed = true;
                        }

                        int index_after = GUIElement_Utils.UpDownButton( i, list.Count );
                        if ( i != index_after ) {
                            list.Swap( i, index_after );
                            GUI.changed = true;
                        }
                        EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 10 ) );
                        if ( GUILayout.Button( "-", GUILayout.Width( 20 ) ) ) {
                            list.RemoveAt( i );
                            GUI.changed = true;
                            i--;
                        }
                    }
                }
                EditorGUI.indentLevel++;
                var rect = EditorGUILayout.GetControlRect( GUILayout.Width( 20 ) );
                rect = EditorGUI.IndentedRect( rect );
                rect.width = 20;
                EditorGUI.indentLevel--;
                if ( GUI.Button( rect, "+" ) ) {
                    list.Add( list[list.Count - 1] );
                    GUI.changed = true;
                }
            } else {
                EditorGUILayout.LabelField( content, new GUIContent( "Unsupported Type: " + type ) );
                fieldRect = GUILayoutUtility.GetLastRect( );
            }
            return value;
        }
    }
}
#endif