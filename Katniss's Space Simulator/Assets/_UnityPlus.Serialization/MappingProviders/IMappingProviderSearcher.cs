using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    public interface IMappingProviderSearcher<TContext, T>
    {
        /// <summary>
        /// Tries to get the value for the corresponding context and type.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="type">The type.</param>
        /// <param name="value">The value corresponding to the given context-type pair.</param>
        /// <returns>Whether or not the value was successfully retrieved.</returns>
        public bool TryGet( TContext context, Type type, out T value );

        /// <summary>
        /// Tries to set the value for the corresponding context and type.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="type">The type.</param>
        /// <param name="value">The value corresponding to the given context-type pair.</param>
        /// <returns>Whether or not the value was successfully set.</returns>
        public bool TrySet( TContext context, Type type, T value );
    }
}