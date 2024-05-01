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
	public static class Persistent_DateTime
	{
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedPrimitive GetData( this DateTime dateTime, IReverseReferenceMap l = null )
		{
			// DateTime should be saved as an ISO-8601 string.
			return (SerializedPrimitive)dateTime.ToString( "s", CultureInfo.InvariantCulture );
		}
		
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static DateTime ToDateTime( this SerializedData data, IForwardReferenceMap l = null ) 
		{
			return DateTime.ParseExact( (string)data, "s", CultureInfo.InvariantCulture );
		}
	}
}