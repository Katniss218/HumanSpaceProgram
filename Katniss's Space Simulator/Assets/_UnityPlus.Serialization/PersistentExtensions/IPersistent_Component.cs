using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public static class IPersistent_Component
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializedData GetData( this Component c, ISaver s )
        {
            switch( c )
            {
                case IPersistent comp:
                    return comp.GetData( s );
                case Transform comp:
                    return comp.GetData( s );
                case MeshFilter comp:
                    return comp.GetData( s );
                case MeshRenderer comp:
                    return comp.GetData( s );

                    // particle system

                    // navmesh stuff

                    // colliders

                    // rigidbody

                    // etc.
            }
            return null;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void SetData( this Component component, ILoader l, SerializedData data )
        {
            // component "data" means that the component (which is a referencable object) has already been added by an object action, and we're now reading its data.

            SerializedObject jsonObj = (SerializedObject)data;
            switch( component )
            {
                case IPersistent comp:
                    comp.SetData( l, jsonObj ); break;
                case Transform comp:
                    comp.SetData( l, jsonObj ); break;
                case MeshFilter comp:
                    comp.SetData( l, jsonObj ); break;
                case MeshRenderer comp:
                    comp.SetData( l, jsonObj ); break;
            }
        }
    }
}