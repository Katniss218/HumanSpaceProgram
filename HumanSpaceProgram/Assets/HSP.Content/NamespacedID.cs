﻿using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP
{
    /// <summary>
    /// An identifier that consists of a namespace and a name.
    /// </summary>
    [Serializable]
    public struct NamespacedID
    {
        [SerializeField]
        string _data;

        const string SEPARATOR = "::";

        public NamespacedID( string @namespace, string name )
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
            if( obj is NamespacedID id )
                return this._data.Equals( id._data );

            return false;
        }

        public override string ToString()
        {
            return _data.ToString();
        }

        public static NamespacedID Parse( string str )
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

            return new NamespacedID()
            {
                _data = str
            };
        }

        [MapsInheritingFrom( typeof( NamespacedID ) )]
        public static SerializationMapping NamespacedIdentifierMapping()
        {
            return new PrimitiveSerializationMapping<NamespacedID>()
            {
                OnSave = ( o, s ) => (SerializedPrimitive)o.ToString(),
                OnLoad = ( data, l ) => NamespacedID.Parse( (string)data )
            };
        }
    }
}