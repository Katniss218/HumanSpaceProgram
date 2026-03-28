using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization.Descriptors
{
    /// <summary>
    /// Base class for types that require stack recursion (Classes, Structs).
    /// </summary>
    public abstract class CompositeDescriptor : ICompositeDescriptor
    {
        /// <summary>
        /// The type being described, which should be a composite type (has one or more members).
        /// </summary>
        public abstract Type MappedType { get; }

        public abstract int GetStepCount( object target );
        public abstract IMemberInfo GetMemberInfo( int stepIndex );

        public virtual IEnumerator<IMemberInfo> GetMemberEnumerator( object target ) => null;

        public abstract object CreateInitialTarget( SerializedData data, SerializationContext ctx );
        public virtual ObjectStructure DetermineObjectStructure( Type declaredType, Type actualType, SerializationConfiguration config, out bool needsId, out bool needsType )
        {
            SerializationHelpers.DetermineObjectStructure( declaredType, actualType, out needsId, out needsType );

            return ObjectStructure.InlineMetadata;
        }

        public virtual int GetConstructionStepCount( object target ) => 0;
        public virtual object Construct( object initialTarget ) => initialTarget;

        public virtual int GetMethodCount() => 0;
        public virtual IMethodInfo GetMethodInfo( int methodIndex ) => throw new IndexOutOfRangeException();
    }
}