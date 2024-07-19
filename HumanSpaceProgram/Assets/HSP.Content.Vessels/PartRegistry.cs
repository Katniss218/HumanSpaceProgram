using HSP.Content.Vessels.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.ReferenceMaps;

namespace HSP.Content.Vessels
{
    /// <summary>
    /// A registry containing factories of Unity hierarchies.
    /// </summary>
    public static class PartRegistry
    {
        private static Dictionary<NamespacedIdentifier, PartFactory> _registry = new Dictionary<NamespacedIdentifier, PartFactory>();

        /// <summary>
        /// Registers a Unity hierarchy factory under the specified mod and part IDs.
        /// </summary>
        public static void Register( NamespacedIdentifier namespacedPartId, PartFactory factory )
        {
            _registry.Add( namespacedPartId, factory );
        }

        /// <summary>
        /// Unregisters a Unity hierarchy factory under the specified mod and part IDs.
        /// </summary>
        public static void Unregister( NamespacedIdentifier namespacedPartId )
        {
            _registry.Remove( namespacedPartId );
        }
        
        public static void UnregisterAll()
        {
            _registry.Clear();
        }

        /// <summary>
        /// Loads all registered part metadata from their sources.
        /// </summary>
        public static PartMetadata[] LoadAllMetadata()
        {
            List<PartMetadata> assets = new List<PartMetadata>();

            foreach( var kvp in _registry )
            {
                assets.Add( kvp.Value.LoadMetadata() );
            }

            return assets.ToArray();
        }

        /// <summary>
        /// Loads all registered part metadata from a specified mod from their sources.
        /// </summary>
        public static PartMetadata[] LoadAllMetadata( string modId )
        {
            List<PartMetadata> assets = new List<PartMetadata>();

            foreach( var kvp in _registry )
            {
                if( kvp.Key.BelongsTo( modId ) )
                {
                    assets.Add( kvp.Value.LoadMetadata() );
                }
            }

            return assets.ToArray();
        }

        /// <summary>
        /// Loads a specified registered part metadata from its source.
        /// </summary>
        public static PartMetadata LoadMetadata( NamespacedIdentifier namespacedPartId )
        {
            if( _registry.TryGetValue( namespacedPartId, out PartFactory factory ) )
            {
                return factory.LoadMetadata();
            }

            return null;
        }

        /// <summary>
        /// Loads a specified registered unity hierarchy from its source.
        /// </summary>
        public static GameObject Load( NamespacedIdentifier namespacedPartId )
        {
            if( _registry.TryGetValue( namespacedPartId, out PartFactory factory ) )
            {
                return factory.Load( new ForwardReferenceStore() );
            }

            return null;
        }

        /// <summary>
        /// Loads a specified registered unity hierarchy from its source.
        /// </summary>
        public static GameObject Load( NamespacedIdentifier namespacedPartId, IForwardReferenceMap refMap )
        {
            if( refMap == null )
            {
                throw new ArgumentNullException( nameof( refMap ), $"Reference map can't be null." );
            }

            if( _registry.TryGetValue( namespacedPartId, out PartFactory factory ) )
            {
                return factory.Load( refMap );
            }

            return null;
        }
    }
}