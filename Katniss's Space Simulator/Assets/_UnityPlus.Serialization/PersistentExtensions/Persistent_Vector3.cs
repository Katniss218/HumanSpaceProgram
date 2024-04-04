using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization
{
	public static class Persistent_Vector3
	{
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedData GetData( this Vector3 v, IReverseReferenceMap s = null )
		{
			return new SerializedArray() { (SerializedPrimitive)v.x, (SerializedPrimitive)v.y, (SerializedPrimitive)v.z };
		}
		
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetData( this ref Vector3 v, SerializedData data, IForwardReferenceMap l = null )
		{
			v.x = (float)data[0];
			v.y = (float)data[1];
			v.z = (float)data[2];
		}
		
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3 ToVector3( this SerializedData data, IForwardReferenceMap l = null ) 
		{
            return new Vector3( (float)data[0], (float)data[1], (float)data[2] );
		}
	}
}