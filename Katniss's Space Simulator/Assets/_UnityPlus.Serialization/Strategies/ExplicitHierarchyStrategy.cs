using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization.ComponentData;

namespace UnityPlus.Serialization.Strategies
{
    /// <summary>
    /// Can be used to save the scene using the factory-gameobjectdata scheme.
    /// </summary>
    public sealed class PrefabAndDataStrategy
    {
        // Object actions are suffixed by _Object
        // Data actions are suffixed by _Data

#warning TODO - something to tell the strategy where to put the JSON file(s) and how to structure them.

        private static string jsonO;
        private static string jsonD;

        private static IEnumerable<GameObject> GetRootGameObjects()
        {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        }

        private static SerializedObject WriteAssetGameObject( Saver s, GameObject go )
        {
            ClonedGameObject cbf = go.GetComponent<ClonedGameObject>();
            if( cbf == null )
            {
                return null;
            }

            Guid objectGuid = s.GetID( go );

            SerializedObject goJson = new SerializedObject()
            {
                { Saver_Ex_References.ID, s.WriteGuid(objectGuid) },
                { "prefab", s.WriteAssetReference(cbf.OriginalAsset) }
            };

            return goJson;
        }

        private static GameObject ReadAssetGameObject( Loader l, SerializedData goJson )
        {
            Guid objectGuid = l.ReadGuid( goJson[Saver_Ex_References.ID] );

            GameObject prefab = l.ReadAssetReference<GameObject>( goJson["prefab"] );

            if( prefab == null )
            {
                Debug.LogWarning( $"Couldn't find a prefab `{goJson["prefab"]}`." );
            }

            GameObject go = ClonedGameObject.Instantiate( prefab );

            l.SetID( go, objectGuid );

            return go;
        }

        public void SaveSceneObjects_Object( Saver s )
        {
            // saves the information about what exists and what factory can be used to create that thing.

            // this should save to a file. to a specified dir.

            IEnumerable<GameObject> rootObjects = GetRootGameObjects();

            SerializedArray objectsJson = new SerializedArray();

            foreach( var go in rootObjects )
            {
#warning TODO - if root doesn't have factory component, look through children.
                // maybe some sort of customizable tag/layer masking

                SerializedObject goJson = WriteAssetGameObject( s, go );
                if( goJson == null )
                    continue;
                objectsJson.Add( goJson );
            }

            var sb = new StringBuilder();
            new Serialization.Json.JsonStringWriter( objectsJson, sb ).Write();
            jsonO = sb.ToString();

            TMPro.TMP_InputField inp = UnityEngine.Object.FindObjectOfType<TMPro.TMP_InputField>();
            inp.text = jsonO;
            Debug.Log( jsonO );
        }

        public void SaveSceneObjects_Data( Saver s )
        {
            // saves the persistent information about the existing objects.

            // persistent information is one that is expected to change and be preserved (i.e. health, inventory, etc).

            IEnumerable<GameObject> rootObjects = GetRootGameObjects();

            SerializedArray objectsJson = new SerializedArray();

#warning TODO - loop through children to save/load comps.
            foreach( var go in rootObjects )
            {
                ClonedGameObject cbf = go.GetComponent<ClonedGameObject>();
                if( cbf == null )
                {
                    continue;
                }
                Guid id = s.GetID( go );

                SerializedArray componentsJson = new SerializedArray();

                Component[] comps = go.GetComponents<Component>();
                int i = 0;
                foreach( var comp in comps )
                {
                    var dataJson = comp.GetData( s );

                    if( dataJson != null )
                    {
                        SerializedObject compJson = new SerializedObject()
                        {
#warning TODO - ugly magic string value of `predicate_type`.
                            { "predicate_type", "index" },
                            { "predicate", new SerializedObject()
                            {
                                { "index", i }
                            } },
                            { "data", dataJson }
                        };
                        componentsJson.Add( compJson );
                    }
                    i++;
                }

                if( componentsJson.Any() )
                {
                    objectsJson.Add( new SerializedObject()
                    {
                        { "$ref", id.ToString( "D" ) },
                        { "components", componentsJson }
                    } );
                }
            }

            var sb = new StringBuilder();
            new Serialization.Json.JsonStringWriter( objectsJson, sb ).Write();
            jsonD = sb.ToString();

            TMPro.TMP_InputField inp = UnityEngine.Object.FindObjectOfType<TMPro.TMP_InputField>();
            inp.text = jsonD;
            Debug.Log( jsonD );
        }

        public void LoadSceneObjects_Object( Loader l )
        {
            // Assumes that factories are already registered.

            // create dummy GOs with factories.

            SerializedArray objectsJson = (SerializedArray)new Serialization.Json.JsonStringReader( jsonO ).Read();

            foreach( var goJson in objectsJson )
            {
                ReadAssetGameObject( l, goJson );
            }
        }

        public void LoadSceneObjects_Data( Loader l )
        {
            // loop through object data, get the corresponding objects using ID from registry, and apply.

            SerializedArray objectsJson = (SerializedArray)new Serialization.Json.JsonStringReader( jsonD ).Read();

            foreach( var goJson in objectsJson )
            {
                object obj = l.Get( l.ReadGuid( goJson["$ref"] ) );

                GameObject go = (GameObject)obj;

                Component[] comps = go.GetComponents();

                foreach( var compjson in (SerializedArray)goJson["components"] )
                {
                    var func = GameObjectData.PredicateRegistry[(string)compjson["predicate_type"]];
#warning TODO - self-serialize for these.
                    Component comp = func( comps, (int)(compjson["predicate"]["index"]) );

                    comp.SetData( l, compjson["data"] );
                }
            }
        }
    }
}