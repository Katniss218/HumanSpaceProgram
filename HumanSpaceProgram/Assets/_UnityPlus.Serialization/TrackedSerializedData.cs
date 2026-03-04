using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// A wrapper around SerializedData that maintains a link to its Parent.
    /// This allows for upward navigation (Parent) and in-place replacement (via Parent[Key] = NewValue).
    /// </summary>
    public readonly struct TrackedSerializedData : IEquatable<TrackedSerializedData>
    {
        /// <summary>
        /// The current data node.
        /// </summary>
        public SerializedData Value { get; }

        /// <summary>
        /// The parent node containing this data.
        /// </summary>
        public SerializedData Parent { get; }

        /// <summary>
        /// The absolute root of the hierarchy.
        /// </summary>
        public SerializedData Root { get; }

        /// <summary>
        /// The dictionary key in the Parent (if Parent is Object). Null otherwise.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The array index in the Parent (if Parent is Array). -1 otherwise.
        /// </summary>
        public int Index { get; }

        public bool IsByIndex => Index != -1;
        public bool IsByName => Index == -1 && Name != null;
        /// <summary>
        /// Whether this TrackedSerializedData is the root of the traversal (i.e., has no Parent).
        /// </summary>
        public bool IsRoot => Parent == null;

        public TrackedSerializedData( SerializedData rootValue )
        {
            this.Value = rootValue;
            this.Parent = null;
            this.Root = rootValue;
            this.Name = null;
            this.Index = -1;
        }

        public TrackedSerializedData( SerializedData value, SerializedData parent, string name, SerializedData root )
        {
            this.Value = value;
            this.Parent = parent;
            this.Root = root;
            this.Name = name;
            this.Index = -1;
        }

        public TrackedSerializedData( SerializedData value, SerializedData parent, int index, SerializedData root )
        {
            this.Value = value;
            this.Parent = parent;
            this.Root = root;
            this.Name = null;
            this.Index = index;
        }

        /// <summary>
        /// Creates a new tracked child by index.
        /// </summary>
        public TrackedSerializedData Child( int index )
        {
            return new TrackedSerializedData( Value[index], Value, index, Root );
        }

        /// <summary>
        /// Creates a new tracked child by name.
        /// </summary>
        public TrackedSerializedData Child( string name )
        {
            return new TrackedSerializedData( Value[name], Value, name, Root );
        }

        public bool TryGetValue( string name, out TrackedSerializedData result )
        {
            if( this.Value.TryGetValue( name, out var childValue ) )
            {
                result = new TrackedSerializedData( childValue, this.Value, name, Root );
                return true;
            }
            result = default;
            return false;
        }

        public bool TryGetValue( int index, out TrackedSerializedData result )
        {
            if( this.Value is SerializedArray arr && index >= 0 && index < arr.Count )
            {
                result = new TrackedSerializedData( arr[index], this.Value, index, Root );
                return true;
            }
            result = default;
            return false;
        }

        /// <summary>
        /// Replaces the value of this node in the Parent container.
        /// </summary>
        public void Replace( SerializedData newValue )
        {
            if( IsRoot ) 
                throw new InvalidOperationException( "Cannot replace root node via TrackedSerializedData." );

            if( IsByName )
            {
                ((SerializedObject)Parent)[Name] = newValue;
            }
            else if( IsByIndex )
            {
                ((SerializedArray)Parent)[Index] = newValue;
            }
        }

        public IEnumerable<TrackedSerializedData> EnumerateChildren()
        {
            if( Value is SerializedObject obj )
            {
                foreach( var kvp in obj )
                {
                    yield return new TrackedSerializedData( kvp.Value, Value, kvp.Key, Root );
                }
            }
            else if( Value is SerializedArray arr )
            {
                for( int i = 0; i < arr.Count; ++i )
                {
                    yield return new TrackedSerializedData( arr[i], Value, i, Root );
                }
            }
        }

        public bool Equals( TrackedSerializedData other )
        {
            // Identity equality based on the DOM node reference
            return ReferenceEquals( Value, other.Value )
                && ReferenceEquals( Parent, other.Parent )
                && Name == other.Name
                && Index == other.Index;
        }

        public override bool Equals( object obj ) => obj is TrackedSerializedData other && Equals( other );

        public override int GetHashCode()
        {
            return HashCode.Combine( Value, Parent, Name, Index );
        }

        public static bool operator ==( TrackedSerializedData left, TrackedSerializedData right ) => left.Equals( right );
        public static bool operator !=( TrackedSerializedData left, TrackedSerializedData right ) => !left.Equals( right );
    }
}