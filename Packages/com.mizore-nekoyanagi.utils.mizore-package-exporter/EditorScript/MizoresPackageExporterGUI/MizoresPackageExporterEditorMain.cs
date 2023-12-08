using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor {

#if UNITY_EDITOR
    public static class MizoresPackageExporterEditorMain
    {
        /// <summary>
        /// 複数オブジェクトの編集
        /// </summary>
        public static void EditMultiple( MizoresPackageExporterEditor ed ) {
            var targets = ed.targets;
            var t = ed.t;

            Undo.RecordObjects( targets, ExporterTexts.Undo );

            var targetlist = targets.Select( v => v as MizoresPackageExporter ).ToArray( );

            // Targets
            GUI.enabled = false;
            foreach ( var item in targetlist ) {
                EditorGUILayout.ObjectField( item, typeof( MizoresPackageExporter ), false );
            }
            GUI.enabled = true;

            ExporterUtils.SeparateLine( );

            GUI_Objects.Draw( ed, t, targetlist );
            GUI_DynamicPath.Draw( ed, t, targetlist );

            ExporterUtils.SeparateLine( );
           
            GUI_ReferencesObjects.Draw( ed, t, targetlist );

            ExporterUtils.SeparateLine( );

            GUI_ExcludeObjects.Draw( ed, t, targetlist );
            GUI_Excludes.Draw( ed, t, targetlist );

            if ( targets.Length == 1 ) {
                ExporterUtils.SeparateLine( );
                SingleGUI_DynamicPathVariables.Draw( ed, t );
            }

            ExporterUtils.SeparateLine( );
            GUI_BatchExporter.Draw( ed, t, targetlist );

            // ExportPackage
            ExporterUtils.SeparateLine( );
            GUI_ExportPackage.Draw( ed, targetlist );
        }
    }
#endif
}
