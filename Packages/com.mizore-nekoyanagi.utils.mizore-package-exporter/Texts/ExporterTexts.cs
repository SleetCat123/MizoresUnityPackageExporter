using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public static class ExporterTexts {
        public const string TEXT_RESOURCES_PATH = "MizoresPackageExporter/";
        public const string DEFAULT_KEY = "en";
        static Dictionary<string, Dictionary<string, string>> _table = new Dictionary<string, Dictionary<string, string>>( );
        static string[] languageList = new string[] { DEFAULT_KEY, "jp" };
        public static string[] LanguageList => languageList;
        public static string GetTextAssetResourcesPath( string language = DEFAULT_KEY ) {
            return TEXT_RESOURCES_PATH + language;
        }
        public static void Clear( ) {
            _table.Clear( );
        }
        static string Get( string key, params object[] args ) {
            return string.Format( Get( key ), args );
        }
        static string Get( string key ) {
#if UNITY_EDITOR
            return GetFromLanguage( ExporterEditorPrefs.Language, key );
#else
            return GetFromLanguage( DEFAULT_KEY, key );
#endif
        }
        static string GetFromLanguage( string language, string key ) {
            Dictionary<string, string> t;
            if ( !_table.TryGetValue( language, out t ) ) {
                var path = GetTextAssetResourcesPath( language );
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
                                // ExporterUtils.DebugLog( $"[{values[0]}] = {registerText}" );
                            }
                        }
                    }
                    _table.Add( language, t );
                    // ExporterUtils.DebugLog( "Load finish: " + path );
                }
            }
            string result;
            if ( t != null && t.TryGetValue( key, out result ) ) {
                return result;
            } else if ( language != DEFAULT_KEY ) {
                return GetFromLanguage( DEFAULT_KEY, key );
            } else {
                return "[NotFound: " + key + "]";
            }
        }

        public static string Undo => Get( "Undo" );
        public static string Yes => Get( "Yes" );
        public static string No => Get( "No" );
        public static string BatchExportFolder => Get( "BatchExportFolder" );
        public static string BatchExportListLabel => Get( "BatchExportListLabel" );
        public static string BatchExportListFile => Get( "BatchExportListFile" );
        public static string BatchExportFolderMode => Get( "BatchExportFolderMode" );
        public static string BatchExportRegex => Get( "BatchExportRegex" );
        public static string BatchExportRegexError( string name, string message ) => Get( "BatchExportRegexError", name, message );
        public static string BatchExportMode => Get( "BatchExportMode" );
        public static string BatchExportNoTagError( string name ) => Get( "BatchExportNoTagError", name, ExporterConsts_Keys.KEY_BATCH_EXPORTER, ExporterConsts_Keys.KEY_FORMATTED_BATCH_EXPORTER );
        public static string BatchVariableTooltip => Get( "BatchVariableTooltip" );
        public static string FormattedBatchVariableTooltip => Get( "FBatchVariableTooltip" );

        public static string DateVariableTooltip( string example ) => Get( "DateVariableTooltip", example );

        public static string FoldoutObjects( string range ) => Get( "FoldoutObjects", range );
        public static string FoldoutObjectsTooltip => Get( "FoldoutObjectsTooltip" );
        public static string FoldoutReferences( string range ) => Get( "FoldoutReferences", range );
        public static string FoldoutReferencesTooltip => Get( "FoldoutReferencesTooltip" );
        public static string FoldoutExcludes( string range ) => Get( "FoldoutExcludes", range );
        public static string FoldoutExcludesPreview => Get( "FoldoutExcludesPreview" );
        public static string FoldoutExcludeObjects( string range ) => Get( "FoldoutExcludeObjects", range );
        public static string FoldoutDynamicPath( string range ) => Get( "FoldoutDynamicPath", range );
        public static string FoldoutDynamicPathPreview => Get( "FoldoutDynamicPathPreview" );
        public static string FoldoutVariables( string range ) => Get( "FoldoutVariables", range );
        public static string FoldoutBatchExportEnabled => Get( "FoldoutBatchExportEnabled" );
        public static string FoldoutBatchExportDisabled => Get( "FoldoutBatchExportDisabled" );
        public static string FoldoutPostProcessScript => Get( "FoldoutPostProcessScript" );
        public static string Variables => Get( "Variables" );
        public static string Version => Get( "Version" );
        public static string VersionSource => Get( "VersionSource" );
        public static string VersionFormat => Get( "VersionFormat" );
        public static string BatchFormat => Get( "BatchFormat" );
        public static string PackageName => Get( "PackageName" );
        public static string LabelExportPackage => Get( "LabelExportPackage" );
        public static string ButtonAddNameOverride => Get( "ButtonAddNameOverride" );
        public static string ButtonRemoveNameOverride => Get( "ButtonRemoveNameOverride" );
        public static string ButtonCleanNameOverride => Get( "ButtonCleanNameOverride" );
        public static string SettingOverrideVersion => Get( "SettingOverrideVersion" );
        public static string SettingOverrideVersionFormat => Get( "SettingOverrideVersionFormat" );
        public static string SettingOverrideBatchFormat => Get( "SettingOverrideBatchFormat" );
        public static string SettingOverridePackageName => Get( "SettingOverridePackageName" );
        public static string LogCleanNameOverride( int count ) => Get( "LogCleanNameOverride", count );
        public static string ButtonCheck => Get( "ButtonCheck" );
        public static string ButtonExportNone => Get( "ButtonExportNone" );
        public static string ButtonExportAll => Get( "ButtonExportAll" );
        public static string ButtonExportPackage => Get( "ButtonExportPackage" );
        public static string ButtonExportPackages => Get( "ButtonExportPackages" );
        public static string ButtonOpen => Get( "ButtonOpen" );
        public static string DiffLabel => Get( "DiffLabel" );
        public static string DiffTooltip => Get( "DiffTooltip" );
        public static string ButtonFolder => Get( "ButtonFolder" );
        public static string ButtonFile => Get( "ButtonFile" );
        public static string ExportLogNotFound( string path ) => Get( "ExportLogNotFound", path );
        public static string ExportLogFailedTargetEmpty( string path ) => Get( "ExportLogFailedTargetEmpty", path );
        public static string ExportLogFailed( string path ) => Get( "ExportLogFailed", path );
        public static string ExportLogAllFileExists => Get( "ExportLogAllFileExists" );
        public static string ExportLogSuccess( string path ) => Get( "ExportLogSuccess", path );
        public static string ExcludesWereEmpty => Get( "ExcludesWereEmpty" );
        public static string CopyTarget( string text ) => Get( "CopyTarget", text );
        public static string CopyTargetWithValue( string type, int index ) => Get( "CopyTargetWithValue", type, index );
        public static string CopyTargetNoValue => Get( "CopyTargetNoValue" );
        public static string PasteTarget( string text ) => Get( "PasteTarget", text );
        public static string PasteTargetWithValue( string type, string text ) => Get( "PasteTargetWithValue", type, text );
        public static string PasteTargetNoValue => Get( "PasteTargetNoValue" );
        public static string IncompatibleVersion( string name ) => Get( "IncompatibleVersion", name );
        public static string IncompatibleVersionForceOpen => Get( "IncompatibleVersionForceOpen" );
        public static string FileListWindowTitle => Get( "FileListWindowTitle" );
        public static string FileListViewFullPath => Get( "FileListViewFullPath" );
        public static string FileListTreeView => Get( "FileListTreeView" );
        public static string FileListViewReferencedFiles => Get( "FileListViewReferencedFiles" );
        public static string FileListViewExcludeFiles => Get( "FileListViewExcludeFiles" );
        public static string FileListClose => Get( "FileListClose" );
        public static string FileListNotFoundPathPrefix => Get( "FileListNotFoundPathPrefix" );
        public static string FileListExcludesPathPrefix => Get( "FileListExcludesPathPrefix" );
        public static string FileListReferencesPathPrefix => Get( "FileListReferencesPathPrefix" );

        public static string FileListCategoryExport => Get( "FileListCategoryExport" );
        public static string FileListCategoryExportEmpty => Get( "FileListCategoryExportEmpty" );
        public static string FileListCategoryNotFound => Get( "FileListCategoryNotFound" );
        public static string FileListCategoryExcludes => Get( "FileListCategoryExcludes" );
        public static string FileListCategoryReferences => Get( "FileListCategoryReferences" );

        public static string FileListTooltipReferencedBy => Get( "FileListTooltipReferencedBy" );

        public static string ExportListEmpty => Get( "ExportListEmpty" );

        public static string ConvertToRelativePath => Get( "ConvertToRelativePath" );
        public static string ConvertToAbsolutePath => Get( "ConvertToAbsolutePath" );
        public static string ConvertAllPathsToAbsolute => Get( "ConvertAllPathsToAbsolute" );
        public static string ConvertAllPathsToRelative => Get( "ConvertAllPathsToRelative" );
        public static string UseRelativePath => Get( "UseRelativePath" );

        public static string AdvancedMode => Get( "AdvancedMode" );

        public static string UsePostProcessScript => Get( "UsePostProcessScript" );
        public static string UsePostProcessScriptConfirm => Get( "UsePostProcessScriptConfirm" );
        public static string PostProcessScript => Get( "PostProcessScript" );
        public static string PostProcessScriptTooltip => Get( "PostProcessScriptTooltip" );
        public static string PostProcessScriptResetAllFields => Get( "PostProcessScriptResetAllFields" );
        public static string PostProcessScriptFields => Get( "PostProcessScriptFields" );
        public static string PostProcessScriptNotFound => Get( "PostProcessScriptNotFound" );
        public static string PostProcessScriptNotImplement => Get( "PostProcessScriptNotImplement" );
        public static string PostProcessScriptCleanUnusedFields => Get( "PostProcessScriptCleanUnusedFields" );

        public static string EditOnlySingle(string text) => string.Format( Get( "EditOnlySingle" ), text );
    }
}
