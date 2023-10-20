using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace UnityPlus.Serialization
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
            SerializerUtils.WriteReferencedChildrenRecursive( s, go, ref sArr, "" );

            SerializedObject goJson = new SerializedObject()
            {
                { SerializerUtils.ID, s.WriteGuid( guidComp.GetPersistentGuid() ) },
                { "children_ids", sArr }
            };

            return goJson;
        }

        //public void Save_Object( ISaver s )
        public IEnumerator Save_Object( ISaver s )
        {
            if( string.IsNullOrEmpty( ObjectsFilename ) )
            {
                throw new InvalidOperationException( $"Can't save objects, file name is not set." );
            }

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

            var sb = new StringBuilder();
            new Serialization.Json.JsonStringWriter( objectsJson, sb ).Write();
            File.WriteAllText( ObjectsFilename, sb.ToString(), Encoding.UTF8 );
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

                SerializerUtils.TryWriteData( s, comp, compData, ref objects );
            }

            SerializedData goData = go.GetData( s );
            objects.Add( new SerializedObject()
            {
                { $"{SerializerUtils.REF}", s.WriteGuid( guidComp.GetPersistentGuid() ) },
                { "data", goData }
            } );
        }

        /// <summary>
        /// Saves the data about the gameobjects and their persistent components. Does not include child objects.
        /// </summary>
        public IEnumerator Save_Data( ISaver s )
        {
            if( string.IsNullOrEmpty( DataFilename ) )
            {
                throw new InvalidOperationException( $"Can't save objects, file name is not set." );
            }

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

            var sb = new StringBuilder();
            new Serialization.Json.JsonStringWriter( objData, sb ).Write();
            File.WriteAllText( DataFilename, sb.ToString(), Encoding.UTF8 );
        }

        private static void AssignIDsToReferencedChildren( ILoader l, GameObject go, ref SerializedArray sArr )
        {
            // Set the IDs of all objects in the array.
            foreach( var s in sArr )
            {
                Guid id = l.ReadGuid( s["$id"] );
                string path = s["path"];

                var refObj = go.GetComponentOrGameObject( path );

                l.SetReferenceID( refObj, id );
            }
        }

        public IEnumerator Load_Object( ILoader l )
        {
            if( string.IsNullOrEmpty( ObjectsFilename ) )
            {
                throw new InvalidOperationException( $"Can't load objects, file name is not set." );
            }
            if( !File.Exists( ObjectsFilename ) )
            {
                throw new InvalidOperationException( $"Can't load objects, file `{ObjectsFilename}` doesn't exist." );
            }

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

            string objectsStr = File.ReadAllText( ObjectsFilename, Encoding.UTF8 );
            SerializedArray objectsJson = (SerializedArray)new Serialization.Json.JsonStringReader( objectsStr ).Read();

            foreach( var goJson in objectsJson )
            {
                Guid objectGuid = l.ReadGuid( goJson[SerializerUtils.ID] );
                SerializedArray refChildren = (SerializedArray)goJson["children_ids"];
                AssignIDsToReferencedChildren( l, (GameObject)l.Get( objectGuid ), ref refChildren );

                yield return null;
            }
        }

        public IEnumerator Load_Data( ILoader l )
        {
            if( string.IsNullOrEmpty( DataFilename ) )
            {
                throw new InvalidOperationException( $"Can't load objects' data, file name is not set." );
            }

            string dataStr = File.ReadAllText( DataFilename, Encoding.UTF8 );
            SerializedArray data = (SerializedArray)new Serialization.Json.JsonStringReader( dataStr ).Read();

            foreach( var dataElement in data )
            {
                Guid id = l.ReadGuid( dataElement["$ref"] );
                object obj = l.Get( id );
                switch( obj )
                {
                    case GameObject go:
                        go.SetData( l, dataElement["data"] );
                        break;

                    case Component comp:
                        try
                        {
                            comp.SetData( l, dataElement["data"] );
                        }
                        catch( Exception ex )
                        {
                            Debug.LogError( $"[{nameof( JsonPreexistingGameObjectsStrategy )}] Failed to deserialize data of component with ID: `{dataElement?["$ref"] ?? "<null>"}`." );
                            Debug.LogException( ex );
                        }
                        break;
                }

                yield return null;
            }
        }
    }
}