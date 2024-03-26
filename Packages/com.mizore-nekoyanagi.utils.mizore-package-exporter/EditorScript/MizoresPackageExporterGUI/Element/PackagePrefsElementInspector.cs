#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#if UNITY_EDITOR
namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public static class PackagePrefsElementInspector {
        public static bool Draw<T>( MizoresPackageExporter t, ObjectRefElement element ) where T : Object {
            element.exporter = t;
            EditorGUILayout.BeginHorizontal( );

            Rect textrect = EditorGUILayout.GetControlRect( );
            string path = element.Path;
            string prevPath = path;
            // 右クリックメニュー
            var ev = Event.current;
            if ( ev.type == EventType.ContextClick && textrect.Contains( ev.mousePosition ) ) {
                var menu = new GenericMenu( );
                menu.AddItem( new GUIContent( ExporterTexts.CopyText ), false, ( ) => EditorGUIUtility.systemCopyBuffer = path );
                menu.AddItem( new GUIContent( ExporterTexts.PasteText ), false, ( ) => path = EditorGUIUtility.systemCopyBuffer );
                menu.AddSeparator( "" );
                if ( PathUtils.IsRelativePath( path ) ) {
                    menu.AddItem( new GUIContent( ExporterTexts.ConvertToAbsolutePath ), false, ( ) => path = PathUtils.GetProjectAbsolutePath( t.GetDirectoryPath( ), path ) );
                } else {
                    menu.AddItem( new GUIContent( ExporterTexts.ConvertToRelativePath ), false, ( ) => path = PathUtils.GetRelativePath( t.GetDirectoryPath( ), path ) );
                }

                menu.ShowAsContext( );
                ev.Use( );
            }
            path = EditorGUI.TextField( textrect, path );
            if ( ExporterUtils.DragDrop( textrect, ExporterUtils.Filter_HasPersistentObject ) ) {
                path = AssetDatabase.GetAssetPath( DragAndDrop.objectReferences[0] );
            }

            EditorGUI.BeginChangeCheck( );
            var obj = EditorGUILayout.ObjectField( element.Object, typeof( T ), false );
            if ( EditorGUI.EndChangeCheck( ) ) {
                element.exporter = t;
                element.SetObject( obj );
            }

            GUIElement_Utils.BrowseType browseType;
            string fileExtension = null;
            var type = typeof( T );
            if ( type == typeof( DefaultAsset ) ) {
                browseType = GUIElement_Utils.BrowseType.Folder;
            } else if ( type == typeof( TextAsset ) ) {
                browseType = GUIElement_Utils.BrowseType.File;
                fileExtension = "txt,json,csv";
            } else if ( type == typeof( Object ) ) {
                browseType = GUIElement_Utils.BrowseType.FileAndFolder;
            } else {
                browseType = GUIElement_Utils.BrowseType.None;
            }
            bool browse = GUIElement_Utils.BrowseButtons( t, path, out string resultPath,
                browseType,
                fileExtension,
                forceAbsolute: true
                );
            if ( browse ) {
                path = resultPath;
            }

            if ( prevPath != path ) {
                ExporterUtils.DebugLog( "Path changed: " + prevPath + " -> " + path );
                path = PathUtils.ToValidPath( path );
                element.Path = path;
                GUI.changed = true;
            }
            if ( browse ) {
                // OpenFilePanelなどを使用した場合に以下のエラーが出るのでreturnして回避
                // 'EndLayoutGroup: BeginLayoutGroup must be called first.'
                return true;
            } else {
                EditorGUILayout.EndHorizontal( );
                return false;
            }
        }
    }
}
#endif