using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
#if UNITY_EDITOR
    public class VerticalBoxScope : EditorGUILayout.VerticalScope {
        public VerticalBoxScope( ) : base( GUI.skin.box ) { }
        public VerticalBoxScope( params GUILayoutOption[] options ) : base( GUI.skin.box, options ) { }
        public static Rect BeginVerticalBox( ) {
            return EditorGUILayout.BeginVertical( GUI.skin.box );
        }
        public static void EndVerticalBox( ) {
            EditorGUILayout.EndVertical( );
        }
    }
#endif
}
