namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    [System.Serializable]
    public class DynamicPathElement {
        public string path;
        public bool searchReference;
        public DynamicPathElement( string path, bool searchReference ) {
            this.path = path;
            this.searchReference = searchReference;
        }
        public DynamicPathElement( string path ) {
            this.path = path;
            this.searchReference = true;
        }
        public DynamicPathElement() { 
            this.path = string.Empty;
            this.searchReference = true;
        }
    }
}
