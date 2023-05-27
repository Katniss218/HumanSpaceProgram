using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KSS.Core.Scenes
{
    public class SceneManager : MonoBehaviour
    {
        static SceneManager _instance;
        public static SceneManager Instance
        {
            get
            {
                if( _instance == null )
                {
                    _instance = FindObjectOfType<SceneManager>();
                }
                return _instance;
            }
        }

        public void UnloadScene( string scene, Action onAfterUnloaded )
        {
            StartCoroutine( UnloadAsync( scene, onAfterUnloaded ) );
        }

        public void LoadScene( string scene, bool additive, bool localPhysics, Action onAfterLoaded )
        {
            StartCoroutine( LoadAsync( scene, additive, localPhysics, onAfterLoaded ) );
        }

        static IEnumerator UnloadAsync( string sceneToUnload, Action onAfterUnloaded )
        {

            Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName( sceneToUnload );
            AsyncOperation op = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync( scene );

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

            AsyncOperation op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync( sceneToLoad, new LoadSceneParameters( lm, lp ) );
            op.completed += ( x ) =>
            {
                Scene scene2 = UnityEngine.SceneManagement.SceneManager.GetSceneByName( sceneToLoad );
                UnityEngine.SceneManagement.SceneManager.SetActiveScene( scene2 );
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