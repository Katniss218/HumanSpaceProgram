using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Holds the mutable state of a single serialization operation.
    /// </summary>
    /// <remarks>
    /// Used to reduce the number of parameters passed into a method.
    /// </remarks>
    public class SerializationState
    {
        public readonly SerializationContext Context;
        public readonly ExecutionStack Stack;
        public readonly HashSet<object> VisitedObjects;

        /// <summary>
        /// The final result of the operation (Reference to the root object or root DataNode).
        /// </summary>
        public object RootResult { get; set; }

        public SerializationState( SerializationContext context )
        {
            Context = context;
            Stack = new ExecutionStack();
            VisitedObjects = new HashSet<object>( ReferenceEqualityComparer.Instance );
            RootResult = null;
        }

        public class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();
            public new bool Equals( object x, object y ) => ReferenceEquals( x, y );
            public int GetHashCode( object obj ) => RuntimeHelpers.GetHashCode( obj );
        }
    }
}
