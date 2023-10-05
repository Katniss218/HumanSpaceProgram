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
        // unify some of the gameobject-level save/load methods. Format-agnostic.

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void WriteGameObjectData( ISaver s, GameObject go, ref SerializedArray objects )
        {
            objects.Add( new SerializedObject()
            {
                { "$ref", s.WriteGuid( s.GetReferenceID( go ) ) },
                { "name", go.name },
                { "layer", go.layer },
                { "is_active", go.activeSelf },
                { "is_static", go.isStatic },
                { "tag", go.tag }
            } );
        }
        
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void WriteGameObjectComponentsData( ISaver s, GameObject go, ref SerializedArray objects )
        {
            Component[] comps = go.GetComponents();
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