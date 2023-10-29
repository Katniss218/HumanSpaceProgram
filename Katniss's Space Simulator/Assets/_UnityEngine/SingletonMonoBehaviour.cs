using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
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
                }
                return __instance;
            }
        }
    }
}
