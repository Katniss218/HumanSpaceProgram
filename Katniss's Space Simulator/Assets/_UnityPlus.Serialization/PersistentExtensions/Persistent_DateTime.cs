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
		public static SerializedData GetData( this DateTime dateTime, IReverseReferenceMap l = null )
		{
			// DateTime should be saved as an ISO-8601 string.
			return (SerializedPrimitive)dateTime.ToString( "s", CultureInfo.InvariantCulture );
		}
		
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static DateTime AsDateTime( this SerializedData data, IForwardReferenceMap l = null ) 
		{
			return DateTime.ParseExact( data.AsString(), "s", CultureInfo.InvariantCulture );
		}
	}
}