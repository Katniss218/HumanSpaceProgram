using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Core
{
    public static class Vessel_SerializationMappings
    {
        [SerializationMappingProvider( typeof( Vessel ) )]
        public static SerializationMapping VesselMapping()
        {
            return new MemberwiseSerializationMapping<Vessel>()
            {
                ("display_name", new Member<Vessel, bool>( o => o.enabled )),
                ("root_part", new Member<Vessel, Transform>( ObjectContext.Ref, o => o.RootPart )),
                ("on_after_recalculate_parts", new Member<Vessel, Action>( o => o.OnAfterRecalculateParts ))
            };
        }
        /*
        public SerializedData GetData( IReverseReferenceMap s )
        {
            SerializedObject ret = (SerializedObject)IPersistent_Behaviour.GetData( this, s );

            ret.AddAll( new SerializedObject()
            {
                { "display_name", this.DisplayName.GetData() },
                { "root_part", s.WriteObjectReference( this.RootPart ) },
                { "on_after_recalculate_parts", this.OnAfterRecalculateParts.GetData( s ) }
            } );

            return ret;
        }

        public void SetData( SerializedData data, IForwardReferenceMap l )
        {
            IPersistent_Behaviour.SetData( this, data, l );

            if( data.TryGetValue( "display_name", out var displayName ) )
                this.DisplayName = displayName.AsString();

            if( data.TryGetValue( "root_part", out var rootPart ) )
                this.RootPart = (Transform)l.ReadObjectReference( rootPart );

            if( data.TryGetValue( "on_after_recalculate_parts", out var onAfterRecalculateParts ) )
                this.OnAfterRecalculateParts = (Action)onAfterRecalculateParts.AsDelegate( l );
        }*/
    }
}