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
        public Func<GameObject[]> RootObjectsGetter { get; }
        /// <summary>
        /// Determines which objects (including child objects) returned by the <see cref="RootObjectsGetter"/> will be excluded from saving.
        /// </summary>
        public uint IncludedObjectsMask { get; set; } = uint.MaxValue;

        public JsonExplicitHierarchyGameObjectsStrategy( Func<GameObject[]> rootObjectsGetter )
        {
            if( rootObjectsGetter == null )
            {
                throw new ArgumentNullException( nameof( rootObjectsGetter ), $"Object getter func must not be null." );
            }
            this.RootObjectsGetter = rootObjectsGetter;
        }

        private static void CreateGameObjectHierarchy( ILoader l, SerializedData goJson, GameObject parent )
        {
            Guid objectGuid = l.ReadGuid( goJson[SerializerUtils.ID] );

            GameObject go = new GameObject();
            l.SetReferenceID( go, objectGuid );

            if( parent != null )
            {
                go.transform.SetParent( parent.transform );
            }

            SerializedArray components = (SerializedArray)goJson["components"];
            foreach( var compData in components )
            {
                try
                {
                    Guid compID = l.ReadGuid( compData["$id"] );
                    Type compType = l.ReadType( compData["$type"] );

                    Component co = go.GetTransformOrAddComponent( compType );

                    l.SetReferenceID( co, compID );
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"[{nameof( JsonExplicitHierarchyGameObjectsStrategy )}] Failed to deserialize a component with ID: `{compData?["$id"] ?? "<null>"}`." );
                    Debug.LogException( ex );
                }
            }

            SerializedArray children = (SerializedArray)goJson["children"];
            foreach( var childData in children )
            {
                try
                {
                    CreateGameObjectHierarchy( l, childData, go );
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"[{nameof( JsonExplicitHierarchyGameObjectsStrategy )}] Failed to deserialize a child GameObject with ID: `{childData?["$id"] ?? "<null>"}`." );
                    Debug.LogException( ex );
                }
            }
        }

        private void SaveGameObjectHierarchy( GameObject go, ISaver s, ref SerializedArray arr )
        {
            if( !go.IsInLayerMask( IncludedObjectsMask ) )
            {
                return;
            }

            Guid objectGuid = s.GetReferenceID( go );

            // recursive.
            SerializedObject obj = new SerializedObject()
            {
                { $"{SerializerUtils.ID}", s.WriteGuid(objectGuid) }
            };

            SerializedArray children = new SerializedArray();

            foreach( Transform child in go.transform )
            {
                SaveGameObjectHierarchy( child.gameObject, s, ref children );
            }

            SerializedArray components = new SerializedArray();

            foreach( var comp in go.GetComponents<Component>() )
            {
                Guid id = s.GetReferenceID( comp );
                SerializedObject compObj = new SerializedObject()
                {
                    { $"{SerializerUtils.ID}", s.WriteGuid(id) },
                    { "$type", s.WriteType(comp.GetType()) }
                };

                components.Add( compObj );
            }

            obj.Add( "children", children );
            obj.Add( "components", components );

            arr.Add( obj );
        }

        public IEnumerator Save_Object( ISaver s )
        {
            if( string.IsNullOrEmpty( ObjectsFilename ) )
            {
                throw new InvalidOperationException( $"Can't save objects, file name is not set." );
            }

            IEnumerable<GameObject> rootObjects = RootObjectsGetter();

            SerializedArray objData = new SerializedArray();

            foreach( var go in rootObjects )
            {
                SaveGameObjectHierarchy( go, s, ref objData );

                yield return null;
            }

            var sb = new StringBuilder();
            new Serialization.Json.JsonStringWriter( objData, sb ).Write();
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
                    Debug.LogWarning( $"[{nameof( JsonExplicitHierarchyGameObjectsStrategy )}] Couldn't serialize component '{comp}': {ex.Message}." );
                    Debug.LogException( ex );
                }

                SerializerUtils.TryWriteData( s, comp, compData, ref objects );
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
                SaveGameObjectDataRecursive( s, go, ref objData );

                yield return null;
            }

            var sb = new StringBuilder();
            new Serialization.Json.JsonStringWriter( objData, sb ).Write();
            File.WriteAllText( DataFilename, sb.ToString(), Encoding.UTF8 );
        }

        //public void Load_Object( ILoader s )
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

#warning TODO - this should be loaded asynchronously from a file or multiple files - jsonstreamreader.
            string objectsStr = File.ReadAllText( ObjectsFilename, Encoding.UTF8 );
            SerializedArray objectsJson = (SerializedArray)new Serialization.Json.JsonStringReader( objectsStr ).Read();

            foreach( var goJson in objectsJson )
            {
                try
                {
                    CreateGameObjectHierarchy( l, goJson, null );
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"[{nameof( JsonExplicitHierarchyGameObjectsStrategy )}] Failed to deserialize a root GameObject with ID: `{goJson?["$id"] ?? "<null>"}`." );
                    Debug.LogException( ex );
                }

                yield return null;
            }
        }

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
                            Debug.LogError( $"[{nameof( JsonExplicitHierarchyGameObjectsStrategy )}] Failed to deserialize data of component with ID: `{dataElement?["$ref"] ?? "<null>"}`." );
                            Debug.LogException( ex );
                        }
                        break;
                }

                yield return null;
            }
        }
    }
}