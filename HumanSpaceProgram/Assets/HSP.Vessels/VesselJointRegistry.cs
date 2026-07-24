using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.Vessels
{
    public static class VesselJointRegistry
    {
        private static readonly Dictionary<(Type, Type), Func<FAttachNode, FAttachNode, ConfigurableJoint>> _factories = new();

        public static void RegisterHandler<T1, T2>( Func<FAttachNode, FAttachNode, ConfigurableJoint> factory )
            where T1 : FAttachNode
            where T2 : FAttachNode
        {
            if(factory == null)
                throw new ArgumentNullException( nameof(factory) );
            // TODO - type-check the actual delegate arguments.



            var key = GetKey( typeof( T1 ), typeof( T2 ) );
            _factories[key] = factory;
        }

        /// <summary>
        /// Create a joint using the appropriate factory based on the types of the provided nodes. <br/>
        /// The order of the nodes does not matter, the factory will be called with the correct order based on the registered types.
        /// </summary>
        /// <returns></returns>
        public static UnityEngine.Joint CreateJoint( FAttachNode nodeA, FAttachNode nodeB )
        {
            if( nodeA == null || nodeB == null )
                return null;

            (Type, Type) key = GetKey( nodeA.GetType(), nodeB.GetType() );
            if( _factories.TryGetValue( key, out var factory ) )
            {
                Type typeA = nodeA.GetType();
                Type typeB = nodeB.GetType();

                if( typeA == key.Item1 )
                {
                    return factory( nodeA, nodeB );
                }
                else
                {
                    return factory( nodeB, nodeA );
                }
            }

            Debug.LogWarning( $"No joint factory registered for {nodeA.GetType().Name} and {nodeB.GetType().Name}" );
            return null;
        }

        private static (Type, Type) GetKey( Type t1, Type t2 )
        {
            // Symmetric key: sort by full name to ensure consistency
            return string.Compare( t1.FullName, t2.FullName, StringComparison.Ordinal ) < 0
                ? (t1, t2)
                : (t2, t1);
        }
    }
}