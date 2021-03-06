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

        public static bool EditorPrefFoldout( string key, string label ) {
            bool result = true;
#if UNITY_EDITOR
            bool before = EditorPrefs.GetBool( key, true );
            if ( before ) {
                label = "▼ " + label;
            } else {
                label = "▶ " + label;
            }
            result= EditorGUI.BeginFoldoutHeaderGroup( EditorGUILayout.GetControlRect( ), before, label );
            // result = EditorGUILayout.Foldout( before, label, true, EditorStyles.foldoutHeader );
            if ( before != result ) {
                EditorPrefs.SetBool( key, result );
            }
            EditorGUI.EndFoldoutHeaderGroup( );
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
