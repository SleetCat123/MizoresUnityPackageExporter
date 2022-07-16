using UnityEngine;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.MizoresPackageExporterConsts;
using System.Linq;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.SingleEditor
{
    public static class SingleEditorGUI
    {
        static bool Filter<T>( Object[] objectReferences ) {
            return objectReferences.Any( v => EditorUtility.IsPersistent( v ) && v is T );
        }
        static void AddObjects<T>( MizoresPackageExporter t, List<PackagePrefsElement> list, Object[] objectReferences ) {
            list.AddRange(
                objectReferences.
                Where( v => EditorUtility.IsPersistent( v ) && v is T ).
                Select( v => new PackagePrefsElement( v ) )
                );
            EditorUtility.SetDirty( t );
        }
        public static void EditSingle( UnityPackageExporterEditor ed ) {
            var t = ed.t;

            using ( var s = new EditorGUI.DisabledGroupScope( true ) ) {
                EditorGUILayout.ObjectField( t, typeof( MizoresPackageExporter ), false );
            }
            Undo.RecordObject( t, ExporterTexts.t_Undo );

            ExporterUtils.SeparateLine( );

            // ↓ Objects
            if ( ExporterUtils.EditorPrefFoldout(
                Const.EDITOR_PREF_FOLDOUT_OBJECT,
                string.Format( ExporterTexts.t_Objects, t.objects.Count ),
                ( objectReferences ) => Filter<Object>( objectReferences ),
                ( objectReferences ) => AddObjects<Object>( t, t.objects, objectReferences )
                ) ) {
                SingleGUIElement_PackagePrefsElementList.Draw<Object>( t, t.objects );
            }
            // ↑ Objects

            // ↓ Dynamic Path
            SingleGUI_DynamicPath.Draw( ed, t );
            // ↑ Dynamic Path

            ExporterUtils.SeparateLine( );

            // ↓ References
            if ( ExporterUtils.EditorPrefFoldout(
                Const.EDITOR_PREF_FOLDOUT_REFERENCES,
                string.Format( ExporterTexts.t_References, t.references.Count ),
                ( objectReferences ) => Filter<Object>( objectReferences ),
                ( objectReferences ) => AddObjects<Object>( t, t.references, objectReferences )
                ) ) {
                SingleGUIElement_PackagePrefsElementList.Draw<Object>( t, t.references );
            }
            // ↑ References

            ExporterUtils.SeparateLine( );

            // ↓ Exclude Objects
            if ( ExporterUtils.EditorPrefFoldout(
                Const.EDITOR_PREF_FOLDOUT_EXCLUDE_OBJECTS,
                string.Format( ExporterTexts.t_ExcludeObjects, t.excludeObjects.Count ),
                ( objectReferences ) => Filter<Object>( objectReferences ),
                ( objectReferences ) => AddObjects<Object>( t, t.excludeObjects, objectReferences )
                ) ) {
                SingleGUIElement_PackagePrefsElementList.Draw<Object>( t, t.excludeObjects );
            }
            // ↑ Exclude Objects

            // ↓ Excludes
            SingleGUI_Excludes.Draw( t );
            // ↑ Excludes

            ExporterUtils.SeparateLine( );
            // ↓ Dynamic Path Variables
            SingleGUI_DynamicPathVariables.Draw( t );
            // ↑ Dynamic Path Variables
            ExporterUtils.SeparateLine( );

            // ↓ Version File
            SingleGUI_VersionFile.Draw( t );
            // ↑ Version File

            ExporterUtils.SeparateLine( );

            // ExportPackage
            SingleGUI_ExportPackage.Draw( t );
        }
    }
#endif
}
