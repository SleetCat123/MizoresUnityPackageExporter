using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
#if UNITY_EDITOR
    public static class CustomFoldout {
        public class FoldoutFuncs {
            public System.Func<Object[], bool> canDragDrop;
            public System.Action<Object[]> onDragPerform;
            public System.Action onRightClick;
        }
        public static bool EditorPrefFoldout( string key, string label ) {
            return EditorPrefFoldout( key, new GUIContent( label ), null );
        }
        public static bool EditorPrefFoldout( string key, string label, FoldoutFuncs funcs ) {
            return EditorPrefFoldout( key, new GUIContent( label ), funcs );
        }
        public static bool EditorPrefFoldout( string key, GUIContent label ) {
            return EditorPrefFoldout( key, label, null );
        }
        public static bool EditorPrefFoldout( string key, GUIContent label, FoldoutFuncs funcs ) {
            bool before = EditorPrefsCache.GetBool( key, true );
            Rect rect = EditorGUILayout.GetControlRect( GUILayout.Height( 22 ) );

            var style = new GUIStyle("ShurikenModuleTitle");
            style.font = new GUIStyle( EditorStyles.label ).font;
            style.fontSize = 13;
            style.fixedHeight = 22;
            style.contentOffset = new Vector2( 19f, -2f );
            var temp_color = GUI.backgroundColor;
            GUI.backgroundColor *= new Color( 1f, 1f, 1f, 0.7f );

            bool pushed = GUI.Button( rect, label, style );
            GUI.backgroundColor = temp_color;
            bool result = before;
            if ( pushed ) {
                result = !before;
            } 
            // result = EditorGUILayout.Foldout( before, label, true, EditorStyles.foldoutHeader );
            var ev = Event.current;
            var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
            if ( ev.type == EventType.Repaint ) {
                EditorStyles.foldout.Draw( toggleRect, false, false, result, false );
            }

            if ( funcs != null && ExporterUtils.DragDrop( rect, funcs.canDragDrop ) ) {
                funcs.onDragPerform( DragAndDrop.objectReferences );
                result = true;
            }

            if ( before != result ) {
                EditorPrefsCache.SetBool( key, result );
            }

            Event currentEvent = Event.current;
            if ( funcs != null && funcs.onRightClick != null && currentEvent.type == EventType.ContextClick && rect.Contains( currentEvent.mousePosition ) ) {
                funcs.onRightClick( );
                currentEvent.Use( );
            }
            return result;
        }
    }
#endif
}
