#if UNITY_EDITOR
using UnityEditor;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public static class ExporterEditorPrefs {
        private const string PREFIX = "MizorePackageExporter_";

        private const string DEBUG = PREFIX + "DebugMode";
        public static bool DebugMode {
            get => EditorPrefs.GetBool( DEBUG, false );
            set => EditorPrefs.SetBool( DEBUG, value );
        }

        private const string LANGUAGE = PREFIX + "Language";
        public static string Language {
            get => EditorPrefs.GetString( LANGUAGE, ExporterTexts.DEFAULT_KEY );
            set => EditorPrefs.SetString( LANGUAGE, value );
        }

        public const string FOLDOUT_OBJECT = PREFIX + "Foldout_Object";
        //public static bool FoldoutObject {
        //    get => EditorPrefs.GetBool( FOLDOUT_OBJECT, true );
        //    set => EditorPrefs.SetBool( FOLDOUT_OBJECT, value );
        //}

        public const string FOLDOUT_REFERENCES = PREFIX + "Foldout_References";
        //public static bool FoldoutPreferences {
        //    get => EditorPrefs.GetBool( FOLDOUT_REFERENCES, true );
        //    set => EditorPrefs.SetBool( FOLDOUT_REFERENCES, value );
        //}

        public const string FOLDOUT_EXCLUDES = PREFIX + "Foldout_Excludes";
        //public static bool FoldoutExcludes {
        //    get => EditorPrefs.GetBool( FOLDOUT_EXCLUDES, true );
        //    set => EditorPrefs.SetBool( FOLDOUT_EXCLUDES, value );
        //}

        public const string FOLDOUT_EXCLUDES_PREVIEW = PREFIX + "Foldout_ExcludesPreview";
        //public static bool FoldoutExcludesPreview {
        //    get => EditorPrefs.GetBool( FOLDOUT_EXCLUDES_PREVIEW, true );
        //    set => EditorPrefs.SetBool( FOLDOUT_EXCLUDES_PREVIEW, value );
        //}

        public const string FOLDOUT_EXCLUDE_OBJECTS = PREFIX + "Foldout_ExcludeObjects";
        //public static bool FoldoutExcludeObjects {
        //    get => EditorPrefs.GetBool( FOLDOUT_EXCLUDE_OBJECTS, true );
        //    set => EditorPrefs.SetBool( FOLDOUT_EXCLUDE_OBJECTS, value );
        //}

        public const string FOLDOUT_DYNAMICPATH = PREFIX + "Foldout_DynamicPath";
        //public static bool FoldoutDynamicPath {
        //    get => EditorPrefs.GetBool( FOLDOUT_DYNAMICPATH, true );
        //    set => EditorPrefs.SetBool( FOLDOUT_DYNAMICPATH, value );
        //}

        public const string FOLDOUT_DYNAMICPATH_PREVIEW = PREFIX + "Foldout_DynamicPathPreview";
        //public static bool FoldoutDynamicPathPreview {
        //    get => EditorPrefs.GetBool( FOLDOUT_DYNAMICPATH_PREVIEW, true );
        //    set => EditorPrefs.SetBool( FOLDOUT_DYNAMICPATH_PREVIEW, value );
        //}

        public const string FOLDOUT_VARIABLES = PREFIX + "Foldout_Variables";
        //public static bool FoldoutVariables {
        //    get => EditorPrefs.GetBool( FOLDOUT_VARIABLES, true );
        //    set => EditorPrefs.SetBool( FOLDOUT_VARIABLES, value );
        //}

        public const string FOLDOUT_EXPORT_SETTING = PREFIX + "Foldout_ExportSetting";
        //public static bool FoldoutBatchExport {
        //    get => EditorPrefs.GetBool( FOLDOUT_BATCHEXPORT, true );
        //    set => EditorPrefs.SetBool( FOLDOUT_BATCHEXPORT, value );
        //}

        public const string FOLDOUT_POST_EXPORT = PREFIX + "Foldout_PostExport";

        private const string FILELIST_TREEVIEW_FULLPATH = PREFIX + "FileList_FullPath";
        public static bool FileListTreeViewFullPath {
            get => EditorPrefs.GetBool( FILELIST_TREEVIEW_FULLPATH, false );
            set => EditorPrefs.SetBool( FILELIST_TREEVIEW_FULLPATH, value );
        }

        private const string FILELIST_FLATVIEW_FULLPATH = PREFIX + "FileList_FullPath_FlatView";
        public static bool FileListFlatViewFullPath {
            get => EditorPrefs.GetBool( FILELIST_FLATVIEW_FULLPATH, true );
            set => EditorPrefs.SetBool( FILELIST_FLATVIEW_FULLPATH, value );
        }

        private const string FILELIST_VIEW_EXCLUDE_FILES = PREFIX + "FileList_View_ExcludeFiles";
        public static bool FileListViewExcludeFiles {
            get => EditorPrefs.GetBool( FILELIST_VIEW_EXCLUDE_FILES, true );
            set => EditorPrefs.SetBool( FILELIST_VIEW_EXCLUDE_FILES, value );
        }

        private const string FILELIST_VIEW_REFERENCED_FILES = PREFIX + "FileList_View_ReferencedFiles";
        public static bool FileListViewReferencedFiles {
            get => EditorPrefs.GetBool( FILELIST_VIEW_REFERENCED_FILES, true );
            set => EditorPrefs.SetBool( FILELIST_VIEW_REFERENCED_FILES, value );
        }

        private const string FILELIST_TREEVIEW = PREFIX + "FileList_TreeView";
        public static bool FileListTreeView {
            get => EditorPrefs.GetBool( FILELIST_TREEVIEW, true );
            set => EditorPrefs.SetBool( FILELIST_TREEVIEW, value );
        }
        private const string DEFAULT_PATH_TYPE = PREFIX + "DefaultPathType";
        public static PathType DefaultPathType {
            get => ( PathType )EditorPrefs.GetInt( DEFAULT_PATH_TYPE, ( int )PathType.Relative );
            set => EditorPrefs.SetInt( DEFAULT_PATH_TYPE, ( int )value );
        }

        private const string ADVANCED_MODE = PREFIX + "AdvancedMode";
        public static bool AdvancedMode {
            get => EditorPrefs.GetBool( ADVANCED_MODE, false );
            set => EditorPrefs.SetBool( ADVANCED_MODE, value );
        }
    }
}
#endif
