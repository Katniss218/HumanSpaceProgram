using HSP.Content.Vessels.Serialization;
using HSP.Vessels;
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
        private static readonly Dictionary<NamespacedID, PartFactory> _registry = new();

        /// <summary>
        /// Registers a Unity hierarchy factory under the specified mod and part IDs.
        /// </summary>
        public static void Register( NamespacedID namespacedPartId, PartFactory factory )
        {
            _registry.Add( namespacedPartId, factory );
        }

        /// <summary>
        /// Unregisters a Unity hierarchy factory under the specified mod and part IDs.
        /// </summary>
        public static void Unregister( NamespacedID namespacedPartId )
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
        /// <returns>An array of all loaded part metadata. Skips entries that failed to load.</returns>
        public static PartMetadata[] LoadAllMetadata()
        {
            List<PartMetadata> assets = new();

            foreach( var kvp in _registry )
            {
                PartMetadata m;
                try
                {
                    m = kvp.Value.LoadMetadata();
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"Failed to load part metadata for {kvp.Key}: {ex.Message}" );
                    Debug.LogException( ex );
                    continue;
                }

                assets.Add( m );
            }

            return assets.ToArray();
        }

        /// <summary>
        /// Loads all registered part metadata from a specified mod from their sources.
        /// </summary>
        /// <returns>An array of all loaded part metadata. Skips entries that failed to load.</returns>
        public static PartMetadata[] LoadAllMetadata( string modId )
        {
            List<PartMetadata> assets = new();

            foreach( var kvp in _registry )
            {
                if( kvp.Key.ModID == modId )
                {
                    PartMetadata m;
                    try
                    {
                        m = kvp.Value.LoadMetadata();
                    }
                    catch( Exception ex )
                    {
                        Debug.LogError( $"Failed to load part metadata for {kvp.Key}: {ex.Message}" );
                        Debug.LogException( ex );
                        continue;
                    }

                    assets.Add( m );
                }
            }

            return assets.ToArray();
        }

        /// <summary>
        /// Loads a specified registered part metadata from its source.
        /// </summary>
        public static bool TryLoadMetadata( NamespacedID namespacedPartId, out PartMetadata metadata )
        {
            if( _registry.TryGetValue( namespacedPartId, out PartFactory factory ) )
            {
                try
                {
                    metadata = factory.LoadMetadata();
                    return true;
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"Failed to load part metadata for {namespacedPartId}: {ex.Message}" );
                    Debug.LogException( ex );
                }
            }

            metadata = null;
            return false;
        }

        /// <summary>
        /// Loads a specified registered unity hierarchy from its source.
        /// </summary>
        public static bool TryLoad( NamespacedID namespacedPartId, out Vessel partGraph )
        {
            return TryLoad( namespacedPartId, new ForwardReferenceStore(), out partGraph );
        }

        /// <summary>
        /// Loads a specified registered unity hierarchy from its source.
        /// </summary>
        public static bool TryLoad( NamespacedID namespacedPartId, IForwardReferenceMap refMap, out Vessel partGraph )
        {
            if( _registry.TryGetValue( namespacedPartId, out PartFactory factory ) )
            {
                try
                {
                    partGraph = factory.Load( refMap );
                    return true;
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"Failed to load part data for '{namespacedPartId}': {ex.Message}" );
                    Debug.LogException( ex );
                }
            }

            partGraph = null;
            return false;
        }
    }
}