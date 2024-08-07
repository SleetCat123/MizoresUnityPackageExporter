#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#if UNITY_EDITOR
namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public static class PackagePrefsElementInspector {
        public static bool Draw<T>( MizoresPackageExporter t, ObjectRefElement element ) where T : Object {
            EditorGUILayout.BeginHorizontal( );

            Rect textrect = EditorGUILayout.GetControlRect( GUILayout.MinWidth( 30 ) );
            var prevElement = new ObjectRefElement( element );
            // 右クリックメニュー
            var ev = Event.current;
            if ( ev.type == EventType.ContextClick && textrect.Contains( ev.mousePosition ) ) {
                var menu = new GenericMenu( );
                menu.AddItem( new GUIContent( ExporterTexts.CopyText ), false, ( ) => EditorGUIUtility.systemCopyBuffer = element.Path );
                menu.AddItem( new GUIContent( ExporterTexts.PasteText ), false, ( ) => element.SetPath( EditorGUIUtility.systemCopyBuffer ) );
                menu.AddSeparator( "" );
                if ( element.IsGUID ) {
                    menu.AddItem( new GUIContent( ExporterTexts.ConvertToAbsolutePath ), false, ( ) => {
                        var newPath = AssetDatabase.GUIDToAssetPath( element.Path );
                        newPath = PathUtils.ToValidPath( newPath );
                        element.SetPath( newPath );
                        ExporterUtils.DebugLog( "Path changed: " + prevElement + " -> " + element );
                        GUI.changed = true;
                    } );
                    menu.AddItem( new GUIContent( ExporterTexts.ConvertToRelativePath ), false, ( ) => {
                        var newPath = AssetDatabase.GUIDToAssetPath( element.Path );
                        newPath = PathUtils.GetRelativePath( t.GetDirectoryPath( ), newPath );
                        newPath = PathUtils.ToValidPath( newPath );
                        element.SetPath( newPath );
                        ExporterUtils.DebugLog( "Path changed: " + prevElement + " -> " + element );
                        GUI.changed = true;
                    } );
                    menu.AddDisabledItem( new GUIContent( ExporterTexts.ConvertToGUID ) );
                } else {
                    if ( PathUtils.IsRelativePath( element.Path ) ) {
                        menu.AddItem( new GUIContent( ExporterTexts.ConvertToAbsolutePath ), false, ( ) => {
                            var newPath = PathUtils.GetProjectAbsolutePath( t.GetDirectoryPath( ), element.Path );
                            newPath = PathUtils.ToValidPath( newPath );
                            element.SetPath( newPath );
                            ExporterUtils.DebugLog( "Path changed: " + prevElement + " -> " + element );
                            GUI.changed = true;
                        } );
                        menu.AddDisabledItem( new GUIContent( ExporterTexts.ConvertToRelativePath ) );
                    } else {
                        menu.AddDisabledItem( new GUIContent( ExporterTexts.ConvertToAbsolutePath ) );
                        menu.AddItem( new GUIContent( ExporterTexts.ConvertToRelativePath ), false, ( ) => {
                            var newPath = PathUtils.GetRelativePath( t.GetDirectoryPath( ), element.Path );
                            newPath = PathUtils.ToValidPath( newPath );
                            element.SetPath( newPath );
                            ExporterUtils.DebugLog( "Path changed: " + prevElement + " -> " + element );
                            GUI.changed = true;
                        } );
                    }
                    menu.AddItem( new GUIContent( ExporterTexts.ConvertToGUID ), false, ( ) => {
                        var newPath = AssetDatabase.AssetPathToGUID( element.GetConvertedPath( t ) );
                        element.SetGUID( newPath );
                        ExporterUtils.DebugLog( "Path changed: " + prevElement + " -> " + element );
                        GUI.changed = true;
                    } );
                }

                menu.ShowAsContext( );
                ev.Use( );
            }
            using ( new EditorGUI.DisabledScope( element.IsGUID ) ) {
                EditorGUI.BeginChangeCheck( );
                var path = EditorGUI.TextField( textrect, element.Path );
                path = PathUtils.ToValidPath( path );
                if ( EditorGUI.EndChangeCheck( ) ) {
                    element.SetPath( path );
                    ExporterUtils.DebugLog( "Path changed: " + prevElement + " -> " + element );
                    GUI.changed = true;
                }
            }
            // ドラッグドロップ
            if ( ExporterUtils.DragDrop( textrect, ExporterUtils.Filter_HasPersistentObject ) ) {
                var path = AssetDatabase.GetAssetPath( DragAndDrop.objectReferences[0] );
                path = PathUtils.ToValidPath( path );
                switch ( ExporterEditorPrefs.DefaultPathType ) {
                    case PathType.Absolute:
                        element.SetPath( path );
                        ExporterUtils.DebugLog( "Path changed: " + prevElement + " -> " + element );
                        GUI.changed = true;
                        break;
                    case PathType.Relative:
                        element.SetPath( PathUtils.GetRelativePath( t.GetDirectoryPath( ), path ) );
                        ExporterUtils.DebugLog( "Path changed: " + prevElement + " -> " + element );
                        GUI.changed = true;
                        break;
                    case PathType.GUID:
                        element.SetGUID( AssetDatabase.AssetPathToGUID( path ) );
                        ExporterUtils.DebugLog( "Path changed: " + prevElement + " -> " + element );
                        GUI.changed = true;
                        break;
                }
            }

            EditorGUI.BeginChangeCheck( );
            var obj = element.GetObject( t );
            obj = EditorGUILayout.ObjectField( obj, typeof( T ), false, GUILayout.MinWidth( 30 ), GUILayout.MaxWidth( 100 ) );
            if ( EditorGUI.EndChangeCheck( ) ) {
                element.SetPathAutoDetect( t, obj );
                GUI.changed = true;
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
            bool browse = GUIElement_Utils.BrowseButtons( t, ref element, browseType, fileExtension, forceAbsolute: true );
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
