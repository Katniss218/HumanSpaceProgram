using UnityEngine;
using UnityEngine.SceneManagement;

namespace HSP.SceneManagement
{
    /// <summary>
    /// The base class that all HSP scenes should derive from.
    /// </summary>
    /// <remarks>
    /// Usage Example: `public sealed class GameplaySceneManager : HSPSceneManager<![CDATA[<]]>GameplaySceneManager<![CDATA[>]]>`.
    /// </remarks>
    /// <typeparam name="T">The derived scene type.</typeparam>
    public abstract class HSPScene<T> : SingletonMonoBehaviour<T>, IHSPScene where T : HSPScene<T>
    {
        /// <summary>
        /// Override this to tell the loader to load a custom scene (included in an asset bundle or in the project).
        /// </summary>
        public static string UNITY_SCENE_NAME { get => null; }

        // The Unity scene associated with (backing) our scene manager instance.
        private UnityEngine.SceneManagement.Scene _unityScene;

        public UnityEngine.SceneManagement.Scene UnityScene
        {
            get => _unityScene;
        }

        /// <summary>
        /// Override this to perform any initialisation logic that should be run when the scene is loaded.
        /// </summary>
        protected abstract void OnLoad();
        /// <summary>
        /// Override this to perform any cleanup logic that should be run when the scene is unloaded.
        /// </summary>
        protected abstract void OnUnload();

        /// <summary>
        /// Override this to perform any initialisation logic that should be run when the scene becomes the 'foreground' scene.
        /// </summary>
        protected abstract void OnActivate();
        /// <summary>
        /// Override this to perform any cleanup logic that should be run when the scene is no longer the 'foreground' scene.
        /// </summary>
        protected abstract void OnDeactivate();

        public void _onload()
        {
            this.OnLoad();
        }
        public void _onunload()
        {
            this.OnUnload();
        }
        public void _onactivate()
        {
            this.OnActivate();
        }
        public void _ondeactivate()
        {
            this.OnDeactivate();
        }

        internal static T GetOrCreateSceneManagerInActiveScene( Scene unityScene )
        {
            if( !instanceExists || instance == null )
            {
                // create
                GameObject go = new GameObject( $"_ {typeof( T ).Name} _" );
                go.AddComponent<T>(); // Will be found and validated against multiple instances later.
            }

            instance._unityScene = unityScene;

            return instance;
        }
    }
}