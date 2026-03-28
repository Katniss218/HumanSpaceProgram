using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    public class MapsInheritingFromAttribute : MappingProviderAttribute
    {
        /// <summary>
        /// Specifies a method that returns a mapping used for mapping the specified target type.
        /// </summary>
        /// <param name="mappedType">The type that will be mapped by the returned mapping.</param>
        public MapsInheritingFromAttribute( Type mappedType )
        {
            if( !mappedType.IsClass && !mappedType.IsValueType )
                throw new ArgumentException( $"{nameof( MapsInheritingFromAttribute )} can only be used to map class or struct types.", nameof( mappedType ) );

            this.MappedType = mappedType;
        }
    }
}