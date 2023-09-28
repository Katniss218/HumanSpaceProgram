using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityPlus.AssetManagement;

namespace UnityPlus.Serialization.Strategies
{
    /// <summary>
    /// Can be used to save the scene using the factory-gameobjectdata scheme.
    /// </summary>
    [Obsolete( "Incomplete" )]
    public sealed class JsonExplicitHierarchyStrategy
    {
        // Object actions are suffixed by _Object
        // Data actions are suffixed by _Data

        public string ObjectsFilename { get; set; }
        public string DataFilename { get; set; }

        public int IncludedObjectsMask { get; set; } = int.MaxValue;

        // TODO - We might want to specify which components to *not* serialize, because they might be managed entirely by a supervisor.
        //      - This shouldn't be a problem if the components are deterministic though.

        // doesn't matter if its json actually.

        private static IEnumerable<GameObject> GetRootGameObjects()
        {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        }

        private static Component GetTransformOrAddComponent(GameObject go, Type componentType )
        {
            if( componentType == typeof( Transform ) )
            {
                return go.transform;
            }
            else
            {
                return go.AddComponent( componentType );
            }
        }

        private static void CreateGameObjectWithComponents( ILoader l, SerializedData goJson, GameObject parent )
        {
            Guid objectGuid = l.ReadGuid( goJson[ISaver_Ex_References.ID] );

            GameObject go = new GameObject();
            l.SetID( go, objectGuid );

            if( parent != null )
            {
                go.transform.SetParent( parent.transform );
            }

            SerializedArray components = (SerializedArray)goJson["components"];
            foreach( var c in components )
            {
                try
                {
                    Type compType = l.ReadType( c["$type"] );
                    Guid compID = l.ReadGuid( c["$id"] );

                    Component co = GetTransformOrAddComponent( go, compType );

                    l.SetID( co, compID );
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"[{nameof( JsonExplicitHierarchyStrategy )}] Failed to deserialize a component of GameObject with ID: `{objectGuid}`." );
                    Debug.LogException( ex );
                }
            }

            SerializedArray children = (SerializedArray)goJson["children"];
            foreach( var c in children )
            {
                try
                {
                    CreateGameObjectWithComponents( l, c, go );
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"[{nameof( JsonExplicitHierarchyStrategy )}] Failed to deserialize a child GameObject of GameObject with ID: `{objectGuid}`." );
                    Debug.LogException( ex );
                }
            }
        }

        private void WriteGameObjectHierarchy( GameObject go, ISaver s, ref SerializedArray arr )
        {
            if( !go.IsInLayerMask( IncludedObjectsMask ) )
            {
                return;
            }

            Guid objectGuid = s.GetID( go );

            // recursive.
            SerializedObject obj = new SerializedObject()
            {
                { "$id", s.WriteGuid(objectGuid) }
            };

            SerializedArray children = new SerializedArray();

            foreach( Transform child in go.transform )
            {
                WriteGameObjectHierarchy( child.gameObject, s, ref children );
            }

            SerializedArray components = new SerializedArray();

            foreach( var comp in go.GetComponents<Component>() )
            {
                Guid id = s.GetID( comp );
                SerializedObject compObj = new SerializedObject()
                {
                    { "$id", s.WriteGuid(id) },
                    { "$type", s.WriteType(comp.GetType()) }
                };

                components.Add( compObj );
            }

            obj.Add( "children", children );
            obj.Add( "components", components );

            arr.Add( obj );
        }

        public IEnumerator SaveSceneObjects_Object( ISaver s )
        {
            IEnumerable<GameObject> rootObjects = GetRootGameObjects();

            SerializedArray objectsJson = new SerializedArray();

            foreach( var go in rootObjects )
            {
                // maybe some sort of customizable tag/layer masking

                WriteGameObjectHierarchy( go, s, ref objectsJson );

                yield return null;
            }

            var sb = new StringBuilder();
            new Serialization.Json.JsonStringWriter( objectsJson, sb ).Write();
            File.WriteAllText( ObjectsFilename, sb.ToString(), Encoding.UTF8 );
        }
        private void SaveObjectDataRecursive( ISaver s, GameObject go, ref SerializedArray objects )
        {
            if( !go.IsInLayerMask( IncludedObjectsMask ) )
            {
                return;
            }

            Guid id = s.GetID( go );

            SerializedArray components = new SerializedArray();

            // components' properties.
            Component[] comps = go.GetComponents();
            int i = 0;
            foreach( var comp in comps )
            {
                var dataJson = comp.GetData( s );

                if( dataJson != null )
                {
                    Guid cid = s.GetID( comp );
                    SerializedObject compData = new SerializedObject()
                    {
                        { "$ref", s.WriteGuid(cid) },
                        { "data", dataJson }
                    };
                    components.Add( compData );
                }
                i++;
            }

            objects.Add( new SerializedObject()
            {
                { "$ref", id.ToString( "D" ) },
                { "name", go.name },
                { "layer", go.layer },
                { "is_active", go.activeSelf },
                { "is_static", go.isStatic },
                { "tag", go.tag },
                { "components", components }
            } );

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

            IEnumerable<GameObject> rootObjects = GetRootGameObjects();

            SerializedArray objData = new SerializedArray();

            foreach( var go in rootObjects )
            {
                SaveObjectDataRecursive( s, go, ref objData );

                yield return null;
            }

            var sb = new StringBuilder();
            new Serialization.Json.JsonStringWriter( objData, sb ).Write();
            File.WriteAllText( DataFilename, sb.ToString(), Encoding.UTF8 );
        }

        public IEnumerator LoadSceneObjects_Object( ILoader l )
        {
#warning TODO - this should be loaded asynchronously from a file or multiple files - jsonstreamreader.
            string objectsStr = File.ReadAllText( ObjectsFilename, Encoding.UTF8 );
            SerializedArray objectsJson = (SerializedArray)new Serialization.Json.JsonStringReader( objectsStr ).Read();

            foreach( var goJson in objectsJson )
            {
                try
                {
                    CreateGameObjectWithComponents( l, goJson, null );
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"[{nameof( JsonExplicitHierarchyStrategy )}] Failed to deserialize a root GameObject, ID: `{goJson?["$id"]}`." );
                    Debug.LogException( ex );
                }

                yield return null;
            }
        }

        public IEnumerator LoadSceneObjects_Data( ILoader l )
        {
            string dataStr = File.ReadAllText( ObjectsFilename, Encoding.UTF8 );
            SerializedArray data = (SerializedArray)new Serialization.Json.JsonStringReader( dataStr ).Read();

            foreach( var goData in data )
            {
                try
                {
                    Guid goId = l.ReadGuid( goData["$ref"] );

                    GameObject go = (GameObject)l.Get( goId );

                    go.name = (string)goData["name"];
                    go.layer = (int)goData["layer"];
                    go.SetActive( (bool)goData["is_active"] );
                    go.isStatic = (bool)goData["is_static"];
                    go.tag = (string)goData["tag"];
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"[{nameof( JsonExplicitHierarchyStrategy )}] Failed to deserialize data of gameobject with ID: `{goData?["$ref"]}`." );
                    Debug.LogException( ex );
                }

                foreach( var componentData in (SerializedArray)goData["components"] ) // the components don't have to be under the gameobject. They will be found anyway.
                {
                    try
                    {
                        Guid compId = l.ReadGuid( componentData["$ref"] );

                        Component comp = (Component)l.Get( compId );

                        comp.SetData( l, componentData["data"] );
                    }
                    catch( Exception ex )
                    {
                        Debug.LogError( $"[{nameof( JsonExplicitHierarchyStrategy )}] Failed to deserialize data of component with ID: `{componentData?["$ref"]}`." );
                        Debug.LogException( ex );
                    }
                }

                yield return null;
            }
        }
    }
}