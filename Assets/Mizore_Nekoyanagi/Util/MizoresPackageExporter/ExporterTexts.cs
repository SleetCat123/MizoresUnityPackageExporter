#if UNITY_EDITOR
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter
{
    public class ExporterTexts
    {
        public const string TEXT_UNDO = "PackagePrefs";
        public const string TEXT_OBJECTS = "Objects ({0})";
        public const string TEXT_REFERENCES = "References ({0})";
        public const string TEXT_EXCLUDES = "Excludes ({0})";
        public const string TEXT_EXCLUDES_PREVIEW = "Excludes Preview";
        public const string TEXT_EXCLUDE_OBJECTS = "Exclude Objects ({0})";
        public const string TEXT_DYNAMIC_PATH = "Dynamic Path ({0})";
        public const string TEXT_DYNAMIC_PATH_PREVIEW = "Dynamic Path Preview";
        public const string TEXT_DYNAMIC_PATH_VARIABLES = "Dynamic Path Variables ({0})";
        public const string TEXT_VERSION_FILE = "Version File";
        public const string TEXT_VERSION_FORMAT = "Format";
        public const string TEXT_PACKAGE_NAME = "Package Name";
        public const string TEXT_BUTTON_CHECK = "Check";
        public const string TEXT_BUTTON_EXPORT = "Export to unitypackage";
        public const string TEXT_BUTTON_EXPORT_M = "Export to unitypackages";
        public const string TEXT_BUTTON_OPEN = "Open";
        public const string TEXT_DIFF_LABEL = "?";
        public const string EN_TEXT_DIFF_TOOLTIP = "Some values are different.";
        public const string JP_TEXT_DIFF_TOOLTIP = "一部のオブジェクトの値が異なっています。";
        public const string TEXT_BUTTON_FOLDER = "Folder";
        public const string TEXT_BUTTON_FILE = "File";
        public const string TEXT_EXPORT_LOG_NOT_FOUND_PATH_PREFIX = "[ ! Not Found ! ] ";
        public const string TEXT_EXPORT_LOG_DEPENDENCY_PATH_PREFIX = "[Dependency]";
        public const string EN_TEXT_EXPORT_LOG_NOT_FOUND = "[{0}] is not exists.\n";
        public const string JP_TEXT_EXPORT_LOG_NOT_FOUND = "[{0}]は存在しません。\n";
        public const string EN_TEXT_EXPORT_LOG_FAILED = "Export has been cancelled.\n";
        public const string JP_TEXT_EXPORT_LOG_FAILED = "エクスポートは中断されました。\n";
        public const string EN_TEXT_EXPORT_LOG_ALL_FILE_EXISTS = "All files or directories exist.\n";
        public const string JP_TEXT_EXPORT_LOG_ALL_FILE_EXISTS = "ファイル／フォルダの存在チェックが完了しました。\n";
        public const string EN_TEXT_EXPORT_LOG_SUCCESS = "[{0}] Export completed.\n";
        public const string JP_TEXT_EXPORT_LOG_SUCCESS = "[{0}]のエクスポートに成功しました。\n";
        public const string TEXT_COPY_TARGET = "Copy {0}";
        public const string TEXT_COPY_TARGET_WITH_VALUE = "Copy [{1}] ({0})";
        public const string TEXT_PASTE_TARGET = "Paste {0}";
        public const string TEXT_PASTE_TARGET_WITH_VALUE = "Paste [{1}] ({0})";
        public const string TEXT_PASTE_TARGET_NO_VALUE = "Paste";
        public const string TEXT_INCOMPATIBLE_VERSION = "This object is incompatible with the current version.\nこのオブジェクトは現在のバージョンとの互換性がありません。";
        public const string TEXT_FILELIST_VIEW_FULLPATH = "FullPath";
        public const string TEXT_FILELIST_CLOSE = "Close";

        public static string t_Undo => TEXT_UNDO;
        public static string t_Objects => TEXT_OBJECTS;
        public static string t_References => TEXT_REFERENCES;
        public static string t_Excludes => TEXT_EXCLUDES;
        public static string t_ExcludesPreview => TEXT_EXCLUDES_PREVIEW;
        public static string t_ExcludeObjects => TEXT_EXCLUDE_OBJECTS;
        public static string t_DynamicPath => TEXT_DYNAMIC_PATH;
        public static string t_DynamicPathPreview => TEXT_DYNAMIC_PATH_PREVIEW;
        public static string t_DynamicPath_Variables => TEXT_DYNAMIC_PATH_VARIABLES;
        public static string t_VersionFile => TEXT_VERSION_FILE;
        public static string t_VersionFormat => TEXT_VERSION_FORMAT;
        public static string t_PackageName => TEXT_PACKAGE_NAME;
        public static string t_Label_ExportPackage => TEXT_BUTTON_EXPORT;
        public static string t_Button_Check => TEXT_BUTTON_CHECK;
        public static string t_Button_ExportPackage => TEXT_BUTTON_EXPORT;
        public static string t_Button_ExportPackages => TEXT_BUTTON_EXPORT_M;
        public static string t_Button_Open => TEXT_BUTTON_OPEN;
        public static string t_Diff_Label => TEXT_DIFF_LABEL;
        public static string t_Diff_Tooltip => EN_TEXT_DIFF_TOOLTIP + JP_TEXT_DIFF_TOOLTIP;
        public static string t_Button_Folder => TEXT_BUTTON_FOLDER;
        public static string t_Button_File => TEXT_BUTTON_FILE;
        public static string t_ExportLog_NotFound => EN_TEXT_EXPORT_LOG_NOT_FOUND + JP_TEXT_EXPORT_LOG_NOT_FOUND;
        public static string t_ExportLog_NotFoundPathPrefix => TEXT_EXPORT_LOG_NOT_FOUND_PATH_PREFIX;
        public static string t_ExportLog_DependencyPathPrefix => TEXT_EXPORT_LOG_DEPENDENCY_PATH_PREFIX;
        public static string t_ExportLog_Failed => EN_TEXT_EXPORT_LOG_FAILED + JP_TEXT_EXPORT_LOG_FAILED;
        public static string t_ExportLog_AllFileExists => EN_TEXT_EXPORT_LOG_ALL_FILE_EXISTS + JP_TEXT_EXPORT_LOG_ALL_FILE_EXISTS;
        public static string t_ExportLog_Success => EN_TEXT_EXPORT_LOG_SUCCESS + JP_TEXT_EXPORT_LOG_SUCCESS;
        public static string t_CopyTarget => TEXT_COPY_TARGET;
        public static string t_CopyTargetWithValue => TEXT_COPY_TARGET_WITH_VALUE;
        public static string t_PasteTarget => TEXT_PASTE_TARGET;
        public static string t_PasteTargetWithValue => TEXT_PASTE_TARGET_WITH_VALUE;
        public static string t_PasteTargetNoValue => TEXT_PASTE_TARGET_NO_VALUE;
        public static string t_IncompatibleVersion => TEXT_INCOMPATIBLE_VERSION;
        public static string t_FileList_ViewFullPath => TEXT_FILELIST_VIEW_FULLPATH;
        public static string t_FileList_Close => TEXT_FILELIST_CLOSE;
    }
}
