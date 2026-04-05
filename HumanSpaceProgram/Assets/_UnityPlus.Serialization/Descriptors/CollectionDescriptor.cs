using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization.Descriptors
{
    public abstract class CollectionDescriptor : ICollectionDescriptor
    {
        /// <summary>
        /// The type being described, which should be a collection type (Array, List, etc.).
        /// </summary>
        public abstract Type MappedType { get; }

        public abstract object Resize( object target, int newSize );
        public abstract int GetStepCount( object target );
        public abstract IMemberInfo GetMemberInfo( int stepIndex );

        public virtual IEnumerator<IMemberInfo> GetMemberEnumerator( object target ) => null;
        public abstract object CreateInitialTarget( SerializedData data, SerializationContext ctx );
        public virtual ObjectStructure DetermineObjectStructure( Type declaredType, Type actualType, SerializationConfiguration config, out bool needsId, out bool needsType )
        {
            SerializationHelpers.DetermineObjectStructure( declaredType, actualType, out needsId, out needsType );

            // Set as Wrapped always (for now).
            // This can be relaxed if contextual ID generation is added, which would store which objects are actually referenced by anything.
            // But implementing that is not trivial, as we would have to either:
            //   1. modify the SerializedObjects when referee is encountered,
            //   2. do a pre-pass to determine which objects are referenced by anything
            if( /*(needsId || needsType) &&*/ !config.ForceStandardJson )
            {
                return ObjectStructure.Wrapped;
            }
            return ObjectStructure.Unwrapped;
        }

        public virtual int GetConstructionStepCount( object target ) => 0;
        public object Construct( object initialTarget ) => initialTarget;

        public virtual int GetMethodCount() => 0;
        public virtual IMethodInfo GetMethodInfo( int methodIndex ) => throw new IndexOutOfRangeException();
    }
}