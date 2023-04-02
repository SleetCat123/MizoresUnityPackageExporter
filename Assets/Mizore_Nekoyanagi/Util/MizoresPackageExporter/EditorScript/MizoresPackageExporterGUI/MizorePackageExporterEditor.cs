using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor
{

#if UNITY_EDITOR
    [CustomEditor( typeof( MizoresPackageExporter ) ), CanEditMultipleObjects]
    public class MizoresPackageExporterEditor : Editor
    {
        public ExporterEditorLogs logs = new ExporterEditorLogs( );
        public string _variableKeyTemp = string.Empty;
        public MizoresPackageExporter t;

        private void OnEnable( ) {
            t = target as MizoresPackageExporter;
        }
        public override void OnInspectorGUI( ) {
            foreach ( var item in targets ) {
                var exporter = item as MizoresPackageExporter;
                if ( !exporter.IsCompatible ) {
                    EditorGUILayout.HelpBox( string.Format( ExporterTexts.t_IncompatibleVersion, exporter.name ), MessageType.Error );
                    if ( GUILayout.Button( ExporterTexts.t_IncompatibleVersion_ForceOpen ) ) {
                        // SetDirtyはしない
                        exporter.ConvertToCurrentVersion( force: true );
                    }
                    return;
                }
                if ( !exporter.IsCurrentVersion ) {
                    exporter.ConvertToCurrentVersion( );
                    EditorUtility.SetDirty( exporter );
                }
            }
            MultipleEditor.MizoresPackageExporterEditorMain.EditMultiple( this );

            logs.DrawUI( );
        }
        public static void Export( ExporterEditorLogs logs, Object[] targets ) {
            var targetlist = targets.Select( v => v as MizoresPackageExporter ).ToArray( );
            Export( logs, targetlist );
        }
        public static void Export( ExporterEditorLogs logs, MizoresPackageExporter[] targets ) {
            logs.Clear( );
            for ( int i = 0; i < targets.Length; i++ ) {
                var item = targets[i];
                item.Export( logs );
            }
        }
        public void Export( ) {
            Export( logs, targets );
        }
    }
#endif
}
