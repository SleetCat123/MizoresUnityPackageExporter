using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#if UNITY_EDITOR
namespace MizoreNekoyanagi.PublishUtil.PackageExporter
{
    public static class PackagePrefsElementInspector
    {
        public static void Draw<T>( PackagePrefsElement element ) where T : UnityEngine.Object {
            bool changed = false;
            EditorGUI.BeginChangeCheck( );
            element.Object = EditorGUILayout.ObjectField( element.Object, typeof( T ), false );
            if ( EditorGUI.EndChangeCheck( ) ) {
                changed = true;
            }

            EditorGUI.BeginChangeCheck( );
            Rect textrect = EditorGUILayout.GetControlRect( );
            string path = EditorGUI.TextField( textrect, element.Path );
            if ( ExporterUtils.DragDrop( textrect, ExporterUtils.Filter_HasPersistentObject ) ) {
                GUI.changed = true;
                path = AssetDatabase.GetAssetPath( DragAndDrop.objectReferences[0] );
            }

            if ( EditorGUI.EndChangeCheck( ) ) {
                // パスが変更されたらオブジェクトを置き換える
                Object o = AssetDatabase.LoadAssetAtPath<T>( path );
                if ( o != null ) {
                    element.Object = o;
                }
                changed = true;
            }
            GUI.changed = changed;
        }
    }
}
#endif