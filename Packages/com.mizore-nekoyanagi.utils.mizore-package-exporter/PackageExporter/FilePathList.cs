using System.Collections.Generic;
#if UNITY_EDITOR
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public class FilePathList {
        public IEnumerable<string> paths;
        public IEnumerable<string> excludePaths;
        public Dictionary<string, HashSet<string>> referencedPaths;
    }
}
