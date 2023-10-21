using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace UnityPlus.Serialization
{
    public static class SerializerUtils
    {
        /// <summary>
        /// The special token name for a reference ID (part of Object).
        /// </summary>
        public const string ID = "$id";

        /// <summary>
        /// The special token name for a reference (part of Reference).
        /// </summary>
        public const string REF = "$ref";

        /// <summary>
        /// The special token name for an asset reference.
        /// </summary>
        public const string ASSETREF = "$assetref";

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void TryWriteData( ISaver s, object obj, SerializedData data, ref SerializedArray objects )
        {
            if( data != null )
            {
                objects.Add( new SerializedObject()
                {
                    { $"{SerializerUtils.REF}", s.WriteGuid( s.GetReferenceID( obj ) ) },
                    { "data", data }
                } );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void TryWriteDataWithChildrenPaths( ISaver s, object obj, SerializedData data, SerializedArray childrenPaths, ref SerializedArray objects )
        {
            if( data != null )
            {
                objects.Add( new SerializedObject()
                {
                    { $"{SerializerUtils.REF}", s.WriteGuid( s.GetReferenceID( obj ) ) },
                    { "data", data },
                    { "children_ids", childrenPaths }
                } );
            }
        }

        public static void WriteReferencedChildrenRecursive( ISaver s, GameObject go, ref SerializedArray sArr, string parentPath )
        {
            // write the IDs of referenced components/child gameobjects of the parent into the array, along with the path to them.

            // root is always added, recursive children might not be.
            if( !string.IsNullOrEmpty( parentPath ) )
            {
                if( s.TryGetID( go, out Guid id ) )
                {
                    sArr.Add( new SerializedObject()
                {
                    { $"{SerializerUtils.ID}", s.WriteGuid( id ) },
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
                        { $"{SerializerUtils.ID}", s.WriteGuid( id ) },
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
    }
}