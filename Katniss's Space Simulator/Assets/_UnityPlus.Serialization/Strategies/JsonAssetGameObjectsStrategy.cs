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

namespace UnityPlus.Serialization.Strategies
{
    /// <summary>
    /// Can be used to save the scene using the factory-gameobjectdata scheme.
    /// </summary>
    /// <remarks>
    /// - Object actions are suffixed by _Object <br />
    /// - Data actions are suffixed by _Data
    /// </remarks>
    public sealed class JsonAssetGameObjectsStrategy
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
        public Func<GameObject[]> RootObjectsGetter { get; }
        /// <summary>
        /// Determines which objects returned by the <see cref="RootObjectsGetter"/> will be excluded from saving.
        /// </summary>
        public uint IncludedObjectsMask { get; set; } = uint.MaxValue;

        public JsonAssetGameObjectsStrategy( Func<GameObject[]> rootObjectsGetter )
        {
            if( rootObjectsGetter == null )
            {
                throw new ArgumentNullException( nameof( rootObjectsGetter ), $"Object getter func must not be null." );
            }
            this.RootObjectsGetter = rootObjectsGetter;
        }

        private static SerializedObject WriteAssetGameObject( ISaver s, GameObject go, ClonedGameObject cbf )
        {
            Guid objectGuid = s.GetReferenceID( go );

            SerializedArray sArr = new SerializedArray();
            SerializerUtils.WriteReferencedChildrenRecursive( s, go, ref sArr, "" );

            SerializedObject goJson = new SerializedObject()
            {
                { SerializerUtils.ID, s.WriteGuid(objectGuid) },
                { "prefab", s.WriteAssetReference(cbf.OriginalAsset) },
                { "children_ids", sArr }
            };

            return goJson;
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

        private static GameObject ReadAssetGameObject( ILoader l, SerializedData goJson )
        {
            Guid objectGuid = l.ReadGuid( goJson[SerializerUtils.ID] );

            GameObject prefab = l.ReadAssetReference<GameObject>( goJson["prefab"] );

            if( prefab == null )
            {
                Debug.LogWarning( $"Couldn't find a prefab `{goJson["prefab"]}`." );
            }

            GameObject go = ClonedGameObject.Instantiate( prefab );

            l.SetReferenceID( go, objectGuid );

            SerializedArray refChildren = (SerializedArray)goJson["children_ids"];
            AssignIDsToReferencedChildren( l, go, ref refChildren );

            return go;
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
                // maybe some sort of customizable tag/layer masking

                ClonedGameObject cloneComp = go.GetComponent<ClonedGameObject>();
                if( cloneComp == null )
                {
#warning TODO - if root doesn't have factory component, look through children.
                    continue;
                }

                SerializedObject goJson = WriteAssetGameObject( s, go, cloneComp );
                objectsJson.Add( goJson );

                yield return null;
            }

            var sb = new StringBuilder();
            new Serialization.Json.JsonStringWriter( objectsJson, sb ).Write();
            File.WriteAllText( ObjectsFilename, sb.ToString(), Encoding.UTF8 );
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
                    Debug.LogWarning( $"[{nameof( JsonAssetGameObjectsStrategy )}] Couldn't serialize component '{comp}': {ex.Message}." );
                    Debug.LogException( ex );
                }

                SerializerUtils.TryWriteData( s, go, compData, ref objects );
            }

            SerializedData goData = go.GetData( s );
            SerializerUtils.TryWriteData( s, go, goData, ref objects );

            foreach( Transform ct in go.transform )
            {
                SaveGameObjectDataRecursive( s, ct.gameObject, ref objects );
            }
        }

        //public void Save_Data( ISaver s )
        public IEnumerator Save_Data( ISaver s )
        {
            if( string.IsNullOrEmpty( DataFilename ) )
            {
                throw new InvalidOperationException( $"Can't save objects' data, file name is not set." );
            }

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

            var sb = new StringBuilder();
            new Serialization.Json.JsonStringWriter( objData, sb ).Write();
            File.WriteAllText( DataFilename, sb.ToString(), Encoding.UTF8 );
        }

        //public void Load_Object( ILoader l )
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

            string objectsStr = File.ReadAllText( ObjectsFilename, Encoding.UTF8 );
            SerializedArray objectsJson = (SerializedArray)new Serialization.Json.JsonStringReader( objectsStr ).Read();

            foreach( var goJson in objectsJson )
            {
                ReadAssetGameObject( l, goJson );

                yield return null;
            }
        }

        //public void Load_Data( ILoader l )
        public IEnumerator Load_Data( ILoader l )
        {
            if( string.IsNullOrEmpty( DataFilename ) )
            {
                throw new InvalidOperationException( $"Can't load objects' data, file name is not set." );
            }
            if( !File.Exists( DataFilename ) )
            {
                throw new InvalidOperationException( $"Can't load objects' data, file `{DataFilename}` doesn't exist." );
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