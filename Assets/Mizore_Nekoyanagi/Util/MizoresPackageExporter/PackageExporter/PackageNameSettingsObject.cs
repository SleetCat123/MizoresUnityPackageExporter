using UnityEngine;
#if UNITY_EDITOR
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    [CreateAssetMenu( menuName = "MizoreNekoyanagi/PackageNameSettings" )]
    public class PackageNameSettingsObject : ScriptableObject {
        public PackageNameSettings settings = new PackageNameSettings();
    }
}
