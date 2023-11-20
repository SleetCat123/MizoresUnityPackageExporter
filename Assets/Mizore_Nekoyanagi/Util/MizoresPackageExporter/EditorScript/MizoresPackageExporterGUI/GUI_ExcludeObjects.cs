using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor {

#if UNITY_EDITOR
    public static class GUI_ExcludeObjects {
        public static void Draw( MizoresPackageExporterEditor ed, MizoresPackageExporter t, MizoresPackageExporter[] targetlist ) {
            MinMax excludeObjects_count = MinMax.Create( targetlist, v => v.excludeObjects.Count );
            if ( ExporterUtils.EditorPrefFoldout(
                ExporterEditorPrefs.FOLDOUT_EXCLUDE_OBJECTS,
                ExporterTexts.FoldoutExcludeObjects( excludeObjects_count.ToString( ) ),
                new ExporterUtils.FoldoutFuncs( ) {
                    canDragDrop = objectReferences => excludeObjects_count.SameValue && ExporterUtils.Filter_HasPersistentObject( objectReferences ),
                    onDragPerform = ( objectReferences ) => ExporterUtils.AddObjects( targetlist, v => v.excludeObjects, objectReferences ),
                    onRightClick = ( ) => GUIElement_CopyPasteList.OnRightClickFoldout<PackagePrefsElement>( targetlist, ExporterTexts.FoldoutExcludeObjects, ( ex ) => ex.excludeObjects, ( ex, list ) => ex.excludeObjects = list )
                }
                ) ) {
                GUIElement_PackagePrefsElementList.Draw<Object>( t, targetlist, ( v ) => v.excludeObjects );
            }
        }
    }
#endif
}