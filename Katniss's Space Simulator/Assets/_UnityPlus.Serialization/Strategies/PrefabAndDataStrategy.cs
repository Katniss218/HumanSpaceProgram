using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization.ComponentData;

namespace UnityPlus.Serialization.Strategies
{
    /// <summary>
    /// Can be used to save the scene using the factory-gameobjectdata scheme.
    /// </summary>
    [Obsolete( "Incomplete" )]
    public sealed class ExplicitHierarchyStrategy
    {
        // Object actions are suffixed by _Object
        // Data actions are suffixed by _Data

    }
}