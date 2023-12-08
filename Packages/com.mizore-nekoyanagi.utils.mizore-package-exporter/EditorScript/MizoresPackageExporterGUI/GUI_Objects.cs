using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor {
#if UNITY_EDITOR
    public static class GUI_Objects {
        public static void Draw( MizoresPackageExporterEditor ed, MizoresPackageExporter t, MizoresPackageExporter[] targetlist ) {
            MinMax objects_count = MinMax.Create( targetlist, v => v.objects.Count );
            if ( ExporterUtils.EditorPrefFoldout(
                ExporterEditorPrefs.FOLDOUT_OBJECT,
                new GUIContent( ExporterTexts.FoldoutObjects( objects_count.ToString( ) ), ExporterTexts.FoldoutObjectsTooltip ),
                new ExporterUtils.FoldoutFuncs( ) {
                    canDragDrop = objectReferences => objects_count.SameValue && ExporterUtils.Filter_HasPersistentObject( objectReferences ),
                    onDragPerform = ( objectReferences ) => ExporterUtils.AddObjects( targetlist, v => v.objects, objectReferences ),
                    onRightClick = ( ) => GUIElement_CopyPasteList.OnRightClickFoldout<PackagePrefsElement>( targetlist, ExporterTexts.FoldoutObjects, ( ex ) => ex.objects, ( ex, list ) => ex.objects = list )
                }
                ) ) {
                GUIElement_PackagePrefsElementList.Draw<Object>( t, targetlist, ( v ) => v.objects );
            }
        }

    }
#endif
}