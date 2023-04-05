using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
#endif

namespace MizoreNekoyanagi.PublishUtil.PackageExporter
{
    public struct MinMax
    {
        public int min, max;
        public bool SameValue => min == max;

        public MinMax( int min, int max ) {
            this.min = min;
            this.max = max;
        }

        public string GetRangeString( ) {
            if ( SameValue ) {
                return min.ToString( );
            } else {
                return $"{min}-{max}";
            }
        }

        public static MinMax Create<T>( IEnumerable<T> list, System.Func<T, int> func ) {
            int min = list.Min( func );
            int max = list.Max( func );
            return new MinMax( min, max );
        }
    }
}
