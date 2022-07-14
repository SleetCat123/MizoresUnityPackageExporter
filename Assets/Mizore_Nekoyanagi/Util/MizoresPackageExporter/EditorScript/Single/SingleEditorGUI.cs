using UnityEngine;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.MizoresPackageExporterConsts;
#if UNITY_EDITOR
using UnityEditor;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.SingleEditor
{
    public static class SingleEditorGUI
    {
        public static void EditSingle( UnityPackageExporterEditor ed ) {
            var t = ed.t;

            using ( var s = new EditorGUI.DisabledGroupScope( true ) ) {
                EditorGUILayout.ObjectField( t, typeof( MizoresPackageExporter ), false );
            }
            Undo.RecordObject( t, ExporterTexts.t_Undo );

            ExporterUtils.SeparateLine( );

            // ↓ Objects
            if ( ExporterUtils.EditorPrefFoldout( Const.EDITOR_PREF_FOLDOUT_OBJECT, ExporterTexts.t_Objects ) ) {
                SingleGUIElement_PackagePrefsElementList.Draw<Object>( t, t.objects );
            }
            // ↑ Objects

            // ↓ Dynamic Path
            SingleGUI_DynamicPath.Draw( ed, t );
            // ↑ Dynamic Path

            ExporterUtils.SeparateLine( );

            // ↓ References
            if ( ExporterUtils.EditorPrefFoldout( Const.EDITOR_PREF_FOLDOUT_REFERENCES, ExporterTexts.t_References ) ) {
                SingleGUIElement_PackagePrefsElementList.Draw<DefaultAsset>( t, t.references );
            }
            // ↑ References

            ExporterUtils.SeparateLine( );

            // ↓ Exclude Objects
            if ( ExporterUtils.EditorPrefFoldout( Const.EDITOR_PREF_FOLDOUT_EXCLUDE_OBJECTS, ExporterTexts.t_ExcludeObjects ) ) {
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
