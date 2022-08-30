using UnityEngine;
using Const = MizoreNekoyanagi.PublishUtil.PackageExporter.MizoresPackageExporterConsts;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.SingleEditor
{
#if UNITY_EDITOR
    public static class SingleGUI_DynamicPath
    {
        static void AddObjects( MizoresPackageExporter t, List<string> list, Object[] objectReferences ) {
            list.AddRange(
                objectReferences.
                Where( v => EditorUtility.IsPersistent( v ) ).
                Select( v => AssetDatabase.GetAssetPath( v ) )
                );
            EditorUtility.SetDirty( t );
        }
        public static void Draw( MizoresPackageExporterEditor ed, MizoresPackageExporter t ) {
            // ↓ Dynamic Path
            if ( ExporterUtils.EditorPrefFoldout(
                Const.EDITOR_PREF_FOLDOUT_DYNAMICPATH,
                string.Format( ExporterTexts.t_DynamicPath, t.dynamicpath.Count ),
                new ExporterUtils.FoldoutFuncs( ) {
                    canDragDrop = ExporterUtils.Filter_HasPersistentObject,
                    onDragPerform = ( objectReferences ) => AddObjects( t, t.dynamicpath, objectReferences ),
                    onRightClick = ( ) => SingleGUIElement_CopyPaste.OnRightClickFoldout( t, ExporterTexts.t_DynamicPath, t.dynamicpath, ( list ) => t.dynamicpath = list )
                }
                ) ) {
                Event currentEvent = Event.current;
                for ( int i = 0; i < t.dynamicpath.Count; i++ ) {
                    using ( var scope = new EditorGUILayout.HorizontalScope( ) ) {
                        ExporterUtils.Indent( 1 );
                        EditorGUILayout.LabelField( i.ToString( ), GUILayout.Width( 30 ) );

                        // 値編集
                        EditorGUI.BeginChangeCheck( );
                        Rect textrect = EditorGUILayout.GetControlRect( );
                        string item = t.dynamicpath[i];
                        item = EditorGUI.TextField( textrect, item );
                        item = ed.BrowseButtons( item );
                        if ( ExporterUtils.DragDrop( textrect, ExporterUtils.Filter_HasPersistentObject ) ) {
                            GUI.changed = true;
                            item = AssetDatabase.GetAssetPath( DragAndDrop.objectReferences[0] );
                        }
                        if ( EditorGUI.EndChangeCheck( ) ) {
                            t.dynamicpath[i] = item;
                            EditorUtility.SetDirty( t );
                        }

                        // ボタン
                        int index_after = ExporterUtils.UpDownButton( i, t.dynamicpath.Count );
                        if ( i != index_after ) {
                            t.dynamicpath.Swap( i, index_after );
                            EditorUtility.SetDirty( t );
                        }
                        EditorGUILayout.LabelField( string.Empty, GUILayout.Width( 10 ) );
                        if ( GUILayout.Button( "-", GUILayout.Width( 15 ) ) ) {
                            t.dynamicpath.RemoveAt( i );
                            i--;
                            EditorUtility.SetDirty( t );
                        }
                    }
                }
                using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                    ExporterUtils.Indent( 1 );
                    if ( GUILayout.Button( "+", GUILayout.Width( 60 ) ) ) {
                        t.dynamicpath.Add( string.Empty );
                        EditorUtility.SetDirty( t );
                    }
                }
            }
            // ↑ Dynamic Path

            // ↓ Dynamic Path Preview
            if ( ExporterUtils.EditorPrefFoldout(
                Const.EDITOR_PREF_FOLDOUT_DYNAMICPATH_PREVIEW, ExporterTexts.t_DynamicPathPreview ) ) {
                for ( int i = 0; i < t.dynamicpath.Count; i++ ) {
                    using ( var horizontalScope = new EditorGUILayout.HorizontalScope( ) ) {
                        // プレビュー
                        string previewpath = t.ConvertDynamicPath( t.dynamicpath[i] );
                        ExporterUtils.Indent( 1 );
                        EditorGUILayout.LabelField( i.ToString( ), GUILayout.Width( 30 ) );
                        EditorGUILayout.LabelField( new GUIContent( previewpath, previewpath ) );
                    }
                }
            }
            // ↑ Dynamic Path Preview
        }
    }
#endif
}
