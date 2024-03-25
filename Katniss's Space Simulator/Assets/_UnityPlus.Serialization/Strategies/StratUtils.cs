using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityPlus.Serialization.Strategies
{
    /// <summary>
    /// Another class with common strategy utilities.
    /// </summary>
    public static class StratUtils
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void TryWriteData( IReverseReferenceMap s, object obj, SerializedData data, ref SerializedArray objects )
        {
            if( data != null )
            {
                objects.Add( new SerializedObject()
                {
                    { KeyNames.REF, s.WriteGuid( s.GetID( obj ) ) },
                    { "data", data }
                } );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void TryWriteDataWithChildrenPaths( IReverseReferenceMap s, object obj, SerializedData data, SerializedArray childrenPaths, ref SerializedArray objects )
        {
            if( data != null )
            {
                objects.Add( new SerializedObject()
                {
                    { KeyNames.REF, s.WriteGuid( s.GetID( obj ) ) },
                    { "data", data },
                    { "children_ids", childrenPaths }
                } );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void AssignIDsToReferencedChildren( IForwardReferenceMap l, GameObject go, ref SerializedArray sArr )
        {
            // Set the IDs of all objects in the array.
            foreach( var s in sArr )
            {
                Guid id = l.ReadGuid( s[KeyNames.ID] );
                string path = s["path"];

                var refObj = go.GetComponentOrGameObject( path );

                l.SetObj( id, refObj );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void WriteReferencedChildrenRecursive( IReverseReferenceMap s, GameObject go, ref SerializedArray sArr, string parentPath )
        {
            // write the IDs of referenced components/child gameobjects of the parent into the array, along with the path to them.

            // root is always added, recursive children might not be.
            if( !string.IsNullOrEmpty( parentPath ) )
            {
                if( s.TryGetID( go, out Guid id ) )
                {
                    sArr.Add( new SerializedObject()
                    {
                        { KeyNames.ID, s.WriteGuid( id ) },
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
                        { KeyNames.ID, s.WriteGuid( id ) },
                        { "path", $"{parentPath}*{i:#########0}" }
                    } );
                }
                i++;
            }

            i = 0;
            foreach( Transform ct in go.transform )
            {
                string path = $"{i:#########0}:"; // colon at the end is important
                WriteReferencedChildrenRecursive( s, ct.gameObject, ref sArr, path );
                i++;
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static UnityEngine.Object GetComponentOrGameObject( this GameObject root, string path )
        {
            if( string.IsNullOrEmpty( path ) )
            {
                return root;
            }

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

        //
        //  explicit hierarchy writing.
        //

        /// <summary>
        /// Saves the components a gameobject (object pass).
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializedArray SaveComponents_Objects( GameObject go, IReverseReferenceMap s )
        {
            SerializedArray components = new SerializedArray();

            foreach( var comp in go.GetComponents() )
            {
                Guid id = s.GetID( comp );
                SerializedObject compObj = new SerializedObject()
                {
                    { KeyNames.ID, s.WriteGuid( id ) },
                    { KeyNames.TYPE, s.WriteType( comp.GetType() ) }
                };

                if( comp is IPersistsObjects po )
                {
                    var sData = po.GetObjects( s );
                    compObj.Add( "objects", sData );
                }

                components.Add( compObj );
            }
            return components;
        }

        /// <summary>
        /// Saves the hierarchy of a gameobject (object pass).
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void SaveGameObjectHierarchy_Objects( GameObject go, IReverseReferenceMap s, uint includedObjectsMask, SerializedArray siblingsArray )
        {
            if( go == null )
            {
                return;
            }
            if( !go.IsInLayerMask( includedObjectsMask ) )
            {
                return;
            }

            Guid objectGuid = s.GetID( go );

            // recursive.
            SerializedObject obj = new SerializedObject()
            {
                { KeyNames.ID, s.WriteGuid(objectGuid) }
            };

            SerializedArray children = new SerializedArray();

            foreach( Transform child in go.transform )
            {
                SaveGameObjectHierarchy_Objects( child.gameObject, s, includedObjectsMask, children );

            }

            SerializedArray components = SaveComponents_Objects( go, s );

            obj.Add( "children", children );
            obj.Add( "components", components );

            siblingsArray.Add( obj );
        }

        /// <summary>
        /// Saves the hierarchy of a gameobject (data pass).
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void SaveGameObjectHierarchy_Data( IReverseReferenceMap s, GameObject go, uint includedObjectsMask, ref SerializedArray dataArray )
        {
            if( go == null )
            {
                return;
            }
            if( !go.IsInLayerMask( includedObjectsMask ) )
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
                    Debug.LogWarning( $"Couldn't serialize component '{comp}': {ex.Message}." );
                    Debug.LogException( ex );
                }

                StratUtils.TryWriteData( s, comp, compData, ref dataArray );
            }

            SerializedData goData = go.GetData( s );
            StratUtils.TryWriteData( s, go, goData, ref dataArray );

            foreach( Transform ct in go.transform )
            {
                SaveGameObjectHierarchy_Data( s, ct.gameObject, includedObjectsMask, ref dataArray );
            }
        }

        /// <summary>
        /// Loads (instantiates) a hierarchy of gameobjects from saved data (object pass).
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static GameObject InstantiateHierarchyObjects( IForwardReferenceMap l, SerializedData goJson, GameObject parent, List<Behaviour> behsUGLYDONTDOTHIS )
        {
            Guid objectGuid = l.ReadGuid( goJson[KeyNames.ID] );

            GameObject go = new GameObject();
            l.SetObj( objectGuid, go );

            if( parent != null )
            {
                go.transform.SetParent( parent.transform );
            }

            SerializedArray components = (SerializedArray)goJson["components"];
            foreach( var compData in components )
            {
                try
                {
                    Guid compID = l.ReadGuid( compData[KeyNames.ID] );
                    Type compType = l.ReadType( compData[KeyNames.TYPE] );

                    Component co = go.GetTransformOrAddComponent( compType );

                    if( co is Behaviour b ) // disable to prevent 'start' firing prematurely if async.
                    {
                        b.enabled = false;
                        behsUGLYDONTDOTHIS?.Add( b );
                    }
                    l.SetObj( compID, co );
                    
                    if( co is IPersistsObjects po )
                    {
                        var objData = compData["objects"];
                        po.SetObjects( l, objData );
                    }

                }
                catch( Exception ex )
                {
                    Debug.LogError( $"Failed to deserialize a component with ID: `{compData?[KeyNames.ID] ?? "<null>"}`." );
                    Debug.LogException( ex );
                }
            }

            SerializedArray children = (SerializedArray)goJson["children"];
            foreach( var childData in children )
            {
                try
                {
                    InstantiateHierarchyObjects( l, childData, go, behsUGLYDONTDOTHIS );
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"Failed to deserialize a child GameObject with ID: `{childData?[KeyNames.ID] ?? "<null>"}`." );
                    Debug.LogException( ex );
                }
            }
            return go;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void ApplyDataToHierarchyElement( IForwardReferenceMap l, SerializedData dataElement )
        {
            // Get whatever the data is pointing to.
            // If it's a gameobject or a component on a gameobject, apply the data to it.

            Guid id = l.ReadGuid( dataElement[KeyNames.REF] );
            object obj = l.GetObj( id );

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
                        Debug.LogError( $"Failed to deserialize data of component of type {comp.GetType()}, with ID: `{dataElement?[KeyNames.REF] ?? "<null>"}`." );
                        Debug.LogException( ex );
                    }
                    break;
            }
        }
    }
}