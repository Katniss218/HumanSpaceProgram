using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization.Strategies
{
    public static class ExplicitHierarchyGameObjectsPatchFactory
    {
        /// <summary>
        /// Creates a collection of patches that can turn `from` into `to`.
        /// </summary>
        /// <remarks>
        /// Swapping the parameters around will result in the inverse patch being created.
        /// </remarks>
        /// <param name="from">The original.</param>
        /// <param name="to">The target.</param>
        /// <returns>The collection of patches.</returns>
        public static PatchCollection CreatePatches( (SerializedArray o, SerializedArray d) from, (SerializedArray o, SerializedArray d) to )
        {
            throw new NotImplementedException();

            List<SerializedData> missingO = new List<SerializedData>();
            List<SerializedData> excessO = new List<SerializedData>();
            foreach( var oFrom in from.o )
            {
                foreach( var oTo in to.o )
                {
                    // add or remove objects patches here.
                }
            }

            foreach( var dFrom in from.d )
            {
                foreach( var dTo in to.d )
                {
                    // if both point to the same object. 
                }
            }
        }
    }
}