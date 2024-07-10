using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization.ReferenceMaps;

namespace UnityPlus.Serialization
{
    [Obsolete( "Not finished yet" )]
    public class SerializationUnitAsyncSaver<T> : ISaver
    {
        private struct Entry
        {
            public SerializationMapping mapping;
            public object obj;
            public SerializedData parentData;
        }

        private SerializedData[] _data;
        private T[] _objects;

        private Type _memberType; // Specifies the type that all serialized/deserialized objects will derive from. May be `typeof(object)`
        private int _context = default;

        private readonly Stack<Entry> _dynamicStack;

        public IReverseReferenceMap RefMap { get; set; }

        internal SerializationUnitAsyncSaver( T[] objects, Type memberType, int context )
        {
            this.RefMap = new BidirectionalReferenceStore();
            this._objects = objects;
            this._memberType = memberType;
            this._context = context;

            this._dynamicStack = new Stack<Entry>();
        }

        //
        //  Acting methods.
        //

        /// <summary>
        /// Performs serialization of the previously specified objects.
        /// </summary>
        public IEnumerator SerializeAsync( GameObject coroutineContainer )
        {
            this.SaveCallback();

            yield return null;
        }

        /// <summary>
        /// Performs serialization of the previously specified objects.
        /// </summary>
        public IEnumerator SerializeAsync( GameObject coroutineContainer, IReverseReferenceMap s )
        {
            if( s == null )
                throw new ArgumentNullException( nameof( s ), $"The reference map to use can't be null." );

            this.RefMap = s;
            this.SaveCallback();

            yield return null;
        }

        //
        //  Retrieval methods.
        //

        /// <summary>
        /// Returns the data that was serialized.
        /// </summary>
        public IEnumerable<SerializedData> GetData()
        {
            return _data;
        }

        /// <summary>
        /// Returns the data that was serialized, but only of objects that are of the specified type.
        /// </summary>
        public IEnumerable<SerializedData> GetDataOfType<TDerived>()
        {
            return _data.Where( d =>
            {
                return d.TryGetValue( KeyNames.TYPE, out var type ) && typeof( TDerived ).IsAssignableFrom( type.DeserializeType() );
            } );
        }

        private void SaveCallback()
        {
            _data = new SerializedData[_objects.Length];

            for( int i = 0; i < _objects.Length; i++ )
            {
                T obj = _objects[i];

                var mapping = SerializationMappingRegistry.GetMapping<T>( _context, obj );

                _data[i] = mapping.SafeSave<T>( obj, this );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void PushToDynamicStack( SerializationMapping mapping, object obj, SerializedData parentData )
        {
            _dynamicStack.Push( new Entry() { obj = obj, mapping = mapping, parentData = parentData } );
        }
    }
}