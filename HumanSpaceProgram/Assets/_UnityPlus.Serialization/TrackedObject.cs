using System;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Represents a specific node in the object graph during traversal.
    /// Encapsulates the current object, its owner (Parent), and the member info used to access/modify it.
    /// </summary>
    public readonly struct TrackedObject : IEquatable<TrackedObject>
    {
        /// <summary>
        /// The current object instance. 
        /// For value types, this is a boxed copy.
        /// </summary>
        public object Target { get; }

        /// <summary>
        /// The object that owns the Target.
        /// </summary>
        public object Parent { get; }

        /// <summary>
        /// The member info describing how Target is accessed on Parent.
        /// </summary>
        public IMemberInfo Member { get; }

        /// <summary>
        /// Whether this TrackedObject is the root of the traversal (i.e., has no Parent).
        /// </summary>
        public bool IsRoot => Parent == null;

        public TrackedObject( object target )
        {
            Target = target;
            Parent = null;
            Member = null;
        }

        public TrackedObject( object target, object parent, IMemberInfo member )
        {
            Target = target;
            Parent = parent;
            Member = member;
        }

        /// <summary>
        /// Creates a new TrackedObject with an updated Target value, keeping Parent and Member the same.
        /// Used during instantiation or value modification.
        /// </summary>
        public TrackedObject WithTarget( object newTarget )
        {
            return new TrackedObject( newTarget, Parent, Member );
        }

        public bool Equals( TrackedObject other )
        {
            // Identity comparison
            return ReferenceEquals( Target, other.Target )
                && ReferenceEquals( Parent, other.Parent )
                && Equals( Member, other.Member );
        }

        public override bool Equals( object obj ) => obj is TrackedObject other && Equals( other );
        public override int GetHashCode() => HashCode.Combine( Target, Parent, Member );
        public static bool operator ==( TrackedObject left, TrackedObject right ) => left.Equals( right );
        public static bool operator !=( TrackedObject left, TrackedObject right ) => !left.Equals( right );
    }
}