﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MizoreNekoyanagi.PublishUtil.PackageExporter {
    [System.Serializable]
    public class SearchPath : System.IEquatable<SearchPath>, System.ICloneable, ISerializationCallbackReceiver {
        // 互換性のためSerialize対象にしておく
        public SearchPathType searchType;
        [SerializeField]string s_searchType;
        public string value;

        public SearchPath( ) {
            this.searchType = SearchPathType.Exact;
            this.value = string.Empty;
        }
        public SearchPath( SearchPathType searchType, string value ) {
            this.searchType = searchType;
            this.value = value;
        }
        public SearchPath( SearchPath source ) {
            this.searchType = source.searchType;
            this.value = source.value;
        }
        public object Clone( ) {
            return new SearchPath( this );
        }

        public override string ToString( ) {
            return $"{value}({searchType.GetString( )})";
        }

        public override int GetHashCode( ) {
            return value.GetHashCode( ) ^ searchType.GetHashCode( );
        }
        public bool Equals( SearchPath other ) {
            return this.value == other.value && this.searchType == other.searchType;
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
            switch ( searchType ) {
                default:
                case SearchPathType.Disabled:
                    return false;
                case SearchPathType.Exact:
                    return value == path;
                case SearchPathType.Partial:
                    return path.Contains( value );
                case SearchPathType.Partial_IgnoreCase:
                    return path.ToLowerInvariant( ).Contains( value.ToLowerInvariant( ) );
                case SearchPathType.Regex:
                    return Regex.IsMatch( path, value );
                case SearchPathType.Regex_IgnoreCase:
                    return Regex.IsMatch( path, value, RegexOptions.IgnoreCase );
            }
        }
        public IEnumerable<string> Filter( IEnumerable<string> paths, bool exclude, bool includeSubfiles ) {
            ExporterUtils.DebugLog( ToString( ) );
            ExporterUtils.DebugLog( "Paths: \n" + string.Join( "\n", paths ) + "\n" );
            Regex regex = null;
            if ( searchType == SearchPathType.Regex || searchType == SearchPathType.Regex_IgnoreCase ) {
                try {
                    if ( searchType == SearchPathType.Regex_IgnoreCase ) {
                        regex = new Regex( value, RegexOptions.IgnoreCase );
                    } else {
                        regex = new Regex( value, RegexOptions.None );
                    }
                } catch ( System.Exception e ) {
                    Debug.LogError( e );
                    if ( exclude ) {
                        return paths;
                    } else {
                        return new string[0];
                    }
                }
            }
            if ( searchType == SearchPathType.Disabled || string.IsNullOrEmpty( value ) ) {
                if ( exclude ) {
                    return paths;
                } else {
                    return new string[0];
                }
            }

            List<string> folders = new List<string>( );
            List<string> result;
            if ( exclude ) {
                result = new List<string>( paths );
            } else {
                result = new List<string>( );
            }
            if ( searchType == SearchPathType.Exact ) {
                if ( Directory.Exists( value ) && paths.Any( v => v.StartsWith( value ) ) ) {
                    if ( exclude ) {
                        result.Remove( value );
                    } else {
                        result.Add( value );
                    }
                    if ( includeSubfiles ) {
                        ExporterUtils.DebugLog( "Folder: " + value + "/" );
                        folders.Add( value + "/" );
                    }
                } else if ( paths.Contains( value ) ) {
                    if ( exclude ) {
                        result.Remove( value );
                    } else {
                        result.Add( value );
                    }
                }
            } else {
                foreach ( var path in paths ) {
                    switch ( searchType ) {
                        case SearchPathType.Partial:
                        case SearchPathType.Partial_IgnoreCase:
                            bool b;
                            if ( searchType == SearchPathType.Partial_IgnoreCase ) {
                                // 小文字に変換
                                b = path.ToLowerInvariant( ).Contains( value.ToLowerInvariant( ) );
                            } else {
                                b = path.Contains( value );
                            }
                            if ( b ) {
                                if ( exclude ) {
                                    result.Remove( path );
                                } else {
                                    result.Add( path );
                                }
                                if ( includeSubfiles ) {
                                    folders.Add( path + "/" );
                                }
                            }
                            break;
                        case SearchPathType.Regex:
                        case SearchPathType.Regex_IgnoreCase:
                            if ( regex.IsMatch( path ) ) {
                                if ( exclude ) {
                                    result.Remove( path );
                                } else {
                                    result.Add( path );
                                }
                                if ( includeSubfiles ) {
                                    folders.Add( path + "/" );
                                }
                            }
                            break;
                    }
                }
            }
            if ( includeSubfiles ) {
                ExporterUtils.DebugLog( "Folders: \n" + string.Join( "\n", folders ) + "\n" );
                var subfiles = paths.Where( v1 => folders.Any( v2 => v1.StartsWith( v2 ) ) );
                ExporterUtils.DebugLog( "Subfiles: \n" + string.Join( "\n", subfiles ) + "\n" );
                if ( exclude ) {
                    return result.Except( subfiles );
                } else {
                    return result.Concat( subfiles );
                }
            } else {
                return result;
            }
        }

        public void OnBeforeSerialize( ) {
            s_searchType = searchType.GetString( );
        }

        public void OnAfterDeserialize( ) {
            if ( string.IsNullOrEmpty( s_searchType ) ) {
                // enumのstring保存が未実装なデータを読み込んだ場合のみ発生
                // enumの順番が変わった場合はここで対応する
            } else {
                searchType = SearchPathTypeExtensions.Parse( s_searchType );
            }
        }
    }
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
    public static class SearchPathTypeExtensions {
        public static string GetString( this SearchPathType value ) {
            switch ( value ) {
                case SearchPathType.Disabled: return "Disabled";
                case SearchPathType.Exact: return "Exact";
                case SearchPathType.Partial: return "Partial";
                case SearchPathType.Partial_IgnoreCase: return "Partial_IgnoreCase";
                case SearchPathType.Regex: return "Regex";
                case SearchPathType.Regex_IgnoreCase: return "Regex_IgnoreCase";
                default: throw new System.ArgumentException( );
            }
        }
        public static SearchPathType Parse( string value ) {
            switch ( value ) {
                case "Disabled": return SearchPathType.Disabled;
                case "Exact": return SearchPathType.Exact;
                case "Partial": return SearchPathType.Partial;
                case "Partial_IgnoreCase": return SearchPathType.Partial_IgnoreCase;
                case "Regex": return SearchPathType.Regex;
                case "Regex_IgnoreCase": return SearchPathType.Regex_IgnoreCase;
                default: throw new System.ArgumentException( );
            }
        }
    }
}
