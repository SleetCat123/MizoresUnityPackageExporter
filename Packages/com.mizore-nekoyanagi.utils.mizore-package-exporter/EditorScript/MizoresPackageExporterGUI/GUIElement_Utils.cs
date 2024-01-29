using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
#if UNITY_EDITOR
    public class VerticalBoxScope : EditorGUILayout.VerticalScope {
        public VerticalBoxScope( ) : base( GUI.skin.box ) { }
        public VerticalBoxScope( params GUILayoutOption[] options ) : base( GUI.skin.box, options ) { }
        public static Rect BeginVerticalBox( ) {
            return EditorGUILayout.BeginVertical( GUI.skin.box );
        }
    }
    public static class GUIElement_Utils {
        public enum BrowseType {
            None = 0,
            File = 1 << 0,
            Folder = 1 << 1,
            FileAndFolder = File | Folder,
        }
        public static bool BrowseButtons( MizoresPackageExporter t, string path, out string result, BrowseType browseType, string fileExtension = null, bool forceAbsolute = false ) {
            result = path;
            if ( browseType == BrowseType.None ) {
                return false;
            }
            bool browse = false;
            bool folder = false;
            float width = 30;
            float height = 20;
            if ( browseType.HasFlag( BrowseType.Folder ) ) {
                var folderContent = new GUIContent( IconCache.FolderIcon, ExporterTexts.ButtonFolder );
                if ( GUILayout.Button( folderContent, GUILayout.Width( width ), GUILayout.Height( height ) ) ) {
                    browse = true;
                    folder = true;
                }
            }
            if ( browseType.HasFlag( BrowseType.File ) ) {
                var fileContent = new GUIContent( IconCache.FileIcon, ExporterTexts.ButtonFile );
                if ( GUILayout.Button( fileContent, GUILayout.Width( width ), GUILayout.Height( height ) ) ) {
                    browse = true;
                    folder = false;
                }
            }
            if ( browse ) {
                if ( t != null ) {
                    path = t.ConvertDynamicPath( path );
                    if ( PathUtils.IsRelativePath( path ) ) {
                        var dir = t.GetDirectoryPath( );
                        path = PathUtils.GetProjectAbsolutePath( dir, path );
                    }
                }
                if ( folder ) {
                    if ( !Directory.Exists( path ) ) {
                        path = t.GetDirectoryPath( );
                    }
                    path = EditorUtility.OpenFolderPanel( null, path, null );
                } else {
                    if ( !File.Exists( path ) ) {
                        path = t.GetDirectoryPath( );
                    }
                    path = EditorUtility.OpenFilePanel( null, path, fileExtension );
                }
                if ( string.IsNullOrEmpty( path ) == false ) {
                    path = PathUtils.ToValidPath( path );
                    if ( ExporterEditorPrefs.UseRelativePath && !forceAbsolute ) {
                        var dir = t.GetDirectoryPath( );
                        path = PathUtils.GetRelativePath( dir, path );
                    }
                    GUI.changed = true;
                    //EditorUtility.SetDirty( t );
                    result = path;
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
#if UNITY_EDITOR
            return GUILayout.Button( IconCache.RemoveIconContent, GUILayout.Width( 20 ), GUILayout.Height( 20 ) );
#else
            return false;
#endif
        }
        public static bool PlusButton( ) {
#if UNITY_EDITOR
            return GUILayout.Button( IconCache.AddIconContent, GUILayout.Width( 40 ), GUILayout.Height( 20 ) );
#else
            return false;
#endif
        }
    }
#endif
}
