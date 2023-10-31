using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UnityPlus.Serialization.Strategies
{
    /// <summary>
    /// A serialization strategy that can round-trip full scene data.
    /// </summary>
    /// <remarks>
    /// - Object actions are suffixed by _Object <br />
    /// - Data actions are suffixed by _Data
    /// </remarks>
    public sealed class JsonExplicitHierarchyGameObjectsStrategy
    {
        /// <summary>
        /// The name of the file into which the object data will be saved.
        /// </summary>
        public string ObjectsFilename { get; set; }
        /// <summary>
        /// The name of the file into which the data data will be saved.
        /// </summary>
        public string DataFilename { get; set; }

        /// <summary>
        /// Determines which objects will be saved.
        /// </summary>
        public Func<IEnumerable<GameObject>> RootObjectsGetter { get; }
        /// <summary>
        /// Determines which objects (including child objects) returned by the <see cref="RootObjectsGetter"/> will be excluded from saving.
        /// </summary>
        public uint IncludedObjectsMask { get; set; } = uint.MaxValue;

        public JsonExplicitHierarchyGameObjectsStrategy( Func<IEnumerable<GameObject>> rootObjectsGetter )
        {
            if( rootObjectsGetter == null )
            {
                throw new ArgumentNullException( nameof( rootObjectsGetter ), $"Object getter func must not be null." );
            }
            this.RootObjectsGetter = rootObjectsGetter;
        }

        public IEnumerator SaveAsync_Object( ISaver s )
        {
            StratCommon.ValidateFileOnSave( ObjectsFilename, StratCommon.OBJECTS_NOUN );

            IEnumerable<GameObject> rootObjects = RootObjectsGetter();

            SerializedArray objData = new SerializedArray();

            foreach( var go in rootObjects )
            {
                StratUtils.SaveGameObjectHierarchy_Objects( go, s, IncludedObjectsMask, objData );

                yield return null;
            }

            StratCommon.WriteToFile( ObjectsFilename, objData );
        }

        public IEnumerator SaveAsync_Data( ISaver s )
        {
            StratCommon.ValidateFileOnSave( DataFilename, StratCommon.OBJECTS_DATA_NOUN );

            IEnumerable<GameObject> rootObjects = RootObjectsGetter();

            SerializedArray objData = new SerializedArray();

            foreach( var go in rootObjects )
            {
                StratUtils.SaveGameObjectHierarchy_Data( s, go, IncludedObjectsMask, ref objData );

                yield return null;
            }

            StratCommon.WriteToFile( DataFilename, objData );
        }

        List<Behaviour> behsToReenable = new List<Behaviour>();

        public IEnumerator LoadAsync_Object( ILoader l )
        {
            StratCommon.ValidateFileOnLoad( ObjectsFilename, StratCommon.OBJECTS_NOUN );
            SerializedArray objects = (SerializedArray)StratCommon.ReadFromFile( ObjectsFilename );

            foreach( var goJson in objects )
            {
                try
                {
                    StratUtils.InstantiateHierarchyObjects( l, goJson, null, behsToReenable );
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"[{nameof( JsonExplicitHierarchyGameObjectsStrategy )}] Failed to deserialize a root GameObject with ID: `{goJson?[KeyNames.ID] ?? "<null>"}`." );
                    Debug.LogException( ex );
                }

                yield return null;
            }
        }

        public IEnumerator LoadAsync_Data( ILoader l )
        {
            StratCommon.ValidateFileOnLoad( DataFilename, StratCommon.OBJECTS_DATA_NOUN );
            SerializedArray data = (SerializedArray)StratCommon.ReadFromFile( DataFilename );

            foreach( var dataElement in data )
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