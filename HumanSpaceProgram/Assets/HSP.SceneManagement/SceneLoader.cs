using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HSP.SceneManagement
{
    /// <summary>
    /// Can load and unload scenes easily.
    /// </summary>
    public class SceneLoader : SingletonMonoBehaviour<SceneLoader>
    {
        private static HashSet<string> _loadedScenes = new();

        public static bool IsSceneLoaded( string sceneName )
        {
            return _loadedScenes.Contains( sceneName );
        }

        public static void UnloadActiveSceneAsync( Action onAfterUnloaded )
        {
            instance.StartCoroutine( UnloadCoroutine( SceneManager.GetActiveScene().name, onAfterUnloaded ) );
        }

        public static void UnloadSceneAsync( string sceneName, Action onAfterUnloaded )
        {
            if( string.IsNullOrEmpty( sceneName ) )
            {
                throw new ArgumentNullException( nameof( sceneName ), $"The scene to unload can't be null." );
            }

            instance.StartCoroutine( UnloadCoroutine( sceneName, onAfterUnloaded ) );
        }

        public static void LoadSceneAsync( string sceneName, bool additive, bool hasLocalPhysics, Action onAfterLoaded )
        {
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

            onAfterLoaded?.Invoke();
        }
    }
}