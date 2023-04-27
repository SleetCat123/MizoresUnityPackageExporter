#if UNITY_EDITOR
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
#if UNITY_EDITOR
            return Get( ExporterEditorPrefs.Language, key );
#else
            return Get( DEFAULT_KEY, key );
#endif
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
                                var registerText = values[1].Replace( "\\.", "," );
                                t[values[0]] = registerText.Replace( "\\n", "\n" );
                                ExporterUtils.DebugLog( $"[{values[0]}] = {registerText}" );
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

        public static string Undo => Get( "Undo" );
        public static string BatchExportRegex => Get( "BatchExportRegex" );
        public static string BatchExportRegexError => Get( "BatchExportRegexError" );
        public static string BatchExportMode => Get( "BatchExportMode" );
        public static string BatchExportNoTagError => Get( "BatchExportNoTagError" );
        public static string BatchVariableTooltip => Get( "BatchVariableTooltip" );

        public static string DateVariableTooltip => Get( "DateVariableTooltip" );

        public static string FoldoutObjects => Get( "FoldoutObjects" );
        public static string FoldoutObjectsTooltip => Get( "FoldoutObjectsTooltip" );
        public static string FoldoutReferences => Get( "FoldoutReferences" );
        public static string FoldoutReferencesTooltip => Get( "FoldoutReferencesTooltip" );
        public static string FoldoutExcludes => Get( "FoldoutExcludes" );
        public static string FoldoutExcludesPreview => Get( "FoldoutExcludesPreview" );
        public static string FoldoutExcludeObjects => Get( "FoldoutExcludeObjects" );
        public static string FoldoutDynamicPath => Get( "FoldoutDynamicPath" );
        public static string FoldoutDynamicPathPreview => Get( "FoldoutDynamicPathPreview" );
        public static string FoldoutVariables => Get( "FoldoutVariables" );
        public static string FoldoutBatchExportEnabled => Get( "FoldoutBatchExportEnabled" );
        public static string FoldoutBatchExportDisabled => Get( "FoldoutBatchExportDisabled" );
        public static string FoldoutPackageName => Get( "FoldoutPackageName" );
        public static string Version => Get( "Version" );
        public static string VersionSource => Get( "VersionSource" );
        public static string VersionFormat => Get( "VersionFormat" );
        public static string PackageName => Get( "PackageName" );
        public static string LabelExportPackage => Get( "LabelExportPackage" );
        public static string ButtonCheck => Get( "ButtonCheck" );
        public static string ButtonExportPackage => Get( "ButtonExportPackage" );
        public static string ButtonExportPackages => Get( "ButtonExportPackages" );
        public static string ButtonOpen => Get( "ButtonOpen" );
        public static string DiffLabel => Get( "DiffLabel" );
        public static string DiffTooltip => Get( "DiffTooltip" );
        public static string ButtonFolder => Get( "ButtonFolder" );
        public static string ButtonFile => Get( "ButtonFile" );
        public static string ExportLogNotFound => Get( "ExportLogNotFound" );
        public static string ExportLogFailed => Get( "ExportLogFailed" );
        public static string ExportLogAllFileExists => Get( "ExportLogAllFileExists" );
        public static string ExportLogSuccess => Get( "ExportLogSuccess" );
        public static string ExcludesWereEmpty => Get( "ExcludesWereEmpty" );
        public static string CopyTarget => Get( "CopyTarget" );
        public static string CopyTargetWithValue => Get( "CopyTargetWithValue" );
        public static string CopyTargetNoValue => Get( "CopyTargetNoValue" );
        public static string PasteTarget => Get( "PasteTarget" );
        public static string PasteTargetWithValue => Get( "PasteTargetWithValue" );
        public static string PasteTargetNoValue => Get( "PasteTargetNoValue" );
        public static string IncompatibleVersion => Get( "IncompatibleVersion" );
        public static string IncompatibleVersionForceOpen => Get( "IncompatibleVersionForceOpen" );
        public static string FileListViewFullPath => Get( "FileListViewFullPath" );
        public static string FileListTreeView => Get( "FileListTreeView" );
        public static string FileListViewReferencedFiles => Get( "FileListViewReferencedFiles" );
        public static string FileListViewExcludeFiles => Get( "FileListViewExcludeFiles" );
        public static string FileListClose => Get( "FileListClose" );
        public static string FileListNotFoundPathPrefix => Get( "FileListNotFoundPathPrefix" );
        public static string FileListExcludesPathPrefix => Get( "FileListExcludesPathPrefix" );
        public static string FileListReferencesPathPrefix => Get( "FileListReferencesPathPrefix" );
        public static string FileListCategoryNotFound => Get( "FileListCategoryNotFound" );
        public static string FileListCategoryExcludes => Get( "FileListCategoryExcludes" );
        public static string FileListCategoryReferences => Get( "FileListCategoryReferences" );
        public static string FileListTooltipReferencedBy => Get( "FileListTooltipReferencedBy" );

        public static string ExportListEmpty => Get( "ExportListEmpty" );
    }
}
