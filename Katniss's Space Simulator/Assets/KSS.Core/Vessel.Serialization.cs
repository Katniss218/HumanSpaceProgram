using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Core
{
    public sealed partial class Vessel : IPersistsData
    {
        public SerializedData GetData( IReverseReferenceMap s )
        {
            SerializedObject ret = (SerializedObject)Persistent_Behaviour.GetData( this, s );

            ret.AddAll( new SerializedObject()
            {
                { "display_name", this.DisplayName },
                { "root_part", s.WriteObjectReference( this.RootPart ) },
                { "on_after_recalculate_parts", s.WriteDelegate( this.OnAfterRecalculateParts ) }
            } );

            return ret;
        }

        public void SetData( SerializedData data, IForwardReferenceMap l )
        {
            Persistent_Behaviour.SetData( this, data, l );

            if( data.TryGetValue( "display_name", out var displayName ) )
                this.DisplayName = (string)displayName;

            if( data.TryGetValue( "root_part", out var rootPart ) )
                this.RootPart = (Transform)l.ReadObjectReference( rootPart );

            if( data.TryGetValue( "on_after_recalculate_parts", out var onAfterRecalculateParts ) )
                this.OnAfterRecalculateParts = (Action)l.ReadDelegate( onAfterRecalculateParts );
        }
    }
}