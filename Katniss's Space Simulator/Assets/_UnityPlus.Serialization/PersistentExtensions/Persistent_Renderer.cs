using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class Persistent_Renderer
    {
        public static SerializedData GetData( this Renderer r, IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { "is_enabled", r.enabled.GetData() }
            };
        }

        public static void SetData( this Renderer r, SerializedData data, IForwardReferenceMap l )
        {
            if( data.TryGetValue( "is_enabled", out var isEnabled ) )
                r.enabled = isEnabled.ToBoolean();
        }
    }
}