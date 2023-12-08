using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor {
#if UNITY_EDITOR
    public static class GUI_ReferencesObjects {
        public static void Draw( MizoresPackageExporterEditor ed, MizoresPackageExporter t, MizoresPackageExporter[] targetlist ) {
            MinMax references_count = MinMax.Create( targetlist, v => v.references.Count );
            if ( ExporterUtils.EditorPrefFoldout(
                ExporterEditorPrefs.FOLDOUT_REFERENCES,
                new GUIContent( ExporterTexts.FoldoutReferences( references_count.ToString( ) ), ExporterTexts.FoldoutReferencesTooltip ),
                new ExporterUtils.FoldoutFuncs( ) {
                    canDragDrop = objectReferences => references_count.SameValue && ExporterUtils.Filter_HasPersistentObject( objectReferences ),
                    onDragPerform = ( objectReferences ) => ExporterUtils.AddObjects( targetlist, v => v.references, objectReferences ),
                    onRightClick = ( ) => GUIElement_CopyPasteList.OnRightClickFoldout<PackagePrefsElement>( targetlist, ExporterTexts.FoldoutReferences, ( ex ) => ex.references, ( ex, list ) => ex.references = list )
                }
                ) ) {
                GUIElement_PackagePrefsElementList.Draw<Object>( t, targetlist, ( v ) => v.references );
            }
        }
    }
#endif
}