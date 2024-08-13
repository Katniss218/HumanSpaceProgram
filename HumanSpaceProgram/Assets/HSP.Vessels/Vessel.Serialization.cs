using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vessels
{
    public static class Vessel_SerializationMappings
    {
        [MapsInheritingFrom( typeof( Vessel ) )]
        public static SerializationMapping VesselMapping()
        {
            return new MemberwiseSerializationMapping<Vessel>()
            {
                ("display_name", new Member<Vessel, bool>( o => o.enabled )),
                ("root_part", new Member<Vessel, Transform>( ObjectContext.Ref, o => o.RootPart )),
                ("on_after_recalculate_parts", new Member<Vessel, Action>( o => o.OnAfterRecalculateParts ))
            };
        }
    }
}