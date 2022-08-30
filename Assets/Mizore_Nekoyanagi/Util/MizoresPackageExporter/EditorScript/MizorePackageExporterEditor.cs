﻿using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter
{
#if UNITY_EDITOR
    [CustomEditor( typeof( MizoresPackageExporter ) ), CanEditMultipleObjects]
    public class MizoresPackageExporterEditor : Editor
    {
        public MizoresPackageExporterEditorValues values = new MizoresPackageExporterEditorValues( );
        public MizoresPackageExporter t;
        private void OnEnable( ) {
            t = target as MizoresPackageExporter;
        }
        static string ToAssetsPath( string path ) {
            string datapath = Application.dataPath;
            if ( path.StartsWith( datapath ) ) {
                path = path.Substring( datapath.Length - "Assets".Length );
            }
            return path;
        }
        public string BrowseButtons( string text ) {
            string result = text;
            if ( GUILayout.Button( ExporterTexts.TEXT_BUTTON_FOLDER, GUILayout.Width( 50 ) ) ) {
                text = EditorUtility.OpenFolderPanel( null, t.ConvertDynamicPath( text ), null );
                text = ToAssetsPath( text );
                if ( string.IsNullOrEmpty( text ) == false ) {
                    GUI.changed = true;
                    result = text;
                }
            }
            if ( GUILayout.Button( ExporterTexts.TEXT_BUTTON_FILE, GUILayout.Width( 50 ) ) ) {
                text = EditorUtility.OpenFilePanel( null, t.ConvertDynamicPath( text ), null );
                text = ToAssetsPath( text );
                if ( string.IsNullOrEmpty( text ) == false ) {
                    GUI.changed = true;
                    result = text;
                }
            }
            return result;
        }
        public override void OnInspectorGUI( ) {
            if ( targets.Length != 1 ) {
                foreach ( var item in targets ) {
                    var exporter = item as MizoresPackageExporter;
                    if ( !exporter.IsCompatible ) {
                        EditorGUILayout.HelpBox( ExporterTexts.t_IncompatibleVersion, MessageType.Error );
                        return;
                    }
                    if ( !exporter.IsCurrentVersion ) {
                        exporter.ConvertToCurrentVersion( );
                        EditorUtility.SetDirty( exporter );
                    }
                }
                MultipleEditor.MultipleEditorGUI.EditMultiple( this );
            } else {
                if ( !t.IsCompatible ) {
                    EditorGUILayout.HelpBox( ExporterTexts.t_IncompatibleVersion,  MessageType.Error );
                    return;
                }
                if ( !t.IsCurrentVersion ) {
                    t.ConvertToCurrentVersion( );
                    EditorUtility.SetDirty( t );
                }
                SingleEditor.SingleEditorGUI.EditSingle( this );
            }

            if ( string.IsNullOrEmpty( values._helpBoxText ) == false ) {
                EditorGUILayout.HelpBox( values._helpBoxText.Trim( ), values._helpBoxMessageType );
            }
        }
    }
#endif
}