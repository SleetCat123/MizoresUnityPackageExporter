using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.SingleEditor
{
    public static class SingleGUIElement_CopyPaste
    {
        public static void OnRightClickFoldout<T>( MizoresPackageExporter t, string labelFormat, List<T> list, System.Action<List<T>> setList ) {
            GenericMenu menu = new GenericMenu( );
            {
                string label = string.Format( ExporterTexts.t_CopyTarget, string.Format( labelFormat, list.Count ) );
                menu.AddItem( new GUIContent( label ), false, CopyCache.Copy, list );
            }
            if ( CopyCache.CanPaste<List<T>>( ) ) {
                var cache = CopyCache.GetCache<List<T>>( clone: false );
                string label = string.Format( ExporterTexts.t_PasteTarget, string.Format( labelFormat, cache.Count ) );
                menu.AddItem( new GUIContent( label ), false, ( ) => {
                    setList( CopyCache.GetCache<List<T>>( ) );
                    EditorUtility.SetDirty( t );
                } );
            } else {
                menu.AddDisabledItem( new GUIContent( ExporterTexts.t_PasteTargetNoValue ) );
            }
            menu.ShowAsContext( );
        }
        public static void OnRightClickFoldout<TKey, TValue>( MizoresPackageExporter t, string labelFormat, Dictionary<TKey, TValue> table, System.Action<Dictionary<TKey, TValue>> setTable ) {
            GenericMenu menu = new GenericMenu( );
            {
                string label = string.Format( ExporterTexts.t_CopyTarget, string.Format( labelFormat, table.Count ) );
                menu.AddItem( new GUIContent( label ), false, CopyCache.Copy, table );
            }
            if ( CopyCache.CanPaste<Dictionary<TKey, TValue>>( ) ) {
                var cache = CopyCache.GetCache<Dictionary<TKey, TValue>>( clone: false );
                string label = string.Format( ExporterTexts.t_PasteTarget, string.Format( labelFormat, cache.Count ) );
                menu.AddItem( new GUIContent( label ), false, ( ) => {
                    setTable( CopyCache.GetCache<Dictionary<TKey, TValue>>( ) );
                    EditorUtility.SetDirty( t );
                } );
            } else {
                menu.AddDisabledItem( new GUIContent( ExporterTexts.t_PasteTargetNoValue ) );
            }
            menu.ShowAsContext( );
        }
    }
}
#endif
