using System;
using System.Runtime.CompilerServices;
using UnityPlus.AssetManagement;

namespace UnityPlus.Serialization
{
	public static class IForwardReferenceMap_Ex_References
	{
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static T ReadObjectReference<T>( this IForwardReferenceMap l, SerializedData data ) where T : class
        {
			if( data == null )
				return null;

			if( data.TryGetValue( KeyNames.REF, out SerializedData refData ) )
			{
				Guid guid = refData.DeserializeGuid();

				return l.GetObj( guid ) as T;
			}
			return null;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static T ReadAssetReference<T>( this IForwardReferenceMap l, SerializedData data ) where T : class
        {
            if( data == null )
                return null;

            if( data.TryGetValue( KeyNames.ASSETREF, out SerializedData refData ) )
			{
				string assetID = (string)refData;

				return AssetRegistry.Get<T>( assetID );
			}
			return null;
		}
	}
}