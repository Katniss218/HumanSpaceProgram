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
    public static class IPersistent_SphereCollider
    {
        public static SerializedData GetData( this SphereCollider sc, IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { "radius", sc.radius },
                { "center", sc.center.GetData() },
                { "is_trigger", sc.isTrigger }
            };
        }

        public static void SetData( this SphereCollider sc, SerializedData data, IForwardReferenceMap l )
        {
            if( data.TryGetValue( "radius", out var radius ) )
                sc.radius = (float)radius;

            if( data.TryGetValue( "center", out var center ) )
                sc.center = center.ToVector3();

            if( data.TryGetValue( "is_trigger", out var isTrigger ) )
                sc.isTrigger = (bool)isTrigger;
        }
    }
}