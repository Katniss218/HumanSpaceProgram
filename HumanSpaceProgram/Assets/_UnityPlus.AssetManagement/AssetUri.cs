using System;
using System.Collections.Generic;

namespace UnityPlus.AssetManagement
{
    /// <summary>
    /// Represents a parsed Asset ID in the format: ModId::Path?Query
    /// </summary>
    public readonly struct AssetUri
    {
        public readonly string OriginalString;

        /// <summary>
        /// The Mod ID (Namespace) of the asset.
        /// </summary>
        public readonly string ModID;

        /// <summary>
        /// The virtual path of the asset (excluding ModID and Query).
        /// </summary>
        public readonly string Path;

        /// <summary>
        /// The parsed query parameters. Null if no query was present.
        /// </summary>
        public readonly Dictionary<string, string> QueryParams;

        /// <summary>
        /// Returns the ID format without the query string (e.g. "ModId::Path").
        /// Useful for resolvers to find the base resource.
        /// </summary>
        public string BaseID => $"{ModID}::{Path}";

        private AssetUri( string original, string modId, string path, Dictionary<string, string> queryParams )
        {
            OriginalString = original;
            ModID = modId;
            Path = path;
            QueryParams = queryParams;
        }

        public static bool TryParse( string assetId, out AssetUri uri )
        {
            if( string.IsNullOrEmpty( assetId ) )
            {
                uri = default;
                return false;
            }

            int separatorIndex = assetId.IndexOf( "::", StringComparison.Ordinal );
            if( separatorIndex < 0 )
            {
                // Format doesn't match ModId::Path
                uri = default;
                return false;
            }

            string modId = assetId.Substring( 0, separatorIndex );
            string remainder = assetId.Substring( separatorIndex + 2 );
            string path = remainder;
            Dictionary<string, string> queryParams = null;

            int queryIndex = remainder.IndexOf( '?' );
            if( queryIndex >= 0 )
            {
                path = remainder.Substring( 0, queryIndex );
                string queryStr = remainder.Substring( queryIndex + 1 );
                queryParams = ParseQuery( queryStr );
            }

            uri = new AssetUri( assetId, modId, path, queryParams );
            return true;
        }

        private static Dictionary<string, string> ParseQuery( string query )
        {
            var dict = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );
            if( string.IsNullOrEmpty( query ) )
                return dict;

            string[] pairs = query.Split( '&' );
            foreach( var pair in pairs )
            {
                if( string.IsNullOrWhiteSpace( pair ) ) 
                    continue;

                int eqIndex = pair.IndexOf( '=' );
                if( eqIndex < 0 )
                {
                    dict[pair] = "true"; // Flag style
                }
                else
                {
                    string key = pair.Substring( 0, eqIndex );
                    string val = pair.Substring( eqIndex + 1 );
                    dict[key] = val;
                }
            }
            return dict;
        }

        public override string ToString() => OriginalString;
    }
}