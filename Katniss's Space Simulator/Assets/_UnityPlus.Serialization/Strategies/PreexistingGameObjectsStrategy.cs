using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;

namespace UnityPlus.Serialization.Strategies
{
    /// <summary>
    /// Serializes only the data of already existing scene objects.
    /// </summary>
    /// <remarks>
    /// - Object actions are suffixed by _Object <br />
    /// - Data actions are suffixed by _Data
    /// </remarks>
    public sealed class PreexistingGameObjectsStrategy
    {
        /// <summary>
        /// Determines which objects will have their data saved, and loaded.
        /// </summary>
        public Func<IEnumerable<GameObject>> RootObjectsGetter { get; }

        public ISerializedDataHandler DataHandler { get; }

        /// <summary>
        /// Determines which objects returned by the <see cref="RootObjectsGetter"/> will be excluded from saving.
        /// </summary>
        public uint IncludedObjectsMask { get; set; } = uint.MaxValue;

        SerializedData _objects;
        SerializedData _data;

        /// <param name="rootObjectsGetter">Determines which objects will have their data saved, and loaded.</param>
        public PreexistingGameObjectsStrategy( ISerializedDataHandler dataHandler, Func<IEnumerable<GameObject>> rootObjectsGetter )
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

        private static SerializedObject WriteGameObject( ISaver s, GameObject go, PreexistingReference guidComp )
        {
            SerializedArray sArr = new SerializedArray();
            StratUtils.WriteReferencedChildrenRecursive( s, go, ref sArr, "" );

            SerializedObject goJson = new SerializedObject()
            {
                { KeyNames.ID, s.WriteGuid( guidComp.GetPersistentGuid() ) },
                { "children_ids", sArr }
            };

            return goJson;
        }

        private void SaveGameObjectData( ISaver s, GameObject go, PreexistingReference guidComp, ref SerializedArray objects )
        {
            if( !go.IsInLayerMask( IncludedObjectsMask ) )
            {
                return;
            }

            Component[] comps = go.GetComponents();
            for( int i = 0; i < comps.Length; i++ )
            {
                Component comp = comps[i];
                SerializedData compData = null;
                try
                {
                    compData = comp.GetData( s );
                }
                catch( Exception ex )
                {
                    Debug.LogWarning( $"[{nameof( PreexistingGameObjectsStrategy )}] Couldn't serialize component '{comp}': {ex.Message}." );
                    Debug.LogException( ex );
                }

                StratUtils.TryWriteData( s, comp, compData, ref objects );
            }

            SerializedData goData = go.GetData( s );
            objects.Add( new SerializedObject()
            {
                { KeyNames.REF, s.WriteGuid( guidComp.GetPersistentGuid() ) },
                { "data", goData }
            } );
        }

        /// <summary>
        /// Saves the data about the gameobjects and their persistent components. Does not include child objects.
        /// </summary>
        public IEnumerator SaveAsync_Data( ISaver s )
        {
            IEnumerable<GameObject> rootObjects = RootObjectsGetter();

            SerializedArray objData = new SerializedArray();

            foreach( var go in rootObjects )
            {
                PreexistingReference guidComp = go.GetComponent<PreexistingReference>();
                if( guidComp == null )
                {
                    continue;
                }

                SaveGameObjectData( s, go, guidComp, ref objData );

                yield return null;
            }

            this._data = objData;
        }

        public IEnumerator SaveAsync_Object( ISaver s )
        {
            IEnumerable<GameObject> rootObjects = RootObjectsGetter();

            SerializedArray objData = new SerializedArray();

            foreach( var go in rootObjects )
            {
                PreexistingReference guidComp = go.GetComponent<PreexistingReference>();
                if( guidComp == null )
                {
                    continue;
                }

                SerializedObject goJson = WriteGameObject( s, go, guidComp );
                objData.Add( goJson );

                yield return null;
            }

            // Cleanup Stage. \/

            this._objects = objData;
            DataHandler.WriteObjectsAndData( _objects, _data );
            this._objects = null;
            this._data = null;
        }

        public IEnumerator LoadAsync_Object( ILoader l )
        {
            IEnumerable<GameObject> rootObjects = RootObjectsGetter.Invoke();
            foreach( var go in rootObjects )
            {
                PreexistingReference guidComp = go.GetComponent<PreexistingReference>();
                if( guidComp == null )
                {
                    continue;
                }

                l.SetObj( guidComp.GetPersistentGuid(), go );

                yield return null;
            }

            // Loads the IDs of objects returned by the getter func, then gets them by ID from the loader's reference dict.

            (_objects, _data) = DataHandler.ReadObjectsAndData();

            foreach( var goData in (SerializedArray)_objects )
            {
                Guid objectGuid = l.ReadGuid( goData[KeyNames.ID] );
                SerializedArray refChildren = (SerializedArray)goData["children_ids"];
                StratUtils.AssignIDsToReferencedChildren( l, (GameObject)l.GetObj( objectGuid ), ref refChildren );

                yield return null;
            }
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

            this._objects = null;
            this._data = null;
        }
    }
}