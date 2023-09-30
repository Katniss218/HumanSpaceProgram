using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityEngine
{
    /// <summary>
    /// An interface restricting placement of other interfaces to components
    /// </summary>
    public interface IComponent
    {
        Transform transform { get; }
        GameObject gameObject { get; }
        string tag { get; set; }

        // TODO - we can add the rest of methods and fields actually.
    }
}
