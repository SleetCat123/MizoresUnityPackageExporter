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
            // デバッグモード
            EditorGUI.BeginChangeCheck( );
            bool debugmode = EditorGUILayout.Toggle( "Debug Mode", EditorPrefsCache.GetBool( ExporterConsts_Editor.EDITOR_PREF_DEBUG, false ) );
            if ( EditorGUI.EndChangeCheck( ) ) {
                EditorPrefsCache.SetBool( ExporterConsts_Editor.EDITOR_PREF_DEBUG, debugmode );
            }

            ExporterUtils.SeparateLine( );

            // 言語選択
            EditorGUI.BeginChangeCheck( );
            int languageIndex = System.Array.IndexOf( ExporterTexts.LanguageList, EditorPrefsCache.GetString( ExporterConsts_Editor.EDITOR_PREF_LANGUAGE, ExporterTexts.DEFAULT_KEY ) );
            if ( languageIndex == -1 ) {
                languageIndex = 0;
                EditorPrefsCache.SetString( ExporterConsts_Editor.EDITOR_PREF_LANGUAGE, ExporterTexts.DEFAULT_KEY );
            }
            using ( new GUILayout.HorizontalScope( ) ) {
                languageIndex = EditorGUILayout.Popup( "Language", languageIndex, ExporterTexts.LanguageList );
                if ( EditorPrefsCache.GetBool( ExporterConsts_Editor.EDITOR_PREF_DEBUG ) ) {
                    if ( GUILayout.Button( "Reload", GUILayout.Width( 60 ) ) ) {
                        ExporterTexts.Clear( );
                    }
                }
            }
            if ( EditorGUI.EndChangeCheck( ) ) {
                EditorPrefsCache.SetString( ExporterConsts_Editor.EDITOR_PREF_LANGUAGE, ExporterTexts.LanguageList[languageIndex] );
            }

            ExporterUtils.SeparateLine( );
            
            // Exporterのファイルバージョンの互換性チェック
            foreach ( var item in targets ) {
                var exporter = item as MizoresPackageExporter;
                if ( !exporter.IsCompatible ) {
                    EditorGUILayout.HelpBox( string.Format( ExporterTexts.t_IncompatibleVersion, exporter.name ), MessageType.Error );
                    if ( GUILayout.Button( ExporterTexts.t_IncompatibleVersionForceOpen ) ) {
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

            // エディタ本体
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
