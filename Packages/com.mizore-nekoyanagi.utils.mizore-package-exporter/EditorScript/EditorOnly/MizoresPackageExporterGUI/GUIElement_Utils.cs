using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {

#if UNITY_EDITOR
    public static class GUIElement_Utils {
        public enum BrowseType {
            None = 0,
            File = 1 << 0,
            Folder = 1 << 1,
            FileAndFolder = File | Folder,
        }
        public static bool BrowseButtons( MizoresPackageExporter t, ref ObjectRefElement element, BrowseType browseType, string fileExtension = null, bool forceAbsolute = false ) {
            if ( browseType == BrowseType.None ) {
                return false;
            }
            bool browse = false;
            bool folder = false;
            float minWidth = 25;
            float width = 30;
            float height = 20;
            if ( browseType.HasFlag( BrowseType.Folder ) ) {
                var folderContent = new GUIContent( IconCache.FolderIcon, ExporterTexts.ButtonFolder );
                if ( GUILayout.Button( folderContent, GUILayout.MinWidth( minWidth ), GUILayout.MaxWidth( width ), GUILayout.Height( height ) ) ) {
                    browse = true;
                    folder = true;
                }
            }
            if ( browseType.HasFlag( BrowseType.File ) ) {
                var fileContent = new GUIContent( IconCache.FileIcon, ExporterTexts.ButtonFile );
                if ( GUILayout.Button( fileContent, GUILayout.MinWidth( minWidth ), GUILayout.MaxWidth( width ), GUILayout.Height( height ) ) ) {
                    browse = true;
                    folder = false;
                }
            }
            if ( browse ) {
                var path = element.GetConvertedPath( t );
                if ( folder ) {
                    if ( string.IsNullOrEmpty( path ) ) {
                        path = t.GetDirectoryPath( );
                    } else {
                        if ( File.Exists( path ) ) {
                            path = Path.GetDirectoryName( path );
                        } else if ( !Directory.Exists( path ) ) {
                            path = Path.GetDirectoryName( path );
                            if ( !Directory.Exists( path ) ) {
                                path = t.GetDirectoryPath( );
                            }
                        }
                    }
                    path = EditorUtility.OpenFolderPanel( null, path, null );
                } else {
                    if ( string.IsNullOrEmpty( path ) ) {
                        path = t.GetDirectoryPath( );
                    } else {
                        if ( !File.Exists( path ) ) {
                            path = Path.GetDirectoryName( path );
                            if ( !Directory.Exists( path ) ) {
                                path = t.GetDirectoryPath( );
                            }
                        }
                    }
                    path = EditorUtility.OpenFilePanel( null, path, fileExtension );
                }
                if ( string.IsNullOrEmpty( path ) == false ) {
                    path = PathUtils.ToValidPath( path );
                    var type = ExporterEditorPrefs.DefaultPathType;
                    if ( forceAbsolute ) {
                        type = PathType.Absolute;
                    }
                    var prevElement = new ObjectRefElement( element );
                    switch ( type ) {
                        case PathType.Absolute:
                            element.SetPath( path );
                            break;
                        case PathType.Relative:
                            element.SetPath( PathUtils.GetRelativePath( t.GetDirectoryPath( ), path ) );
                            break;
                        case PathType.GUID:
                            element.SetGUID( AssetDatabase.GUIDToAssetPath( path ) );
                            break;
                    }
                    ExporterUtils.DebugLog( "Path changed: " + prevElement + " -> " + element );
                    EditorUtility.SetDirty( t );
                }
            }
            return browse;
        }

        public static int UpDownButton( int index, int listLength, int buttonWidth = 15 ) {
            index = Mathf.Clamp( index, 0, listLength - 1 );
#if UNITY_EDITOR
            var w = GUILayout.Width( buttonWidth );
            using ( var scope = new EditorGUI.DisabledGroupScope( index == 0 ) ) {
                if ( GUILayout.Button( "↑", w ) ) {
                    index = index - 1;
                }
            }
            using ( var scope = new EditorGUI.DisabledGroupScope( index == listLength - 1 ) ) {
                if ( GUILayout.Button( "↓", w ) ) {
                    index = index + 1;
                }
            }
#endif
            return index;
        }
        public static bool MinusButton( ) {
            return GUILayout.Button( IconCache.RemoveIconContent, GUILayout.Width( 20 ), GUILayout.Height( 20 ) );
        }
        public static bool PlusButton( ) {
            var rect = EditorGUILayout.GetControlRect( GUILayout.Height( 17 ) );
            rect = EditorGUI.IndentedRect( rect );
            rect.width = 40;
            return GUI.Button( rect, IconCache.AddIconContent );
        }
    }
#endif
}
