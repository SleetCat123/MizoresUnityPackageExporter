using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil
{
    [CreateAssetMenu( menuName = "MizoreNekoyanagi/UnityPackageExporter" )]
    public class MizoresPackageExporter : ScriptableObject
    {
        [System.Serializable]
        public class PackagePrefsElement
        {
            [SerializeField]
            private Object obj;
            [SerializeField]
            private string path;

            public Object Object {
                get {
#if UNITY_EDITOR
                    if ( obj == null && !string.IsNullOrEmpty( path ) ) {
                        obj = AssetDatabase.LoadAssetAtPath<Object>( path );
                    }
#endif
                    return obj;
                }
                set {
#if UNITY_EDITOR
                    //if ( obj != value ) {
                    if ( value != null ) {
                        path = AssetDatabase.GetAssetPath( value.GetInstanceID( ) );
                    } else {
                        path = string.Empty;
                    }
                    //}
#endif
                    obj = value;
                }
            }

            public string Path {
                get {
#if UNITY_EDITOR
                    if ( obj != null ) {
                        path = AssetDatabase.GetAssetPath( obj.GetInstanceID( ) );
                    }
#endif
                    return path;
                }
            }
        }

        public List<PackagePrefsElement> objects = new List<PackagePrefsElement>( );
        public List<string> dynamicpath = new List<string>( );

        public const string EXPORT_FOLDER_PATH = "MizorePackageExporter/";
        public const string EXPORT_LOG_NOT_FOUND = "[{0}] is not exists. The export has been cancelled.\n[{0}]は存在しません。エクスポートは中断されました。\n";
        public string ExportPath { get { return EXPORT_FOLDER_PATH + this.name + ".unitypackage"; } }

        string ConvertDynamicPath( string path ) {
            path = path.Replace( "%name%", name );
            return path;
        }
        public void Export( ) {
#if UNITY_EDITOR
            var list = objects.Where( v => !string.IsNullOrWhiteSpace( v.Path ) ).Select( v => v.Path );
            list = list.Concat( dynamicpath.Where( v => !string.IsNullOrWhiteSpace( v ) ).Select( v => ConvertDynamicPath( v ) ) );

            // console
            StringBuilder sb = new StringBuilder( );
            sb.Append( "Start Export: " ).Append( name );
            sb.AppendLine( );
            foreach ( var item in list ) {
                sb.AppendLine( item );
            }
            Debug.Log( sb );
            // ファイルが存在するか確認
            foreach ( var item in list ) {
                if ( Path.GetExtension( item ).Length != 0 ) {
                    if ( File.Exists( item ) == false ) {
                        var text = string.Format( EXPORT_LOG_NOT_FOUND, item );
                        UnityPackageExporterEditor.HelpBoxText += text;
                        UnityPackageExporterEditor.HelpBoxMessageType = MessageType.Error;
                        Debug.LogError( text );
                        return;
                    }
                } else if ( Directory.Exists( item ) == false ) {
                    var text = string.Format( EXPORT_LOG_NOT_FOUND, item );
                    UnityPackageExporterEditor.HelpBoxText += text;
                    UnityPackageExporterEditor.HelpBoxMessageType = MessageType.Error;
                    Debug.LogError( text );
                    return;
                }
            }
            //

            string[] pathNames = list.ToArray( );
            string exportPath = ExportPath;
            if ( Directory.Exists( exportPath ) == false ) {
                Directory.CreateDirectory( Path.GetDirectoryName( exportPath ) );
            }
            AssetDatabase.ExportPackage( pathNames, exportPath, ExportPackageOptions.Recurse );
            EditorUtility.RevealInFinder( exportPath );
            Debug.Log( this.name + "をエクスポートしました。" );
#endif
        }

#if UNITY_EDITOR
        [CustomEditor( typeof( MizoresPackageExporter ) ), CanEditMultipleObjects]
        public class UnityPackageExporterEditor : Editor
        {
            public struct MinMax
            {
                public int min, max;

                public MinMax( int min, int max ) {
                    this.min = min;
                    this.max = max;
                }

                public static MinMax Create<T>( IEnumerable<T> list, System.Func<T, int> func ) {
                    int min = list.Min( func );
                    int max = list.Max( func );
                    return new MinMax( min, max );
                }
            }

            public const string TEXT_UNDO = "PackagePrefs";
            public const string TEXT_OBJECTS = "Objects";
            public const string TEXT_DYNAMIC_PATH = "Dynamic Path";
            public const string TEXT_BUTTON_EXPORT = "Export to unitypackage";
            public const string TEXT_BUTTON_EXPORT_M = "Export to unitypackages";
            public const string TEXT_BUTTON_OPEN = "Open";
            public const string TEXT_DIFF_LABEL = "?";
            public const string TEXT_DIFF_TOOLTIP = "Some values are different.\n一部のオブジェクトの値が異なっています。";

            public string t_Undo => TEXT_UNDO;
            public string t_Objects => TEXT_OBJECTS;
            public string t_DynamicPath => TEXT_DYNAMIC_PATH;
            public string t_Button_ExportPackage => TEXT_BUTTON_EXPORT;
            public string t_Button_ExportPackages => TEXT_BUTTON_EXPORT_M;
            public string t_Button_Open => TEXT_BUTTON_OPEN;
            public string t_Diff_Label => TEXT_DIFF_LABEL;
            public string t_Diff_Tooltip => TEXT_DIFF_TOOLTIP;

            public static string HelpBoxText;
            public static MessageType HelpBoxMessageType;
            Vector2 scroll;
            MizoresPackageExporter t;
            private void OnEnable( ) {
                HelpBoxText = null;
                t = target as MizoresPackageExporter;
            }
            public static void Resize<T>( List<T> list, int newSize, System.Func<T> newvalue ) {
                if ( list.Count == newSize ) {
                    return;
                } else if ( list.Count < newSize ) {
                    var array = new T[newSize - list.Count];
                    if ( newvalue != null ) {
                        for ( int i = 0; i < array.Length; i++ ) {
                            array[i] = newvalue.Invoke( );
                        }
                    }
                    list.AddRange( array );
                } else {
                    list.RemoveRange( newSize, list.Count - newSize );
                }
            }
            public static void Resize<T>( List<T> list, int newSize ) {
                if ( list.Count == newSize ) {
                    return;
                } else if ( list.Count < newSize ) {
                    list.AddRange( new T[newSize - list.Count] );
                } else {
                    list.RemoveRange( newSize, list.Count - newSize );
                }
            }
            /// <summary>
            /// 複数オブジェクトの編集
            /// </summary>
            void EditMultiple( ) {
                if ( targets.Length <= 1 ) return;
                scroll = EditorGUILayout.BeginScrollView( scroll );
                Undo.RecordObjects( targets, t_Undo );

                var targetlist = targets.Select( v => v as MizoresPackageExporter );

                // Targets
                GUI.enabled = false;
                foreach ( var item in targetlist ) {
                    EditorGUILayout.ObjectField( item, typeof( MizoresPackageExporter ), false );
                }
                GUI.enabled = true;

                // ↓ Objects
                EditorGUILayout.Separator( );
                EditorGUILayout.LabelField( t_Objects, EditorStyles.boldLabel );
                MinMax objects_count = MinMax.Create( targetlist, v => v.objects.Count );
                for ( int i = 0; i < objects_count.max; i++ ) {
                    using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                        // 全てのオブジェクトの値が同じか
                        bool same = i < objects_count.min && targetlist.All( v => t.objects[i].Object == v.objects[i].Object );

                        if ( same ) {
                            EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 30 ) );
                        } else {
                            EditorGUILayout.LabelField( new GUIContent( t_Diff_Label, t_Diff_Tooltip ), GUILayout.Width( 30 ) );
                        }

                        EditorGUI.BeginChangeCheck( );
                        Object obj;
                        if ( same ) {
                            obj = EditorGUILayout.ObjectField( t.objects[i].Object, typeof( Object ), false );
                        } else {
                            obj = EditorGUILayout.ObjectField( null, typeof( Object ), false );
                        }
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            foreach ( var item in targetlist ) {
                                Resize( item.objects, Mathf.Max( i + 1, item.objects.Count ), ( ) => new PackagePrefsElement( ) );
                                item.objects[i].Object = obj;
                                objects_count = MinMax.Create( targetlist, v => v.objects.Count );
                                EditorUtility.SetDirty( item );
                            }
                        }

                        EditorGUI.BeginChangeCheck( );
                        string path;
                        if ( same ) {
                            path = EditorGUILayout.TextField( t.objects[i].Path );
                        } else {
                            path = EditorGUILayout.TextField( string.Empty );
                        }
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            // パスが変更されたらオブジェクトを置き換える
                            Object o = AssetDatabase.LoadAssetAtPath<Object>( path );
                            if ( o != null ) {
                                foreach ( var item in targetlist ) {
                                    Resize( item.objects, Mathf.Max( i + 1, item.objects.Count ), ( ) => new PackagePrefsElement( ) );
                                    item.objects[i].Object = o;
                                    objects_count = MinMax.Create( targetlist, v => v.objects.Count );
                                    EditorUtility.SetDirty( item );
                                }
                            }
                        }

                        if ( GUILayout.Button( "-", GUILayout.Width( 15 ) ) ) {
                            foreach ( var item in targetlist ) {
                                Resize( item.objects, Mathf.Max( i + 1, item.objects.Count ), ( ) => new PackagePrefsElement( ) );
                                item.objects.RemoveAt( i );
                                objects_count = MinMax.Create( targetlist, v => v.objects.Count );
                                EditorUtility.SetDirty( item );
                            }
                            i--;
                        }
                    }
                }
                if ( GUILayout.Button( "+", GUILayout.Width( 60 ) ) ) {
                    foreach ( var item in targetlist ) {
                        Resize( item.objects, objects_count.max + 1 );
                        EditorUtility.SetDirty( item );
                    }
                }
                // ↑ Objects

                // ↓ Dynamic Path
                EditorGUILayout.Separator( );
                EditorGUILayout.LabelField( t_DynamicPath, EditorStyles.boldLabel );
                var dpath_count = MinMax.Create( targetlist, v => v.dynamicpath.Count );
                for ( int i = 0; i < dpath_count.max; i++ ) {
                    using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                        // 全てのオブジェクトの値が同じか
                        bool same = i < dpath_count.min && targetlist.All( v => t.dynamicpath[i] == v.dynamicpath[i] );

                        if ( same ) {
                            EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 30 ) );
                        } else {
                            EditorGUILayout.LabelField( new GUIContent( t_Diff_Label, t_Diff_Tooltip ), GUILayout.Width( 30 ) );
                        }

                        EditorGUI.BeginChangeCheck( );
                        string path;
                        if ( same ) {
                            path = EditorGUILayout.TextField( t.dynamicpath[i] );
                        } else {
                            path = EditorGUILayout.TextField( string.Empty );
                        }
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            foreach ( var item in targetlist ) {
                                Resize( item.dynamicpath, Mathf.Max( i + 1, item.dynamicpath.Count ) );
                                item.dynamicpath[i] = path;
                                dpath_count = MinMax.Create( targetlist, v => v.dynamicpath.Count );
                                EditorUtility.SetDirty( item );
                            }
                        }

                        if ( GUILayout.Button( "-", GUILayout.Width( 15 ) ) ) {
                            foreach ( var item in targetlist ) {
                                Resize( item.dynamicpath, Mathf.Max( i + 1, item.dynamicpath.Count ) );
                                item.dynamicpath.RemoveAt( i );
                                dpath_count = MinMax.Create( targetlist, v => v.dynamicpath.Count );
                                EditorUtility.SetDirty( item );
                            }
                            i--;
                        }
                    }
                }
                if ( GUILayout.Button( "+", GUILayout.Width( 60 ) ) ) {
                    foreach ( var item in targetlist ) {
                        Resize( item.dynamicpath, dpath_count.max + 1, ( ) => string.Empty );
                        EditorUtility.SetDirty( item );
                    }
                }
                // ↑ Dynamic Path

                EditorGUILayout.EndScrollView( );

                // Export Button
                EditorGUILayout.Separator( );
                if ( GUILayout.Button( t_Button_ExportPackages ) ) {
                    foreach ( var item in targetlist ) {
                        item.Export( );
                    }
                }

                // 出力先一覧
                EditorGUILayout.Separator( );
                foreach ( var item in targets ) {
                    var path = ( item as MizoresPackageExporter ).ExportPath;
                    EditorGUILayout.LabelField( new GUIContent( path, path ) );
                }
                if ( GUILayout.Button( TEXT_BUTTON_OPEN, GUILayout.Width( 60 ) ) ) {
                    EditorUtility.RevealInFinder( EXPORT_FOLDER_PATH );
                }
            }
            void EditSingle( ) {
                using ( var s = new EditorGUI.DisabledGroupScope( true ) ) {
                    EditorGUILayout.ObjectField( t, typeof( MizoresPackageExporter ), false );
                }
                scroll = EditorGUILayout.BeginScrollView( scroll );
                Undo.RecordObject( t, t_Undo );

                // ↓ Objects
                EditorGUILayout.LabelField( t_Objects, EditorStyles.boldLabel );
                for ( int i = 0; i < t.objects.Count; i++ ) {
                    using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                        PackagePrefsElement item = t.objects[i];
                        EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 30 ) );

                        EditorGUI.BeginChangeCheck( );
                        item.Object = EditorGUILayout.ObjectField( item.Object, typeof( Object ), false );
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            EditorUtility.SetDirty( t );
                        }

                        EditorGUI.BeginChangeCheck( );
                        string path = item.Path;
                        path = EditorGUILayout.TextField( path );
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            // パスが変更されたらオブジェクトを置き換える
                            Object o = AssetDatabase.LoadAssetAtPath<Object>( path );
                            if ( o != null ) {
                                item.Object = o;
                            }
                            EditorUtility.SetDirty( t );
                        }

                        if ( GUILayout.Button( "-", GUILayout.Width( 15 ) ) ) {
                            t.objects.RemoveAt( i );
                            i--;
                            EditorUtility.SetDirty( t );
                        }
                    }
                }
                if ( GUILayout.Button( "+", GUILayout.Width( 60 ) ) ) {
                    t.objects.Add( new PackagePrefsElement( ) );
                    EditorUtility.SetDirty( t );
                }
                // ↑ Objects

                // ↓ Dynamic Path
                EditorGUILayout.Separator( );
                EditorGUILayout.LabelField( t_DynamicPath, EditorStyles.boldLabel );
                for ( int i = 0; i < t.dynamicpath.Count; i++ ) {
                    using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                        EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 30 ) );

                        EditorGUI.BeginChangeCheck( );
                        t.dynamicpath[i] = EditorGUILayout.TextField( t.dynamicpath[i] );
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            EditorUtility.SetDirty( t );
                        }

                        if ( GUILayout.Button( "-", GUILayout.Width( 15 ) ) ) {
                            t.dynamicpath.RemoveAt( i );
                            i--;
                            EditorUtility.SetDirty( t );
                        }
                    }
                }
                if ( GUILayout.Button( "+", GUILayout.Width( 60 ) ) ) {
                    t.dynamicpath.Add( string.Empty );
                    EditorUtility.SetDirty( t );
                }
                // ↑ Dynamic Path

                // Export Button
                EditorGUILayout.EndScrollView( );
                EditorGUILayout.Separator( );
                if ( GUILayout.Button( t_Button_ExportPackage ) ) {
                    t.Export( );
                }
                using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                    EditorGUILayout.LabelField( new GUIContent( t.ExportPath, t.ExportPath ) );
                    if ( GUILayout.Button( TEXT_BUTTON_OPEN, GUILayout.Width( 60 ) ) ) {
                        if ( File.Exists( t.ExportPath ) ) {
                            EditorUtility.RevealInFinder( t.ExportPath );
                        } else {
                            EditorUtility.RevealInFinder( EXPORT_FOLDER_PATH );
                        }
                    }
                }
            }
            public override void OnInspectorGUI( ) {
                if ( targets.Length != 1 ) {
                    EditMultiple( );
                } else {
                    EditSingle( );
                }

                if ( string.IsNullOrEmpty( HelpBoxText ) == false ) {
                    EditorGUILayout.HelpBox( HelpBoxText.Trim( ), HelpBoxMessageType );
                }
            }
        }
#endif
    }
}
