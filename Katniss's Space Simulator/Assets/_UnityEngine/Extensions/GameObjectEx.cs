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
        /// Gets every component attached to the gameobject, including <see cref="Transform"/>.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Component[] GetComponents( this GameObject gameObject )
        {
            return gameObject.GetComponents<Component>();
        }

        public static bool IsInLayerMask( this GameObject gameObject, int layerMask )
        {
            return ((1 << gameObject.layer) & layerMask) != 0;
        }
    }
}