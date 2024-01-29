using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#if UNITY_EDITOR
namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public static class PackagePrefsElementInspector {
        public static void Draw<T>( MizoresPackageExporter t, PackagePrefsElement element ) where T : UnityEngine.Object {
            using ( new EditorGUILayout.HorizontalScope( ) ) {
                EditorGUI.BeginChangeCheck( );
                element.Object = EditorGUILayout.ObjectField( element.Object, typeof( T ), false );
                if ( EditorGUI.EndChangeCheck( ) ) {
                    GUI.changed = true;
                }

                EditorGUI.BeginChangeCheck( );
                Rect textrect = EditorGUILayout.GetControlRect( );
                string path = EditorGUI.TextField( textrect, element.Path );
                if ( ExporterUtils.DragDrop( textrect, ExporterUtils.Filter_HasPersistentObject ) ) {
                    GUI.changed = true;
                    path = AssetDatabase.GetAssetPath( DragAndDrop.objectReferences[0] );
                }

                bool browse = GUIElement_Utils.BrowseButtons( t, path, out string resultPath, forceAbsolute: true );
                if ( browse ) {
                    path = resultPath;
                }

                if ( EditorGUI.EndChangeCheck( ) ) {
                    path = PathUtils.ToValidPath( path );
                    // パスが変更されたらオブジェクトを置き換える
                    Object o = AssetDatabase.LoadAssetAtPath<T>( path );
                    if ( o == null ) {
                        Debug.LogWarning( "Not found asset at path: " + path );
                    } else {
                        // オブジェクトが取得できた場合のみ置き換える
                        element.Object = o;
                    }
                    GUI.changed = true;
                }
            }
        }
    }
}
#endif