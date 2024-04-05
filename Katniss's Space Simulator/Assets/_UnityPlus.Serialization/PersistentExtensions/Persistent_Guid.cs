using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace UnityPlus.Serialization
{
	public static class Persistent_Guid
	{
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedPrimitive GetData( this Guid guid, IReverseReferenceMap l = null )
		{
			// GUIDs should be saved in the '00000000-0000-0000-0000-000000000000' format, with dashes, and without extra anything.
			return (SerializedPrimitive)guid.ToString( "D" );
		}
		
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Guid ToGuid( this SerializedData data, IForwardReferenceMap l = null ) 
		{
			return Guid.ParseExact( (string)data, "D" );
		}
	}
}