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
        /// 先頭が一致
        /// </summary>
        StartsWith,
        /// <summary>
        /// 末尾が一致
        /// </summary>
        EndsWith,
        /// <summary>
        /// 正規表現
        /// </summary>
        Regex,
    }
    [System.Serializable]
    public class SearchPathTypeData : EnumData<SearchPathType> {
        public static implicit operator SearchPathTypeData( SearchPathType value ) {
            return new SearchPathTypeData { value = value };
        }
    }
}
