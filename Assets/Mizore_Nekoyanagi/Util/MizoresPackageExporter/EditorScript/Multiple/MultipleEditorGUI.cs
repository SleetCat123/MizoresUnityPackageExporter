using UnityEngine;
using System.Linq;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.MizoresPackageExporterConsts;
using static MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterUtils;
using MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.MultipleEditor
{

#if UNITY_EDITOR
    public static class MultipleEditorGUI
    {
        /// <summary>
        /// 複数オブジェクトの編集
        /// </summary>
        public static void EditMultiple( MizoresPackageExporterEditor ed ) {
            var targets = ed.targets;
            var t = ed.t;

            if ( targets.Length <= 1 ) return;
            Undo.RecordObjects( targets, ExporterTexts.t_Undo );

            var targetlist = targets.Select( v => v as MizoresPackageExporter );

            // Targets
            GUI.enabled = false;
            foreach ( var item in targetlist ) {
                EditorGUILayout.ObjectField( item, typeof( MizoresPackageExporter ), false );
            }
            GUI.enabled = true;

            ExporterUtils.SeparateLine( );

            // ↓ Objects
            MinMax objects_count = MinMax.Create( targetlist, v => v.objects.Count );
            if ( ExporterUtils.EditorPrefFoldout( 
                Const.EDITOR_PREF_FOLDOUT_OBJECT,
                string.Format( ExporterTexts.t_Objects, objects_count.GetRangeString( ) ),
                new FoldoutFuncs( ) {
                    onRightClick = ( ) => MultipleGUIElement_CopyPaste.OnRightClickFoldout<PackagePrefsElement>( targetlist, ExporterTexts.t_Objects, ( ex, list ) => ex.objects = list )
                }
                ) ) {
                MultipleGUIElement_PackagePrefsElementList.Draw<Object>( t, targetlist, ( v ) => v.objects );
            }
            // ↑ Objects

            // ↓ Dynamic Path
            MultipleGUI_DynamicPath.Draw( ed, t, targetlist );
            // ↑ Dynamic Path

            ExporterUtils.SeparateLine( );
            // ↓ References
            MinMax references_count = MinMax.Create( targetlist, v => v.references.Count );
            if ( ExporterUtils.EditorPrefFoldout( 
                Const.EDITOR_PREF_FOLDOUT_REFERENCES,
                string.Format( ExporterTexts.t_References, references_count.GetRangeString( ) ),
                new FoldoutFuncs( ) {
                    onRightClick = ( ) => MultipleGUIElement_CopyPaste.OnRightClickFoldout<PackagePrefsElement>( targetlist, ExporterTexts.t_References, ( ex, list ) => ex.references = list )
                }
                ) ) {
                MultipleGUIElement_PackagePrefsElementList.Draw<Object>( t, targetlist, ( v ) => v.references );
            }
            // ↑ References

            ExporterUtils.SeparateLine( );

            // ↓ Exclude Objects
            MinMax excludeObjects_count = MinMax.Create( targetlist, v => v.excludeObjects.Count );
            if ( ExporterUtils.EditorPrefFoldout(
                Const.EDITOR_PREF_FOLDOUT_EXCLUDE_OBJECTS,
                string.Format( ExporterTexts.t_ExcludeObjects, excludeObjects_count.GetRangeString( ) ),
                new FoldoutFuncs( ) {
                    onRightClick = ( ) => MultipleGUIElement_CopyPaste.OnRightClickFoldout<PackagePrefsElement>( targetlist, ExporterTexts.t_ExcludeObjects, ( ex, list ) => ex.excludeObjects = list )
                }
                ) ) {
                MultipleGUIElement_PackagePrefsElementList.Draw<Object>( t, targetlist, ( v ) => v.excludeObjects );
            }
            // ↑ Exclude Objects

            // ↓ Excludes
            MultipleGUI_Excludes.Draw( ed, t, targetlist );
            // ↑ Excludes

            ExporterUtils.SeparateLine( );

            // ↓ Version File
            GUI_VersionFile.Draw( t, targetlist );
            // ↑ Version File

            ExporterUtils.SeparateLine( );

            // ExportPackage
            MultipleGUI_ExportPackage.Draw( ed, targetlist );
        }
    }
#endif
}
