using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
    [DisallowMultipleComponent]
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
    {
        private static T __instance;

        protected static T instance
        {
            get
            {
                if( __instance == null )
                {
                    __instance = FindObjectOfType<T>();
                    if( __instance == null )
                    {
                        throw new InvalidOperationException( $"Requested {nameof( MonoBehaviour )} {typeof( T ).Name} was not found." );
                    }
                }
                return __instance;
            }
        }

        protected static bool exists
        {
            get
            {
                if( __instance == null )
                {
                    __instance = FindObjectOfType<T>();
                }
                return __instance != null;
            }
        }
    }
}