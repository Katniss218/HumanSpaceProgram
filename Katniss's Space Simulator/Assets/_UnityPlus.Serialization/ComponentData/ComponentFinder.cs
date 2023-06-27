using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization.ComponentData
{
    /// <summary>
    /// Stores persistent data about a gameobject.
    /// </summary>
    public static class ComponentFinderPredicate
    {
        public static Dictionary<string, Func<Component[], object, Component>> Registry { get; } = new Dictionary<string, Func<Component[], object, Component>>()
        {
            // funny thing is, these could be *assets* in the normal registry.
            { "index", ComponentFinder.GetComponentByIndex },
            { "type-and-index", ComponentFinder.GetComponentByTypeAndIndex }
        };
    }

    /// <summary>
    /// Implements static methods that can be used to find a component on a gameobject by a predicate, and data associated with that predicate.
    /// </summary>
    public static class ComponentFinder
    {
        public static Component GetComponentByIndex( Component[] components, object data )
        {
            // Data is `object` because it has to be added to a common list.

            int index = (int)data;
            if( index < 0 || index >= components.Length )
            {
                return null;
            }
            return components[index];
        }

        public static Component GetComponentByTypeAndIndex( Component[] components, object data )
        {
            // Data is `object` because it has to be added to a common list.

            (Type type, int index) = ((Type, int))data;

            int current = 0;
            foreach( var comp in components )
            {
                if( comp.GetType() == type )
                {
                    if( current == index )
                    {
                        return comp;
                    }
                    current++;
                }
            }
            return null;
        }
    }
}
