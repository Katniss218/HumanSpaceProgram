using System;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Serializes an object as a reference ("$ref") using the ReferenceMap.
    /// Used when the member context is set to ObjectContext.Ref.
    /// </summary>
    public class ReferenceDescriptor<T> : PrimitiveDescriptor<T> where T : class
    {
        public override void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx )
        {
            data = ctx.ReverseMap.WriteObjectReference( (T)target );
        }

        public override DeserializationResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result )
        {
            result = null;
            if( data == null )
                return DeserializationResult.Success;

            if( !data.TryGetValue( KeyNames.REF, out SerializedData refVal ) )
            {
                return DeserializationResult.Success; 
            }

            Guid refGuid = refVal.DeserializeGuid();
            if( refGuid == Guid.Empty )
            {
                return DeserializationResult.Success; 
            }

            if( ctx.ForwardMap.TryGetObj( refGuid, out object existingObj ) )
            {
                result = existingObj;
                return DeserializationResult.Success;
            }

            return DeserializationResult.Deferred;
        }
    }
}