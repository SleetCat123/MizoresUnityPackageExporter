using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter
{
    public static class CopyCache
    {
        private static object cache;

        private static bool IsList( Type type ) {
            return type.IsGenericType && type.GetGenericTypeDefinition( ) == typeof( List<> );
        }
        private static bool IsDictionary( Type type ) {
            return type.IsGenericType && type.GetGenericTypeDefinition( ) == typeof( Dictionary<,> );
        }
        public static bool CanPaste<T>( ) {
            return cache is T;
        }
        static object Clone( object value ) {
            var type = value.GetType( );
            if ( IsList( type ) ) {
                var listOriginal = value as IList;
                var list = (IList)Activator.CreateInstance( type, listOriginal.Count );
                for ( int i = 0; i < listOriginal.Count; i++ ) {
                    var listValue = listOriginal[i];
                    if ( listValue == null ) {
                        list.Add( null );
                    } else {
                        var clonable = (ICloneable)listValue;
                        list.Add( clonable.Clone( ) );
                    }
                }
                return list;
            } else if ( IsDictionary( type ) ) {
                var dictOriginal = value as IDictionary;
                var dict = (IDictionary)Activator.CreateInstance( type );
                foreach ( DictionaryEntry kvp in dictOriginal ) {
                    var kvpKey = ( (ICloneable)kvp.Key ).Clone( );
                    var kvpValue = kvp.Value;
                    if ( kvpValue != null ) {
                        kvpValue = ( (ICloneable)kvpValue ).Clone( );
                    }
                    dict[kvpKey] = kvpValue;
                }
                return dict;
            } else if ( type.IsArray ) {
                var arrayOriginal = value as Array;
                var arrayType = type.GetElementType( );
                var array = Array.CreateInstance( arrayType, arrayOriginal.Length );
                for ( int i = 0; i < array.Length; i++ ) {
                    var arrayValue = arrayOriginal.GetValue( i );
                    if ( arrayValue == null ) {
                    } else {
                        var clonable = (ICloneable)arrayValue;
                        array.SetValue( clonable.Clone( ), i );
                    }
                }
                return array;
            } else if ( value is ICloneable ) {
                return ( value as ICloneable ).Clone( );
            } else {
                throw new ArgumentException( );
            }
        }
        public static void Copy( object value ) {
            cache = Clone( value );
        }
        public static T GetCache<T>( bool clone = true ) {
            if ( clone ) {
                return (T)Clone( cache );
            } else {
                return (T)cache;
            }
        }
    }
}
