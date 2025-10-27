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
        private static List<IHSPScene> _loadedScenes = new();

        private static HashSet<Type> _loadingScenes = new();
        private static HashSet<Type> _unloadingScenes = new();

        private sealed class PendingLoad
        {
            public object LoadData;
            public bool AsForeground;
            public Action OnAfterLoaded;

            public PendingLoad( object loadData, bool asForeground, Action onAfterLoaded )
            {
                LoadData = loadData;
                AsForeground = asForeground;
                OnAfterLoaded = onAfterLoaded;
            }
        }

        private sealed class PendingUnload
        {
            public Action OnAfterUnloaded;

            public PendingUnload( Action onAfterUnloaded )
            {
                OnAfterUnloaded = onAfterUnloaded;
            }
        }

        private static Dictionary<Type, PendingLoad> _pendingLoads = new();
        private static Dictionary<Type, PendingUnload> _pendingUnloads = new();

        private static IHSPScene _foregroundScene = null;

        /// <summary>
        /// Moves the specified GameObject to the given HSP scene. <br/>
        /// The scene must currently be loaded.
        /// </summary>
        /// <typeparam name="TLoadedScene">The type specifying the scene to use.</typeparam>
        public static void MoveGameObjectToScene<TLoadedScene>( GameObject gameObject ) where TLoadedScene : IHSPScene
        {
            IHSPScene scene = _loadedScenes.FirstOrDefault( s => s.GetType() == typeof( TLoadedScene ) );
            if( scene == null )
                throw new InvalidOperationException( $"The scene '{typeof( TLoadedScene ).Name}' is not loaded." );

            SceneManager.MoveGameObjectToScene( gameObject, scene.UnityScene );
        }

        /// <summary>
        /// Moves the specified GameObject to the given HSP scene. <br/>
        /// The scene must currently be loaded.
        /// </summary>
        public static void MoveGameObjectToScene( GameObject gameObject, IHSPScene scene )
        {
            if( !scene.UnityScene.isLoaded )
                throw new InvalidOperationException( $"The scene '{scene.GetType().Name}' is not loaded." );

            SceneManager.MoveGameObjectToScene( gameObject, scene.UnityScene );
        }

        //
        //
        //

        /// <summary>
        /// Gets the HSP scene that the given GameObject a is part of.
        /// </summary>
        public static IHSPScene GetScene( GameObject gameObject )
        {
            if( gameObject == null )
            {
                throw new ArgumentNullException( nameof( gameObject ), "GameObject cannot be null." );
            }

            UnityEngine.SceneManagement.Scene scene = gameObject.scene;
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
        /// Gets all of the currently loaded HSP scenes.
        /// </summary>
        public static IEnumerable<IHSPScene> GetLoadedScenes()
        {
            return _loadedScenes;
        }

        /// <summary>
        /// Gets the Unity scene associated with the given HSP scene. <br/>
        /// The scene must currently be loaded.
        /// </summary>
        /// <typeparam name="TLoadedScene">The type specifying the scene to use.</typeparam>
        public static UnityEngine.SceneManagement.Scene UnityScene<TLoadedScene>() where TLoadedScene : IHSPScene
        {
            Type sceneType = typeof( TLoadedScene );
            IHSPScene scene = _loadedScenes.FirstOrDefault( s => s.GetType() == sceneType );
            if( scene == null )
                throw new InvalidOperationException( $"The scene '{sceneType.Name}' is not loaded." );

            return scene.UnityScene;
        }

        /// <summary>
        /// Checks if the given HSP scene is currently loaded.
        /// </summary>
        /// <typeparam name="TScene">The type specifying the scene to check.</typeparam>
        public static bool IsLoaded<TScene>() where TScene : IHSPScene
        {
            Type sceneType = typeof( TScene );
            return _loadedScenes.Any( s => s.GetType() == sceneType );
        }
        /// <summary>
        /// Checks if the given HSP scene is currently loaded.
        /// </summary>
        /// <typeparam name="TScene">The type specifying the scene to check.</typeparam>
        public static bool IsLoaded( IHSPScene scene )
        {
            Type sceneType = scene.GetType();
            return _loadedScenes.Any( s => s.GetType() == sceneType );
        }

        public static bool IsLoading<TScene>() where TScene : IHSPScene
        {
            return _loadingScenes.Contains( typeof( TScene ) );
        }

        public static bool IsLoading( IHSPScene scene )
        {
            return _loadingScenes.Contains( scene.GetType() );
        }

        public static bool IsUnloading<TScene>() where TScene : IHSPScene
        {
            return _unloadingScenes.Contains( typeof( TScene ) );
        }

        public static bool IsUnloading( IHSPScene scene )
        {
            return _unloadingScenes.Contains( scene.GetType() );
        }

        /// <summary>
        /// Gets the HSP scene that is currently the 'active' scene. <br/>
        /// The active Unity scene is also the scene that is backing this HSP scene.
        /// </summary>
        public static IHSPScene ForegroundScene => _foregroundScene;

        /// <summary>
        /// Checks if the given HSP scene is currently a loaded foreground scene.
        /// </summary>
        /// <typeparam name="TScene">The type specifying the scene to check.</typeparam>
        public static bool IsForeground<TScene>() where TScene : IHSPScene
        {
            Type sceneType = typeof( TScene );
            IHSPScene scene = _loadedScenes.FirstOrDefault( s => s.GetType() == sceneType );
            if( scene == null )
                return false;

            return scene == _foregroundScene;
        }

        /// <summary>
        /// Checks if the given HSP scene is currently a loaded background scene. <br/>
        /// </summary>
        /// <typeparam name="TScene">The type specifying the scene to check.</typeparam>
        public static bool IsBackground<TScene>() where TScene : IHSPScene
        {
            Type sceneType = typeof( TScene );
            IHSPScene scene = _loadedScenes.FirstOrDefault( s => s.GetType() == sceneType );
            if( scene == null )
                return false;

            return scene != _foregroundScene;
        }

        /// <summary>
        /// Sets the given loaded HSP scene as the foreground scene. <br/>
        /// </summary>
        /// <typeparam name="TLoadedScene">The type specifying the scene to set as foreground.</typeparam>
        public static void SetAsForeground<TLoadedScene>() where TLoadedScene : IHSPScene
        {
            Type sceneType = typeof( TLoadedScene );
            IHSPScene scene = _loadedScenes.FirstOrDefault( s => s.GetType() == sceneType );
            if( scene == null )
                throw new InvalidOperationException( $"Can't set the foreground scene to '{sceneType.Name}' that is not loaded." );

            if( scene == _foregroundScene )
                return;

            if( _foregroundScene != null )
            {
                Debug.Log( $"Setting the HSP scene '{sceneType.Name}' as foreground. The current foreground HSP scene '{_foregroundScene.GetType().Name}' will be set as background." );
                _foregroundScene._ondeactivate();
            }
            else
            {
                Debug.Log( $"Setting the HSP scene '{sceneType.Name}' as foreground." );
            }

            _foregroundScene = scene;
            SceneManager.SetActiveScene( _foregroundScene.UnityScene );
            _foregroundScene._onactivate();
        }

        /// <summary>
        /// Sets the given loaded HSP scene as a background scene. <br/>
        /// </summary>
        /// <remarks>
        /// Sets the always loaded Unity scene as the active scene.
        /// </remarks>
        /// <typeparam name="TLoadedScene">The type specifying the scene to set as background.</typeparam>
        public static void SetAsBackground<TLoadedScene>() where TLoadedScene : IHSPScene
        {
            Type sceneType = typeof( TLoadedScene );
            IHSPScene scene = _loadedScenes.FirstOrDefault( s => s.GetType() == sceneType );
            if( scene == null )
                throw new InvalidOperationException( $"Can't set the background scene to '{sceneType.Name}' that is not loaded." );

            if( scene != _foregroundScene ) // Scene is already background.
                return;

            Debug.Log( $"Setting the HSP scene '{sceneType.Name}' as background." );

            _foregroundScene._ondeactivate();
            _foregroundScene = null;
            SceneManager.SetActiveScene( AlwaysLoadedScene.Instance.UnityScene );
        }

        //
        //
        //

        /// <summary>
        /// Starts loading the given HSP scene asynchronously. The scene will be loaded as the foreground scene, deactivating the previous foreground scene if there is one. <br/>
        /// The scene to load must not currently be loaded.
        /// </summary>
        /// <remarks>
        /// The scene is loaded additively, meaning that it will not replace any currently loaded scenes. <br/>
        /// If you wish to do so, use <br/>
        /// - <see cref="UnloadAsync{TOld}"/> or <br/>
        /// - <see cref="ReplaceAsync{TOld, TNew}"/> instead.
        /// </remarks>
        /// <typeparam name="TNewScene">The type specifying the scene to load.</typeparam>
        public static void LoadAsync<TNewScene>( Action onAfterLoaded = null ) where TNewScene : IHSPScene
        {
            StartSceneLoadCoroutine( typeof( TNewScene ), null, true, onAfterLoaded );
        }

        /// <summary>
        /// Starts loading the given HSP scene asynchronously. The scene will be loaded as the foreground scene, deactivating the previous foreground scene if there is one. <br/>
        /// The scene to load must not currently be loaded.
        /// </summary>
        /// <remarks>
        /// The scene is loaded additively, meaning that it will not replace any currently loaded scenes. <br/>
        /// If you wish to do so, use <br/>
        /// - <see cref="UnloadAsync{TOld}"/> or <br/>
        /// - <see cref="ReplaceAsync{TOld, TNew}"/> instead.
        /// </remarks>
        /// <typeparam name="TNewScene">The type specifying the scene to load.</typeparam>
        public static void LoadAsync<TNewScene, TLoadData>( TLoadData loadData, Action onAfterLoaded = null ) where TNewScene : IHSPScene<TLoadData>
        {
            StartSceneLoadCoroutine( typeof( TNewScene ), loadData, true, onAfterLoaded );
        }

        /// <summary>
        /// Starts loading the given HSP scene asynchronously. The scene will be loaded as a background scene. <br/>
        /// The scene to load must not currently be loaded.
        /// </summary>
        /// <remarks>
        /// The scene is loaded additively, meaning that it will not replace the currently loaded scenes. <br/>
        /// If you wish to do so, use <br/>
        /// - <see cref="UnloadAsync{TOld}"/> or <br/>
        /// - <see cref="ReplaceAsync{TOld, TNew}"/> instead.
        /// </remarks>
        /// <typeparam name="TNewScene">The type specifying the scene to load.</typeparam>
        public static void LoadAsBackgroundAsync<TNewScene>( Action onAfterLoaded = null ) where TNewScene : IHSPScene
        {
            StartSceneLoadCoroutine( typeof( TNewScene ), null, false, onAfterLoaded );
        }

        /// <summary>
        /// Starts loading the given HSP scene asynchronously. The scene will be loaded as a background scene. <br/>
        /// The scene to load must not currently be loaded.
        /// </summary>
        /// <remarks>
        /// The scene is loaded additively, meaning that it will not replace the currently loaded scenes. <br/>
        /// If you wish to do so, use <br/>
        /// - <see cref="UnloadAsync{TOld}"/> or <br/>
        /// - <see cref="ReplaceAsync{TOld, TNew}"/> instead.
        /// </remarks>
        /// <typeparam name="TNewScene">The type specifying the scene to load.</typeparam>
        public static void LoadAsBackgroundAsync<TNewScene, TLoadData>( TLoadData loadData, Action onAfterLoaded = null ) where TNewScene : IHSPScene<TLoadData>
        {
            StartSceneLoadCoroutine( typeof( TNewScene ), loadData, false, onAfterLoaded );
        }

        /// <summary>
        /// Starts unloading a given HSP scene asynchronously. <br/>
        /// The scene to unload must currently be loaded.
        /// </summary>
        /// <typeparam name="TOldScene">The type specifying the scene to unload.</typeparam>
        public static void UnloadAsync<TOldScene>( Action onAfterUnloaded = null ) where TOldScene : IHSPScene
        {
            StartSceneUnloadCoroutine( typeof( TOldScene ), onAfterUnloaded );
        }

        /// <summary>
        /// Starts unloading a given HSP scene asynchronously. <br/>
        /// The scene to unload must currently be loaded.
        /// </summary>
        public static void UnloadAsync( IHSPScene scene, Action onAfterUnloaded = null )
        {
            StartSceneUnloadCoroutine( scene.GetType(), onAfterUnloaded );
        }

        /// <summary>
        /// Starts unloading the given HSP scene that is currently the foreground scene asynchronously.
        /// </summary>
        public static void UnloadForegroundSceneAsync( Action onAfterUnloaded = null )
        {
            if( _foregroundScene == null )
            {
                throw new InvalidOperationException( "There is currently no loaded foreground scene." );
            }

            StartSceneUnloadCoroutine( _foregroundScene.GetType(), onAfterUnloaded );
        }

        /// <summary>
        /// Starts unloading the given HSP scene, and then starts loading a new HSP scene asynchronously. <br/>
        /// The scene to unload must currently be loaded.
        /// </summary>
        /// <remarks>
        /// If the scene to be unloaded is currently set as foreground, the new scene replacing it will also be set as foreground.
        /// </remarks>
        /// <typeparam name="TOldScene">The type specifying the scene to unload.</typeparam>
        /// <typeparam name="TNewScene">The type specifying the scene to load.</typeparam>
        public static void ReplaceAsync<TOldScene, TNewScene>( Action onAfterUnloaded = null, Action onAfterLoaded = null ) where TOldScene : IHSPScene where TNewScene : IHSPScene
        {
            bool isForeground = IsForeground<TOldScene>();
            StartSceneUnloadCoroutine( typeof( TOldScene ), () =>
            {
                onAfterUnloaded?.Invoke();
                StartSceneLoadCoroutine( typeof( TNewScene ), null, isForeground, onAfterLoaded );
            } );
        }

        /// <summary>
        /// Starts unloading the given HSP scene, and then starts loading a new HSP scene asynchronously. <br/>
        /// The scene to unload must currently be loaded.
        /// </summary>
        /// <remarks>
        /// If the scene to be unloaded is currently set as foreground, the new scene replacing it will also be set as foreground.
        /// </remarks>
        /// <typeparam name="TOldScene">The type specifying the scene to unload.</typeparam>
        /// <typeparam name="TNewScene">The type specifying the scene to load.</typeparam>
        public static void ReplaceAsync<TOldScene, TNewScene, TLoadData>( TLoadData loadData, Action onAfterUnloaded = null, Action onAfterLoaded = null ) where TOldScene : IHSPScene where TNewScene : IHSPScene<TLoadData>
        {
            bool isForeground = IsForeground<TOldScene>();
            StartSceneUnloadCoroutine( typeof( TOldScene ), () =>
            {
                onAfterUnloaded?.Invoke();
                StartSceneLoadCoroutine( typeof( TNewScene ), loadData, isForeground, onAfterLoaded );
            } );
        }

        /// <summary>
        /// Starts unloading the given HSP scene that is currently the foreground scene, and then starts loading a new HSP scene asynchronously.
        /// </summary>
        /// <typeparam name="TNewScene">The type specifying the scene to load.</typeparam>
        public static void ReplaceForegroundScene<TNewScene>( Action onAfterUnloaded = null, Action onAfterLoaded = null ) where TNewScene : IHSPScene
        {
            if( _foregroundScene == null )
            {
                throw new InvalidOperationException( "There is currently no loaded foreground scene." );
            }

            StartSceneUnloadCoroutine( _foregroundScene.GetType(), () =>
            {
                onAfterUnloaded?.Invoke();
                StartSceneLoadCoroutine( typeof( TNewScene ), null, true, onAfterLoaded );
            } );
        }
        /// <summary>
        /// Starts unloading the given HSP scene that is currently the foreground scene, and then starts loading a new HSP scene asynchronously.
        /// </summary>
        /// <typeparam name="TNewScene">The type specifying the scene to load.</typeparam>
        public static void ReplaceForegroundScene<TNewScene, TLoadData>( TLoadData loadData, Action onAfterUnloaded = null, Action onAfterLoaded = null ) where TNewScene : IHSPScene<TLoadData>
        {
            if( _foregroundScene == null )
            {
                throw new InvalidOperationException( "There is currently no loaded foreground scene." );
            }

            StartSceneUnloadCoroutine( _foregroundScene.GetType(), () =>
            {
                onAfterUnloaded?.Invoke();
                StartSceneLoadCoroutine( typeof( TNewScene ), loadData, true, onAfterLoaded );
            } );
        }

        //
        //      COROUTINES BELOW
        //

        private static void StartSceneLoadCoroutine( Type sceneType, object loadData, bool asForeground, Action onAfterLoaded )
        {
            if( _loadedScenes.Any( s => s.GetType() == sceneType ) )
                throw new InvalidOperationException( $"Can't load the scene '{sceneType.Name}' that is already loaded." );

            if( _loadingScenes.Contains( sceneType ) )
                throw new InvalidOperationException( $"Can't load the scene '{sceneType.Name}' that is already loading." );

            // Queue for immediate load if that scene is still being unloaded (call came from within one of the scene unload callbacks).
            if( _unloadingScenes.Contains( sceneType ) )
            {
                if( _pendingLoads.ContainsKey( sceneType ) )
                    throw new InvalidOperationException( $"Load for scene '{sceneType.Name}' has already been queued while unloading." );

                _pendingLoads[sceneType] = new PendingLoad( loadData, asForeground, onAfterLoaded );
                return;
            }

            _loadingScenes.Add( sceneType );
            instance.StartCoroutine( LoadCoroutine( sceneType, loadData, asForeground, onAfterLoaded ) );
        }

        private static void StartSceneUnloadCoroutine( Type sceneType, Action onAfterUnloaded )
        {
            if( _unloadingScenes.Contains( sceneType ) )
                throw new InvalidOperationException( $"Can't unload the scene '{sceneType.Name}' that is already unloading." );

            // Queue for immediate unload if that scene is still being loaded (call came from within one of the scene load callbacks).
            if( _loadingScenes.Contains( sceneType ) )
            {
                if( _pendingUnloads.ContainsKey( sceneType ) )
                    throw new InvalidOperationException( $"Unload for scene '{sceneType.Name}' has already been queued while loading." );

                _pendingUnloads[sceneType] = new PendingUnload( onAfterUnloaded );
                return;
            }

            IHSPScene scene = _loadedScenes.FirstOrDefault( s => s.GetType() == sceneType );
            if( scene == null )
                throw new InvalidOperationException( $"Can't unload the scene '{sceneType.Name}' that is not loaded." );

            _unloadingScenes.Add( sceneType );
            instance.StartCoroutine( UnloadCoroutine( scene, onAfterUnloaded ) );
        }

        private static IEnumerator LoadCoroutine( Type newSceneType, object loadData, bool asForeground, Action onAfterLoaded )
        {
            bool implementsGenericIHSPScene = ImplementsGenericIHSPScene( newSceneType );
            if( implementsGenericIHSPScene && loadData == null )
            {
                throw new InvalidOperationException( $"The scene '{newSceneType.Name}' requires load data, but none was provided." );
            }

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

            try
            {
                if( unitySceneName != null )
                {
                    if( asForeground )
                        Debug.Log( $"Loading Unity scene '{unitySceneName}' as part of the HSP scene '{newSceneType.Name}' (loading as foreground)." );
                    else
                        Debug.Log( $"Loading Unity scene '{unitySceneName}' as part of the HSP scene '{newSceneType.Name}'." );

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
                        if( asForeground )
                        {
                            _foregroundScene = newScene;
                        }

                        if( implementsGenericIHSPScene )
                        {
                            //newScene._onload( loadData );
                            method = newSceneType.GetMethod( "_onload", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy, null, new Type[] { loadData.GetType() }, null );
                            method.Invoke( newScene, new object[] { loadData } );
                        }
                        else
                        {
                            newScene._onload();
                        }

                        if( asForeground )
                        {
                            newScene._onactivate();
                        }

                        onAfterLoaded?.Invoke();
                        _loadingScenes.Remove( newSceneType );

                        // Check for any pending unloads that were queued while loading
                        if( _pendingUnloads.TryGetValue( newSceneType, out var pendingUnload ) )
                        {
                            _pendingUnloads.Remove( newSceneType );
                            StartSceneUnloadCoroutine( newSceneType, pendingUnload.OnAfterUnloaded );
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
                    if( asForeground )
                        Debug.Log( $"Creating a new Unity scene '{newSceneType.Name}' as part of the HSP scene '{newSceneType.Name}' (loading as foreground)." );
                    else
                        Debug.Log( $"Creating a new Unity scene '{newSceneType.Name}' as part of the HSP scene '{newSceneType.Name}'." );

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
                    _loadingScenes.Remove( newSceneType );
                    _loadedScenes.Add( newScene );
                    if( asForeground )
                    {
                        _foregroundScene = newScene;
                    }

                    if( implementsGenericIHSPScene )
                    {
                        //newScene._onload( loadData );
                        method = newSceneType.GetMethod( "_onload", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy, null, new Type[] { loadData.GetType() }, null );
                        method.Invoke( newScene, new object[] { loadData } );
                    }
                    else
                    {
                        newScene._onload();
                    }

                    if( asForeground )
                    {
                        newScene._onactivate();
                    }

                    onAfterLoaded?.Invoke();
                    _loadingScenes.Remove( newSceneType );

                    // Check for any pending unloads that were queued while loading
                    if( _pendingUnloads.TryGetValue( newSceneType, out var pendingUnload ) )
                    {
                        _pendingUnloads.Remove( newSceneType );
                        StartSceneUnloadCoroutine( newSceneType, pendingUnload.OnAfterUnloaded );
                    }
                }
            }
            finally
            {
                _loadingScenes.Remove( newSceneType );
            }
        }

        private static IEnumerator UnloadCoroutine( IHSPScene scene, Action onAfterUnloaded )
        {
            Scene unityScene = scene.UnityScene;
            Type sceneType = scene.GetType();

            Debug.Log( $"Unloading Unity scene '{unityScene.name}' as part of the HSP scene '{scene.GetType().Name}'." );

            try
            {
                if( _foregroundScene == scene )
                {
                    _foregroundScene._ondeactivate();
                    _foregroundScene = null;
                }

                // Stops Unity calling `Start()` on gameobjects if we immediately unload the scene.
                // This is important, because someone might spawn more gameobjects inside Start, which will then leak into the wrong scene (they'll spawn in the always loaded scene).
                foreach( var gameObject in unityScene.GetRootGameObjects() )
                {
                    gameObject.SetActive( false );
                }

                SceneManager.SetActiveScene( AlwaysLoadedScene.Instance.UnityScene );
                scene._onunload();

                AsyncOperation op = SceneManager.UnloadSceneAsync( unityScene );

                // Wait until the asynchronous scene fully loads
                while( !op.isDone )
                {
                    yield return null;
                }

                _loadedScenes.Remove( scene );
                onAfterUnloaded?.Invoke();
                _unloadingScenes.Remove( sceneType );

                // Check for any pending unloads that were queued while loading
                if( _pendingLoads.TryGetValue( sceneType, out var pendingLoad ) )
                {
                    _pendingLoads.Remove( sceneType );
                    StartSceneLoadCoroutine( sceneType, pendingLoad.LoadData, pendingLoad.AsForeground, pendingLoad.OnAfterLoaded );
                }
            }
            finally
            {
                _unloadingScenes.Remove( sceneType );
            }
        }

        private static bool ImplementsGenericIHSPScene( Type type )
        {
            Type targetInterface = typeof( IHSPScene<> );
            Type[] interfaces = type.GetInterfaces();

            foreach( var iface in interfaces )
            {
                if( iface.IsGenericType )
                {
                    Type genericDef = iface.GetGenericTypeDefinition();
                    if( genericDef == targetInterface )
                        return true;
                }
            }

            return false;
        }
    }
}