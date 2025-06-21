using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HSP.SceneManagement
{
    /// <summary>
    /// Can load and unload scenes easily.
    /// </summary>
    public class SceneLoader : SingletonMonoBehaviour<SceneLoader>
    {
        private static List<IScene> _loadedScenes = new();

        private static IScene _activeScene = null;

        /// <summary>
        /// Gets the scene that is currently the 'main' scene.
        /// </summary>
        public static IScene ActiveScene => _activeScene;

        public static bool IsSceneLoaded<T>() where T : IScene
        {
            return _loadedScenes.Any( s => s.GetType() == typeof( T ) );
        }

        public static IEnumerable<IScene> GetLoadedScenes()
        {
            return _loadedScenes;
        }

        public static void LoadSceneAsync<TNewScene>() where TNewScene : SceneManager<TNewScene>
        {
        }

        public static void UnloadSceneAsync<TOldScene>() where TOldScene : SceneManager<TOldScene>
        {
        }
#warning TODO - load as background or foreground (does/doesn't invoke onactivate)
        public static void UnloadActiveSceneAsync()
        {
        }

        public static void ReplaceSceneAsync<TOldScene, TNewScene>() where TOldScene : SceneManager<TOldScene> where TNewScene : SceneManager<TNewScene>
        {
        }

        public static void ReplaceActiveScene<TNewScene>() where TNewScene : SceneManager<TNewScene>
        {
            //if( _activeScene != null )
            //{
            //    _activeScene.OnDeactivate();

            //    _activeScene.OnUnload();
            //}

            //_loadedScenes.Remove( _activeScene );
            //var newScene = SceneManager<TNewScene>.Instance;
            //_activeScene = null;

            UnloadSceneAsync( () =>
            {
                LoadSceneAsync( ... );
            } );
        }

        //public static void UnloadActiveSceneAsync( Action onAfterUnloaded )
        //{
        //    instance.StartCoroutine( UnloadCoroutine( SceneManager.GetActiveScene().name, onAfterUnloaded ) );
        //}

        private static void UnloadSceneAsync( string sceneName, Action onAfterUnloaded )
        {
            if( string.IsNullOrEmpty( sceneName ) )
            {
                throw new ArgumentNullException( nameof( sceneName ), $"The scene to unload can't be null." );
            }

            instance.StartCoroutine( UnloadCoroutine( sceneName, onAfterUnloaded ) );
        }

        private static void LoadSceneAsync( string sceneName, bool additive, bool hasLocalPhysics, Action onAfterLoaded )
        {

#warning TODO - if manager is already in the scene, don't create. otherwise  -create.
            if( string.IsNullOrEmpty( sceneName ) )
            {
                throw new ArgumentNullException( nameof( sceneName ), $"The scene to load can't be null." );
            }

            instance.StartCoroutine( LoadCoroutine( sceneName, additive, hasLocalPhysics, onAfterLoaded ) );
        }

        private static IEnumerator UnloadCoroutine( string sceneToUnload, Action onAfterUnloaded )
        {
            Scene scene = SceneManager.GetSceneByName( sceneToUnload );
            AsyncOperation op = SceneManager.UnloadSceneAsync( scene );

            // Wait until the asynchronous scene fully loads
            while( !op.isDone )
            {
                yield return null;
            }

            _loadedScenes.Remove( sceneToUnload );
            _activeSceneName = SceneManager.GetActiveScene().name;

            onAfterUnloaded?.Invoke();
        }

        private static IEnumerator LoadCoroutine( string sceneToLoad, bool additive, bool hasLocalPhysics, Action onAfterLoaded )
        {
            LoadSceneMode lm = additive ? LoadSceneMode.Additive : LoadSceneMode.Single;
            LocalPhysicsMode lp = hasLocalPhysics ? LocalPhysicsMode.Physics3D : LocalPhysicsMode.None;

            AsyncOperation op = SceneManager.LoadSceneAsync( sceneToLoad, new LoadSceneParameters( lm, lp ) );
            op.completed += ( x ) =>
            {
                Scene scene = SceneManager.GetSceneByName( sceneToLoad );
                SceneManager.SetActiveScene( scene );
            };

            // Wait until the asynchronous scene fully loads
            while( !op.isDone )
            {
                yield return null;
            }

            if( !additive )
                _loadedScenes.Clear();
            _loadedScenes.Add( sceneToLoad );
            _activeSceneName = SceneManager.GetActiveScene().name;

            onAfterLoaded?.Invoke();
        }
    }
}