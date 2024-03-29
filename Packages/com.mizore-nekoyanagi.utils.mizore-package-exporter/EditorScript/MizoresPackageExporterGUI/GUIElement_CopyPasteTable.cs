﻿using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterEditor {
    public static class GUIElement_CopyPasteTable {
        static bool TableIsPerfectMatch<TKey, TValue>( IEnumerable<MizoresPackageExporter> targets, Func<MizoresPackageExporter, Dictionary<TKey, TValue>> getList ) {
            if ( targets.Count( ) == 1 ) {
                return true;
            }
            var t = getList( targets.First( ) );
            var lists = targets.Select( v => getList( v ) );
            foreach ( var item in lists ) {
                if ( t.Count != item.Count ) {
                    return false;
                }
                if ( t.Except( item ).Any( ) ) {
                    return false;
                }
            }
            return true;
        }
        public static void OnRightClickFoldout<TKey, TValue>( IEnumerable<MizoresPackageExporter> targets, Func<string, string> labelFormat, Func<MizoresPackageExporter, Dictionary<TKey, TValue>> getTable, Action<MizoresPackageExporter, Dictionary<TKey, TValue>> setTable ) {
            GenericMenu menu = new GenericMenu( );
            var firstList = getTable( targets.First( ) );
            bool perfectMatch = TableIsPerfectMatch( targets, getTable );
            if ( perfectMatch ) {
                var copyLabel = ExporterTexts.CopyTarget( labelFormat( firstList.Count.ToString( ) ) );
                menu.AddItem( new GUIContent( copyLabel ), false, CopyCache.Copy, firstList );
            } else {
                menu.AddDisabledItem( new GUIContent( ExporterTexts.CopyTargetNoValue ) );
            }
            if ( CopyCache.CanPaste<Dictionary<TKey, TValue>>( ) ) {
                var cache = CopyCache.GetCache<Dictionary<TKey, TValue>>( clone: false );
                string label = ExporterTexts.PasteTarget( labelFormat( cache.Count.ToString( ) ) );
                menu.AddItem( new GUIContent( label ), false, ( ) => {
                    foreach ( var t in targets ) {
                        setTable( t, CopyCache.GetCache<Dictionary<TKey, TValue>>( ) );
                        EditorUtility.SetDirty( t );
                    }
                } );
            } else {
                menu.AddDisabledItem( new GUIContent( ExporterTexts.PasteTargetNoValue ) );
            }
            menu.ShowAsContext( );
        }
    }
}
#endif
