using UnityEngine;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor {

#if UNITY_EDITOR
    public class GUI_ExcludeObjects {
        GUIElement_PackagePrefsElementList<Object> list;
        public GUI_ExcludeObjects( ) {
            var field = typeof( MizoresPackageExporter ).GetField( "excludeObjects", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            list = new GUIElement_PackagePrefsElementList<Object>( field );
        }
        public void Draw( MizoresPackageExporterEditor ed, MizoresPackageExporter t, MizoresPackageExporter[] targetlist ) {
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
                list.Draw( t, targetlist );
            }
        }
    }
#endif
}