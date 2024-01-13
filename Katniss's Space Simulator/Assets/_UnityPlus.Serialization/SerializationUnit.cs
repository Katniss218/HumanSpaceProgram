using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    public struct SerializationUnit
    {
        // Holds instance registry and instance details.


        // a patch can't be applied to existing objects or asset objects.
        // patches can only be applied to serialization units.

        // a serialization unit can have different representations though.
        // gameobjects store their components on them, not loosely due to how they have to be added.

        // so we need to support 2 modes/cases:
        // - nested instance definitions
        // - inline instance details

        // maybe the way it's accessed is abstract and arbitrary?

        public SerializedArray InstanceRegistry { get; }
        public SerializedArray InstanceDetails { get; }

        public SerializationUnit( SerializedArray instanceRegistry, SerializedArray instanceDetails )
        {
            this.InstanceRegistry = instanceRegistry;
            this.InstanceDetails = instanceDetails;
        }

        // each inline details can be separated out. Details can always be "flat" 1-D array.
        // registry is a different beast though.
    }
}
