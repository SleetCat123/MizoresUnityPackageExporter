#if UNITY_EDITOR
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public interface IExportPostProcess {
        void OnExported( string exporterPath, string packagePath, FilePathList list );
    }
}
