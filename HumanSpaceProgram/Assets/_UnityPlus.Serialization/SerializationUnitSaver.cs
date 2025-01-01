using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization.ReferenceMaps;

namespace UnityPlus.Serialization
{
    public class SerializationUnitSaver<T> : ISaver
    {
        private bool[] _finishedMembers;
        private SerializedData[] _data;
        private T[] _objects;

        private int _context = default;

        public IReverseReferenceMap RefMap { get; set; }

        internal SerializationUnitSaver( T[] objects, int context )
        {
            this.RefMap = new BidirectionalReferenceStore();
            this._objects = objects;
            this._context = context;
        }

        public bool ShouldPause()
        {
            return false;
        }

        //
        //  Acting methods.
        //

        /// <summary>
        /// Performs serialization of the previously specified objects.
        /// </summary>
        public void Serialize( int maxIters = 10 )
        {
            this._finishedMembers = new bool[_objects.Length];
            this._data = new SerializedData[_objects.Length];

            for( int i = 0; i < maxIters; i++ )
            {
                MappingResult result = this.SaveCallback();
                if( result != MappingResult.Progressed )
                    return;
            }
        }

        /// <summary>
        /// Performs serialization of the previously specified objects.
        /// </summary>
        public void Serialize( IReverseReferenceMap s, int maxIters = 10 )
        {
            if( s == null )
                throw new ArgumentNullException( nameof( s ), $"The reference map to use can't be null." );

            this._finishedMembers = new bool[_objects.Length];
            this._data = new SerializedData[_objects.Length];
            this.RefMap = s;

            for( int i = 0; i < maxIters; i++ )
            {
                MappingResult result = this.SaveCallback();
                if( result != MappingResult.Progressed )
                    return;
            }
        }

        //
        //  Retrieval methods.
        //

        /// <summary>
        /// Returns the data that was serialized.
        /// </summary>
        public IEnumerable<SerializedData> GetData()
        {
            if( _data == null )
                throw new InvalidOperationException( $"Can't get the saved data before any has been saved." );

            return _data;
        }

        /// <summary>
        /// Returns the data that was serialized, but only of objects that are of the specified type.
        /// </summary>
        public IEnumerable<SerializedData> GetDataOfType<TDerived>()
        {
            if( _data == null )
                throw new InvalidOperationException( $"Can't get the saved data before any has been saved." );

            return _data.Where( d =>
            {
                return d.TryGetValue( KeyNames.TYPE, out var type ) && typeof( TDerived ).IsAssignableFrom( type.DeserializeType() );
            } );
        }

        private MappingResult SaveCallback()
        {
            bool anyFailed = false;
            bool anyFinished = false;
            bool anyProgressed = false;

            for( int i = 0; i < _objects.Length; i++ )
            {
                if( _finishedMembers[i] )
                    continue;

                T obj = _objects[i];

                var mapping = SerializationMappingRegistry.GetMapping<T>( _context, obj );

                var data = _data[i];
                MappingResult memberResult = mapping.SafeSave<T>( obj, ref data, this );
                switch( memberResult )
                {
                    case MappingResult.Finished:
                        _finishedMembers[i] = true;
                        anyFinished = true;
                        break;
                    case MappingResult.Failed:
                        anyFailed = true;
                        break;
                    case MappingResult.Progressed:
                        anyProgressed = true;
                        break;
                }

                _data[i] = data;
            }

            return MappingResult_Ex.GetCompoundResult( anyFailed, anyFinished, anyProgressed );
        }
    }
}