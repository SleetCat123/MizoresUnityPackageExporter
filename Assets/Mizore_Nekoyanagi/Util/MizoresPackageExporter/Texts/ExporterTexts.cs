﻿#if UNITY_EDITOR
#endif

using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter
{
    public static class ExporterTexts
    {
        public const string DEFAULT_KEY = "en";
        static Dictionary<string, Dictionary<string, string>> _table = new Dictionary<string, Dictionary<string, string>>( );
        static string[] languageList = new string[] { DEFAULT_KEY, "jp" };
        public static string[] LanguageList => languageList;
        public static void Clear( ) {
            _table.Clear( );
        }
        static string Get( string key ) {
            return Get( EditorPrefsCache.GetString( ExporterConsts_Editor.EDITOR_PREF_LANGUAGE, DEFAULT_KEY ), key );
        }
        static string Get( string language, string key ) {
            Dictionary<string, string> t;
            if ( !_table.TryGetValue( language, out t ) ) {
                var path = "MizoresPackageExporter/" + language;
                ExporterUtils.DebugLog( "LoadText: " + path );
                var text = Resources.Load<TextAsset>( path );
                if ( text == null ) {
                    ExporterUtils.DebugLogWarning( "Load failed: " + path );
                } else {
                    t = new Dictionary<string, string>( );
                    using ( var reader = new StringReader( text.text ) ) {
                        while ( reader.Peek( ) > -1 ) {
                            string line = reader.ReadLine( );
                            string[] values = line.Split( ',' );
                            if ( values.Length >= 2 ) {
                                t[values[0]] = values[1];
                                ExporterUtils.DebugLog( $"[{values[0]}] = {values[1]}" );
                            }
                        }
                    }
                    _table.Add( language, t );
                    ExporterUtils.DebugLog( "Load finish: " + path );
                }
            }
            string result;
            if ( t != null && t.TryGetValue( key, out result ) ) {
                return result;
            } else if ( language != DEFAULT_KEY ) {
                return Get( DEFAULT_KEY, key );
            } else {
                return "[NotFound: " + key + "]";
            }
        }

        public static string t_Undo => Get( "Undo" );
        public static string t_Objects => Get( "Objects" );
        public static string t_References => Get( "References" );
        public static string t_Excludes => Get( "Excludes" );
        public static string t_ExcludesPreview => Get( "ExcludesPreview" );
        public static string t_ExcludeObjects => Get( "ExcludeObjects" );
        public static string t_DynamicPath => Get( "DynamicPath" );
        public static string t_DynamicPathPreview => Get( "DynamicPathPreview" );
        public static string t_DynamicPathVariables => Get( "DynamicPathVariables" );
        public static string t_Version => Get( "Version" );
        public static string t_VersionSource => Get( "VersionSource" );
        public static string t_VersionFormat => Get( "VersionFormat" );
        public static string t_PackageName => Get( "PackageName" );
        public static string t_LabelExportPackage => Get( "LabelExportPackage" );
        public static string t_ButtonCheck => Get( "ButtonCheck" );
        public static string t_ButtonExportPackage => Get( "ButtonExportPackage" );
        public static string t_ButtonExportPackages => Get( "ButtonExportPackages" );
        public static string t_ButtonOpen => Get( "ButtonOpen" );
        public static string t_DiffLabel => Get( "DiffLabel" );
        public static string t_DiffTooltip => Get( "DiffTooltip" );
        public static string t_ButtonFolder => Get( "ButtonFolder" );
        public static string t_ButtonFile => Get( "ButtonFile" );
        public static string t_ExportLogNotFound => Get( "ExportLogNotFound" );
        public static string t_ExportLogNotFoundPathPrefix => Get( "ExportLogNotFoundPathPrefix" );
        public static string t_ExportLogDependencyPathPrefix => Get( "ExportLogDependencyPathPrefix" );
        public static string t_ExportLogFailed => Get( "ExportLogFailed" );
        public static string t_ExportLogAllFileExists => Get( "ExportLogAllFileExists" );
        public static string t_ExportLogSuccess => Get( "ExportLogSuccess" );
        public static string t_ExcludesWereEmpty => Get( "ExcludesWereEmpty" );
        public static string t_CopyTarget => Get( "CopyTarget" );
        public static string t_CopyTargetWithValue => Get( "CopyTargetWithValue" );
        public static string t_CopyTargetNoValue => Get( "CopyTargetNoValue" );
        public static string t_PasteTarget => Get( "PasteTarget" );
        public static string t_PasteTargetWithValue => Get( "PasteTargetWithValue" );
        public static string t_PasteTargetNoValue => Get( "PasteTargetNoValue" );
        public static string t_IncompatibleVersion => Get( "IncompatibleVersion" );
        public static string t_IncompatibleVersionForceOpen => Get( "IncompatibleVersionForceOpen" );
        public static string t_FileListViewFullPath => Get( "FileListViewFullPath" );
        public static string t_FileListClose => Get( "FileListClose" );
    }
}