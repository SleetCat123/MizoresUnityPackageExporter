using UnityEngine;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.MizoresPackageExporterConsts;
using System.Linq;
using System.Collections.Generic;
using MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor;
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
        public static void EditSingle( MizoresPackageExporterEditor ed ) {
            var t = ed.t;
            var targetlist = ed.targets.Select( v => v as MizoresPackageExporter );

            if ( t.debugmode ) {
                EditorGUILayout.LabelField( "[DEBUG]" );
            }

            using ( var s = new EditorGUI.DisabledGroupScope( true ) ) {
                EditorGUILayout.ObjectField( t, typeof( MizoresPackageExporter ), false );
            }
            Undo.RecordObject( t, ExporterTexts.t_Undo );

            ExporterUtils.SeparateLine( );

            // ↓ Objects
            if ( ExporterUtils.EditorPrefFoldout(
                Const.EDITOR_PREF_FOLDOUT_OBJECT,
                string.Format( ExporterTexts.t_Objects, t.objects.Count ),
                new ExporterUtils.FoldoutFuncs( ) {
                    canDragDrop = ( objectReferences ) => Filter<Object>( objectReferences ),
                    onDragPerform = ( objectReferences ) => AddObjects<Object>( t, t.objects, objectReferences ),
                    onRightClick = ( ) => SingleGUIElement_CopyPaste.OnRightClickFoldout( t, ExporterTexts.t_Objects, t.objects, ( list ) => t.objects = list )
                }
                ) ) {
                SingleGUIElement_PackagePrefsElementList.Draw<Object>( t, t.objects );
            }
            // ↑ Objects

            // ↓ Dynamic Path
            GUI_DynamicPath.Draw( ed, t, targetlist );
            // ↑ Dynamic Path

            ExporterUtils.SeparateLine( );

            // ↓ References
            if ( ExporterUtils.EditorPrefFoldout(
                Const.EDITOR_PREF_FOLDOUT_REFERENCES,
                string.Format( ExporterTexts.t_References, t.references.Count ),
                new ExporterUtils.FoldoutFuncs( ) {
                    canDragDrop = ( objectReferences ) => Filter<Object>( objectReferences ),
                    onDragPerform = ( objectReferences ) => AddObjects<Object>( t, t.references, objectReferences ),
                    onRightClick = ( ) => SingleGUIElement_CopyPaste.OnRightClickFoldout( t, ExporterTexts.t_References, t.references, ( list ) => t.references = list )
                }
                ) ) {
                SingleGUIElement_PackagePrefsElementList.Draw<Object>( t, t.references );
            }
            // ↑ References

            ExporterUtils.SeparateLine( );

            // ↓ Exclude Objects
            if ( ExporterUtils.EditorPrefFoldout(
                Const.EDITOR_PREF_FOLDOUT_EXCLUDE_OBJECTS,
                string.Format( ExporterTexts.t_ExcludeObjects, t.excludeObjects.Count ),
                 new ExporterUtils.FoldoutFuncs( ) {
                     canDragDrop = ( objectReferences ) => Filter<Object>( objectReferences ),
                     onDragPerform = ( objectReferences ) => AddObjects<Object>( t, t.excludeObjects, objectReferences ),
                     onRightClick = ( ) => SingleGUIElement_CopyPaste.OnRightClickFoldout( t, ExporterTexts.t_ExcludeObjects, t.excludeObjects, ( list ) => t.excludeObjects = list )
                 }
                ) ) {
                SingleGUIElement_PackagePrefsElementList.Draw<Object>( t, t.excludeObjects );
            }
            // ↑ Exclude Objects

            // ↓ Excludes
            GUI_Excludes.Draw( ed, t, targetlist );
            // ↑ Excludes

            ExporterUtils.SeparateLine( );
            // ↓ Dynamic Path Variables
            SingleGUI_DynamicPathVariables.Draw( ed, t );
            // ↑ Dynamic Path Variables
            ExporterUtils.SeparateLine( );

            // ↓ Version File
            GUI_VersionFile.Draw( t, targetlist );
            // ↑ Version File

            ExporterUtils.SeparateLine( );

            // ExportPackage
            GUI_ExportPackage.Draw( ed, targetlist );
        }
    }
}
#endif
