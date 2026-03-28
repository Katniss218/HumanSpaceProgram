using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    public class MapsImplementingAttribute : MappingProviderAttribute
    {
        /// <summary>
        /// Specifies a method that returns a mapping used for mapping the specified target type.
        /// </summary>
        /// <param name="mappedType">The type that will be mapped by the returned mapping.</param>
        public MapsImplementingAttribute( Type mappedType )
        {
            if( !mappedType.IsInterface )
                throw new ArgumentException( $"{nameof( MapsImplementingAttribute )} can only be used to map interface types.", nameof( mappedType ) );

            this.MappedType = mappedType;
        }
    }
}
