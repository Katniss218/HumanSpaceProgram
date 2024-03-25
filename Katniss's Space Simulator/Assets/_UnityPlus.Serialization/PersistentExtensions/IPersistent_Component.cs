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
        public static SerializedData GetData( this Component c, IReverseReferenceMap s )
        {
#warning TODO - store info about enabled/disabled.
            switch( c )
            {
                case IPersistsData comp:
                    return comp.GetData( s );
                case Transform comp:
                    return comp.GetData( s );
                case MeshFilter comp:
                    return comp.GetData( s );
                case MeshRenderer comp:
                    return comp.GetData( s );
                case BoxCollider comp:
                    return comp.GetData( s );
                case SphereCollider comp:
                    return comp.GetData( s );
                case CapsuleCollider comp:
                    return comp.GetData( s );
                case MeshCollider comp:
                    return comp.GetData( s );
                case Rigidbody comp:
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
        public static void SetData( this Component component, IForwardReferenceMap l, SerializedData data )
        {
            // component "data" means that the component (which is a referencable object) has already been added by an object action, and we're now reading its data.

            switch( component )
            {
                case IPersistsData comp:
                    comp.SetData( l, data ); break;
                case Transform comp:
                    comp.SetData( l, data ); break;
                case MeshFilter comp:
                    comp.SetData( l, data ); break;
                case MeshRenderer comp:
                    comp.SetData( l, data ); break;
                case BoxCollider comp:
                    comp.SetData( l, data ); break;
                case SphereCollider comp:
                    comp.SetData( l, data ); break;
                case CapsuleCollider comp:
                    comp.SetData( l, data ); break;
                case MeshCollider comp:
                    comp.SetData( l, data ); break;
                case Rigidbody comp:
                    comp.SetData( l, data ); break;
            }
        }
    }
}