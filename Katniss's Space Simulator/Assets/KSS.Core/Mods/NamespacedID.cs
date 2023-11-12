using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core.Mods
{
    /// <summary>
    /// An ID that comprises a namespace and a name
    /// </summary>
    [Serializable]
    public struct NamespacedID
    {
        [SerializeField]
        string _data;

        public NamespacedID( string namespaceId, string nameId )
        {
            // modId is the directory name of `GameData/<modId>/Parts/<partId>`.

            _data = $"{namespaceId}::{nameId}";
        }

        public bool BelongsTo( string namespaceId )
        {
            return _data.StartsWith( namespaceId );
        }

        public override string ToString()
        {
            return _data.ToString();
        }

        public static NamespacedID Parse( string s )
        {
            string[] parts = s.Split( "::" );
            if( parts.Length != 2 )
                throw new ArgumentException( $"Namespaced string must contain a namespace and a name.", nameof( s ) );
            if( string.IsNullOrEmpty( parts[0] ) || string.IsNullOrEmpty( parts[0] ) )
            {
                throw new ArgumentException( $"Namespace and a name must both be a nonnull, nonzero length strings.", nameof( s ) );
            }
            return new NamespacedID() { _data = s };
        }
    }
}