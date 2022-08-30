namespace MizoreNekoyanagi.PublishUtil.PackageExporter
{
    public static class MizoresPackageExporterConsts
    {
        public const string EXPORT_FOLDER_PATH = "MizorePackageExporter/";

        public const string EDITOR_PREF_PREFIX = "MizorePackageExporter_";
        public const string EDITOR_PREF_FOLDOUT_OBJECT = EDITOR_PREF_PREFIX + "Foldout_Object";
        public const string EDITOR_PREF_FOLDOUT_REFERENCES = EDITOR_PREF_PREFIX + "Foldout_References";
        public const string EDITOR_PREF_FOLDOUT_EXCLUDES = EDITOR_PREF_PREFIX + "Foldout_Excludes";
        public const string EDITOR_PREF_FOLDOUT_EXCLUDES_PREVIEW = EDITOR_PREF_PREFIX + "Foldout_ExcludesPreview";
        public const string EDITOR_PREF_FOLDOUT_EXCLUDE_OBJECTS = EDITOR_PREF_PREFIX + "Foldout_ExcludeObjects";
        public const string EDITOR_PREF_FOLDOUT_DYNAMICPATH = EDITOR_PREF_PREFIX + "Foldout_DynamicPath";
        public const string EDITOR_PREF_FOLDOUT_DYNAMICPATH_PREVIEW = EDITOR_PREF_PREFIX + "Foldout_DynamicPathPreview";
        public const string EDITOR_PREF_FOLDOUT_DYNAMICPATH_VARIABLES = EDITOR_PREF_PREFIX + "Foldout_DynamicPathVariables";
        public const string EDITOR_PREF_FOLDOUT_VERSIONFILE = EDITOR_PREF_PREFIX + "Foldout_VersionFile";

        public const string EDITOR_PREF_FILELIST_VIEW_FULLPATH = EDITOR_PREF_PREFIX + "FileList_FullPath";
    }
    public static class MizoresPackageExporterConsts_Keys
    {
        public const string KEY_NAME= "%name%";
        public const string KEY_VERSION = "%version%";
        public const string KEY_FORMATTED_VERSION = "%versionf%";
        public const string KEY_PACKAGE_NAME= "%packagename%";
    }
}
