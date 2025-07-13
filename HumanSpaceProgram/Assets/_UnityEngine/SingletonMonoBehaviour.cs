
namespace UnityEngine
{
    /// <summary>
    /// SingletonMonoBehaviour is a base class for Unity scripts that can have at most one instance. <br />
    /// The instance is available via a static field, and is loaded lazily.
    /// </summary>
    /// <remarks>
    /// Usage Example: `public class PlayerManager : SingletonMonoBehaviour<![CDATA[<]]>PlayerManager<![CDATA[>]]>`.
    /// </remarks>
    /// <typeparam name="T">The derived singleton type.</typeparam>
    [DisallowMultipleComponent]
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
    {
        private static T __instance;

        /// <summary>
        /// Gets the cached instance. <br/>
        /// If nothing is cached, attempts to find the instance.
        /// </summary>
        /// <remarks>
        /// Throws an exception if the number of active instances is not exactly 1.
        /// </remarks>
        protected static T instance
        {
            get
            {
                if( __instance == null )
                {
                    T[] instances = FindObjectsOfType<T>( true );
                    if( instances.Length == 0 )
                    {
                        throw new SingletonInstanceException( $"Requested {nameof( MonoBehaviour )} {typeof( T ).Name} was not found." );
                    }
                    if( instances.Length > 1 )
                    {
                        throw new SingletonInstanceException( $"Too many instances of {nameof( MonoBehaviour )} {typeof( T ).Name}." );
                    }

                    __instance = instances[0];
                }
                return __instance;
            }
        }

        /// <summary>
        /// Checks if exactly 1 instance of this behaviour exists.
        /// </summary>
        protected static bool instanceExists
        {
            get
            {
                if( __instance == null )
                {
                    T[] instances = FindObjectsOfType<T>( true );
                    if( instances.Length != 1 )
                    {
                        return false;
                    }

                    __instance = instances[0]; // Might as well assign it, since we already have it.
                }
                return __instance != null;
            }
        }
    }
}