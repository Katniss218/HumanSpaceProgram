using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityPlus.Serialization.DataHandlers;

namespace UnityPlus.Serialization.Strategies
{
    /// <summary>
    /// A serialization strategy that can round-trip full scene data.
    /// </summary>
    /// <remarks>
    /// - Object actions are suffixed by _Object <br />
    /// - Data actions are suffixed by _Data
    /// </remarks>
    public sealed class ExplicitHierarchyGameObjectsStrategy
    {
        /// <summary>
        /// Determines which objects will be saved.
        /// </summary>
        public Func<IEnumerable<GameObject>> RootObjectsGetter { get; }

        public ISerializedDataHandler DataHandler { get; }

        /// <summary>
        /// Determines which objects (including child objects) returned by the <see cref="RootObjectsGetter"/> will be excluded from saving.
        /// </summary>
        public uint IncludedObjectsMask { get; set; } = uint.MaxValue;

        SerializedData _objects;
        SerializedData _data;

        public List<GameObject> LastSpawnedRoots { get; private set; } = new List<GameObject>();

        public ExplicitHierarchyGameObjectsStrategy( ISerializedDataHandler dataHandler, Func<IEnumerable<GameObject>> rootObjectsGetter )
        {
            if( dataHandler == null )
            {
                throw new ArgumentNullException( nameof( dataHandler ), $"Serialized data handler must not be null." );
            }
            if( rootObjectsGetter == null )
            {
                throw new ArgumentNullException( nameof( rootObjectsGetter ), $"Object getter func must not be null." );
            }
            this.DataHandler = dataHandler;
            this.RootObjectsGetter = rootObjectsGetter;
        }

        public IEnumerator SaveAsync_Data( IReverseReferenceMap s )
        {
            IEnumerable<GameObject> rootObjects = this.RootObjectsGetter();

            SerializedArray objData = new SerializedArray();

            foreach( var go in rootObjects )
            {
                StratUtils.SaveGameObjectHierarchy_Data( s, go, this.IncludedObjectsMask, ref objData );

                yield return null;
            }

            this._data = objData;
        }

        public IEnumerator SaveAsync_Object( IReverseReferenceMap s )
        {
            IEnumerable<GameObject> rootObjects = this.RootObjectsGetter();

            SerializedArray objData = new SerializedArray();

            foreach( var go in rootObjects )
            {
                StratUtils.SaveGameObjectHierarchy_Objects( go, s, this.IncludedObjectsMask, objData );

                yield return null;
            }

            // Cleanup Stage. \/

            this._objects = objData;
            DataHandler.WriteObjectsAndData( _objects, _data );
            this._objects = null;
            this._data = null;
        }

        List<Behaviour> behsToReenable = new List<Behaviour>();

        public IEnumerator LoadAsync_Object( IForwardReferenceMap l )
        {
            (_objects, _data) = DataHandler.ReadObjectsAndData();

            this.LastSpawnedRoots.Clear();
            foreach( var goJson in (SerializedArray)this._objects )
            {
                try
                {
                    GameObject root = StratUtils.InstantiateHierarchyObjects( l, goJson, null, this.behsToReenable );
                    this.LastSpawnedRoots.Add( root );
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"[{nameof( ExplicitHierarchyGameObjectsStrategy )}] Failed to deserialize a root GameObject with ID: `{goJson?[KeyNames.ID] ?? "<null>"}`." );
                    Debug.LogException( ex );
                }

                yield return null;
            }
        }

        public IEnumerator LoadAsync_Data( IForwardReferenceMap l )
        {
            foreach( var dataElement in (SerializedArray)this._data )
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