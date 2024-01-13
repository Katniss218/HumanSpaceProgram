using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Core
{
    public sealed partial class Vessel : IPersistent
    {
        public SerializedData GetData( IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { "display_name", this.DisplayName },
                { "root_part", s.WriteObjectReference( this.RootPart ) },
                { "on_after_recalculate_parts", s.WriteDelegate( this.OnAfterRecalculateParts ) }
            };
        }

        public void SetData( IForwardReferenceMap l, SerializedData data )
        {
            if( data.TryGetValue( "display_name", out var displayName ) )
                this.DisplayName = (string)displayName;

            if( data.TryGetValue( "root_part", out var rootPart ) )
                this.RootPart = (Transform)l.ReadObjectReference( rootPart );

            if( data.TryGetValue( "on_after_recalculate_parts", out var onAfterRecalculateParts ) )
                this.OnAfterRecalculateParts = (Action)l.ReadDelegate( onAfterRecalculateParts );
        }
    }
}