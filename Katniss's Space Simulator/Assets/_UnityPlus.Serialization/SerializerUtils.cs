using System;
using System.Collections.Generic;
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
        /// Writes the 'data' part of a gameobject (only gameobject, not components).
        /// </summary>
        /// <param name="objects">The serialized array to add the serialized data object to.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void WriteGameObjectData( ISaver s, GameObject gameObject, ref SerializedArray objects )
        {
            objects.Add( new SerializedObject()
            {
                { "$ref", s.WriteGuid( s.GetReferenceID( gameObject ) ) },
                { "name", gameObject.name },
                { "layer", gameObject.layer },
                { "is_active", gameObject.activeSelf },
                { "is_static", gameObject.isStatic },
                { "tag", gameObject.tag }
            } );
        }

        /// <summary>
        /// Writes the 'data' part for all of the gameobject's components.
        /// </summary>
        /// <param name="objects">The serialized array to add the serialized data objects to.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void WriteGameObjectComponentsData( ISaver s, GameObject gameObject, ref SerializedArray objects )
        {
            Component[] comps = gameObject.GetComponents();
            for( int i = 0; i < comps.Length; i++ )
            {
                Component comp = comps[i];
                SerializedData data = null;
                try
                {
                    data = comp.GetData( s );
                }
                catch( Exception ex )
                {
                    Debug.LogWarning( $"Couldn't serialize component '{comp}': {ex.Message}." );
                    Debug.LogException( ex );
                }

                if( data != null )
                {
                    SerializedObject compData = new SerializedObject()
                    {
                        { "$ref", s.WriteGuid( s.GetReferenceID( comp ) ) },
                        { "data", data }
                    };
                    objects.Add( compData ); // components' data stored 'loosely' among everything else.
                }
            }
        }
    }
}