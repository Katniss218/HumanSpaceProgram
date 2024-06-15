using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Specifies that a given method should be included when searching for <see cref="SerializationMapping"/>s to be used in serialization.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method )]
    public class SerializationMappingProviderAttribute : Attribute
    {
        /// <summary>
        /// Specifies the type that this mapping will be used to map.
        /// </summary>
        public Type MappedType { get; set; }

        /// <summary>
        /// Specifies the context that this mapping will be used in.
        /// </summary>
        public int Context
        {
            get => Contexts[0];
            set => Contexts = new int[] { value };
        }

        /// <summary>
        /// Specifies a number of contexts that this mapping will be used in.
        /// </summary>
        public int[] Contexts { get; set; } = new int[] { 0 }; // By default, use the default context (zero).

        /// <summary>
        /// Specifies a method that returns a mapping used for mapping the specified target type.
        /// </summary>
        /// <param name="mappedType">The type that will be mapped by the returned mapping.</param>
        public SerializationMappingProviderAttribute( Type mappedType )
        {
            this.MappedType = mappedType;
        }
    }
}