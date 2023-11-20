#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter
{
    public static class EditorPrefsCache
    {
        static Dictionary<string, bool> _table_bool = new Dictionary<string, bool>( );
        public static void RefleshBool( string key, bool defaultValue = false ) {
            if ( _table_bool.ContainsKey( key ) ) {
                _table_bool[key] = EditorPrefs.GetBool( key, defaultValue );
            }
        }
        public static bool GetBool( string key, bool defaultValue = false ) {
            bool value;
            if ( _table_bool.TryGetValue( key, out value ) ) {
                return value;
            } else {
                value = EditorPrefs.GetBool( key, defaultValue );
                _table_bool[key] = value;
                return value;
            }
        }
        public static void SetBool( string key, bool value ) {
            var prev = _table_bool[key];
            _table_bool[key] = value;
            if ( prev != value ) {
                EditorPrefs.SetBool( key, value );
            }
        }

        static Dictionary<string, string> _table_string = new Dictionary<string, string>( );
        public static void RefleshString( string key, string defaultValue = "" ) {
            if ( _table_string.ContainsKey( key ) ) {
                _table_string[key] = EditorPrefs.GetString( key, defaultValue );
            }
        }
        public static string GetString( string key, string defaultValue = "" ) {
            string value;
            if ( _table_string.TryGetValue( key, out value ) ) {
                return value;
            } else {
                value = EditorPrefs.GetString( key, defaultValue );
                _table_string[key] = value;
                return value;
            }
        }
        public static void SetString( string key, string value ) {
            var prev = _table_string[key];
            _table_string[key] = value;
            if ( prev != value ) {
                EditorPrefs.SetString( key, value );
            }
        }
    }
}
#endif