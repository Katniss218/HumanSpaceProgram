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
                { "radius", cc.radius.AsSerialized() },
                { "height", cc.height.AsSerialized() },
                { "direction", cc.direction.GetData() },
                { "center", cc.center.GetData() },
                { "is_trigger", cc.isTrigger.GetData() }
            };
        }

        public static void SetData( this CapsuleCollider cc, SerializedData data, IForwardReferenceMap l )
        {
            if( data.TryGetValue( "radius", out var radius ) )
                cc.radius = radius.AsFloat();

            if( data.TryGetValue( "height", out var height ) )
                cc.height = height.AsFloat();

            if( data.TryGetValue( "direction", out var direction ) )
                cc.direction = direction.AsInt32();

            if( data.TryGetValue( "center", out var center ) )
                cc.center = center.AsVector3();

            if( data.TryGetValue( "is_trigger", out var isTrigger ) )
                cc.isTrigger = isTrigger.AsBoolean();
        }
    }
}