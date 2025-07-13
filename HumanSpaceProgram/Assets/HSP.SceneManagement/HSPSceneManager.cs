using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HSP.SceneManagement
{
    /// <summary>
    /// Manages the loading and unloading of scenes within HSP.
    /// </summary>
    /// <remarks>
    /// Use this instead of using the Unity SceneManager directly.
    /// </remarks>
    public class HSPSceneManager : SingletonMonoBehaviour<HSPSceneManager>
    {
        // Only 1 scene of a given type can be loaded at a time.
        // The scene monobehs are singletons so this makes sense.

        private static HashSet<IHSPScene> _loadedScenes = new();

        private static IHSPScene _foregroundScene = null;

        /// <summary>
        /// Gets the scene that is currently the 'active' scene. <br/>
        /// The active Unity scene is also the scene that is backing this HSP scene.
        /// </summary>
        public static IHSPScene ForegroundScene => _foregroundScene;

        /// <summary>
        /// Checks if the scene specified by the given type is currently loaded.
        /// </summary>
        /// <typeparam name="TScene">The type specifying the scene to check.</typeparam>
        public static bool IsLoaded<TScene>() where TScene : IHSPScene
        {
            return _loadedScenes.Any( s => s.GetType() == typeof( TScene ) );
        }

        /// <summary>
        /// Checks if the scene specified by the given type is currently loaded.
        /// </summary>
        /// <typeparam name="TScene">The type specifying the scene to check.</typeparam>
        public static bool IsForeground<TLoadedScene>() where TLoadedScene : IHSPScene
        {
            IHSPScene scene = _loadedScenes.FirstOrDefault( s => s.GetType() == typeof( TLoadedScene ) );
            if( scene == null )
                throw new InvalidOperationException( $"The scene '{typeof( TLoadedScene ).Name}' is not loaded." );

            return scene == _foregroundScene;
        }

        public static void MoveGameObjectToScene<TLoadedScene>( GameObject gameObject ) where TLoadedScene : IHSPScene
        {
            IHSPScene scene = _loadedScenes.FirstOrDefault( s => s.GetType() == typeof( TLoadedScene ) );
            if( scene == null )
                throw new InvalidOperationException( $"The scene '{typeof( TLoadedScene ).Name}' is not loaded." );

            SceneManager.MoveGameObjectToScene( gameObject, scene.UnityScene );
        }
        
        public static void MoveGameObjectToScene( GameObject gameObject, IHSPScene scene )
        {
            if( !scene.UnityScene.isLoaded )
                throw new InvalidOperationException( $"The scene '{scene.GetType().Name}' is not loaded." );

            SceneManager.MoveGameObjectToScene( gameObject, scene.UnityScene );
        }
        
        public static IHSPScene GetScene( GameObject gameObject )
        {
            if( gameObject == null )
            {
                throw new ArgumentNullException( nameof( gameObject ), "GameObject cannot be null." );
            }

            Scene scene = gameObject.scene;
            if( !scene.isLoaded )
            {
                throw new InvalidOperationException( $"The scene '{scene.name}' is not loaded." );
            }

            var scene2 = _loadedScenes.FirstOrDefault( s => s.UnityScene == scene );
            if( scene2 == null )
            {
                throw new InvalidOperationException( $"The GameObject '{gameObject.name}' is not part of any loaded HSP scene." );
            }
            return scene2;
        }

        /// <summary>
        /// Gets all of the currently loaded scenes.
        /// </summary>
        public static IEnumerable<IHSPScene> GetLoadedScenes()
        {
            return _loadedScenes;
        }

        /// <summary>
        /// Starts the loading of a new scene asynchronously. The scene will be loaded as the foreground scene, deactivating the previous foreground scene if there is one. <br/>
        /// The scene must not currently be loaded.
        /// </summary>
        /// <remarks>
        /// The scene is loaded additively, meaning that it will not replace the currently loaded scenes. <br/>
        /// If you wish to do so, use <br/>
        /// - <see cref="UnloadAsync{TOld}"/> or <br/>
        /// - <see cref="ReplaceAsync{TOld, TNew}"/> instead.
        /// </remarks>
        /// <typeparam name="TNewScene">The type specifying the scene to load.</typeparam>
        /// <param name="onAfterLoaded">An action to be invoked after the new scene finishes loading (optional).</param>
        public static void LoadAsync<TNewScene>( Action onAfterLoaded = null ) where TNewScene : HSPScene<TNewScene>
        {
            StartSceneLoadCoroutine( typeof( TNewScene ), true, onAfterLoaded );
        }

        /// <summary>
        /// Starts the loading of a new scene asynchronously. The scene will be loaded as a background scene. <br/>
        /// The scene must not currently be loaded.
        /// </summary>
        /// <remarks>
        /// The scene is loaded additively, meaning that it will not replace the currently loaded scenes. <br/>
        /// If you wish to do so, use <br/>
        /// - <see cref="UnloadAsync{TOld}"/> or <br/>
        /// - <see cref="ReplaceAsync{TOld, TNew}"/> instead.
        /// </remarks>
        /// <typeparam name="TNewScene">The type specifying the scene to load.</typeparam>
        /// <param name="onAfterLoaded">An action to be invoked after the new scene finishes loading (optional).</param>
        public static void LoadAsBackgroundAsync<TNewScene>( Action onAfterLoaded = null ) where TNewScene : HSPScene<TNewScene>
        {
            StartSceneLoadCoroutine( typeof( TNewScene ), false, onAfterLoaded );
        }

        /// <summary>
        /// Starts the unloading of a specified scene asynchronously. <br/>
        /// The scene must currently be loaded.
        /// </summary>
        /// <typeparam name="TOldScene">The type specifying the scene to unload.</typeparam>
        /// <param name="onAfterUnloaded">An action to be invoked after the scene finishes unloading (optional).</param>
        public static void UnloadAsync<TOldScene>( Action onAfterUnloaded = null ) where TOldScene : HSPScene<TOldScene>
        {
            StartSceneUnloadCoroutine( typeof( TOldScene ), onAfterUnloaded );
        }

        /// <summary>
        /// Starts the unloading of the current foreground scene asynchronously.
        /// </summary>
        /// <param name="onAfterUnloaded">An action to be invoked after the scene finishes unloading (optional).</param>
        public static void UnloadForegroundSceneAsync( Action onAfterUnloaded = null )
        {
            if( _foregroundScene == null )
            {
                throw new InvalidOperationException( "There is currently no loaded foreground scene." );
            }

            StartSceneUnloadCoroutine( _foregroundScene.GetType(), onAfterUnloaded );
        }

        /// <summary>
        /// Starts the unloading of a specified scene, and then the loading of a new scene asynchronously. <br/>
        /// The scene to unload must currently be loaded.
        /// </summary>
        /// <remarks>
        /// If the scene to be unloaded is foreground, the new scene replacing it will also be foreground.
        /// </remarks>
        public static void ReplaceAsync<TOldScene, TNewScene>( Action onAfterUnloaded = null, Action onAfterLoaded = null ) where TOldScene : HSPScene<TOldScene> where TNewScene : HSPScene<TNewScene>
        {
            bool isForeground = IsForeground<TOldScene>();
            StartSceneUnloadCoroutine( typeof( TOldScene ), () =>
            {
                onAfterUnloaded?.Invoke();
                StartSceneLoadCoroutine( typeof( TNewScene ), isForeground, onAfterLoaded );
            } );
        }

        /// <summary>
        /// Starts the unloading of the current foreground scene, and then the loading of a new scene asynchronously.
        /// </summary>
        public static void ReplaceForegroundScene<TNewScene>( Action onAfterUnloaded = null, Action onAfterLoaded = null ) where TNewScene : HSPScene<TNewScene>
        {
            if( _foregroundScene == null )
            {
                throw new InvalidOperationException( "There is currently no loaded foreground scene." );
            }

            StartSceneUnloadCoroutine( _foregroundScene.GetType(), () =>
            {
                onAfterUnloaded?.Invoke();
                StartSceneLoadCoroutine( typeof( TNewScene ), true, onAfterLoaded );
            } );
        }

        public static void SetAsForeground<TLoadedScene>() where TLoadedScene : IHSPScene
        {
            IHSPScene scene = _loadedScenes.FirstOrDefault( s => s.GetType() == typeof( TLoadedScene ) );
            if( scene == null )
                throw new InvalidOperationException( $"Can't set the foreground scene to '{typeof( TLoadedScene ).Name}' that is not loaded." );

            if( scene == _foregroundScene )
                return;

            if( _foregroundScene != null )
            {
                _foregroundScene._ondeactivate();
            }

            _foregroundScene = scene;
            _foregroundScene._onactivate();
        }

        public static void SetAsBackground<TLoadedScene>() where TLoadedScene : IHSPScene
        {
            IHSPScene scene = _loadedScenes.FirstOrDefault( s => s.GetType() == typeof( TLoadedScene ) );
            if( scene == null )
                throw new InvalidOperationException( $"Can't set the background scene to '{typeof( TLoadedScene ).Name}' that is not loaded." );

            // Only one scene can be the foreground scene, so this can check for background too.
            if( scene != _foregroundScene )
                return;
            _foregroundScene._ondeactivate();
            _foregroundScene = null;
            SceneManager.SetActiveScene( AlwaysLoadedScene.Instance.UnityScene );
        }

        //
        //      COROUTINES BELOW
        //

        private static void StartSceneLoadCoroutine( Type sceneType, bool asForeground, Action onAfterLoaded )
        {
            if( _loadedScenes.Any( s => s.GetType() == sceneType ) )
            {
                throw new InvalidOperationException( $"Can't load the scene '{sceneType.Name}' that is already loaded." );
            }

            instance.StartCoroutine( LoadCoroutine( sceneType, asForeground, onAfterLoaded ) );
        }

        private static void StartSceneUnloadCoroutine( Type sceneType, Action onAfterUnloaded )
        {
            IHSPScene scene = _loadedScenes.FirstOrDefault( s => s.GetType() == sceneType );
            if( scene == null )
            {
                throw new InvalidOperationException( $"Can't unload the scene '{sceneType.Name}' that is not loaded." );
            }

            instance.StartCoroutine( UnloadCoroutine( scene, onAfterUnloaded ) );
        }

        private static IEnumerator LoadCoroutine( Type newSceneType, bool asForeground, Action onAfterLoaded )
        {
            const LoadSceneMode lm = LoadSceneMode.Additive;
            const LocalPhysicsMode lp = LocalPhysicsMode.None;

            string unitySceneName = null;
            PropertyInfo property = newSceneType.GetProperty( "UNITY_SCENE_NAME", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static );
            if( property != null )
            {
                unitySceneName = (string)property.GetValue( null );
            }
            else
            {
                FieldInfo field = newSceneType.GetField( "UNITY_SCENE_NAME", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static );
                if( field != null )
                {
                    unitySceneName = (string)field.GetValue( null );
                }
                else
                {
                    // If the scene type does not specify a scene name, we will create a new empty scene.
                }
            }

            Scene previousActiveScene = SceneManager.GetActiveScene();

            if( unitySceneName != null )
            {
                Debug.Log( $"Loading Unity scene '{unitySceneName}' as part of the HSP scene '{newSceneType.Name}'..." );

                AsyncOperation op = SceneManager.LoadSceneAsync( unitySceneName, new LoadSceneParameters( lm, lp ) );
                op.completed += ( x ) =>
                {
                    // When the load finishes, the unity scene gets set as 'active' automatically.
                    Scene newlyLoadedScene = SceneManager.GetSceneByName( unitySceneName );
                    if( asForeground )
                    {
                        if( _foregroundScene != null )
                        {
                            _foregroundScene._ondeactivate();
                        }
                        SceneManager.SetActiveScene( newlyLoadedScene );
                    }

                    MethodInfo method = newSceneType.GetMethod( "GetOrCreateSceneManagerInActiveScene", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy );
                    IHSPScene newScene = (IHSPScene)method.Invoke( null, new object[] { newlyLoadedScene } );
                    _loadedScenes.Add( newScene );
                    newScene._onload();

                    if( asForeground )
                    {
                        _foregroundScene = newScene;
                        _foregroundScene._onactivate();
                    }
                };

                // Wait until the asynchronous scene fully loads
                while( !op.isDone )
                {
                    yield return null;
                }
            }
            else
            {
                Debug.Log( $"Creating a new Unity scene '{newSceneType.Name}' as part of the HSP scene '{newSceneType.Name}'..." );

                Scene newlyLoadedScene = SceneManager.CreateScene( newSceneType.Name, new CreateSceneParameters( lp ) );
                if( asForeground )
                {
                    if( _foregroundScene != null )
                    {
                        _foregroundScene._ondeactivate();
                    }
                    SceneManager.SetActiveScene( newlyLoadedScene );
                }

                MethodInfo method = newSceneType.GetMethod( "GetOrCreateSceneManagerInActiveScene", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy );
                IHSPScene newScene = (IHSPScene)method.Invoke( null, new object[] { newlyLoadedScene } );
                _loadedScenes.Add( newScene );
                newScene._onload();

                if( asForeground )
                {
                    _foregroundScene = newScene;
                    _foregroundScene._onactivate();
                }
            }

            onAfterLoaded?.Invoke();
        }

        private static IEnumerator UnloadCoroutine( IHSPScene scene, Action onAfterUnloaded )
        {
            Scene unityScene = scene.UnityScene;

            Debug.Log( $"Unloading Unity scene '{unityScene.name}' as part of the HSP scene '{scene.GetType().Name}'..." );

            AsyncOperation op = SceneManager.UnloadSceneAsync( unityScene );
            if( _foregroundScene == scene )
            {
                _foregroundScene._ondeactivate();
                _foregroundScene = null;
            }
            SceneManager.SetActiveScene( AlwaysLoadedScene.Instance.UnityScene );

            // Wait until the asynchronous scene fully loads
            while( !op.isDone )
            {
                yield return null;
            }

            _loadedScenes.Remove( scene );
            if( _foregroundScene == scene )
            {
                _foregroundScene = null;
            }
            scene._onunload();

            onAfterUnloaded?.Invoke();
        }
    }
}