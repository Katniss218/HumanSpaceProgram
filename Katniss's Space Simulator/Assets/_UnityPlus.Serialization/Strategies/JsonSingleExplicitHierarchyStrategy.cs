using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization.Strategies
{
    /// <summary>
    /// Explicit hierarchy but for a single root object.
    /// </summary>
    public sealed class JsonSingleExplicitHierarchyStrategy
    {
        /// <summary>
        /// Determines which objects will be saved.
        /// </summary>
        public Func<GameObject> RootObjectGetter { get; set; }

        public ISerializedDataHandler DataHandler { get; }

        public GameObject LastSpawnedRoot { get; private set; }

        SerializedData _objects;
        SerializedData _data;

        public JsonSingleExplicitHierarchyStrategy( ISerializedDataHandler dataHandler, Func<GameObject> rootObjectGetter )
        {
            if( dataHandler == null )
            {
                throw new ArgumentNullException( nameof( dataHandler ), $"Serialized data handler must not be null." );
            }
            if( rootObjectGetter == null )
            {
                throw new ArgumentNullException( nameof( rootObjectGetter ), $"Object getter func must not be null." );
            }
            this.DataHandler = dataHandler;
            this.RootObjectGetter = rootObjectGetter;
        }

        public void Save_Data( ISaver s )
        {
            SerializedArray objData = new SerializedArray();

            StratUtils.SaveGameObjectHierarchy_Data( s, this.RootObjectGetter(), uint.MaxValue, ref objData );

            this._data = objData;
        }
        
        public IEnumerator SaveAsync_Data( ISaver s )
        {
            SerializedArray objData = new SerializedArray();

            StratUtils.SaveGameObjectHierarchy_Data( s, this.RootObjectGetter(), uint.MaxValue, ref objData );

            yield return null;

            this._data = objData;
        }

        public void Save_Object( ISaver s )
        {
            SerializedArray objData = new SerializedArray();

            StratUtils.SaveGameObjectHierarchy_Objects( this.RootObjectGetter(), s, uint.MaxValue, objData );

            // Cleanup Stage. \/

            this._objects = objData;
            DataHandler.WriteObjectsAndData( _objects, _data );
            this._objects = null;
            this._data = null;
        }

        public IEnumerator SaveAsync_Object( ISaver s )
        {
            SerializedArray objData = new SerializedArray();

            StratUtils.SaveGameObjectHierarchy_Objects( this.RootObjectGetter(), s, uint.MaxValue, objData );

            yield return null;

            // Cleanup Stage. \/

            this._objects = objData;
            DataHandler.WriteObjectsAndData( _objects, _data );
            this._objects = null;
            this._data = null;
        }

        List<Behaviour> behsToReenable = new List<Behaviour>();

        public void Load_Object( ILoader l )
        {
            (_objects, _data) = DataHandler.ReadObjectsAndData();

            SerializedData obj = ((SerializedArray)_objects).First();

            try
            {
                this.LastSpawnedRoot = StratUtils.InstantiateHierarchyObjects( l, obj, null, this.behsToReenable );
            }
            catch( Exception ex )
            {
                Debug.LogError( $"Failed to deserialize a root GameObject with ID: `{obj?["$id"] ?? "<null>"}`." );
                Debug.LogException( ex );
            }
        }
        
        public IEnumerator LoadAsync_Object( ILoader l )
        {
            (_objects, _data) = DataHandler.ReadObjectsAndData();

            SerializedData obj = ((SerializedArray)_objects).First();

            try
            {
                this.LastSpawnedRoot = StratUtils.InstantiateHierarchyObjects( l, obj, null, this.behsToReenable );
            }
            catch( Exception ex )
            {
                Debug.LogError( $"Failed to deserialize a root GameObject with ID: `{obj?["$id"] ?? "<null>"}`." );
                Debug.LogException( ex );
            }

            yield return null;
        }

        public void Load_Data( ILoader l )
        {
            foreach( var dataElement in (SerializedArray)_data )
            {
                StratUtils.ApplyDataToHierarchyElement( l, dataElement );
            }

            // Cleanup Stage. \/

            this._objects = null;
            this._data = null;
        }

        public IEnumerator LoadAsync_Data( ILoader l )
        {
            foreach( var dataElement in (SerializedArray)_data )
            {
                StratUtils.ApplyDataToHierarchyElement( l, dataElement );

                yield return null;
            }

            yield return null;

            // Cleanup Stage. \/

            foreach( var beh in this.behsToReenable )
                beh.enabled = true;
            this.behsToReenable = new List<Behaviour>();

            this._objects = null;
            this._data = null;
        }
    }
}