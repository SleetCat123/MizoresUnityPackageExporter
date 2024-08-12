using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public static class LocalizedEnumPopup {
        class ArrayData {
            public string className;
            public Array values;
            public string[] names;

            public string currentLanguage;
            public string[] localizedNames;

            public ArrayData( Type enumType ) {
                className = enumType.Name;
                values = Enum.GetValues( enumType );
                names = Enum.GetNames( enumType );
            }

            public void UpdateLocalization( ) {
                if ( currentLanguage == ExporterEditorPrefs.Language ) {
                    return;
                }
                currentLanguage = ExporterEditorPrefs.Language;
                localizedNames = new string[names.Length];
                for ( int i = 0; i < names.Length; i++ ) {
                    localizedNames[i] = ExporterTexts.Get( $"{className}_{names[i]}" );
                }
            }
        }
        static Dictionary<Type, ArrayData> enumData = new Dictionary<Type, ArrayData>( );
        public static T EnumPopup<T>( string label, EnumData<T> value ) where T : Enum {
            return EnumPopup( label, value.value );
        }
        public static T EnumPopup<T>( EnumData<T> value ) where T : Enum {
            return EnumPopup( string.Empty, value.value );
        }
        public static T EnumPopup<T>( T value ) where T : Enum {
            return EnumPopup( string.Empty, value );
        }
        public static T EnumPopup<T>( string label, T value ) where T : Enum {
            var type = typeof( T );
            ArrayData data;
            if ( enumData.TryGetValue( type, out data ) == false ) {
                data = new ArrayData( type );
                enumData[type] = data;
            }

            data.UpdateLocalization( );
            var index = Array.IndexOf( data.values, value );
            index = EditorGUILayout.Popup( label, index, data.localizedNames );
            return ( T )data.values.GetValue( index );
        }
    }
}
