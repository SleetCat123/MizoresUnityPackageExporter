namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public enum SearchPathType {
        Disabled,
        /// <summary>
        /// 完全一致
        /// </summary>
        Exact,
        /// <summary>
        /// 部分一致
        /// </summary>
        Partial,
        /// <summary>
        /// 部分一致（大文字小文字を無視）
        /// </summary>
        Partial_IgnoreCase,
        /// <summary>
        /// 正規表現
        /// </summary>
        Regex,
        /// <summary>
        /// 正規表現（大文字小文字を無視）
        /// </summary>
        Regex_IgnoreCase,
    }
    [System.Serializable]
    public class SearchPathTypeData : EnumData<SearchPathType> {
        public static implicit operator SearchPathTypeData( SearchPathType value ) {
            return new SearchPathTypeData { value = value };
        }
    }
}
