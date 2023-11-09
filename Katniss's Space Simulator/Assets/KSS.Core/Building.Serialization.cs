using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Core
{
    public sealed partial class Building : IPersistent
    {
        public SerializedData GetData( IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { "display_name", this.DisplayName },
                { "root_part", s.WriteObjectReference( this.RootPart ) },
                { "reference_body", s.WriteObjectReference( this.ReferenceBody ) },
                { "reference_position", s.WriteVector3Dbl( this.ReferencePosition ) },
                { "reference_rotation", s.WriteQuaternionDbl( this.ReferenceRotation ) },
                // { "on_after_recalculate_parts", s.WriteDelegate( this.OnAfterRecalculateParts ) }
            };
        }

        public void SetData( IForwardReferenceMap l, SerializedData data )
        {
            if( data.TryGetValue( "display_name", out var displayName ) )
                this.DisplayName = (string)displayName;
            if( data.TryGetValue( "root_part", out var rootPart ) )
                this.RootPart = (Transform)l.ReadObjectReference( rootPart );
            if( data.TryGetValue( "reference_body", out var referenceBody ) )
                this.ReferenceBody = (CelestialBody)l.ReadObjectReference( referenceBody );
            if( data.TryGetValue( "reference_position", out var referencePosition ) )
                this.ReferencePosition = l.ReadVector3Dbl( referencePosition );
            if( data.TryGetValue( "reference_rotation", out var referenceRotation ) )
                this.ReferenceRotation = l.ReadQuaternionDbl( referenceRotation );
            // if( data.TryGetValue( "on_after_recalculate_parts", out var onAfterRecalculateParts ) )
            //     this.OnAfterRecalculateParts = (Action)l.ReadDelegate( onAfterRecalculateParts );
        }
    }
}