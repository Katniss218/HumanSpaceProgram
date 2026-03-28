
using System;

namespace UnityPlus.Serialization.Descriptors
{
    public abstract class PrimitiveDescriptor<T> : IPrimitiveDescriptor
    {
        public Type MappedType => typeof( T );

        public abstract void SerializeDirect( object target, ref SerializedData data, SerializationContext ctx );
        public abstract DeserializationResult DeserializeDirect( SerializedData data, SerializationContext ctx, out object result );

        public virtual object CreateInitialTarget( SerializedData data, SerializationContext ctx ) => default( T );
        public virtual ObjectStructure DetermineObjectStructure( Type declaredType, Type actualType, SerializationConfiguration config, out bool needsId, out bool needsType )
        {
            SerializationHelpers.DetermineObjectStructure( declaredType, actualType, out needsId, out needsType );

            if( (needsId || needsType) && !config.ForceStandardJson )
            {
                return ObjectStructure.Wrapped;
            }
            return ObjectStructure.Unwrapped;
        }
    }
}