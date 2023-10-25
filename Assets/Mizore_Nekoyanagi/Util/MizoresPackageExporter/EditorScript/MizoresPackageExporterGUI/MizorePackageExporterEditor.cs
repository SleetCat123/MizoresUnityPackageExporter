using UnityEngine;
using System.Linq;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor {

#if UNITY_EDITOR
    [CustomEditor( typeof( MizoresPackageExporter ) ), CanEditMultipleObjects]
    public class MizoresPackageExporterEditor : Editor {
        public ExporterEditorLogs logs = new ExporterEditorLogs( );
        public string _variableKeyTemp = string.Empty;
        public MizoresPackageExporter t;

        private void OnEnable( ) {
            t = target as MizoresPackageExporter;
        }
        public override void OnInspectorGUI( ) {
            // デバッグモード
            EditorGUI.BeginChangeCheck( );
            bool debugmode = EditorGUILayout.Toggle( "Debug Mode", ExporterEditorPrefs.DebugMode );
            if ( EditorGUI.EndChangeCheck( ) ) {
                ExporterEditorPrefs.DebugMode = debugmode;
            }

            if ( debugmode ) {
                EditorGUI.BeginDisabledGroup( true );
                EditorGUILayout.ObjectField( MonoScript.FromScriptableObject( t ), typeof( MonoScript ), false );
                EditorGUI.EndDisabledGroup( );
            }

            ExporterUtils.SeparateLine( );

            // 言語選択
            EditorGUI.BeginChangeCheck( );
            int languageIndex = System.Array.IndexOf( ExporterTexts.LanguageList, ExporterEditorPrefs.Language );
            if ( languageIndex == -1 ) {
                languageIndex = 0;
                ExporterEditorPrefs.Language = ExporterTexts.DEFAULT_KEY;
            }
            using ( new GUILayout.HorizontalScope( ) ) {
                languageIndex = EditorGUILayout.Popup( "Language", languageIndex, ExporterTexts.LanguageList );
                if ( debugmode ) {
                    if ( GUILayout.Button( "Reload", GUILayout.Width( 60 ) ) ) {
                        ExporterTexts.Clear( );
                    }
                    if ( GUILayout.Button( "Open", GUILayout.Width( 50 ) ) ) {
                        var text = Resources.Load<TextAsset>( ExporterTexts.GetTextAssetResourcesPath( ) );
                        EditorUtility.RevealInFinder( AssetDatabase.GetAssetPath( text ) );
                    }
                }
            }
            if ( EditorGUI.EndChangeCheck( ) ) {
                ExporterEditorPrefs.Language = ExporterTexts.LanguageList[languageIndex];
            }

            ExporterUtils.SeparateLine( );

            // Exporterのファイルバージョンの互換性チェック
            foreach ( var item in targets ) {
                var exporter = item as MizoresPackageExporter;
                if ( !exporter.IsCompatible ) {
                    EditorGUILayout.HelpBox( ExporterTexts.IncompatibleVersion( exporter.name ), MessageType.Error );
                    if ( GUILayout.Button( ExporterTexts.IncompatibleVersionForceOpen ) ) {
                        // SetDirtyはしない
                        exporter.ConvertToCurrentVersion( force: true );
                    }
                    return;
                }
                bool converted = exporter.ConvertToCurrentVersion( );
                if ( converted ) {
                    EditorUtility.SetDirty( item );
                }
            }

            // エディタ本体
            MultipleEditor.MizoresPackageExporterEditorMain.EditMultiple( this );

            logs.DrawUI( );
        }
        public static void Export( ExporterEditorLogs logs, MizoresPackageExporter[] targets, HashSet<string> ignorePaths ) {
            logs.Clear( );
            for ( int i = 0; i < targets.Length; i++ ) {
                var item = targets[i];
                item.Export( logs, ignorePaths );
            }
        }
        public void Export( HashSet<string> ignorePaths ) {
            var targetlist = targets.Select( v => v as MizoresPackageExporter ).ToArray( );
            Export( logs, targetlist, ignorePaths );
        }
    }
#endif
}
