﻿using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor {
#if UNITY_EDITOR
    public static class GUIElement_Utils {
        public static string BrowseButtons( MizoresPackageExporter t, string text ) {
            string result = text;
            bool browse = false;
            bool folder = false;
            float width = 30;
            float height = 20;
            var folderContent = new GUIContent( IconCache.FolderIcon, ExporterTexts.ButtonFolder );
            if ( GUILayout.Button( folderContent, GUILayout.Width( width ), GUILayout.Height( height ) ) ) {
                browse = true;
                folder = true;
            }
            var fileContent = new GUIContent( IconCache.FileIcon, ExporterTexts.ButtonFile );
            if ( GUILayout.Button( fileContent, GUILayout.Width( width ), GUILayout.Height( height ) ) ) {
                browse = true;
                folder = false;
            }
            if ( browse ) {
                text = t.ConvertDynamicPath( text );
                if ( PathUtils.IsRelativePath( text ) ) {
                    var dir = t.GetDirectoryPath( );
                    text = PathUtils.GetProjectAbsolutePath( dir, text );
                }
                if ( folder ) {
                    if ( !Directory.Exists( text ) ) {
                        text = t.GetDirectoryPath( );
                    }
                    text = EditorUtility.OpenFolderPanel( null, text, null );
                } else {
                    if ( !File.Exists( text ) ) {
                        text = t.GetDirectoryPath( );
                    }
                    text = EditorUtility.OpenFilePanel( null, text, null );
                }
                if ( string.IsNullOrEmpty( text ) == false ) {
                    text = PathUtils.ToValidPath( text );
                    if ( ExporterEditorPrefs.UseRelativePath ) {
                        var dir = t.GetDirectoryPath( );
                        text = PathUtils.GetRelativePath( dir, text );
                    }
                    GUI.changed = true;
                    //EditorUtility.SetDirty( t );
                    result = text;

                }
            }
            return result;
        }
    }
#endif
}
