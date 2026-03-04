using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization
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

        public virtual int GetConstructionStepCount( object target ) => 0;
        public virtual object Construct( object initialTarget ) => initialTarget;

        public virtual void OnSerializing( object target, SerializationContext context ) { }
        public virtual void OnDeserialized( object target, SerializationContext context ) { }

        public virtual int GetMethodCount() => 0;
        public virtual IMethodInfo GetMethodInfo( int methodIndex ) => throw new IndexOutOfRangeException();
    }
}