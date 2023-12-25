#if UNITY_EDITOR
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public interface IExportPostProcess {
        void OnExported( MizoresPackageExporter packageExporter, string packagePath, FilePathList list, ExporterEditorLogs logs );
    }
}
