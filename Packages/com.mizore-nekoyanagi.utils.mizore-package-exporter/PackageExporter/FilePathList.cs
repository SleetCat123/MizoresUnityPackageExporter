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
    public class FilePathList {
        public IEnumerable<string> paths;
        public IEnumerable<string> excludePaths;
        public Dictionary<string, HashSet<string>> referencedPaths;
    }
}
