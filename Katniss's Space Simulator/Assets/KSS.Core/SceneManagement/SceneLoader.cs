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
    public class SceneLoader : MonoBehaviour
    {
        #region singleton bleh
        static SceneLoader ___instance;
        static SceneLoader _instance
        {
            get
            {
                if( ___instance == null )
                {
                    ___instance = FindObjectOfType<SceneLoader>();
                }
                return ___instance;
            }
        }
        #endregion

        public static void UnloadSceneAsync( string scene, Action onAfterUnloaded )
        {
            _instance.StartCoroutine( UnloadAsync( scene, onAfterUnloaded ) );
        }

        public static void LoadSceneAsync( string scene, bool additive, bool localPhysics, Action onAfterLoaded )
        {
            _instance.StartCoroutine( LoadAsync( scene, additive, localPhysics, onAfterLoaded ) );
        }

        static IEnumerator UnloadAsync( string sceneToUnload, Action onAfterUnloaded )
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

        static IEnumerator LoadAsync( string sceneToLoad, bool additive, bool localPhysics, Action onAfterLoaded )
        {
            LoadSceneMode lm = additive ? LoadSceneMode.Additive : LoadSceneMode.Single;
            LocalPhysicsMode lp = localPhysics ? LocalPhysicsMode.Physics3D : LocalPhysicsMode.None;

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