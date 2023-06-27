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
        /// Gets every component attached to the gameobject that the given component is attached to, including <see cref="Transform"/>.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Component[] GetComponents( this Component component )
        {
            return component.GetComponents<Component>();
        }
    }
}