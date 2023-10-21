using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public static class ComponentEx
    {
        /// <summary>
        /// Gets every component attached to the component's gameobject, including <see cref="Transform"/>.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Component[] GetComponents( this Component component )
        {
            return component.GetComponents<Component>();
        }

        /// <summary>
        /// Gets every component attached to the component's gameobject, including its <see cref="Transform"/>.
        /// </summary>
        /// <param name="results">The list to be filled with returned components. It is resized to match the number of components found.</param>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void GetComponents( this Component component, List<Component> results )
        {
            component.GetComponents( results );
        }

        /// <summary>
        /// Checks if the component's gameobject has a component of a specified type.
        /// </summary>
        /// <remarks>
        /// Don't use this overload if you want to later do something with the component. Use <see cref="Component.GetComponent{T}"/> or <see cref="HasComponent{T}(GameObject, out T)"/> instead.
        /// </remarks>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool HasComponent<T>( this Component component )
        {
            return component.GetComponent<T>() != null;
        }

        /// <summary>
        /// Checks if the component's gameobject has a component of a specified type. Additionally returns the component, if present.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool HasComponent<T>( this Component comp, out T component )
        {
            component = comp.GetComponent<T>();
            return component != null;
        }

        /// <summary>
        /// Checks if the component's gameobject or any of its children (recursive) have a component of a specified type.
        /// </summary>
        /// <remarks>
        /// Don't use this overload if you want to later do something with the component. Use <see cref="Component.GetComponentInChildren{T}"/> or <see cref="GetComponentInChildren{T}(GameObject, out T)"/> instead.
        /// </remarks>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool HasComponentInChildren<T>( this Component component )
        {
            return component.GetComponentInChildren<T>() != null;
        }

        /// <summary>
        /// Checks if the component's gameobject or any of its children (recursive) have a component of a specified type. Additionally returns the component, if present.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool HasComponentInChildren<T>( this Component comp, out T component )
        {
            component = comp.GetComponentInChildren<T>();
            return component != null;
        }
    }
}