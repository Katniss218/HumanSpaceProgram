using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    public sealed class SerializedObject : SerializedData, IDictionary<string, SerializedData>
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

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Add( string name, SerializedData value )
        {
            _children.Add( name, value );
        }

        /*[MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Add( string name, SerializedData value )
        {
            if( value is SerializedObject o ) // idk why, but the values must be first cast to their actual type.
                _children.Add( name, (SerializedData)o );
            else if( value is SerializedArray a )
                _children.Add( name, (SerializedData)a );
            else if( value is SerializedData v )
                _children.Add( name, v );
            else
                throw new ArgumentException( $"The value must be either object, array, or value." );
        }*/

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

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_children).GetEnumerator();
        }
    }
}