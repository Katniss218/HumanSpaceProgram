using System;
using System.Runtime.CompilerServices;
using UnityPlus.AssetManagement;

namespace UnityPlus.Serialization
{
	public static class IForwardReferenceMap_Ex_References
	{
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static object ReadObjectReference( this IForwardReferenceMap l, SerializedData data )
		{
			// should only be called in data actions.

			// A missing '$ref' node means the reference couldn't save properly.

			if( ((SerializedObject)data).TryGetValue( KeyNames.REF, out SerializedData refData ) )
			{
				Guid guid = refData.AsGuid();

				return l.GetObj( guid );
			}
			return null;
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static T ReadAssetReference<T>( this IForwardReferenceMap l, SerializedData data ) where T : class
		{
			if( ((SerializedObject)data).TryGetValue( KeyNames.ASSETREF, out SerializedData refData ) )
			{
				string assetID = refData.AsString();

				return AssetRegistry.Get<T>( assetID );
			}
			return null;
		}
	}
}