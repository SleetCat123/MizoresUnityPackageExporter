using System.Collections.Generic;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public abstract class ExportPostProcess {
        public abstract void OnExported( MizoresPackageExporter packageExporter, string batchExportKey, string packagePath, FilePathList list, ExporterEditorLogs logs );

        public virtual List<PostProcessFileListElement> GetPathList( MizoresPackageExporter packageExporter, string batchExportKey, FilePathList list ) {
            return new List<PostProcessFileListElement>( );
        }
    }
}
