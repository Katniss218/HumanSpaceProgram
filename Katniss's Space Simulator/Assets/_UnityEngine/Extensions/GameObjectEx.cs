using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public static class GameObjectEx
    {
        /// <summary>
        /// A version of <see cref="GameObject.AddComponent(Type)"/> that is safe to use with Transform.
        /// </summary>
        public static Component GetTransformOrAddComponent( this GameObject go, Type componentType )
        {
            if( componentType == typeof( Transform ) )
                return go.transform;

            return go.AddComponent( componentType );
        }

        /// <summary>
        /// Gets every component attached to the gameobject, including its <see cref="Transform"/>.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Component[] GetComponents( this GameObject gameObject )
        {
            return gameObject.GetComponents<Component>();
        }

        /// <summary>
        /// Gets every component attached to the gameobject, including its <see cref="Transform"/>.
        /// </summary>
        /// <param name="results">The list to be filled with returned components. It is resized to match the number of components found.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void GetComponents( this GameObject gameObject, List<Component> results )
        {
            gameObject.GetComponents<Component>( results );
        }

        /// <summary>
        /// Checks if the game object has a component of a specified type.
        /// </summary>
        /// <remarks>
        /// Don't use this overload to do something with the component. Use <see cref="GameObject.GetComponent{T}"/> or <see cref="HasComponent{T}(GameObject, out T)"/>.
        /// </remarks>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool HasComponent<T>( this GameObject gameObject )
        {
            return gameObject.GetComponent<T>() == null;
        }

        /// <summary>
        /// Checks if the game object has a component of a specified type. Additionally returns the component, if present.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool HasComponent<T>( this GameObject gameObject, out T component )
        {
            component = gameObject.GetComponent<T>();
            return component == null;
        }

        public static bool IsInLayerMask( this GameObject gameObject, int layerMask )
        {
            return ((1 << gameObject.layer) & layerMask) != 0;
        }
    }
}