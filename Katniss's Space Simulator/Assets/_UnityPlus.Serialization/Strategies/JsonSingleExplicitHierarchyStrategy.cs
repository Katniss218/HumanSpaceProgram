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
        /// The name of the file into which the object data will be saved.
        /// </summary>
        public string ObjectsFilename { get; set; }
        /// <summary>
        /// The name of the file into which the data data will be saved.
        /// </summary>
        public string DataFilename { get; set; }

        public GameObject LastSpawnedRoot { get; private set; }

        /// <summary>
        /// Determines which objects will be saved.
        /// </summary>
        public Func<GameObject> RootObjectGetter { get; }

        public JsonSingleExplicitHierarchyStrategy( Func<GameObject> rootObjectGetter )
        {
            if( rootObjectGetter == null )
            {
                throw new ArgumentNullException( nameof( rootObjectGetter ), $"Object getter func must not be null." );
            }
            this.RootObjectGetter = rootObjectGetter;
        }

        public void Save_Object( ISaver s )
        {
            StratCommon.ValidateFileOnSave( ObjectsFilename, StratCommon.OBJECTS_NOUN );

            SerializedArray objData = new SerializedArray();

            StratUtils.SaveGameObjectHierarchy_Objects( RootObjectGetter(), s, uint.MaxValue, objData );

            StratCommon.WriteToFile( ObjectsFilename, objData );
        }
        
        public IEnumerator SaveAsync_Object( ISaver s )
        {
            StratCommon.ValidateFileOnSave( ObjectsFilename, StratCommon.OBJECTS_NOUN );

            SerializedArray objData = new SerializedArray();

            StratUtils.SaveGameObjectHierarchy_Objects( RootObjectGetter(), s, uint.MaxValue, objData );

            yield return null;

            StratCommon.WriteToFile( ObjectsFilename, objData );
        }

        public void Save_Data( ISaver s )
        {
            StratCommon.ValidateFileOnSave( DataFilename, StratCommon.OBJECTS_DATA_NOUN );

            SerializedArray objData = new SerializedArray();

            StratUtils.SaveGameObjectHierarchy_Data( s, RootObjectGetter(), uint.MaxValue, ref objData );

            StratCommon.WriteToFile( DataFilename, objData );
        }
        
        public IEnumerator SaveAsync_Data( ISaver s )
        {
            StratCommon.ValidateFileOnSave( DataFilename, StratCommon.OBJECTS_DATA_NOUN );

            SerializedArray objData = new SerializedArray();

            StratUtils.SaveGameObjectHierarchy_Data( s, RootObjectGetter(), uint.MaxValue, ref objData );

            yield return null;

            StratCommon.WriteToFile( DataFilename, objData );
        }

        List<Behaviour> behsToReenable = new List<Behaviour>();

        public void Load_Object( ILoader l )
        {
            StratCommon.ValidateFileOnLoad( ObjectsFilename, StratCommon.OBJECTS_NOUN );
            SerializedArray objects = (SerializedArray)StratCommon.ReadFromFile( ObjectsFilename );

            var obj = objects.First();

            try
            {
                LastSpawnedRoot = StratUtils.InstantiateHierarchyObjects( l, obj, null, behsToReenable );
            }
            catch( Exception ex )
            {
                Debug.LogError( $"Failed to deserialize a root GameObject with ID: `{obj?["$id"] ?? "<null>"}`." );
                Debug.LogException( ex );
            }
        }
        
        public IEnumerator LoadAsync_Object( ILoader l )
        {
            StratCommon.ValidateFileOnLoad( ObjectsFilename, StratCommon.OBJECTS_NOUN );
            SerializedArray objects = (SerializedArray)StratCommon.ReadFromFile( ObjectsFilename );

            var obj = objects.First();

            try
            {
                LastSpawnedRoot = StratUtils.InstantiateHierarchyObjects( l, obj, null, behsToReenable );
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
            StratCommon.ValidateFileOnLoad( DataFilename, StratCommon.OBJECTS_DATA_NOUN );
            SerializedArray dataArray = (SerializedArray)StratCommon.ReadFromFile( DataFilename );

            foreach( var dataElement in dataArray )
            {
                StratUtils.ApplyDataToHierarchyElement( l, dataElement );
            }
        }

        public IEnumerator LoadAsync_Data( ILoader l )
        {
            StratCommon.ValidateFileOnLoad( DataFilename, StratCommon.OBJECTS_DATA_NOUN );
            SerializedArray dataArray = (SerializedArray)StratCommon.ReadFromFile( DataFilename );

            foreach( var dataElement in dataArray )
            {
                StratUtils.ApplyDataToHierarchyElement( l, dataElement );

                yield return null;
            }

            yield return null;

            foreach( var beh in behsToReenable )
                beh.enabled = true;
            behsToReenable = new List<Behaviour>();
        }
    }
}