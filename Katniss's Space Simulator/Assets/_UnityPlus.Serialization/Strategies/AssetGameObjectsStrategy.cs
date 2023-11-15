using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization.DataHandlers;

namespace UnityPlus.Serialization.Strategies
{
    /// <summary>
    /// Can be used to save the scene using the factory-gameobjectdata scheme.
    /// </summary>
    /// <remarks>
    /// - Object actions are suffixed by _Object <br />
    /// - Data actions are suffixed by _Data
    /// </remarks>
    public sealed class AssetGameObjectsStrategy
    {
        /// <summary>
        /// Determines which objects will be saved.
        /// </summary>
        public Func<IEnumerable<GameObject>> RootObjectsGetter { get; }

        public ISerializedDataHandler DataHandler { get; }

        /// <summary>
        /// Determines which objects returned by the <see cref="RootObjectsGetter"/> will be excluded from saving.
        /// </summary>
        public uint IncludedObjectsMask { get; set; } = uint.MaxValue;

        SerializedData _objects;
        SerializedData _data;

        public AssetGameObjectsStrategy( ISerializedDataHandler dataHandler, Func<IEnumerable<GameObject>> rootObjectsGetter )
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

        private static SerializedObject WriteAssetGameObject( ISaver s, GameObject go, ClonedGameObject cbf )
        {
            Guid objectGuid = s.GetID( go );

            SerializedArray sArr = new SerializedArray();
            StratUtils.WriteReferencedChildrenRecursive( s, go, ref sArr, "" );

            SerializedObject goJson = new SerializedObject()
            {
                { KeyNames.ID, s.WriteGuid(objectGuid) },
                { "prefab", s.WriteAssetReference(cbf.OriginalAsset) },
                { "children_ids", sArr }
            };

            return goJson;
        }

        private static GameObject ReadAssetGameObject( ILoader l, SerializedData goJson )
        {
            Guid objectGuid = l.ReadGuid( goJson[KeyNames.ID] );

            GameObject prefab = l.ReadAssetReference<GameObject>( goJson["prefab"] );

            if( prefab == null )
            {
                Debug.LogWarning( $"Couldn't find a prefab `{goJson["prefab"]}`." );
            }

            GameObject go = ClonedGameObject.Instantiate( prefab ); // assumes the asset is immutable and needs to be cloned. God knows if this preserves lambdas and other non-serializable fields.

            l.SetObj( objectGuid, go );

            SerializedArray refChildren = (SerializedArray)goJson["children_ids"];
            StratUtils.AssignIDsToReferencedChildren( l, go, ref refChildren );

            return go;
        }

        private void SaveGameObjectDataRecursive( ISaver s, GameObject go, ref SerializedArray objects )
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
                    Debug.LogWarning( $"[{nameof( AssetGameObjectsStrategy )}] Couldn't serialize component '{comp}': {ex.Message}." );
                    Debug.LogException( ex );
                }

                StratUtils.TryWriteData( s, go, compData, ref objects );
            }

            SerializedData goData = go.GetData( s );
            StratUtils.TryWriteData( s, go, goData, ref objects );

            foreach( Transform ct in go.transform )
            {
                SaveGameObjectDataRecursive( s, ct.gameObject, ref objects );
            }
        }

        // -=-=-=-

        public IEnumerator SaveAsync_Data( ISaver s )
        {
            IEnumerable<GameObject> rootObjects = RootObjectsGetter();

            SerializedArray objData = new SerializedArray();

            foreach( var go in rootObjects )
            {
                if( go.GetComponent<ClonedGameObject>() == null )
                {
                    continue;
                }
                yield return null;

                SaveGameObjectDataRecursive( s, go, ref objData );
            }

            this._data = objData;
        }

        public IEnumerator SaveAdync_Object( ISaver s )
        {
            IEnumerable<GameObject> rootObjects = RootObjectsGetter();

            SerializedArray objData = new SerializedArray();

            foreach( var go in rootObjects )
            {
                // maybe some sort of customizable tag/layer masking

                ClonedGameObject cloneComp = go.GetComponent<ClonedGameObject>();
                if( cloneComp == null )
                {
                    continue;
                }

                SerializedObject goJson = WriteAssetGameObject( s, go, cloneComp );
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
            (_objects, _data) = DataHandler.ReadObjectsAndData();

            foreach( var goData in (SerializedArray)_objects )
            {
                ReadAssetGameObject( l, goData );

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