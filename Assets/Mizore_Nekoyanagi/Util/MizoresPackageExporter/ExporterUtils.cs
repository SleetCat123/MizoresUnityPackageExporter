using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter
{
    public static class ExporterUtils
    {
        public const int EDITOR_INDENT = 15;
        public static void Indent( int indent ) {
#if UNITY_EDITOR
            for ( int i = 0; i < indent; i++ ) {
                EditorGUILayout.LabelField( string.Empty, GUILayout.Width( EDITOR_INDENT ) );
            }
#endif
        }

        public static int UpDownButton( int index, int listLength, int buttonWidth = 15 ) {
            index = Mathf.Clamp( index, 0, listLength - 1 );
#if UNITY_EDITOR
            var w = GUILayout.Width( buttonWidth );
            using ( var scope = new EditorGUI.DisabledGroupScope( index == 0 ) ) {
                if ( GUILayout.Button( "↑", w ) ) {
                    index = index - 1;
                }
            }
            using ( var scope = new EditorGUI.DisabledGroupScope( index == listLength - 1 ) ) {
                if ( GUILayout.Button( "↓", w ) ) {
                    index = index + 1;
                }
            }
#endif
            return index;
        }

        public static void SeparateLine( ) {
#if UNITY_EDITOR
            EditorGUILayout.LabelField( string.Empty, GUI.skin.horizontalSlider );
#endif
        }

        public static void DiffLabel( ) {
#if UNITY_EDITOR
            EditorGUILayout.LabelField( new GUIContent( ExporterTexts.t_Diff_Label, ExporterTexts.t_Diff_Tooltip ), GUILayout.Width( 30 ) );
#endif
        }

        public static bool Filter_HasPersistentObject( Object[] objectReferences ) {
#if UNITY_EDITOR
            return objectReferences.Any( v => EditorUtility.IsPersistent( v ) );
#else
            return false;
#endif
        }
        public static bool DragDrop( Rect rect, System.Func<Object[], bool> canDragDrop ) {
#if UNITY_EDITOR
            if ( canDragDrop != null && rect.Contains( Event.current.mousePosition ) && canDragDrop( DragAndDrop.objectReferences ) ) {
                var eventType = Event.current.type;
                if ( eventType == EventType.DragUpdated || eventType == EventType.DragPerform ) {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                }
                if ( eventType == EventType.DragPerform ) {
                    DragAndDrop.AcceptDrag( );
                    Event.current.Use( );
                    return true;
                }
            }
#endif
            return false;
        }
        public class FoldoutFuncs
        {
            public System.Func<Object[], bool> canDragDrop;
            public System.Action<Object[]> onDragPerform;
            public System.Action onRightClick;
        }
        public static bool EditorPrefFoldout( string key, string label ) {
            return EditorPrefFoldout( key, label, null );
        }
        public static bool EditorPrefFoldout( string key, string label, FoldoutFuncs funcs ) {
            bool result = true;
#if UNITY_EDITOR
            bool before = EditorPrefs.GetBool( key, true );
            Rect rect = EditorGUILayout.GetControlRect( );

            result = EditorGUI.BeginFoldoutHeaderGroup( rect, before, label );
            // result = EditorGUILayout.Foldout( before, label, true, EditorStyles.foldoutHeader );

            if ( funcs != null && DragDrop( rect, funcs.canDragDrop ) ) {
                funcs.onDragPerform( DragAndDrop.objectReferences );
                result = true;
            }

            if ( before != result ) {
                EditorPrefs.SetBool( key, result );
            }
            EditorGUI.EndFoldoutHeaderGroup( );

            Event currentEvent = Event.current;
            if ( funcs != null && funcs.onRightClick != null && currentEvent.type == EventType.ContextClick && rect.Contains( currentEvent.mousePosition ) ) {
                funcs.onRightClick( );
                currentEvent.Use( );
            }
#endif
            return result;
        }

        public struct MinMax
        {
            public int min, max;

            public MinMax( int min, int max ) {
                this.min = min;
                this.max = max;
            }

            public string GetRangeString( ) {
                if ( min == max ) {
                    return min.ToString( );
                } else {
                    return $"{min}-{max}";
                }
            }

            public static MinMax Create<T>( IEnumerable<T> list, System.Func<T, int> func ) {
                int min = list.Min( func );
                int max = list.Max( func );
                return new MinMax( min, max );
            }
        }
        public static void Swap<T>( this List<T> list, int indexA, int indexB ) {
            T temp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = temp;
        }
        public static void ResizeList<T>( List<T> list, int newSize, System.Func<T> newvalue ) {
            if ( list.Count == newSize ) {
                return;
            } else if ( list.Count < newSize ) {
                var array = new T[newSize - list.Count];
                if ( newvalue != null ) {
                    for ( int i = 0; i < array.Length; i++ ) {
                        array[i] = newvalue.Invoke( );
                    }
                }
                list.AddRange( array );
            } else {
                list.RemoveRange( newSize, list.Count - newSize );
            }
        }
        public static void ResizeList<T>( List<T> list, int newSize ) {
            if ( list.Count == newSize ) {
                return;
            } else if ( list.Count < newSize ) {
                list.AddRange( new T[newSize - list.Count] );
            } else {
                list.RemoveRange( newSize, list.Count - newSize );
            }
        }
    }
}
