using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter.BatchExport
{
    [CreateAssetMenu( menuName = "MizoreNekoyanagi/BatchExporter" )]
    public class BatchExporter : ScriptableObject
    {
        public PackagePrefsElement root;
        public MizoresPackageExporter target;
        public string regex;
    }
}