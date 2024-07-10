using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    [AttributeUsage( AttributeTargets.Method )]
    public abstract class MappingProviderAttribute : Attribute
    {
        /// <summary>
        /// Specifies the interface type that this mapping will be used to map.
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
    }
}