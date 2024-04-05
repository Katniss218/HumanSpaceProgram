using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class Persistent_Behaviour
    {
        public static SerializedData GetData( this Behaviour bc, IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { "is_enabled", bc.enabled.GetData() }
            };
        }

        public static void SetData( this Behaviour bc, SerializedData data, IForwardReferenceMap l )
        {
            if( data.TryGetValue( "is_enabled", out var isEnabled ) )
                bc.enabled = isEnabled.ToBoolean();
        }
    }
}