using UnityEngine;
using UnityPlus.AssetManagement;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Serializes an object as an Asset ID ("$assetref") using the AssetRegistry.
    /// Used when the member context is set to ObjectContext.Asset.
    /// </summary>
    public class AssetDescriptor<T> : PrimitiveDescriptor<T> where T : class
    {
        public override void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx )
        {
            if( target == null || target.IsUnityNull() )
            {
                data = null;
                return;
            }

            string assetID = AssetRegistry.GetAssetID( target );

            if( assetID != null )
            {
                data = new SerializedObject()
                {
                    { KeyNames.ASSETREF, (SerializedPrimitive)assetID }
                };
            }
            else
            {
                data = null;
            }
        }

        public override DeserializationResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            result = null;
            if( data == null )
                return DeserializationResult.Success;

            if( data.TryGetValue( KeyNames.ASSETREF, out SerializedData refData ) )
            {
                string assetID = (string)refData;
                result = AssetRegistry.Get<T>( assetID );
            }

            return DeserializationResult.Success;
        }
    }
}