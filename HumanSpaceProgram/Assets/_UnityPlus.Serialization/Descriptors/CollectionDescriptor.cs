using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization
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

        public virtual int GetConstructionStepCount( object target ) => 0;
        public object Construct( object initialTarget ) => initialTarget;

        public virtual void OnSerializing( object target, SerializationContext context ) { }
        public virtual void OnDeserialized( object target, SerializationContext context ) { }

        public virtual int GetMethodCount() => 0;
        public virtual IMethodInfo GetMethodInfo( int methodIndex ) => throw new IndexOutOfRangeException();
    }
}