using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    public static class Object_Ex
    {
        /// <summary>
        /// Checks whether an object is a <see cref="UnityEngine.Object"/> that has been destroyed.
        /// </summary>
        /// <remarks>
        /// Unassigned Unity objects are not truly null, UnityEngine.Object overrides the `==` operator to make empty references equal to null.
        /// </remarks>
        public static bool IsUnityNull( this object obj )
        {
            if( obj == null )
            {
                return true;
            }

            if( obj is UnityEngine.Object unityobject )
            {
                return unityobject == null;
            }

            return false;
        }
    }
}