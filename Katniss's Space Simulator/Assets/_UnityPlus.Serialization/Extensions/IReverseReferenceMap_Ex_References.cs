using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityPlus.AssetManagement;

namespace UnityPlus.Serialization
{
    public static class IReverseReferenceMap_Ex_References
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializedData WriteObjectReference<T>( this IReverseReferenceMap s, T value ) where T : class
        {
            // A missing '$ref' node means the reference is broken.

            if( value.IsUnityNull() )
            {
                return null;
            }

            Guid guid = s.GetID( value );

            return new SerializedObject()
            {
                { KeyNames.REF, guid.SerializeGuid() }
            };
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializedObject WriteAssetReference<T>( this IReverseReferenceMap s, T assetRef ) where T : class
        {
            if( assetRef.IsUnityNull() )
            {
                return null;
            }

            string assetID = AssetRegistry.GetAssetID( assetRef );
            if( assetID == null )
            {
                return null;
            }

            return new SerializedObject()
            {
                { KeyNames.ASSETREF, assetID }
            };
        }
    }
}