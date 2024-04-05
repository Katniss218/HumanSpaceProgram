using System;
using System.Runtime.CompilerServices;
using UnityPlus.AssetManagement;

namespace UnityPlus.Serialization
{
	public static class IReverseReferenceMap_Ex_References
	{
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedData WriteObjectReference( this IReverseReferenceMap s, object value )
		{
			// A missing '$ref' node means the reference is broken.

			if( value == null )
			{
				return new SerializedObject();
			}

			Guid guid = s.GetID( value );

			return new SerializedObject()
			{
				{ KeyNames.REF, guid.GetData() }
			};
		}

		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static SerializedObject WriteAssetReference( this IReverseReferenceMap s, object assetRef )
		{
			if( assetRef == null )
			{
				return new SerializedObject();
			}

			string assetID = AssetRegistry.GetAssetID( assetRef );
			if( assetID == null )
			{
				return new SerializedObject();
			}

			return new SerializedObject()
			{
				{ KeyNames.ASSETREF, assetID }
			};
		}
	}
}