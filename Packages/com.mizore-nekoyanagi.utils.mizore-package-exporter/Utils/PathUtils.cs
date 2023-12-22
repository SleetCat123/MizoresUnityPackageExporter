using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    public static class PathUtils {
        public static string GetRelativePath( string basePath, string path ) {
            if ( IsRelativePath( path ) ) {
                return path;
            }
            // ドライブ表記がないとエラーが出るので対策としてD:/を付ける
            basePath = "D:/" + basePath;
            path = "D:/" + path.Replace( "%", "%25" );
            var baseUri = new System.Uri( basePath );
            var uri = new System.Uri( path );
            var relativeUri = baseUri.MakeRelativeUri( uri );
            var result = "./" + relativeUri.ToString( );
            result = result.Replace( "%25", "%" );
            result = result.Replace( "%25", "%" );
            result = result.Replace( "\\", "/" );
            return result;
        }
        public static string GetProjectAbsolutePath( string basePath, string path ) {
            if ( !IsRelativePath( path ) ) {
                return path;
            }
            // ドライブ表記がないとエラーが出るので対策としてD:/を付ける
            basePath = "D:/" + basePath;
            path = path.Replace( "%", "%25" );
            var baseUri = new System.Uri( basePath );
            var absoluteUri = new System.Uri( baseUri, path );
            var result = absoluteUri.LocalPath;
            result = result.Substring( 3 );
            result = result.Replace( "%25", "%" );
            result = result.Replace( "\\", "/" );
            return result;
        }
        public static bool IsRelativePath( string path ) {
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
            path = path.Replace( "\\", "/" );
            // AssetDatabaseのパスに変換
            var dataPath = Path.GetDirectoryName( Application.dataPath).Replace( "\\", "/" );
            if ( path.StartsWith( dataPath ) ) {
                path = path.Substring( dataPath.Length + 1 );
            }
            path = Regex.Replace( path, @"^[a-zA-Z]:/", "" );
            return path;
        }
    }
}