using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public static class PathUtils {
        public static string GetRelativePath( string basePath, string path ) {
            if ( IsRelativePath( path ) ) {
                return path;
            }

            var baseParts = SplitNormalizedPath( basePath );
            var pathParts = SplitNormalizedPath( path );
            var commonLength = 0;
            while ( commonLength < baseParts.Length && commonLength < pathParts.Length
                && string.Equals( baseParts[commonLength], pathParts[commonLength], System.StringComparison.OrdinalIgnoreCase ) ) {
                commonLength++;
            }

            var resultParts = new System.Collections.Generic.List<string>( );
            for ( var i = commonLength; i < baseParts.Length; i++ ) {
                resultParts.Add( ".." );
            }
            for ( var i = commonLength; i < pathParts.Length; i++ ) {
                resultParts.Add( pathParts[i] );
            }

            if ( resultParts.Count == 0 ) {
                return ".";
            }

            return "./" + string.Join( "/", resultParts );
        }
        public static string GetProjectAbsolutePath( string basePath, string path ) {
            if ( !IsRelativePath( path ) ) {
                return path;
            }

            var parts = new System.Collections.Generic.List<string>( SplitNormalizedPath( basePath ) );
            foreach ( var part in SplitNormalizedPath( path ) ) {
                if ( part == "." ) {
                    continue;
                }

                if ( part == ".." ) {
                    if ( parts.Count > 0 ) {
                        parts.RemoveAt( parts.Count - 1 );
                    }
                    continue;
                }

                parts.Add( part );
            }

            return string.Join( "/", parts );
        }

        static string[] SplitNormalizedPath( string path ) {
            if ( string.IsNullOrEmpty( path ) ) {
                return new string[0];
            }

            return path.Replace( "\\", "/" ).Trim( '/' ).Split( new[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries );
        }
        public static bool IsRelativePath( string path ) {
            if ( string.IsNullOrEmpty( path ) ) {
                return false;
            }
            return path.StartsWith( "." );
        }
        //public static string GetFullPath( string path ) {
        //    if ( Regex.IsMatch( path, @"^[a-zA-Z]:/" ) ) {
        //        return path;
        //    }
        //    var projectPath = Path.GetDirectoryName( Application.dataPath );
        //    path = Path.Combine( projectPath, path );
        //    return path;
        //}
        public static string ToValidPath( string path ) {
            if ( string.IsNullOrEmpty( path ) ) {
                return string.Empty;
            }
            path = path.Replace( "\\", "/" );
            // AssetDatabaseのパスに変換
            var dataPath = Path.GetDirectoryName( Application.dataPath ).Replace( "\\", "/" );
            if ( path.StartsWith( dataPath ) ) {
                path = path.Substring( dataPath.Length + 1 );
            }
            path = Regex.Replace( path, @"^[a-zA-Z]:/", "" );
            return path;
        }

        public static bool IsDynamicPath( string path ) {
            if ( string.IsNullOrEmpty( path ) ) {
                return false;
            }
            return path.Contains( "%" );
        }
    }
}
