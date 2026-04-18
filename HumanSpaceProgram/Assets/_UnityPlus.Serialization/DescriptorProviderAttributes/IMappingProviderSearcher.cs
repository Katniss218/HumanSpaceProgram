using System;
using System.Reflection;

namespace UnityPlus.Serialization
{
    public interface IMappingProviderSearcher
    {
        /// <summary>
        /// Tries to get the bound method for the corresponding context and type.
        /// </summary>
        /// <param name="contextId">The context ID.</param>
        /// <param name="type">The concrete type being queried.</param>
        /// <param name="boundMethod">The generic-bound provider method found and prepared.</param>
        /// <returns>True if a provider was found and successfully bound.</returns>
        bool TryGet( int contextId, Type type, out MethodInfo boundMethod );

        /// <summary>
        /// Registers a provider for a specific context and mapping configuration type.
        /// </summary>
        /// <param name="contextId">The context ID.</param>
        /// <param name="type">The type defined in the attribute (e.g. the base class or interface). Can be null for 'Any' searchers.</param>
        /// <param name="method">The raw provider to register.</param>
        /// <returns>True if registered successfully.</returns>
        bool TrySet( int contextId, Type type, MethodInfo method );

        void Clear();
    }
}