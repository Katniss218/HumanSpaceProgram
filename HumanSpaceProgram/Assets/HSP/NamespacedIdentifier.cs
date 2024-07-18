using System;
using UnityEngine;

namespace HSP
{
    /// <summary>
    /// An identifier that consists of a namespace and a name.
    /// </summary>
    [Serializable]
    public struct NamespacedIdentifier
    {
        [SerializeField]
        string _data;

        const string SEPARATOR = "::";

        public NamespacedIdentifier( string @namespace, string name )
        {
            if( string.IsNullOrEmpty( @namespace ) )
            {
                throw new ArgumentException( $"Namespace and a name must both be a nonnull, nonzero length strings.", nameof( @namespace ) );
            }
            if( string.IsNullOrEmpty( name ) )
            {
                throw new ArgumentException( $"Namespace and a name must both be a nonnull, nonzero length strings.", nameof( name ) );
            }

            _data = @namespace + SEPARATOR + name;
        }

        public bool BelongsTo( string @namespace )
        {
            return _data.StartsWith( @namespace );
        }

        public override int GetHashCode()
        {
            return _data.GetHashCode();
        }

        public override bool Equals( object obj )
        {
            if( obj is NamespacedIdentifier id )
                return this._data.Equals( id._data );

            return false;
        }

        public override string ToString()
        {
            return _data.ToString();
        }

        public static NamespacedIdentifier Parse( string str )
        {
            string[] parts = str.Split( "::" );
            if( parts.Length != 2 )
            {
                throw new ArgumentException( $"Namespaced string must contain a namespace and a name.", nameof( str ) );
            }
            if( string.IsNullOrEmpty( parts[0] ) || string.IsNullOrEmpty( parts[0] ) )
            {
                throw new ArgumentException( $"Namespace and a name must both be a nonnull, nonzero length strings.", nameof( str ) );
            }

            return new NamespacedIdentifier()
            {
                _data = str
            };
        }
    }
}