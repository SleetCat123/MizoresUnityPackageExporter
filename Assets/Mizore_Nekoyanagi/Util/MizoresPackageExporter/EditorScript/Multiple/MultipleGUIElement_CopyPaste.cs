using UnityEngine;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.MultipleEditor
{
    public static class MultipleGUIElement_CopyPaste
    {
        public static void OnRightClickFoldout<T>( IEnumerable< MizoresPackageExporter> targets, string labelFormat, Action<MizoresPackageExporter, List<T>> setList ) {
            GenericMenu menu = new GenericMenu( );
            if ( CopyCache.CanPaste<List<T>>( ) ) {
                var cache = CopyCache.GetCache<List<T>>( clone: false );
                string label = string.Format( ExporterTexts.t_PasteTarget, string.Format( labelFormat, cache.Count ) );
                menu.AddItem( new GUIContent( label ), false, ( ) => {
                    foreach ( var t in targets ) {
                        setList( t, CopyCache.GetCache<List<T>>( ) );
                        EditorUtility.SetDirty( t );
                    }
                } );
            } else {
                menu.AddDisabledItem( new GUIContent( ExporterTexts.t_PasteTargetNoValue ) );
            }
            menu.ShowAsContext( );
        }
    }
}
#endif
