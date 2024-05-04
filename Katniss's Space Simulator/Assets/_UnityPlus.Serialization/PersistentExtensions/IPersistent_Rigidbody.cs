using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityPlus.Serialization
{
    public static class IPersistent_Rigidbody
    {
        public static SerializedData GetData( this Rigidbody rb, IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { "is_kinematic", rb.isKinematic.GetData() }
            };
        }

        public static void SetData( this Rigidbody rb, SerializedData data, IForwardReferenceMap l )
        {
            if( data.TryGetValue( "is_kinematic", out var isKinematic ) )
                rb.isKinematic = isKinematic.AsBoolean();
        }
    }
}