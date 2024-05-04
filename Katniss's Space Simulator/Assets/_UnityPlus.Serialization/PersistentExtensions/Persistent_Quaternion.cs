using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization
{
	public static class Persistent_Quaternion
	{
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedData GetData( this Quaternion q, IReverseReferenceMap s = null )
		{
			return new SerializedArray() { (SerializedPrimitive)q.x, (SerializedPrimitive)q.y, (SerializedPrimitive)q.z, (SerializedPrimitive)q.w };
		}

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Quaternion AsQuaternion( this SerializedData data, IForwardReferenceMap l = null ) 
		{
            return new Quaternion(data[0].AsFloat(), data[1].AsFloat(), data[2].AsFloat(), data[3].AsFloat() );
		}
	}
}