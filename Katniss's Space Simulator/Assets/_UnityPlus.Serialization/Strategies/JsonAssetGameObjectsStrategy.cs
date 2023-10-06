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
    public sealed class JsonAssetGameObjectsStrategy
    {
        // Object actions are suffixed by _Object
        // Data actions are suffixed by _Data

#warning TODO - something to tell the strategy where to put the JSON file(s) and how to structure them.

        public string ObjectsFilename { get; set; }
        public string DataFilename { get; set; }

        public Func<GameObject[]> RootObjectsGetter { get; }
        public int IncludedObjectsMask { get; set; } = int.MaxValue;

        public JsonAssetGameObjectsStrategy( Func<GameObject[]> rootObjectsGetter )
        {
            if( rootObjectsGetter == null )
            {
                throw new ArgumentNullException( nameof( rootObjectsGetter ), $"Object getter func must not be null." );
            }
            this.RootObjectsGetter = rootObjectsGetter;
        }

        private static void WriteReferencedChildrenRecursive( ISaver s, GameObject go, ref SerializedArray sArr, string parentPath )
        {
            // write the IDs of referenced components/child gameobjects of the parent into the array, along with the path to them.

            // root is always added, recursive children might not be.
            if( !string.IsNullOrEmpty( parentPath ) )
            {
                if( s.TryGetID( go, out Guid id ) )
                {
                    sArr.Add( new SerializedObject()
                {
                    { "$id", s.WriteGuid( id ) },
                    { "path", $"{parentPath}" }
                } );
                }
            }

            int i = 0;
            foreach( var comp in go.GetComponents() )
            {
                if( s.TryGetID( comp, out Guid id ) )
                {
                    sArr.Add( new SerializedObject()
                    {
                        { "$id", s.WriteGuid( id ) },
                        { "path", $"{parentPath}*{i.ToString(CultureInfo.InvariantCulture)}" }
                    } );
                }
                i++;
            }

            i = 0;
            foreach( Transform ct in go.transform )
            {
                string path = $"{i.ToString( CultureInfo.InvariantCulture )}:"; // colon at the end is important
                WriteReferencedChildrenRecursive( s, ct.gameObject, ref sArr, path );
                i++;
            }
        }

        private static SerializedObject WriteAssetGameObject( ISaver s, GameObject go, ClonedGameObject cbf )
        {
            Guid objectGuid = s.GetReferenceID( go );

            SerializedArray sArr = new SerializedArray();
            WriteReferencedChildrenRecursive( s, go, ref sArr, "" );

            SerializedObject goJson = new SerializedObject()
            {
                { ISaver_Ex_References.ID, s.WriteGuid(objectGuid) },
                { "prefab", s.WriteAssetReference(cbf.OriginalAsset) },
                { "children_ids", sArr }
            };

            return goJson;
        }

        private static UnityEngine.Object GetComponentOrGameObject( GameObject root, string path )
        {
            if( path == "" )
                return root;

            string[] pathSegments = path.Split( ':' );

            Transform obj = root.transform;
            for( int i = 0; i < pathSegments.Length - 1; i++ )
            {
                int index = int.Parse( pathSegments[i] );
                obj = obj.transform.GetChild( index );
            }

            // component is always last.
            string lastSegment = pathSegments[pathSegments.Length - 1];
            if( lastSegment == "" )
            {
                return obj.gameObject;
            }
            if( lastSegment[0] == '*' )
            {
                int index = int.Parse( lastSegment[1..] );
                return obj.GetComponents()[index];
            }
            else
            {
                int index = int.Parse( lastSegment );
                obj = obj.transform.GetChild( index );
                return obj;
            }
        }

        private static void AssignIDsToReferencedChildren( ILoader l, GameObject go, ref SerializedArray sArr )
        {
            // Set the IDs of all objects in the array.
            foreach( var s in sArr )
            {
                Guid id = l.ReadGuid( s["$id"] );
                string path = s["path"];

                var obj = GetComponentOrGameObject( go, path );

                l.SetReferenceID( obj, id );
            }
        }

        private static GameObject ReadAssetGameObject( ILoader l, SerializedData goJson )
        {
            Guid objectGuid = l.ReadGuid( goJson[ISaver_Ex_References.ID] );

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

        //public void SaveSceneObjects_Object( ISaver s )
        public IEnumerator SaveSceneObjects_Object( ISaver s )
        {
            // saves the information about what exists and what factory can be used to create that thing.

            // this should save to a file. to a directory specified by this strategy.

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

        private static void SaveObjectDataRecursive( ISaver s, GameObject go, ref SerializedArray objects )
        {
            Guid id = s.GetReferenceID( go );

            SerializedArray components = new SerializedArray();

            Component[] comps = go.GetComponents();
            int i = 0;
            foreach( var comp in comps )
            {
                var dataJson = comp.GetData( s );

                if( dataJson != null )
                {
                    Guid cid = s.GetReferenceID( comp );
                    SerializedObject compData = new SerializedObject()
                    {
                        { "$ref", s.WriteGuid(cid) },
                        { "data", dataJson }
                    };
                    components.Add( compData );
                }
                i++;
            }

            if( components.Any() )
            {
                objects.Add( new SerializedObject()
                {
                    { "$ref", id.ToString( "D" ) },
                    { "components", components }
                } );
            }

            foreach( Transform ct in go.transform )
            {
                SaveObjectDataRecursive( s, ct.gameObject, ref objects );
            }
        }

        //public void SaveSceneObjects_Data( ISaver s )
        public IEnumerator SaveSceneObjects_Data( ISaver s )
        {
            // saves the persistent information about the existing objects.

            // persistent information is one that is expected to change and be preserved (i.e. health, inventory, etc).

            IEnumerable<GameObject> rootObjects = RootObjectsGetter();

            SerializedArray objData = new SerializedArray();

            foreach( var go in rootObjects )
            {
                ClonedGameObject cbf = go.GetComponent<ClonedGameObject>();
                if( cbf == null )
                {
                    continue;
                }
                yield return null;

                SaveObjectDataRecursive( s, go, ref objData );
            }

            var sb = new StringBuilder();
            new Serialization.Json.JsonStringWriter( objData, sb ).Write();
            File.WriteAllText( DataFilename, sb.ToString(), Encoding.UTF8 );
        }

        //public void LoadSceneObjects_Object( ILoader l )
        public IEnumerator LoadSceneObjects_Object( ILoader l )
        {
            if( string.IsNullOrEmpty( ObjectsFilename ) )
            {
                throw new InvalidOperationException( $"Can't load scene objects, file name is not set." );
            }
            string objectsStr = File.ReadAllText( ObjectsFilename, Encoding.UTF8 );
            SerializedArray objectsJson = (SerializedArray)new Serialization.Json.JsonStringReader( objectsStr ).Read();

            foreach( var goJson in objectsJson )
            {
                ReadAssetGameObject( l, goJson );

                yield return null;
            }
        }

        //public void LoadSceneObjects_Data( ILoader l )
        public IEnumerator LoadSceneObjects_Data( ILoader l )
        {
            if( string.IsNullOrEmpty( DataFilename ) )
            {
                throw new InvalidOperationException( $"Can't load scene objects, file name is not set." );
            }
            string objectsStr = File.ReadAllText( DataFilename, Encoding.UTF8 );
            SerializedArray objectsJson = (SerializedArray)new Serialization.Json.JsonStringReader( objectsStr ).Read();

            foreach( var goJson in objectsJson )
            {
                object obj = l.Get( l.ReadGuid( goJson["$ref"] ) );

                GameObject go = (GameObject)obj;

                Component[] comps = go.GetComponents();

                foreach( var compjson in (SerializedArray)goJson["components"] ) // the components don't have to be under the gameobject. They will be found anyway.
                {
                    Guid id = l.ReadGuid( compjson["$ref"] );

                    Component comp = (Component)l.Get( id );

                    comp.SetData( l, compjson["data"] );
                }

                yield return null;
            }
        }
    }
}