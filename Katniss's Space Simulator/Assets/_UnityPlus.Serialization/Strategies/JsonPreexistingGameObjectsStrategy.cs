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
        public int IncludedObjectsMask { get; set; } = int.MaxValue;

        /// <param name="rootObjectsGetter">Determines which objects will have their data saved, and loaded.</param>
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

                SerializerUtils.TryWriteData( s, go, compData, ref objects );
            }

            SerializedData goData = go.GetData( s );
            SerializerUtils.TryWriteData( s, go, goData, ref objects );
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
                SaveGameObjectData( s, go, ref objData );

                yield return null;
            }

            var sb = new StringBuilder();
            new Serialization.Json.JsonStringWriter( objData, sb ).Write();
            File.WriteAllText( DataFilename, sb.ToString(), Encoding.UTF8 );
        }

        public IEnumerator Load_Object( ILoader l )
        {
            // Objects are assumed to be already existing in the scene (same as on save).
            // Loop through the objects to be loaded, and 
            GameObject[] objects = RootObjectsGetter.Invoke();
            foreach( var obj in objects )
            {
                PreexistingReference guidComp = obj.GetComponent<PreexistingReference>();
                if( guidComp == null )
                {
                    continue;
                }

                l.SetReferenceID( obj, guidComp.GetPersistentGuid() );

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