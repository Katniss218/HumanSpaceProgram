using System;

namespace UnityPlus.Serialization
{
    public interface IMappingProviderSearcher<TContext, T>
    {
        /// <summary>
        /// Tries to get the value for the corresponding context and type.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="type">The concrete type being queried.</param>
        /// <param name="value">The provider found.</param>
        /// <returns>True if a provider was found.</returns>
        bool TryGet( TContext context, Type type, out T value );

        /// <summary>
        /// Registers a value for a specific context and mapping configuration type.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="type">The type defined in the attribute (e.g. the base class or interface). Can be null for 'Any' searchers.</param>
        /// <param name="value">The provider to register.</param>
        /// <returns>True if registered successfully.</returns>
        bool TrySet( TContext context, Type type, T value );

        void Clear();
    }
}