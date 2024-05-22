using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ExporterUtils = MizoreNekoyanagi.PublishUtil.PackageExporter.ExporterUtils;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporterV1 {
    [Obsolete]
    public class MizoresPackageExporterV1 : ScriptableObject, ISerializationCallbackReceiver {
#if UNITY_EDITOR
        [CustomEditor( typeof( MizoresPackageExporterV1 ) )]
        public class Inspector : Editor {
            public override void OnInspectorGUI( ) {
                EditorGUILayout.HelpBox( PackageExporter.ExporterTexts.ConvertVersionRequired, MessageType.Warning );
                if ( GUILayout.Button( PackageExporter.ExporterTexts.ConvertVersionButton ) ) {
                    if ( EditorUtility.DisplayDialog( "MizoresPackageExporter", PackageExporter.ExporterTexts.ConvertVersionConfirm, PackageExporter.ExporterTexts.ConvertVersionButton, PackageExporter.ExporterTexts.Cancel ) ) {
                        foreach ( var target in targets ) {
                            var v1 = target as MizoresPackageExporterV1;
                            PackageExporter.MizoresPackageExporterUpdator.ConvertToLatest( v1 );
                        }
                    }
                }
            }
        }
#endif

        public const int CURRENT_PACKAGE_EXPORTER_OBJECT_VERSION = 1;

        [System.Serializable]
        private class PackageNameSettingsKVP {
            public string key;
            public PackageNameSettings value;

            public PackageNameSettingsKVP( string key, PackageNameSettings value ) {
                this.key = key;
                this.value = value;
            }
        }

        [SerializeField]
        public int packageExporterVersion = PackageExporter.MizoresPackageExporter.INITIAL_PACKAGE_EXPORTER_OBJECT_VERSION;

        public List<PackagePrefsElement> objects = new List<PackagePrefsElement>( );
        public List<string> dynamicpath = new List<string>( );
        [SerializeField]
        DynamicPathVariable[] s_variables;
        [System.NonSerialized]
        public Dictionary<string, string> variables = new Dictionary<string, string>( );

        public List<PackagePrefsElement> excludeObjects = new List<PackagePrefsElement>( );
        public List<SearchPath> excludes = new List<SearchPath>( );
        public List<PackagePrefsElement> references = new List<PackagePrefsElement>( );

        /// <summary>互換性のため残しておく。今後はpackageNameSettings.versionFileを使用</summary>
        [System.Obsolete, SerializeField]
        public PackagePrefsElement versionFile;

        /// <summary>互換性のため残しておく。今後はpackageNameSettings.versionFormatを使用</summary>
        [System.Obsolete, SerializeField]
        public string versionFormat = null;

        /// <summary>互換性のため残しておく。今後はpackageNameSettings.packageNameを使用</summary>
        [System.Obsolete, SerializeField]
        public string packageName = null;

        public PackageNameSettings packageNameSettings = new PackageNameSettings( );

        [SerializeField]
        PackageNameSettingsKVP[] s_packageNameSettingsOverride;
        [System.NonSerialized]
        public Dictionary<string, PackageNameSettings> packageNameSettingsOverride = new Dictionary<string, PackageNameSettings>( );

        #region BatchExport
        public BatchExportMode batchExportMode;
        public BatchExportFolderMode batchExportFolderMode;
        public List<string> batchExportTexts = new List<string>();
        public PackagePrefsElement batchExportFolderRoot;
        public PackagePrefsElement batchExportListFile;
        public string batchExportFolderRegex;
        #endregion

        public class FilePathList {
            public IEnumerable<string> paths;
            public IEnumerable<string> excludePaths;
            public Dictionary<string, HashSet<string>> referencedPaths;
        }

        public void OnBeforeSerialize( ) {
            s_variables = variables.Select( kvp => new DynamicPathVariable( kvp.Key, kvp.Value ) ).ToArray( );
            s_packageNameSettingsOverride = packageNameSettingsOverride.Select( kvp => new PackageNameSettingsKVP( kvp.Key, kvp.Value ) ).ToArray( );
        }

        public void OnAfterDeserialize( ) {
            if ( s_variables != null ) {
                variables = s_variables.ToDictionary( v => v.key, v => v.value );
            }
            if ( s_packageNameSettingsOverride != null ) {
                packageNameSettingsOverride = s_packageNameSettingsOverride.ToDictionary( v => v.key, v => v.value );
            }
        }

        public enum BatchExportFolderMode {
            All, Files, Folders
        }
        public enum BatchExportMode {
            Single, Texts, Folders, ListFile
        }
        [System.Serializable]
        public class PackagePrefsElement {
            [SerializeField]
            public UnityEngine.Object obj;
            [SerializeField]
            public string path;
            public virtual string Path {
                get {
#if UNITY_EDITOR
                    if ( obj != null ) {
                        path = AssetDatabase.GetAssetPath( obj );
                    }
#endif
                    return path;
                }
            }
            public bool IsEmpty( ) {
                return obj == null && string.IsNullOrEmpty( path );
            }
        }
        [System.Serializable]
        public class DynamicPathVariable {
            public string key, value;

            public DynamicPathVariable( string key, string value ) {
                this.key = key;
                this.value = value;
            }
        }
        public enum VersionSource {
            String, File
        }
        [System.Serializable]
        public class PackageNameSettings {
            /// <summary>jsonのdeserialize用</summary>
            [System.Serializable]
            private class VersionJson {
                public string version;
            }

            public VersionSource versionSource;
            public PackagePrefsElement versionFile;
            public string versionString;
            public string versionFormat;
            public string batchFormat;
            public string packageName;

            public bool useOverride_version = true;
            public bool useOverride_versionFormat;
            public bool useOverride_batchFormat;
            public bool useOverride_packageName;

            public static implicit operator PackageExporter.PackageNameSettings( PackageNameSettings source ) {
                return new PackageExporter.PackageNameSettings( ) {
                    versionSource = ( PackageExporter.VersionSource )source.versionSource,
                    versionFile = new PackageExporter.ObjectRefElement( source.versionFile.Path ),
                    versionString = source.versionString,
                    versionFormat = source.versionFormat,
                    batchFormat = source.batchFormat,
                    packageName = source.packageName,
                    useOverride_version = source.useOverride_version,
                    useOverride_versionFormat = source.useOverride_versionFormat,
                    useOverride_batchFormat = source.useOverride_batchFormat,
                    useOverride_packageName = source.useOverride_packageName,
                };
            }
        }
        [System.Serializable]
        public class SearchPath : ISerializationCallbackReceiver {
            // 互換性のためSerialize対象にしておく
            public SearchPathType searchType;
            [SerializeField]string s_searchType;
            public string value;

            public void OnBeforeSerialize( ) {
                s_searchType = SearchPathTypeExtensions.GetString( searchType );
            }

            public void OnAfterDeserialize( ) {
                if ( string.IsNullOrEmpty( s_searchType ) ) {
                    // enumのstring保存が未実装なデータを読み込んだ場合のみ発生
                    // enumの順番が変わった場合はここで対応する
                } else {
                    searchType = SearchPathTypeExtensions.Parse( s_searchType );
                }
            }
            public enum SearchPathType {
                Disabled,
                /// <summary>
                /// 完全一致
                /// </summary>
                Exact,
                /// <summary>
                /// 部分一致
                /// </summary>
                Partial,
                /// <summary>
                /// 部分一致（大文字小文字を無視）
                /// </summary>
                Partial_IgnoreCase,
                /// <summary>
                /// 正規表現
                /// </summary>
                Regex,
                /// <summary>
                /// 正規表現（大文字小文字を無視）
                /// </summary>
                Regex_IgnoreCase,
            }
            public static class SearchPathTypeExtensions {
                public static string GetString( SearchPathType value ) {
                    switch ( value ) {
                        case SearchPathType.Disabled: return "Disabled";
                        case SearchPathType.Exact: return "Exact";
                        case SearchPathType.Partial: return "Partial";
                        case SearchPathType.Partial_IgnoreCase: return "Partial_IgnoreCase";
                        case SearchPathType.Regex: return "Regex";
                        case SearchPathType.Regex_IgnoreCase: return "Regex_IgnoreCase";
                        default: throw new System.ArgumentException( );
                    }
                }
                public static SearchPathType Parse( string value ) {
                    switch ( value ) {
                        case "Disabled": return SearchPathType.Disabled;
                        case "Exact": return SearchPathType.Exact;
                        case "Partial": return SearchPathType.Partial;
                        case "Partial_IgnoreCase": return SearchPathType.Partial_IgnoreCase;
                        case "Regex": return SearchPathType.Regex;
                        case "Regex_IgnoreCase": return SearchPathType.Regex_IgnoreCase;
                        default: throw new System.ArgumentException( );
                    }
                }
            }
        }
    }
}