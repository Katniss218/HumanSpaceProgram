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
        public static SerializedData GetData( this SphereCollider sc, ISaver s )
        {
            return new SerializedObject()
            {
                { "radius", sc.radius },
                { "center", s.WriteVector3( sc.center ) },
                { "is_trigger", sc.isTrigger }
            };
        }

        public static void SetData( this SphereCollider sc, ILoader l, SerializedObject data )
        {
            if( data.TryGetValue( "radius", out var radius ) )
                sc.radius = (float)radius;

            if( data.TryGetValue( "center", out var center ) )
                sc.center = l.ReadVector3( center );

            if( data.TryGetValue( "is_trigger", out var isTrigger ) )
                sc.isTrigger = (bool)isTrigger;
        }
    }
}