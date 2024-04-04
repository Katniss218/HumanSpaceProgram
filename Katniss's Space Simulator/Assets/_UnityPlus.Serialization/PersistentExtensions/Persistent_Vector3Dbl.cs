using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization
{
	public static class Persistent_Vector3Dbl
	{
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedData GetData( this Vector3Dbl v, IReverseReferenceMap s = null )
		{
			return new SerializedArray() { (SerializedPrimitive)v.x, (SerializedPrimitive)v.y, (SerializedPrimitive)v.z };
		}
		
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetData( this ref Vector3Dbl v, SerializedData data, IForwardReferenceMap l = null )
		{
			v.x = (double)data[0];
			v.y = (double)data[1];
			v.z = (double)data[2];
		}
		
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3Dbl ToVector3Dbl( this SerializedData data, IForwardReferenceMap l = null ) 
		{
            return new Vector3Dbl( (double)data[0], (double)data[1], (double)data[2] );
		}
	}
}