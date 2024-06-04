using System;
using System.Collections.Generic;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public abstract class EnumData<T> : ISerializationCallbackReceiver, IEquatable<EnumData<T>> where T : Enum {
        [System.NonSerialized]
        public T value;
        [SerializeField]
        protected string valueName;

        public EnumData( ) {
            value = default;
        }
        public EnumData( T value ) {
            this.value = value;
        }

        public bool Equals( EnumData<T> other ) {
            if ( other == null ) {
                return false;
            } else {
                return value.Equals( other.value );
            }
        }
        public bool Equals( T other ) {
            return value.Equals( other );
        }
        public override bool Equals( object obj ) {
            if ( obj is null || GetType( ) != obj.GetType( ) ) {
                return false;
            }
            return Equals( ( EnumData<T> )obj );
        }
        public override int GetHashCode( ) {
            return value.GetHashCode( );
        }
        public static bool operator ==( EnumData<T> a, EnumData<T> b ) {
            if ( a is null ) {
                return b is null;
            }
            return a.Equals( b );
        }
        public static bool operator !=( EnumData<T> a, EnumData<T> b ) {
            if ( a is null ) {
                return b is not null;
            }
            return !a.Equals( b );
        }
        public static bool operator ==( EnumData<T> a, T b ) {
            if ( a is null ) {
                return false;
            }
            return a.Equals( b );
        }
        public static bool operator !=( EnumData<T> a, T b ) {
            if ( a is null ) {
                return true;
            }
            return !a.Equals( b );
        }
        public static bool operator ==( T a, EnumData<T> b ) {
            if ( b is null ) {
                return false;
            }
            return b.Equals( a );
        }
        public static bool operator !=( T a, EnumData<T> b ) {
            if ( b is null ) {
                return true;
            }
            return !b.Equals( a );
        }

        public void OnAfterDeserialize( ) {
            if ( string.IsNullOrEmpty( valueName ) ) {
                value = default;
            } else {
                value = EnumCache.GetValue<T>( valueName );
            }
            valueName = null;
        }

        public void OnBeforeSerialize( ) {
            valueName = EnumCache.GetName( value );
        }

        public static implicit operator T( EnumData<T> data ) {
            return data.value;
        }
    }
    public static class EnumCache {
        static Dictionary<Type, string[ ]> enumNames = new Dictionary<Type, string[ ]>( );
        static Dictionary<Type, Array> enumValues = new Dictionary<Type, Array>( );

        public static string[] GetNames( Type type ) {
            string[] result;
            if ( enumNames.TryGetValue( type, out result ) ) {
                return result;
            } else {
                result = Enum.GetNames( type );
                enumNames.Add( type, result );
                return result;
            }
        }
        public static string[] GetNames<T>( ) where T : Enum {
            Type type = typeof( T );
            string[] result;
            if ( enumNames.TryGetValue( type, out result ) ) {
                return result;
            } else {
                result = Enum.GetNames( type );
                enumNames.Add( type, result );
                return result;
            }
        }
        public static string GetName<T>( T value ) where T : Enum {
            return Enum.GetName( typeof( T ), value );
        }
        public static string GetName<T>( EnumData<T> value ) where T : Enum {
            return Enum.GetName( typeof( T ), value.value );
        }

        public static Enum[] GetValues( Type type ) {
            Array result;
            if ( enumValues.TryGetValue( type, out result ) ) {
                return result as Enum[];
            } else {
                result = Enum.GetValues( type );
                enumValues.Add( type, result );
                return result as Enum[];
            }
        }
        public static T[] GetValues<T>( ) where T : Enum {
            Type type = typeof( T );
            Array result;
            if ( enumValues.TryGetValue( type, out result ) ) {
                return result as T[];
            } else {
                result = Enum.GetValues( type );
                enumValues.Add( type, result );
                return result as T[];
            }
        }
        public static T GetValue<T>( string name ) where T : Enum {
            return ( T )Enum.Parse( typeof( T ), name );
        }
    }
}