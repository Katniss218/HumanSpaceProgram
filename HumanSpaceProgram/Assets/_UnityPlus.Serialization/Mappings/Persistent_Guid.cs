using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
	public static class Persistent_Guid
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static string SerializeGuidAsKey( this Guid guid )
        {
            return guid.ToString( "D" );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Guid DeserializeGuidAsKey( this string keyName )
        {
            return Guid.ParseExact( keyName, "D" );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedPrimitive SerializeGuid( this Guid guid )
		{
			// GUIDs should be saved in the '00000000-0000-0000-0000-000000000000' format, with dashes, and without extra anything.
			return (SerializedPrimitive)guid.ToString( "D" );
		}
		
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Guid DeserializeGuid( this SerializedData data ) 
		{
			return Guid.ParseExact( (string)data, "D" );
		}
	}
}