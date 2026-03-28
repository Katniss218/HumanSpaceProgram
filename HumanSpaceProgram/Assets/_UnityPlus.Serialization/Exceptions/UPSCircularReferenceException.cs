using System;
using System.Runtime.Serialization;

namespace UnityPlus.Serialization
{
    public class UPSCircularReferenceException : UPSSerializationException
    {
        public UPSCircularReferenceException( IReadonlySerializationContext ctx, string message )
            : base( ctx, message )
        {
        }
        public UPSCircularReferenceException( IReadonlySerializationContext ctx, string message, string path, IDescriptor descriptor, IMemberInfo member, string operation, Exception innerException )
           : base( ctx, message, path, descriptor, member, operation, innerException )
        {
        }



        protected UPSCircularReferenceException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }
    }
}