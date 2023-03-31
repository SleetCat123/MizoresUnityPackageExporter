using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.SingleEditor
{
    public static class SingleGUIElement_PackagePrefsElementList
    {
        public static void Draw<T>( MizoresPackageExporter t, List<PackagePrefsElement> list ) where T : UnityEngine.Object {
            for ( int i = 0; i < list.Count; i++ ) {
                using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                    PackagePrefsElement item = list[i];
                    ExporterUtils.Indent( 1 );
                    EditorGUILayout.LabelField( i.ToString( ), GUILayout.Width( 30 ) );

                    EditorGUI.BeginChangeCheck( );
                    PackagePrefsElementInspector.Draw<T>( item );
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        EditorUtility.SetDirty( t );
                        Debug.Log("test" );
                    }

                    int index_after = ExporterUtils.UpDownButton( i, list.Count );
                    if ( i != index_after ) {
                        list.Swap( i, index_after );
                        EditorUtility.SetDirty( t );
                    }
                    EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 10 ) );
                    if ( GUILayout.Button( "-", GUILayout.Width( 15 ) ) ) {
                        list.RemoveAt( i );
                        i--;
                        EditorUtility.SetDirty( t );
                    }
                }
            }
            using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                ExporterUtils.Indent( 1 );
                if ( GUILayout.Button( "+", GUILayout.Width( 60 ) ) ) {
                    list.Add( new PackagePrefsElement( ) );
                    EditorUtility.SetDirty( t );
                }
            }
        }
    }
}
#endif
