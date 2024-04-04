using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class IPersistent_Transform
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializedData GetData( this Transform t, IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { "local_position", t.localPosition.GetData() },
                { "local_rotation", t.localRotation.GetData() },
                { "local_scale", t.localScale.GetData() }
            };
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void SetData( this Transform t, SerializedData data, IForwardReferenceMap l )
        {
            if( data.TryGetValue( "local_position", out var localPosition ) )
                t.localPosition = localPosition.ToVector3();

            if( data.TryGetValue( "local_rotation", out var localRotation ) )
                t.localRotation = localRotation.ToQuaternion();

            if( data.TryGetValue( "local_scale", out var localScale ) )
                t.localScale = localScale.ToVector3();
        }
    }
}