using UnityEngine;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor {

#if UNITY_EDITOR
    public class GUI_ExcludeObjects {
        public void Draw( MizoresPackageExporterEditor ed, MizoresPackageExporter t, MizoresPackageExporter[] targetlist ) {
            MinMax excludeObjects_count = MinMax.Create( targetlist, v => v.excludeObjects.Count );
            if ( CustomFoldout.EditorPrefFoldout(
                ExporterEditorPrefs.FOLDOUT_EXCLUDE_OBJECTS,
                ExporterTexts.FoldoutExcludeObjects( excludeObjects_count.ToString( ) ),
                new CustomFoldout.FoldoutFuncs( ) {
                    canDragDrop = objectReferences => excludeObjects_count.SameValue && ExporterUtils.Filter_HasPersistentObject( objectReferences ),
                    onDragPerform = ( objectReferences ) => ExporterUtils.AddObjects( targetlist, v => v.excludeObjects, objectReferences ),
                    onRightClick = ( ) => GUIElement_CopyPasteList.OnRightClickFoldout<ObjectRefElement>( targetlist, ExporterTexts.FoldoutExcludeObjects, ( ex ) => ex.excludeObjects, ( ex, list ) => ex.excludeObjects = list )
                }
                ) ) {
                GUIElement_PackagePrefsElementList<Object, ObjectRefElement>.Draw( targetlist, v => v.excludeObjects );
            }
        }
    }
#endif
}