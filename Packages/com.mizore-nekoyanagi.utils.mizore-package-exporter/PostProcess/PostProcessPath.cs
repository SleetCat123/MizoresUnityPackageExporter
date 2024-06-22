using System.Collections.Generic;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public class PostProcessFileListElement {
        public string path;
        public IEnumerable<string> args;
        public PostProcessFileListElement( string path, IEnumerable<string> args ) {
            this.path = path;
            this.args = args;
        }
        public PostProcessFileListElement( string path ) {
            this.path = path;
            this.args = null;
        }
        public PostProcessFileListElement( string path, params string[] args ) {
            this.path = path;
            this.args = args;
        }
    }
}
