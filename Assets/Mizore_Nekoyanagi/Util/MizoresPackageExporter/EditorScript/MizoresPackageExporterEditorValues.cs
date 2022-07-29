namespace MizoreNekoyanagi.PublishUtil.PackageExporter
{
    public class MizoresPackageExporterEditorValues
    {
        public string _helpBoxText;
#if UNITY_EDITOR
        public UnityEditor.MessageType _helpBoxMessageType;
#endif
        public string _variableKeyTemp;
    }
}