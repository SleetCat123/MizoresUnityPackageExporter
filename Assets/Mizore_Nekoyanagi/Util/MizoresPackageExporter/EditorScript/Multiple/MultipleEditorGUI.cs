using UnityEngine;
using System.Linq;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.MizoresPackageExporterConsts;
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
        public static void EditMultiple( UnityPackageExporterEditor ed ) {
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
            if ( ExporterUtils.EditorPrefFoldout( Const.EDITOR_PREF_FOLDOUT_OBJECT, ExporterTexts.t_Objects ) ) {
                MultipleGUIElement_PackagePrefsElementList.Draw<Object>( t, targetlist, ( v ) => v.objects );
            }
            // ↑ Objects

            // ↓ Dynamic Path
            MultipleGUI_DynamicPath.Draw( ed, t, targetlist );
            // ↑ Dynamic Path

            ExporterUtils.SeparateLine( );
            // ↓ References
            if ( ExporterUtils.EditorPrefFoldout( Const.EDITOR_PREF_FOLDOUT_REFERENCES, ExporterTexts.t_References ) ) {
                MultipleGUIElement_PackagePrefsElementList.Draw<DefaultAsset>( t, targetlist, ( v ) => v.references );
            }
            // ↑ References

            ExporterUtils.SeparateLine( );

            // ↓ Exclude Objects
            if ( ExporterUtils.EditorPrefFoldout( Const.EDITOR_PREF_FOLDOUT_EXCLUDE_OBJECTS, ExporterTexts.t_ExcludeObjects ) ) {
                MultipleGUIElement_PackagePrefsElementList.Draw<Object>( t, targetlist, ( v ) => v.excludeObjects );
            }
            // ↑ Exclude Objects
            ExporterUtils.SeparateLine( );

            // ↓ Version File
            MultipleGUI_VersionFile.Draw( t, targetlist );
            // ↑ Version File

            ExporterUtils.SeparateLine( );

            // ExportPackage
            MultipleGUI_ExportPackage.Draw( t, targetlist );
        }
    }
#endif
}
