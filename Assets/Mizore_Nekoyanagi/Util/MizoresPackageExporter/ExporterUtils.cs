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
