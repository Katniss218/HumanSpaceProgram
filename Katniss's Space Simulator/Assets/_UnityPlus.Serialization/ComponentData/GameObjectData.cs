using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.Serialization.ComponentData
{
    /// <summary>
    /// Stores persistent data about a gameobject.
    /// </summary>
    public static class GameObjectData
    {
        public static Dictionary<string, Func<Component[], object, Component>> PredicateRegistry { get; } = new Dictionary<string, Func<Component[], object, Component>>()
        {
            // funny thing is, these could be *assets* in the normal registry.
            { "index", ComponentFinder.GetComponentByIndex },
            { "type-and-index", ComponentFinder.GetComponentByTypeAndIndex }
        };
    }
}