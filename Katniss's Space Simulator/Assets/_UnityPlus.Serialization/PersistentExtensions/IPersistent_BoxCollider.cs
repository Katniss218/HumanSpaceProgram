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
    public static class IPersistent_BoxCollider
    {
        public static SerializedData GetData( this BoxCollider bc, ISaver s )
        {
            return new SerializedObject()
            {
                { "size", s.WriteVector3( bc.size ) },
                { "center", s.WriteVector3( bc.center ) },
                { "is_trigger", bc.isTrigger }
            };
        }

        public static void SetData( this BoxCollider bc, ILoader l, SerializedObject data )
        {
            if( data.TryGetValue( "size", out var size ) )
                bc.size = l.ReadVector3( size );

            if( data.TryGetValue( "center", out var center ) )
                bc.center = l.ReadVector3( center );

            if( data.TryGetValue( "is_trigger", out var isTrigger ) )
                bc.isTrigger = (bool)isTrigger;
        }
    }
}