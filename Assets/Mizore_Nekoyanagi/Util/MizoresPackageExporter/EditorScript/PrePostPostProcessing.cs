namespace MizoreNekoyanagi.PublishUtil.PackageExporter
{
    public class PrePostPostProcessing
    {
        public delegate void Processing( MizoresPackageExporter exporter, int index );
        public Processing export_preprocessing;
        public Processing export_postprocessing;
        public Processing filelist_preprocessing;
    }
}
