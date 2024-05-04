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
		public static Vector3Dbl AsVector3Dbl( this SerializedData data, IForwardReferenceMap l = null ) 
		{
            return new Vector3Dbl( data[0].AsDouble(), data[1].AsDouble(), data[2].AsDouble() );
		}
	}
}