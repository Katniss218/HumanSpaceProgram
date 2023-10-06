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
    /// Serializes already existing scene objects (gameplay managers).
    /// </summary>
    public sealed class JsonPreexistingGameObjectsStrategy
    {
        public string ObjectsFilename { get; set; }
        public string DataFilename { get; set; }

        public Func<GameObject[]> RootObjectsGetter { get; }
        public int IncludedObjectsMask { get; set; } = int.MaxValue;

        public JsonPreexistingGameObjectsStrategy( Func<GameObject[]> rootObjectsGetter )
        {
            if( rootObjectsGetter == null )
            {
                throw new ArgumentNullException( nameof( rootObjectsGetter ), $"Object getter func must not be null." );
            }
            this.RootObjectsGetter = rootObjectsGetter;
        }

        private void SaveGameObjectData( ISaver s, GameObject go, ref SerializedArray objects )
        {
            if( !go.IsInLayerMask( IncludedObjectsMask ) )
            {
                return;
            }

            SerializerUtils.WriteGameObjectComponentsData( s, go, ref objects );

            SerializerUtils.WriteGameObjectData( s, go, ref objects );
        }

        public IEnumerator Save_Data( ISaver s )
        {
            if( string.IsNullOrEmpty( DataFilename ) )
            {
                throw new InvalidOperationException( $"Can't save scene objects, file name is not set." );
            }
            // saves the persistent information about the existing objects.

            // persistent information is one that is expected to change and be preserved (i.e. health, inventory, etc).

            GameObject[] rootObjects = RootObjectsGetter();

            SerializedArray objData = new SerializedArray();

            foreach( var go in rootObjects )
            {
                SaveGameObjectData( s, go, ref objData );

                yield return null;
            }

            var sb = new StringBuilder();
            new Serialization.Json.JsonStringWriter( objData, sb ).Write();
            File.WriteAllText( DataFilename, sb.ToString(), Encoding.UTF8 );
        }

        public IEnumerator Load_Object( ILoader l )
        {
            GameObject[] objects = RootObjectsGetter.Invoke();
            foreach( var obj in objects )
            {
                // the only difference between this and the prefab strat is that we don't spawn the object on load, it is assumed to already exist in the scene when the loader is run.
                // match the component map to a guid returned from the guid getter for a specific root object.

                PreexistingReference guidComp = obj.GetComponent<PreexistingReference>();
                if( guidComp == null )
                {
                    continue;
                }

                l.SetReferenceID( obj, guidComp.GetGuid() );

                yield return null;
            }
        }
    }
}