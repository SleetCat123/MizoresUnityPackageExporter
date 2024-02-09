using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using System.Reflection;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public static class ExporterUtils {
        public enum GetIconResult {
            ExistsFile, ExistsFolder, NotExistsFile, NotExistsFolder, Dummy
        }
        public static bool IsExists( this GetIconResult value ) {
            switch ( value ) {
                case GetIconResult.ExistsFile:
                case GetIconResult.ExistsFolder:
                    return true;
                case GetIconResult.NotExistsFile:
                case GetIconResult.NotExistsFolder:
                default:
                    return false;
            }
        }
        public static GetIconResult TryGetIcon( string path, out Texture icon ) {
#if UNITY_EDITOR
            if ( Path.GetExtension( path ).Length != 0 ) {
                if ( File.Exists( path ) ) {
                    icon = AssetDatabase.GetCachedIcon( path );
                    return GetIconResult.ExistsFile;
                } else {
                    icon = IconCache.ErrorIcon;
                    return GetIconResult.NotExistsFile;
                }
            } else if ( Directory.Exists( path ) ) {
                icon = AssetDatabase.GetCachedIcon( path );
                return GetIconResult.ExistsFolder;
            } else {
                icon = IconCache.ErrorIcon;
                return GetIconResult.NotExistsFolder;
            }
#else
            icon = null;
            return GetIconResult.NotExistsFile;
#endif
        }

        public static void SeparateLine( float lineHeight = 2f, float margins = 4f ) {
#if UNITY_EDITOR
            // EditorGUILayout.LabelField( string.Empty, GUI.skin.horizontalSlider );
            var baseRect = EditorGUILayout.GetControlRect( GUILayout.Height( margins + lineHeight + margins ) );
            var rect = new Rect(baseRect );
            rect.y += margins;
            rect.height = lineHeight;
            var color = Color.gray;
            EditorGUI.DrawRect( rect, color );
#endif
        }

        public static void DiffLabel( ) {
#if UNITY_EDITOR
            EditorGUILayout.LabelField( new GUIContent( ExporterTexts.DiffLabel, ExporterTexts.DiffTooltip ), GUILayout.Width( 30 ) );
#endif
        }

        public static void AddObjects<TElement>( IEnumerable<MizoresPackageExporter> targetlist, System.Func<MizoresPackageExporter, List<TElement>> getList, Object[] objectReferences ) where TElement : PackagePrefsElement, new() {
#if UNITY_EDITOR
            var add = objectReferences.
                Where( v => EditorUtility.IsPersistent( v ) ).
                Select( v => {
                    var r = new TElement( );
                    r.Object = v;
                    return r;
                });
            foreach ( var item in targetlist ) {
                getList( item ).AddRange( add );
                EditorUtility.SetDirty( item );
            }
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

        public static void Swap<T>( this T[] array, int indexA, int indexB ) {
            T temp = array[indexA];
            array[indexA] = array[indexB];
            array[indexB] = temp;
        }
        public static void Swap( this System.Array array, int indexA, int indexB ) {
            object temp = array.GetValue( indexA );
            array.SetValue( array.GetValue( indexB ), indexA );
            array.SetValue( temp, indexB );
        }
        public static void Swap<T>( this List<T> list, int indexA, int indexB ) {
            T temp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = temp;
        }
        public static void Swap( this IList list, int indexA, int indexB ) {
            object temp = list[indexA];
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

        [System.Serializable]
        class StringArray {
            public string[] array;
            public StringArray( string[] array ) {
                this.array = array;
            }
        }
        public static string ToJson( System.Type type, object value ) {
            if ( type.IsPrimitive || type == typeof( string ) ) {
                // 基本型とstringの場合は文字列に変換
                return value.ToString( );
            } else if ( type.IsArray ) {
                // StringArrayに変換
                var array = value as System.Array;
                var strArray = new string[array.Length];
                var elementType = type.GetElementType( );
                for ( int i = 0; i < array.Length; i++ ) {
                    strArray[i] = ToJson( elementType, array.GetValue( i ) );
                }
                return JsonUtility.ToJson( new StringArray( strArray ) );
            } else if ( type.IsGenericType && type.GetGenericTypeDefinition( ) == typeof( System.Collections.Generic.List<> ) ) {
                var list = value as IList;
                var strArray = new string[list.Count];
                var elementType = type.GetGenericArguments( )[0];
                for ( int i = 0; i < list.Count; i++ ) {
                    strArray[i] = ToJson( elementType, list[i] );
                }
                return JsonUtility.ToJson( new StringArray( strArray ) );
            } else {
                // その他の場合はJsonに変換
                return JsonUtility.ToJson( value );
            }
        }
        public static bool FromJson( string json, System.Type type, out object result ) {
            if ( type.IsPrimitive ) {
                // 基本型の場合は文字列から変換
                try {
                    result = System.Convert.ChangeType( json, type );
                    return true;
                } catch ( System.Exception e ) {
                    Debug.LogError( $"Can't convert value: {json}" );
                    Debug.LogException( e );
                    result = null;
                    return false;
                }
            } else if ( type == typeof( string ) ) {
                // string型の場合はそのまま
                result = json;
                return true;
            } else if ( type.IsArray ) {
                // StringArrayから変換
                try {
                    var strArray = JsonUtility.FromJson<StringArray>( json ).array;
                    var elementType = type.GetElementType( );
                    var array = System.Array.CreateInstance( elementType, strArray.Length );
                    for ( int i = 0; i < strArray.Length; i++ ) {
                        object obj;
                        if ( FromJson( strArray[i], elementType, out obj ) ) {
                            array.SetValue( obj, i );
                        }
                    }
                    result = array;
                    return true;
                } catch ( System.Exception e ) {
                    Debug.LogError( $"Can't convert value: {json}" );
                    Debug.LogException( e );
                    result = null;
                    return false;
                }
            } else if ( type.IsGenericType && type.GetGenericTypeDefinition( ) == typeof( System.Collections.Generic.List<> ) ) {
                // StringArrayから変換
                try {
                    var strArray = JsonUtility.FromJson<StringArray>( json ).array;
                    var elementType = type.GetGenericArguments( )[0];
                    var list = System.Activator.CreateInstance( type ) as IList;
                    for ( int i = 0; i < strArray.Length; i++ ) {
                        object obj;
                        if ( FromJson( strArray[i], elementType, out obj ) ) {
                            list.Add( obj );
                        } else {
                            list.Add( null );
                        }
                    }
                    result = list;
                    return true;
                } catch ( System.Exception e ) {
                    Debug.LogError( $"Can't convert value: {json}" );
                    Debug.LogException( e );
                    result = null;
                    return false;
                }
            } else {
                try {
                    // その他の場合はJsonから変換
                    result = JsonUtility.FromJson( json, type );
                    return true;
                } catch ( System.ArgumentException e ) {
                    Debug.LogError( $"Can't convert value: {json}" );
                    Debug.LogException( e );
                    result = null;
                    return false;
                }
            }
        }

        static Regex _invalidFileCharsRegex;
        /// <summary>
        /// ファイル名に使用できない文字の判定用
        /// </summary>
        public static Regex InvalidFileCharsRegex {
            get {
                if ( _invalidFileCharsRegex == null ) {
                    string invalid = "." + new string( Path.GetInvalidFileNameChars( ) );
                    string pattern = string.Format( "[{0}]", Regex.Escape( invalid ) );
                    _invalidFileCharsRegex = new Regex( pattern );
                }
                return _invalidFileCharsRegex;
            }
        }

        public static void DebugLog( string value ) {
#if UNITY_EDITOR
            if ( ExporterEditorPrefs.DebugMode ) {
                Debug.Log( "[DEBUG] " + value );
            }
#endif
        }
        public static void DebugLogWarning( string value ) {
#if UNITY_EDITOR
            if ( ExporterEditorPrefs.DebugMode ) {
                Debug.LogWarning( "[DEBUG] " + value );
            }
#endif
        }

        public static void DebugLogError( string value ) {
#if UNITY_EDITOR
            if ( ExporterEditorPrefs.DebugMode ) {
                Debug.LogError( "[DEBUG] " + value );
            }
#endif
        }
    }
}
