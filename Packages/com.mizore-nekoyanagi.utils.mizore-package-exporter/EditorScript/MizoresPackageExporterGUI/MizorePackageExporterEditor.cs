using UnityEngine;
using System.Linq;
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

        GUI_Objects gui_Objects;
        GUI_ExcludeObjects gui_ExcludeObjects;

        private void OnEnable( ) {
            t = target as MizoresPackageExporter;
            gui_Objects = new GUI_Objects( );
            gui_ExcludeObjects = new GUI_ExcludeObjects( );
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

            // デフォルトで相対パスを使用するか
            EditorGUI.BeginChangeCheck( );
            bool useRelativePath = EditorGUILayout.Toggle( ExporterTexts.UseRelativePath, ExporterEditorPrefs.UseRelativePath );
            if ( EditorGUI.EndChangeCheck( ) ) {
                ExporterEditorPrefs.UseRelativePath = useRelativePath;
            }

            // 上級者向け
            EditorGUI.BeginChangeCheck( );
            bool advanced = EditorGUILayout.Toggle( ExporterTexts.AdvancedMode, ExporterEditorPrefs.AdvancedMode);
            if ( EditorGUI.EndChangeCheck( ) ) {
                ExporterEditorPrefs.AdvancedMode = advanced;
            }
            if ( advanced ) {
                // PostProcessScriptを使用するか
                EditorGUI.BeginChangeCheck( );
                EditorGUI.indentLevel++;
                bool usePostProcessScript = EditorGUILayout.Toggle( ExporterTexts.UsePostProcessScript, ExporterEditorPrefs.UsePostProcessScript );
                EditorGUI.indentLevel--;
                if ( EditorGUI.EndChangeCheck( ) ) {
                    // trueにするときは確認メッセージを出す
                    if ( usePostProcessScript ) {
                        if ( !EditorUtility.DisplayDialog( ExporterTexts.UsePostProcessScript, ExporterTexts.UsePostProcessScriptConfirm, ExporterTexts.Yes, ExporterTexts.No ) ) {
                            usePostProcessScript = false;
                        }
                    }
                    ExporterEditorPrefs.UsePostProcessScript = usePostProcessScript;
                }
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
            Undo.RecordObjects( targets, ExporterTexts.Undo );

            var targetlist = targets.Select( v => v as MizoresPackageExporter ).ToArray( );

            foreach ( var item in targetlist ) {
                if ( string.IsNullOrEmpty( item.name ) ) {
                    EditorGUILayout.HelpBox( ExporterTexts.ErrorEmptyName, MessageType.Error );
                    return;
                }
            }

            // Targets
            GUI.enabled = false;
            foreach ( var item in targetlist ) {
                EditorGUILayout.ObjectField( item, typeof( MizoresPackageExporter ), false );
            }
            GUI.enabled = true;

            ExporterUtils.SeparateLine( );

            gui_Objects.Draw( this, t, targetlist );
            GUI_DynamicPath.Draw( this, t, targetlist );

            ExporterUtils.SeparateLine( );

            GUI_ReferencesObjects.Draw( this, t, targetlist );

            ExporterUtils.SeparateLine( );

            gui_ExcludeObjects.Draw( this, t, targetlist );
            GUI_Excludes.Draw( this, t, targetlist );

            if ( targets.Length == 1 ) {
                ExporterUtils.SeparateLine( );
                SingleGUI_DynamicPathVariables.Draw( this, t );
            } else {
                ExporterUtils.SeparateLine( );
                EditorGUILayout.HelpBox( ExporterTexts.EditOnlySingle( ExporterTexts.Variables ), MessageType.Info );
            }

            ExporterUtils.SeparateLine( );
            GUI_BatchExporter.Draw( this, t, targetlist );

            if ( ExporterEditorPrefs.UsePostProcessScript ) {
                ExporterUtils.SeparateLine( );
                GUI_PostProcessScript.Draw( this, t, targetlist );
            }

            // ExportPackage
            ExporterUtils.SeparateLine( );
            GUI_ExportPackage.Draw( this, targetlist );
            //

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
