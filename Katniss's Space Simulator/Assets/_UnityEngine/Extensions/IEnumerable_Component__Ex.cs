using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public static class IEnumerable_Component__Ex
    {
        public static IEnumerable<T> GetComponents<T>( this IEnumerable<Component> components ) where T : Component
        {
            return components.SelectMany( c => c.GetComponents<T>() );
        }

        public static IEnumerable<T> GetComponentsInChildren<T>( this IEnumerable<Component> components ) where T : Component
        {
            return components.SelectMany( c => c.GetComponentsInChildren<T>() );
        }

        public static IEnumerable<T> GetComponentsInParent<T>( this IEnumerable<Component> components ) where T : Component
        {
            return components.SelectMany( c => c.GetComponentsInParent<T>() );
        }
    }

    public static class IEnumerable_GameObject__Ex
    {
        public static IEnumerable<T> GetComponents<T>( this IEnumerable<GameObject> gameObjects ) where T : Component
        {
            return gameObjects.SelectMany( c => c.GetComponents<T>() );
        }

        public static IEnumerable<T> GetComponentsInChildren<T>( this IEnumerable<GameObject> gameObjects ) where T : Component
        {
            return gameObjects.SelectMany( c => c.GetComponentsInChildren<T>() );
        }

        public static IEnumerable<T> GetComponentsInParent<T>( this IEnumerable<GameObject> gameObjects ) where T : Component
        {
            return gameObjects.SelectMany( c => c.GetComponentsInParent<T>() );
        }
    }
}