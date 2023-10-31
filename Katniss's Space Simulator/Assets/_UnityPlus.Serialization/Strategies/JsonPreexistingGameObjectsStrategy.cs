using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace UnityPlus.Serialization.Strategies
{
    /// <summary>
    /// Serializes only the data of already existing scene objects.
    /// </summary>
    /// <remarks>
    /// - Object actions are suffixed by _Object <br />
    /// - Data actions are suffixed by _Data
    /// </remarks>
    public sealed class JsonPreexistingGameObjectsStrategy
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
        /// Determines which objects will have their data saved, and loaded.
        /// </summary>
        public Func<GameObject[]> RootObjectsGetter { get; }
        /// <summary>
        /// Determines which objects returned by the <see cref="RootObjectsGetter"/> will be excluded from saving.
        /// </summary>
        public uint IncludedObjectsMask { get; set; } = uint.MaxValue;

        /// <param name="rootObjectsGetter">Determines which objects will have their data saved, and loaded.</param>
        public JsonPreexistingGameObjectsStrategy( Func<GameObject[]> rootObjectsGetter )
        {
            if( rootObjectsGetter == null )
            {
                throw new ArgumentNullException( nameof( rootObjectsGetter ), $"Object getter func must not be null." );
            }
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
                    Debug.LogWarning( $"[{nameof( JsonPreexistingGameObjectsStrategy )}] Couldn't serialize component '{comp}': {ex.Message}." );
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

        public IEnumerator SaveAsync_Object( ISaver s )
        {
            StratCommon.ValidateFileOnSave( ObjectsFilename, StratCommon.OBJECTS_NOUN );

            IEnumerable<GameObject> rootObjects = RootObjectsGetter();

            SerializedArray objectsJson = new SerializedArray();

            foreach( var go in rootObjects )
            {
                PreexistingReference guidComp = go.GetComponent<PreexistingReference>();
                if( guidComp == null )
                {
                    continue;
                }

                SerializedObject goJson = WriteGameObject( s, go, guidComp );
                objectsJson.Add( goJson );

                yield return null;
            }

            StratCommon.WriteToFile( ObjectsFilename, objectsJson );
        }

        /// <summary>
        /// Saves the data about the gameobjects and their persistent components. Does not include child objects.
        /// </summary>
        public IEnumerator SaveAsync_Data( ISaver s )
        {
            StratCommon.ValidateFileOnSave( DataFilename, StratCommon.OBJECTS_DATA_NOUN );

            GameObject[] rootObjects = RootObjectsGetter();

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

            StratCommon.WriteToFile( DataFilename, objData );
        }

        public IEnumerator LoadAsync_Object( ILoader l )
        {
            StratCommon.ValidateFileOnLoad( ObjectsFilename, StratCommon.OBJECTS_NOUN );

            GameObject[] rootObjects = RootObjectsGetter.Invoke();
            foreach( var go in rootObjects )
            {
                PreexistingReference guidComp = go.GetComponent<PreexistingReference>();
                if( guidComp == null )
                {
                    continue;
                }

                l.SetReferenceID( go, guidComp.GetPersistentGuid() );

                yield return null;
            }

            // Loads the IDs of objects returned by the getter func, then gets them by ID from the loader's reference dict.

            SerializedArray objectsJson = (SerializedArray)StratCommon.ReadFromFile( ObjectsFilename );

            foreach( var goJson in objectsJson )
            {
                Guid objectGuid = l.ReadGuid( goJson[KeyNames.ID] );
                SerializedArray refChildren = (SerializedArray)goJson["children_ids"];
                StratUtils.AssignIDsToReferencedChildren( l, (GameObject)l.Get( objectGuid ), ref refChildren );

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
        }
    }
}