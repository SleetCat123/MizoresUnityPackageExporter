#if UNITY_EDITOR
#endif

using System.Collections.Generic;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public abstract class ExportPostProcess {
        public abstract void OnExported( MizoresPackageExporter packageExporter, string packagePath, FilePathList list, ExporterEditorLogs logs );

        public virtual List<PostProcessFileListElement> GetPathList( MizoresPackageExporter packageExporter, FilePathList list ) {
            return new List<PostProcessFileListElement>( );
        }
    }
}
