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
            if( data == null )
                return null;

            if( data.TryGetValue( KeyNames.REF, out SerializedData refData ) )
            {
                Guid guid = refData.DeserializeGuid();

                return l.GetObj( guid );
            }

            return null;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static T ReadObjectReference<T>( this IForwardReferenceMap l, SerializedData data ) where T : class
        {
            return ReadObjectReference( l, data ) as T;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool TryReadObjectReference( this IForwardReferenceMap l, SerializedData data, out object obj )
        {
            if( data == null )
            {
                obj = null;
                return true;
            }

            if( data.TryGetValue( KeyNames.REF, out SerializedData refData ) )
            {
                Guid guid = refData.DeserializeGuid();

                return l.TryGetObj( guid, out obj );
            }

            obj = null;
            return false;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool TryReadObjectReference<T>( this IForwardReferenceMap l, SerializedData data, out T obj ) where T : class
        {
            bool res = TryReadObjectReference( l, data, out var obj2 );
            obj = obj2 as T;
            return res;
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