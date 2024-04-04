using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization
{
	public static class Persistent_QuaternionDbl
	{
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedData GetData( this QuaternionDbl q, IReverseReferenceMap s = null )
		{
			return new SerializedArray() { (SerializedPrimitive)q.x, (SerializedPrimitive)q.y, (SerializedPrimitive)q.z, (SerializedPrimitive)q.w };
		}
		
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void SetData( this ref QuaternionDbl q, SerializedData data, IForwardReferenceMap l = null )
		{
			q.x = (double)data[0];
			q.y = (double)data[1];
			q.z = (double)data[2];
			q.w = (double)data[3];
		}
		
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static QuaternionDbl ToQuaternionDbl( this SerializedData data, IForwardReferenceMap l = null ) 
		{
            return new QuaternionDbl( (double)data[0], (double)data[1], (double)data[2], (double)data[3] );
		}
	}
}