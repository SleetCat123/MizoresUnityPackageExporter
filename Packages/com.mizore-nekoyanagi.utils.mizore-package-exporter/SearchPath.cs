using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    [System.Serializable]
    public class SearchPath : System.IEquatable<SearchPath>, System.ICloneable {
        public SearchPathTypeData searchType;
        [SerializeField]
        private bool preserveCase;
        public bool PreserveCase {
            get {
                return preserveCase;
            }
            set {
                preserveCase = value;
                UpdateRegex( );
            }
        }
        [SerializeField]
        private string value;
        public string Value {
            get {
                return value;
            }
            set {
                this.value = value;
                UpdateRegex( );
            }
        }
        Regex regex;
        void UpdateRegex( ) {
            if ( searchType.value != SearchPathType.Regex ) {
                regex = null;
                return;
            }
            try {
                regex = new Regex( value, preserveCase ? RegexOptions.None : RegexOptions.IgnoreCase );
            } catch ( System.Exception e ) {
                Debug.LogError( e );
                regex = null;
            }
        }

        public SearchPath( ) {
            this.searchType = SearchPathType.Partial;
            this.preserveCase = false;
            this.value = string.Empty;
        }
        public SearchPath( SearchPathType searchType, bool preserveCase, string value ) {
            this.searchType = searchType;
            this.preserveCase = preserveCase;
            this.value = value;
        }
        public SearchPath( SearchPath source ) {
            this.searchType = source.searchType.value;
            this.preserveCase = source.preserveCase;
            this.value = source.value;
        }
        public object Clone( ) {
            return new SearchPath( this );
        }

        public override string ToString( ) {
            return $"{value}({EnumCache.GetName( searchType )} {( preserveCase ? "(PreserveCase)" : "" )})";
        }

        public override int GetHashCode( ) {
            return value.GetHashCode( ) ^ preserveCase.GetHashCode( ) ^ searchType.GetHashCode( );
        }
        public bool Equals( SearchPath other ) {
            return this.value == other.value && this.searchType == other.searchType && this.preserveCase == other.preserveCase;
        }
        public override bool Equals( object obj ) {
            return Equals( ( SearchPath )obj );
        }
        public static bool operator ==( SearchPath a, SearchPath b ) {
            return a.Equals( b );
        }
        public static bool operator !=( SearchPath a, SearchPath b ) {
            return !a.Equals( b );
        }

        public bool IsMatch( string path ) {
            if ( searchType.value == SearchPathType.Regex ) {
                UpdateRegex( );
                return regex.IsMatch( path );
            }
            var a = path;
            var b = value;
            if ( preserveCase == false ) {
                a = path.ToLowerInvariant( );
                b = value.ToLowerInvariant( );
            }
            switch ( searchType.value ) {
                default:
                    throw new System.Exception( $"Unknown search type: {searchType.value}" );
                case SearchPathType.Disabled:
                    return false;
                case SearchPathType.Exact:
                    return a == b;
                case SearchPathType.Partial:
                    return a.Contains( b );
                case SearchPathType.StartsWith:
                    return a.StartsWith( b );
                case SearchPathType.EndsWith:
                    return a.EndsWith( b );
            }
        }
        public IEnumerable<string> GetMatchPaths( IEnumerable<string> paths, bool includeSubfiles ) {
            ExporterUtils.DebugLog( ToString( ) );
            ExporterUtils.DebugLog( "Paths: \n" + string.Join( "\n", paths ) + "\n" );

            if ( searchType == SearchPathType.Disabled || string.IsNullOrEmpty( value ) ) {
                return new string[0];
            }

            HashSet<string> folders = new HashSet<string>( );
            List<string> result = new List<string>( );
            foreach ( var path in paths ) {
                if ( IsMatch( path ) ) {
                    result.Add( path );
                    if ( includeSubfiles ) {
                        folders.Add( path + "/" );
                    }
                }
            }
            if ( includeSubfiles ) {
                ExporterUtils.DebugLog( "Folders: \n" + string.Join( "\n", folders ) + "\n" );
                var subfiles = paths.Where( v1 => folders.Any( v2 => v1.StartsWith( v2 ) ) );
                ExporterUtils.DebugLog( "Subfiles: \n" + string.Join( "\n", subfiles ) + "\n" );
                return result.Concat( subfiles );
            } else {
                return result;
            }
        }
    }
}
