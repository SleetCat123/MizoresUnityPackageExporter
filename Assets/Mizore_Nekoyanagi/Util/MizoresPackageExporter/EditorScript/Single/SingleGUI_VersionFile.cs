using UnityEngine;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.MizoresPackageExporterConsts;
#if UNITY_EDITOR
using UnityEditor;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.SingleEditor
{
    public static class SingleGUI_VersionFile
    {
        public static void Draw( MizoresPackageExporter t ) {
            if ( ExporterUtils.EditorPrefFoldout(
                Const.EDITOR_PREF_FOLDOUT_VERSIONFILE, ExporterTexts.t_VersionFile ) ) {
                using ( new EditorGUILayout.HorizontalScope( ) ) {
                    ExporterUtils.Indent( 1 );
                    EditorGUI.BeginChangeCheck( );
                    t.versionFile.Object = EditorGUILayout.ObjectField( t.versionFile.Object, typeof( TextAsset ), false );
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        t.UpdateExportVersion( );
                        EditorUtility.SetDirty( t );
                    }
                    EditorGUI.BeginChangeCheck( );
                    string path = t.versionFile.Path;
                    path = EditorGUILayout.TextField( path );
                    if ( EditorGUI.EndChangeCheck( ) ) {
                        // パスが変更されたらオブジェクトを置き換える
                        Object o = AssetDatabase.LoadAssetAtPath<TextAsset>( path );
                        if ( o != null ) {
                            t.versionFile.Object = o;
                            t.UpdateExportVersion( );
                        }
                        EditorUtility.SetDirty( t );
                    }
                }
                EditorGUI.BeginChangeCheck( );
                using ( new EditorGUILayout.HorizontalScope( ) ) {
                    ExporterUtils.Indent( 1 );
                    t.versionFormat = EditorGUILayout.TextField( ExporterTexts.t_VersionFormat, t.versionFormat );
                }
                if ( EditorGUI.EndChangeCheck( ) ) {
                    EditorUtility.SetDirty( t );
                }
            }
            EditorGUI.BeginChangeCheck( );
            t.packageName = EditorGUILayout.TextField( ExporterTexts.t_PackageName, t.packageName );
            if ( EditorGUI.EndChangeCheck( ) ) {
                EditorUtility.SetDirty( t );
            }
        }
    }
}
#endif
