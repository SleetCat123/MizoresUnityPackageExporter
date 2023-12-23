using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public class ExporterEditorLogs {
        public enum LogType {
            Info, Warning, Error, Separator
        }
        public class LogElement {
            public LogType type;
            public Texture icon;
            public string text;

            public LogElement( LogType type, string text ) {
                this.type = type;
                this.text = text;
            }
            public LogElement( LogType type, Texture icon, string text ) {
                this.type = type;
                this.icon = icon;
                this.text = text;
            }

            public void DrawUI( ) {
#if UNITY_EDITOR
                Texture logIcon = null;
                switch ( type ) {
                    case LogType.Info:
                        logIcon = IconCache.InfoIcon;
                        break;
                    case LogType.Warning:
                        logIcon = IconCache.WarningIcon;
                        break;
                    case LogType.Error:
                        logIcon = IconCache.ErrorIcon;
                        break;
                }

                var texts = text.Split( '\n' );
                for ( int i = 0; i < texts.Length; i++ ) {
                    var rect = EditorGUILayout.GetControlRect( );
                    var labelRect = rect;
                    var iconSize = 16;
                    if ( logIcon != null ) {
                        if ( i == 0 ) {
                            var iconRect = rect;
                            iconRect.width = 16;
                            iconRect.height = 16;
                            GUI.DrawTexture( iconRect, logIcon );
                        }
                        labelRect.x += iconSize;
                        labelRect.width -= iconSize;
                    }
                    EditorGUI.LabelField( labelRect, new GUIContent( texts[i], icon ) );
                }
#endif
            }
        }
        public List<LogElement> logs = new List<LogElement>( );

        public void Add( ) {
            logs.Add( new LogElement( LogType.Info, string.Empty ) );
        }
        public void Add( string text ) {
            logs.Add( new LogElement( LogType.Info, text ) );
        }
        public void Add( LogType type, string text ) {
            logs.Add( new LogElement( type, text ) );
        }
        public void Add( LogType type, Texture icon, string text ) {
            logs.Add( new LogElement( type, icon, text ) );
        }
        public void AddSeparator( ) {
            logs.Add( new LogElement( LogType.Separator, "----------" ) );
        }
        public void Clear( ) {
            logs.Clear( );
        }
        public void DrawUI( ) {
#if UNITY_EDITOR
            using ( new EditorGUILayout.VerticalScope( EditorStyles.helpBox ) ) {
                foreach ( var item in logs ) {
                    item.DrawUI( );
                }
            }
#endif
        }
    }
}