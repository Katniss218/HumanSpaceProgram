using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// A list of values node.
    /// </summary>
    public sealed class SerializedArray : SerializedData, IList<SerializedData>
    {
        readonly List<SerializedData> _children;

        public int Count => _children.Count;
        public bool IsReadOnly => ((ICollection<SerializedData>)_children).IsReadOnly;

        public SerializedArray()
        {
            _children = new List<SerializedData>();
        }

        public SerializedArray( IEnumerable<SerializedData> children )
        {
            _children = new List<SerializedData>( children );
        }

        public override SerializedData this[int index]
        {
            get => _children[index];
            set => _children[index] = value;
        }

        public override SerializedData this[string name]
        {
            get => throw new NotSupportedException( $"Tried to invoke string indexer, which is not supported on {nameof( SerializedArray )}." );
            set => throw new NotSupportedException( $"Tried to invoke string indexer, which is not supported on {nameof( SerializedArray )}." );
        }

        public override bool TryGetValue( string name, out SerializedData value )
        {
            throw new NotSupportedException( $"Tried to invoke {nameof( TryGetValue )}, which is not supported on {nameof( SerializedArray )}." );
        }

        public override bool TryGetValue<T>( string name, out T value )
        {
            throw new NotSupportedException( $"Tried to invoke {nameof( TryGetValue )}<T>, which is not supported on {nameof( SerializedArray )}." );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Add( SerializedData item )
        {
            _children.Add( item );
        }

        /*[MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Add( SerializedData item )
        {
            if( item is SerializedObject o ) // idk why, but the values must be first cast to their actual type.
                _children.Add( (SerializedData)o );
            else if( item is SerializedArray a )
                _children.Add( (SerializedData)a );
            else if( item is SerializedData v )
                _children.Add( v );
            else
                throw new ArgumentException( $"The value must be either object, array, or value." );
        }*/

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Clear()
        {
            _children.Clear();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool Contains( SerializedData item )
        {
            return _children.Contains( item );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void CopyTo( SerializedData[] array, int arrayIndex )
        {
            _children.CopyTo( array, arrayIndex );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public IEnumerator<SerializedData> GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public int IndexOf( SerializedData item )
        {
            return _children.IndexOf( item );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Insert( int index, SerializedData item )
        {
            _children.Insert( index, item );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool Remove( SerializedData item )
        {
            return _children.Remove( item );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void RemoveAt( int index )
        {
            _children.RemoveAt( index );
        }

        /// <summary>
        /// Combines the two <see cref="SerializedArray"/>s, replacing already existing values with <paramref name="other"/>'s values.
        /// </summary>
        public void AddAll( SerializedArray other )
        {
            foreach( var data in other )
            {
                this._children.Add( data );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_children).GetEnumerator();
        }
    }
}