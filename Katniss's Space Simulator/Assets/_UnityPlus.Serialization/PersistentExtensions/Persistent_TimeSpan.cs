using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace UnityPlus.Serialization
{
	public static class Persistent_TimeSpan
	{
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedPrimitive GetData( this TimeSpan dateTime, IReverseReferenceMap l = null )
		{
            // TimeSpan is saved as `[-][d'.']hh':'mm':'ss['.'fffffff]`.
            return (SerializedPrimitive)dateTime.ToString( "c", CultureInfo.InvariantCulture );
		}
		
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static TimeSpan ToTimeSpan( this SerializedData data, IForwardReferenceMap l = null ) 
		{
			return TimeSpan.ParseExact( (string)data, "c", CultureInfo.InvariantCulture );
		}
	}
}