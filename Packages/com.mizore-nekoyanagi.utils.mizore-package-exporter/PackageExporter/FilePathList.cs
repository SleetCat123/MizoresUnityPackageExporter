using System.Collections.Generic;
#if UNITY_EDITOR
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public class FilePath : System.IEquatable<FilePath> {
        public string path;
        public bool searchReference;
        public FilePath( string path, bool searchReference ) {
            this.path = path.Replace( '\\', '/' );
            this.searchReference = searchReference;
        }

        public bool Equals( FilePath other ) {
            return this.path == other.path && this.searchReference == other.searchReference;
        }
        public override int GetHashCode( ) {
            return path.GetHashCode( ) ^ searchReference.GetHashCode( );
        }
    }

    /// <summary>
    /// 追加コピーパスのプレビュー用要素
    /// </summary>
    public class AdditionalCopyPathPreview {
        public string sourcePath;
        public string destName;
        public AdditionalCopyPathPreview( string sourcePath, string destName ) {
            this.sourcePath = sourcePath;
            this.destName = destName;
        }
    }

    public class FilePathList {
        public string batchExportKey;
        public IEnumerable<string> paths;
        public IEnumerable<string> excludePaths;
        public Dictionary<string, HashSet<string>> referencedPaths;
        public IEnumerable<AdditionalCopyPathPreview> additionalCopyPaths;
    }
}
