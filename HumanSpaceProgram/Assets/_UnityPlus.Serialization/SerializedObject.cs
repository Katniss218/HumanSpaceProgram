using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// A key-value pair node.
    /// </summary>
    public sealed class SerializedObject : SerializedData, IDictionary<string, SerializedData>, IEquatable<SerializedObject>
    {
        readonly Dictionary<string, SerializedData> _children;

        public ICollection<string> Keys => _children.Keys;
        public ICollection<SerializedData> Values => _children.Values;
        public int Count => _children.Count;
        public bool IsReadOnly => ((ICollection<KeyValuePair<string, SerializedData>>)_children).IsReadOnly;

        public override SerializedData this[int index]
        {
            get => throw new NotSupportedException( $"Tried to invoke int indexer, which is not supported on {nameof( SerializedObject )}." );
            set => throw new NotSupportedException( $"Tried to invoke int indexer, which is not supported on {nameof( SerializedObject )}." );
        }

        public override SerializedData this[string name]
        {
            get { return _children[name]; }
            set { _children[name] = value; }
        }

        public SerializedObject()
        {
            _children = new Dictionary<string, SerializedData>();
        }

        public SerializedObject( int capacity )
        {
            _children = new Dictionary<string, SerializedData>( capacity );
        }

        public SerializedObject( IEnumerable<KeyValuePair<string, SerializedData>> children )
        {
            _children = new Dictionary<string, SerializedData>( children );
        }

        public SerializedObject( params KeyValuePair<string, SerializedData>[] children )
        {
            _children = new Dictionary<string, SerializedData>( children );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Add( string name, SerializedData value )
        {
            _children.Add( name, value );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Add( KeyValuePair<string, SerializedData> item )
        {
            ((ICollection<KeyValuePair<string, SerializedData>>)_children).Add( item );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Clear()
        {
            _children.Clear();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool Contains( KeyValuePair<string, SerializedData> item )
        {
            return _children.Contains( item );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool ContainsKey( string key )
        {
            return _children.ContainsKey( key );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void CopyTo( KeyValuePair<string, SerializedData>[] array, int arrayIndex )
        {
            ((ICollection<KeyValuePair<string, SerializedData>>)_children).CopyTo( array, arrayIndex );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public IEnumerator<KeyValuePair<string, SerializedData>> GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool Remove( string key )
        {
            return _children.Remove( key );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool Remove( KeyValuePair<string, SerializedData> item )
        {
            return ((ICollection<KeyValuePair<string, SerializedData>>)_children).Remove( item );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public override bool TryGetValue( string key, out SerializedData value )
        {
            return _children.TryGetValue( key, out value );
        }

        /// <returns>True if the value was found, and matches the specified type.</returns>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public override bool TryGetValue<T>( string key, out T value )
        {
            if( _children.TryGetValue( key, out var tempValue ) && tempValue is T tempValueOfType )
            {
                value = tempValueOfType;
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Combines the two <see cref="SerializedObject"/>s, replacing already existing values with <paramref name="other"/>'s values.
        /// </summary>
        public void AddAll( SerializedObject other )
        {
            foreach( var kvp in other )
            {
                this._children[kvp.Key] = kvp.Value;
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_children).GetEnumerator();
        }

        public override int GetHashCode()
        {
            return _children.GetHashCode();
        }

        public override bool Equals( object obj )
        {
            if( obj is SerializedObject other )
                return this.Equals( other );

            return false;
        }

        public bool Equals( SerializedObject other )
        {
            return this._children.Count == other._children.Count
                && this._children.All( ( thisKvp ) =>
                {
                    if( !other._children.TryGetValue( thisKvp.Key, out var otherVal ) )
                        return false;

                    if( ReferenceEquals( thisKvp.Value, otherVal ) )
                        return true;

                    if( thisKvp.Value != null && thisKvp.Value.Equals( otherVal ) )
                        return true;

                    return false;
                }
            );
        }

        public override string ToString()
        {
            return $"{nameof( SerializedObject )} ({_children.Count} items)";
        }

        public override string DumpToString()
        {
            return DumpToString( 0 );
        }

        internal override string DumpToString( int indentLevel )
        {
            var sb = new System.Text.StringBuilder();
            string indent = string.Concat( Enumerable.Repeat( "- ", indentLevel ) );
            sb.AppendLine( $"{{{{{_children.Count}}}}}" );
            foreach( var kvp in _children )
            {
                sb.Append( $"{indent}- {kvp.Key} = " );
                if( kvp.Value != null )
                {
                    // If it's a primitive, we want it on the same line if possible, or handled by its own DumpToString
                    // Since DumpToString(int) is now on base, we can just call it.
                    // But for primitives, we might want to avoid the newline prefix if we are already indented?
                    // Actually, the previous logic was appending "\n" + obj.DumpToString.
                    // Let's keep it consistent.

                    if( kvp.Value is SerializedPrimitive )
                    {
                        sb.AppendLine( kvp.Value.ToString() );
                    }
                    else
                    {
                        sb.Append( kvp.Value.DumpToString( indentLevel + 1 ) );
                    }
                }
                else
                {
                    sb.AppendLine( "null" );
                }
            }
            return sb.ToString();
        }
    }
}