using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    public class SerializationMappingProviderAttribute : Attribute
    {
        public Type TargetType { get; set; }

        public SerializationMappingProviderAttribute( Type targetType )
        {
            this.TargetType = targetType;
        }
    }
}