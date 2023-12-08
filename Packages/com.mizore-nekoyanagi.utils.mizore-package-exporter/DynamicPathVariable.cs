namespace MizoreNekoyanagi.PublishUtil.PackageExporter
{
    [System.Serializable]
    public class DynamicPathVariable
    {
        public string key, value;

        public DynamicPathVariable( string key, string value ) {
            this.key = key;
            this.value = value;
        }
    }
}
