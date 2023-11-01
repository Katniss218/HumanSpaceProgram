using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KSS.Core.SceneManagement
{
    /// <summary>
    /// Can load and unload scenes easily.
    /// </summary>
    public class SceneLoader
    {
        public static void UnloadActiveSceneAsync( Action onAfterUnloaded )
        {
            AlwaysLoadedManager.Instance.StartCoroutine( UnloadCoroutine( SceneManager.GetActiveScene().name, onAfterUnloaded ) );
        }

        public static void UnloadSceneAsync( string scene, Action onAfterUnloaded )
        {
            if( string.IsNullOrEmpty( scene ) )
            {
                throw new ArgumentNullException( nameof( scene ), $"The scene to unload can't be null." );
            }

            AlwaysLoadedManager.Instance.StartCoroutine( UnloadCoroutine( scene, onAfterUnloaded ) );
        }

        public static void LoadSceneAsync( string scene, bool additive, bool hasLocalPhysics, Action onAfterLoaded )
        {
            if( string.IsNullOrEmpty( scene ) )
            {
                throw new ArgumentNullException( nameof( scene ), $"The scene to load can't be null." );
            }

            AlwaysLoadedManager.Instance.StartCoroutine( LoadCoroutine( scene, additive, hasLocalPhysics, onAfterLoaded ) );
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

            onAfterLoaded?.Invoke();
        }
    }
}