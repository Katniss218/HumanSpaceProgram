using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Core
{
    public static class Vessel_SerializationMappings
    {
        [MapsInheritingFrom( typeof( GameplayVessel ) )]
        public static SerializationMapping VesselMapping()
        {
            return new MemberwiseSerializationMapping<GameplayVessel>()
            {
                ("display_name", new Member<GameplayVessel, bool>( o => o.enabled )),
                ("root_part", new Member<GameplayVessel, Transform>( ObjectContext.Ref, o => o.RootPart )),
                ("on_after_recalculate_parts", new Member<GameplayVessel, Action>( o => o.OnAfterRecalculateParts ))
            };
        }
    }
}