using KSS.Core.Mods;
using KSS.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core
{
    /// <summary>
    /// A registry containing factories of Unity hierarchies.
    /// </summary>
    public static class PartRegistry
    {
        private static Dictionary<NamespacedID, PartFactory> _registry = new Dictionary<NamespacedID, PartFactory>();

        /// <summary>
        /// Registers a Unity hierarchy factory under the specified mod and part IDs.
        /// </summary>
        public static void Register( NamespacedID namespacedPartId, PartFactory factory )
        {
            _registry.Add( namespacedPartId, factory );
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
        public static PartMetadata LoadMetadata( NamespacedID namespacedPartId )
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
        public static GameObject Load( NamespacedID namespacedPartId )
        {
            if( _registry.TryGetValue( namespacedPartId, out PartFactory factory ) )
            {
                return factory.Load();
            }

            return null;
        }
    }

    /// <summary>
    /// Inherit from this class to provide an implementation of a hierarchy instantiator, and a metadata provider.
    /// </summary>
    public abstract class PartFactory
    {
        /// <summary>
        /// Loads the part metadata of this specific part from the original source.
        /// </summary>
        /// <returns>The loaded part metadata.</returns>
        public abstract PartMetadata LoadMetadata();

        /// <summary>
        /// Instantiates the gameobject hierarchy of this specific part from the original source.
        /// </summary>
        /// <returns>The root game object of the instantiated hierarchy.</returns>
        public abstract GameObject Load();
    }

    // TODO - Analogous thing could be done for vessels, although they're just parts.
}