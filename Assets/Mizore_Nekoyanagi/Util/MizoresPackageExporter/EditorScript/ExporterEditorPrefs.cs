#if UNITY_EDITOR
namespace MizoreNekoyanagi.PublishUtil.PackageExporter
{
    public static class ExporterEditorPrefs
    {
        private const string PREFIX = "MizorePackageExporter_";

        private const string DEBUG = PREFIX + "DebugMode";
        public static bool DebugMode {
            get => EditorPrefsCache.GetBool( DEBUG, false );
            set => EditorPrefsCache.SetBool( DEBUG, value );
        }

        private const string LANGUAGE = PREFIX + "Language";
        public static string Language {
            get => EditorPrefsCache.GetString( LANGUAGE, ExporterTexts.DEFAULT_KEY );
            set => EditorPrefsCache.SetString( LANGUAGE, value );
        }

        public const string FOLDOUT_OBJECT = PREFIX + "Foldout_Object";
        //public static bool FoldoutObject {
        //    get => EditorPrefsCache.GetBool( FOLDOUT_OBJECT, true );
        //    set => EditorPrefsCache.SetBool( FOLDOUT_OBJECT, value );
        //}

        public const string FOLDOUT_REFERENCES = PREFIX + "Foldout_References";
        //public static bool FoldoutPreferences {
        //    get => EditorPrefsCache.GetBool( FOLDOUT_REFERENCES, true );
        //    set => EditorPrefsCache.SetBool( FOLDOUT_REFERENCES, value );
        //}

        public const string FOLDOUT_EXCLUDES = PREFIX + "Foldout_Excludes";
        //public static bool FoldoutExcludes {
        //    get => EditorPrefsCache.GetBool( FOLDOUT_EXCLUDES, true );
        //    set => EditorPrefsCache.SetBool( FOLDOUT_EXCLUDES, value );
        //}

        public const string FOLDOUT_EXCLUDES_PREVIEW = PREFIX + "Foldout_ExcludesPreview";
        //public static bool FoldoutExcludesPreview {
        //    get => EditorPrefsCache.GetBool( FOLDOUT_EXCLUDES_PREVIEW, true );
        //    set => EditorPrefsCache.SetBool( FOLDOUT_EXCLUDES_PREVIEW, value );
        //}

        public const string FOLDOUT_EXCLUDE_OBJECTS = PREFIX + "Foldout_ExcludeObjects";
        //public static bool FoldoutExcludeObjects {
        //    get => EditorPrefsCache.GetBool( FOLDOUT_EXCLUDE_OBJECTS, true );
        //    set => EditorPrefsCache.SetBool( FOLDOUT_EXCLUDE_OBJECTS, value );
        //}

        public const string FOLDOUT_DYNAMICPATH = PREFIX + "Foldout_DynamicPath";
        //public static bool FoldoutDynamicPath {
        //    get => EditorPrefsCache.GetBool( FOLDOUT_DYNAMICPATH, true );
        //    set => EditorPrefsCache.SetBool( FOLDOUT_DYNAMICPATH, value );
        //}

        public const string FOLDOUT_DYNAMICPATH_PREVIEW = PREFIX + "Foldout_DynamicPathPreview";
        //public static bool FoldoutDynamicPathPreview {
        //    get => EditorPrefsCache.GetBool( FOLDOUT_DYNAMICPATH_PREVIEW, true );
        //    set => EditorPrefsCache.SetBool( FOLDOUT_DYNAMICPATH_PREVIEW, value );
        //}

        public const string FOLDOUT_VARIABLES = PREFIX + "Foldout_Variables";
        //public static bool FoldoutVariables {
        //    get => EditorPrefsCache.GetBool( FOLDOUT_VARIABLES, true );
        //    set => EditorPrefsCache.SetBool( FOLDOUT_VARIABLES, value );
        //}

        public const string FOLDOUT_EXPORT_SETTING = PREFIX + "Foldout_ExportSetting";
        //public static bool FoldoutBatchExport {
        //    get => EditorPrefsCache.GetBool( FOLDOUT_BATCHEXPORT, true );
        //    set => EditorPrefsCache.SetBool( FOLDOUT_BATCHEXPORT, value );
        //}

        private const string FILELIST_TREEVIEW_FULLPATH = PREFIX + "FileList_FullPath";
        public static bool FileListTreeViewFullPath {
            get => EditorPrefsCache.GetBool( FILELIST_TREEVIEW_FULLPATH, false );
            set => EditorPrefsCache.SetBool( FILELIST_TREEVIEW_FULLPATH, value );
        }

        private const string FILELIST_FLATVIEW_FULLPATH = PREFIX + "FileList_FullPath_FlatView";
        public static bool FileListFlatViewFullPath {
            get => EditorPrefsCache.GetBool( FILELIST_FLATVIEW_FULLPATH, true );
            set => EditorPrefsCache.SetBool( FILELIST_FLATVIEW_FULLPATH, value );
        }

        private const string FILELIST_VIEW_EXCLUDE_FILES = PREFIX + "FileList_View_ExcludeFiles";
        public static bool FileListViewExcludeFiles {
            get => EditorPrefsCache.GetBool( FILELIST_VIEW_EXCLUDE_FILES, true );
            set => EditorPrefsCache.SetBool( FILELIST_VIEW_EXCLUDE_FILES, value );
        }

        private const string FILELIST_VIEW_REFERENCED_FILES = PREFIX + "FileList_View_ReferencedFiles";
        public static bool FileListViewReferencedFiles {
            get => EditorPrefsCache.GetBool( FILELIST_VIEW_REFERENCED_FILES, true );
            set => EditorPrefsCache.SetBool( FILELIST_VIEW_REFERENCED_FILES, value );
        }

        private const string FILELIST_TREEVIEW = PREFIX + "FileList_TreeView";
        public static bool FileListTreeView {
            get => EditorPrefsCache.GetBool( FILELIST_TREEVIEW, true );
            set => EditorPrefsCache.SetBool( FILELIST_TREEVIEW, value );
        }
    }
}
#endif
