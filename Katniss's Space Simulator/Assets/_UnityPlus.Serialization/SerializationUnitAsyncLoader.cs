using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization.ReferenceMaps;

namespace UnityPlus.Serialization
{
    public class SerializationUnitAsyncLoader : ILoader
    {
        private SerializedData[] _data;
        private object[] _objects;

        private Type _memberType; // Specifies the type that all serialized/deserialized objects will derive from. May be `typeof(object)`
        private int _context = default;

        public IForwardReferenceMap RefMap { get; set; }

        public Dictionary<SerializedData, SerializationMapping> MappingCache { get; }

        private Stack<LoadAction> loadActionsToPerform; // something like this?

        internal SerializationUnitAsyncLoader( SerializedData[] data, Type memberType, int context )
        {
            this.RefMap = new BidirectionalReferenceStore();
            this.MappingCache = new Dictionary<SerializedData, SerializationMapping>( new SerializedDataReferenceComparer() );
            this._data = data;
            this._memberType = memberType;
            this._context = context;
        }

        internal SerializationUnitAsyncLoader( object[] objects, SerializedData[] data, Type memberType, int context )
        {
            this.RefMap = new BidirectionalReferenceStore();
            this.MappingCache = new Dictionary<SerializedData, SerializationMapping>( new SerializedDataReferenceComparer() );
            this._objects = objects;
            this._data = data;
            this._memberType = memberType;
            this._context = context;
        }

        //
        //  Acting methods.
        //

        /// <summary>
        /// Performs deserialization of the previously specified objects.
        /// </summary>
        public void Deserialize()
        {
            this.LoadCallback();
            this.LoadReferencesCallback();
        }

        /// <summary>
        /// Performs deserialization of the previously specified objects.
        /// </summary>
        public void Deserialize( IForwardReferenceMap l )
        {
            if( l == null )
                throw new ArgumentNullException( nameof( l ), $"The reference map to use can't be null." );

            this.RefMap = l;
            this.LoadCallback();
            this.LoadReferencesCallback();
        }

        /// <summary>
        /// Performs population of members of the previously specified objects.
        /// </summary>
        public void Populate()
        {
            this.PopulateCallback();
            this.LoadReferencesCallback();
        }

        /// <summary>
        /// Performs population of members of the previously specified objects.
        /// </summary>
        public void Populate( IForwardReferenceMap l )
        {
            if( l == null )
                throw new ArgumentNullException( nameof( l ), $"The reference map to use can't be null." );

            this.RefMap = l;
            this.PopulateCallback();
            this.LoadReferencesCallback();
        }

        //
        //  Retrieval methods.
        //

        /// <summary>
        /// Returns the objects that were deserialized or populated.
        /// </summary>
        public IEnumerable<object> GetObjects()
        {
            return _objects;
        }

        /// <summary>
        /// Returns the objects that were deserialized or populated, but only those that are of the specified type.
        /// </summary>
        public IEnumerable<T> GetObjectsOfType<T>()
        {
            return _objects.OfType<T>();
        }


        private void PopulateCallback()
        {
            // Called by the loader.

            for( int i = 0; i < _data.Length; i++ )
            {
                SerializedData data = _data[i];

                if( data == null )
                    continue;

                if( !data.TryGetValue( KeyNames.TYPE, out var type ) )
                    continue;

                Type type2 = type.DeserializeType();

                var mapping = SerializationMappingRegistry.GetMappingOrNull( _context, type2 );
                MappingCache[data] = mapping;

                // Parity with Member (mostly).
                object member = _objects[i];
                if( MappingHelper.DoPopulate( mapping, ref member, data, this ) )
                {
                    _objects[i] = member;
                }
            }
        }

        private void LoadCallback()
        {
            // Called by the loader.

            _objects = new object[_data.Length];

            for( int i = 0; i < _data.Length; i++ )
            {
                SerializedData data = _data[i];

                if( data == null )
                    continue;

                Type typeToAssignTo = _memberType;

                if( data.TryGetValue( KeyNames.TYPE, out var type ) )
                    typeToAssignTo = type.DeserializeType();

                var mapping = SerializationMappingRegistry.GetMappingOrNull( _context, typeToAssignTo );
                MappingCache[data] = mapping;

                object member = default;
                if( MappingHelper.DoLoad( mapping, ref member, data, this ) )
                {
                    _objects[i] = member;
                }
            }
        }

        private void LoadReferencesCallback()
        {
            // Called by the loader.

            for( int i = 0; i < _data.Length; i++ )
            {
                SerializedData data = _data[i];

                if( data == null )
                    continue;

                var mapping = MappingCache[data];

                if( mapping == null )
                    continue; // error.

                object member = _objects[i];
                if( MappingHelper.DoLoadReferences( mapping, ref member, data, this ) )
                {
                    _objects[i] = member;
                }
            }
        }
    }
}
