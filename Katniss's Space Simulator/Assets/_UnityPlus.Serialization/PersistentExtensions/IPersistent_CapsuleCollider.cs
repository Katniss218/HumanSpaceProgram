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
    public static class IPersistent_CapsuleCollider
    {
        public static SerializedData GetData( this CapsuleCollider cc, IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { "radius", cc.radius },
                { "height", cc.height },
                { "direction", cc.direction },
                { "center", cc.center.GetData() },
                { "is_trigger", cc.isTrigger }
            };
        }

        public static void SetData( this CapsuleCollider cc, SerializedData data, IForwardReferenceMap l )
        {
            if( data.TryGetValue( "radius", out var radius ) )
                cc.radius = (float)radius;

            if( data.TryGetValue( "height", out var height ) )
                cc.height = (float)height;

            if( data.TryGetValue( "direction", out var direction ) )
                cc.direction = (int)direction;

            if( data.TryGetValue( "center", out var center ) )
                cc.center = center.ToVector3();

            if( data.TryGetValue( "is_trigger", out var isTrigger ) )
                cc.isTrigger = (bool)isTrigger;
        }
    }
}