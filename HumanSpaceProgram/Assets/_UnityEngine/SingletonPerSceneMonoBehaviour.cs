using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityPlus.SceneManagement
{
    /// <summary>
    /// SingletonPerSceneMonoBehaviour is a base class for Unity scripts that can have at most one instance per scene (multiple instances across many scenes are allowed). <br />
    /// The instances are available via a static accessor method, and are loaded lazily.
    /// </summary>
    /// <remarks>
    /// Usage Example: `public class PlayerManager : SingletonPerSceneMonoBehaviour<![CDATA[<]]>PlayerManager<![CDATA[>]]>`.
    /// </remarks>
    /// <typeparam name="T">The derived singleton type.</typeparam>
    [DisallowMultipleComponent]
    public class SingletonPerSceneMonoBehaviour<T> : MonoBehaviour where T : SingletonPerSceneMonoBehaviour<T>
    {
        private static Dictionary<Scene, T> __instances = new();

        /// <summary>
        /// Gets the cached instance. <br/>
        /// If nothing is cached, attempts to find the instance.
        /// </summary>
        /// <remarks>
        /// Throws an exception if the number of active instances in the specified scene is not exactly 1, or if there are more than 1 in any loaded scene.
        /// </remarks>
        protected static T GetInstance( Scene scene )
        {
            if( !scene.isLoaded )
            {
                throw new System.ArgumentException( "The scene must be loaded.", nameof( scene ) );
            }

            if( __instances.TryGetValue( scene, out T instance ) )
            {
                if( instance != null )
                {
                    return instance;
                }

                // The instance was destroyed and might need to be reloaded (the scene could've been reloaded).
                __instances.Remove( scene );
            }

            T[] instances = FindObjectsOfType<T>( true );
            if( instances.Length == 0 )
            {
                throw new SingletonInstanceException( $"Requested {nameof( MonoBehaviour )} {typeof( T ).Name} was not found in any loaded scene." );
            }

            foreach( var newInstance in instances )
            {
                Scene instanceScene = newInstance.gameObject.scene;
                if( __instances.ContainsKey( scene ) )
                {
                    throw new SingletonInstanceException( $"Too many instances of {nameof( MonoBehaviour )} {typeof( T ).Name} in scene {instanceScene.name}." );
                }

                __instances[instanceScene] = newInstance;
                if( instanceScene == scene )
                {
                    instance = newInstance; // This is the instance we want to return, but don't return yet,
                                            // assign everything first since we've done the expensive FindObjectsOfType call.
                }
            }

            if( instance == null )
            {
                throw new SingletonInstanceException( $"Requested {nameof( MonoBehaviour )} {typeof( T ).Name} was not found in the requested scene." );
            }

            return instance;
        }
    }
}