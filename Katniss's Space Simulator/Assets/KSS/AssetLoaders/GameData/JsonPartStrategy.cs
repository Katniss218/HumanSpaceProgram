using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Json;
using UnityPlus.Serialization.Strategies;

namespace KSS.AssetLoaders.GameData
{
    /// <summary>
    /// Saves or loads a part from json. Very similar to explicit hierarchy strat, but has a different purpose.
    /// </summary>
    public sealed class JsonPartStrategy
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
        public GameObject RootToSave { private get; set; }

        public void Save_Object( ISaver s )
        {
            StratCommon.ValidateFileOnSave( ObjectsFilename, StratCommon.OBJECTS_NOUN );

            SerializedArray objData = new SerializedArray();

            StratUtils.SaveGameObjectHierarchy_Objects( RootToSave, s, uint.MaxValue, objData );

            StratCommon.WriteToFile( ObjectsFilename, objData );
        }

        public void Save_Data( ISaver s )
        {
            StratCommon.ValidateFileOnSave( DataFilename, StratCommon.OBJECTS_DATA_NOUN );

            SerializedArray objData = new SerializedArray();

            StratUtils.SaveGameObjectHierarchy_Data( s, RootToSave, uint.MaxValue, ref objData );

            StratCommon.WriteToFile( DataFilename, objData );
        }
        public void Load_Object( ILoader l )
        {
            StratCommon.ValidateFileOnLoad( ObjectsFilename, StratCommon.OBJECTS_NOUN );
            SerializedArray objects = (SerializedArray)StratCommon.ReadFromFile( ObjectsFilename );

            var obj = objects.First();

            try
            {
                LastSpawnedRoot = StratUtils.InstantiateHierarchyObjects( l, obj, null, null );
            }
            catch( Exception ex )
            {
                Debug.LogError( $"Failed to deserialize a root GameObject with ID: `{obj?["$id"] ?? "<null>"}`." );
                Debug.LogException( ex );
            }
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
    }
}