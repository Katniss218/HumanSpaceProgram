
namespace UnityEngine
{
    public static class SingletonMonoBehaviourUtils
    {
        public static T FindInstance<T>() where T : MonoBehaviour
        {
            T[] instances = Object.FindObjectsOfType<T>( true );

            if( instances.Length == 0 )
                throw new SingletonInstanceException( $"Requested {nameof( MonoBehaviour )} {typeof( T ).Name} was not found." );

            if( instances.Length > 1 )
                throw new SingletonInstanceException( $"Too many instances of {nameof( MonoBehaviour )} {typeof( T ).Name}." );

            return instances[0];
        }

        public static bool InstanceExists<T>( out T value ) where T : MonoBehaviour
        {
            T[] instances = Object.FindObjectsOfType<T>( true );
            value = instances.Length == 1 ? instances[0] : null;
            return instances.Length == 1;
        }
    }

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
                    __instance = SingletonMonoBehaviourUtils.FindInstance<T>();

                return __instance;
            }
            set // Use with care. Allows custom logic. Allows being set to a nonnull value when another different instance still exists.
            {
                __instance = value;
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
                    return SingletonMonoBehaviourUtils.InstanceExists<T>( out __instance );

                return __instance != null;
            }
        }
    }
}