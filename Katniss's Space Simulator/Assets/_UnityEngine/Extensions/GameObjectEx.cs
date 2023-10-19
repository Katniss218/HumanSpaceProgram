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
        /// A version of <see cref="GameObject.AddComponent(Type)"/> that is safe to use with the <see cref="Transform"/> type.
        /// </summary>
        public static Component GetTransformOrAddComponent( this GameObject gameObject, Type componentType )
        {
            if( componentType == typeof( Transform ) )
                return gameObject.transform;

            return gameObject.AddComponent( componentType );
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
            gameObject.GetComponents( results );
        }

        /// <summary>
        /// Checks if the gameobject has a component of a specified type.
        /// </summary>
        /// <remarks>
        /// Don't use this overload if you want to later do something with the component. Use <see cref="GameObject.GetComponent{T}"/> or <see cref="HasComponent{T}(GameObject, out T)"/> instead.
        /// </remarks>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool HasComponent<T>( this GameObject gameObject )
        {
            return gameObject.GetComponent<T>() != null;
        }

        /// <summary>
        /// Checks if the gameobject has a component of a specified type. Additionally returns the component, if present.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool HasComponent<T>( this GameObject gameObject, out T component )
        {
            component = gameObject.GetComponent<T>();
            return component != null;
        }

        /// <summary>
        /// Checks if the gameobject or any of its children (recursive) have a component of a specified type.
        /// </summary>
        /// <remarks>
        /// Don't use this overload if you want to later do something with the component. Use <see cref="GameObject.GetComponentInChildren{T}"/> or <see cref="GetComponentInChildren{T}(GameObject, out T)"/> instead.
        /// </remarks>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool HasComponentInChildren<T>( this GameObject gameObject )
        {
            return gameObject.GetComponentInChildren<T>() != null;
        }

        /// <summary>
        /// Checks if the gameobject or any of its children (recursive) have a component of a specified type. Additionally returns the component, if present.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool HasComponentInChildren<T>( this GameObject gameObject, out T component )
        {
            component = gameObject.GetComponentInChildren<T>();
            return component != null;
        }

        /// <summary>
        /// Checks if the gameobject is contained in a given layer mask.
        /// </summary>
        public static bool IsInLayerMask( this GameObject gameObject, uint layerMask )
        {
            return ((1 << gameObject.layer) & layerMask) != 0;
        }
    }
}