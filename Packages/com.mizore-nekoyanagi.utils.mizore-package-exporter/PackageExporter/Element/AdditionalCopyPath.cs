using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    /// <summary>
    /// エクスポート後に追加でコピーするファイル/フォルダの設定
    /// </summary>
    [System.Serializable]
    public class AdditionalCopyPath : System.ICloneable {
        [Tooltip( "コピー元のパス" )]
        public ObjectRefElement sourcePath = new ObjectRefElement();

        [Tooltip( "出力先の名前（空の場合は元の名前を使用）。変数置き換え可能" )]
        public string destName = "";

        public AdditionalCopyPath() { }

        public AdditionalCopyPath( string path, string destName = "" ) {
            this.sourcePath = new ObjectRefElement( path );
            this.destName = destName;
        }

        public AdditionalCopyPath( AdditionalCopyPath source ) {
            this.sourcePath = source.sourcePath?.Clone() as ObjectRefElement;
            this.destName = source.destName;
        }

        /// <summary>
        /// 変数置き換え後のソースパスを取得
        /// </summary>
        public string GetConvertedSourcePath( MizoresPackageExporter exporter, string batchExportKey ) {
            if ( sourcePath == null ) {
                return string.Empty;
            }
            return sourcePath.GetConvertedPath( exporter, batchExportKey );
        }

        /// <summary>
        /// 変数置き換え後の出力先名を取得（空の場合はnull）
        /// </summary>
        public string GetConvertedDestName( MizoresPackageExporter exporter, string batchExportKey ) {
            if ( string.IsNullOrWhiteSpace( destName ) ) {
                return null;
            }
            return exporter.ConvertDynamicPath( destName, batchExportKey );
        }

        public object Clone() {
            return new AdditionalCopyPath( this );
        }
    }
}
