using HSP.SceneManagement;
using UnityEngine;

namespace HSP.Vanilla.Scenes.GameplayScene
{
    /// <summary>
    /// Invoked immediately after loading the gameplay scene.
    /// </summary>
    public static class HSPEvent_SCENELOAD_GAMEPLAY
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".gameplayscene.load";
    }

    /// <summary>
    /// Invoked immediately before unloading the gameplay scene.
    /// </summary>
    public static class HSPEvent_SCENEUNLOAD_GAMEPLAY
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".gameplayscene.unload";
    }
    
    /// <summary>
    /// Invoked immediately after the gameplay scene becomes the foreground scene.
    /// </summary>
    public static class HSPEvent_SCENEACTIVATE_GAMEPLAY
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".gameplayscene.activate";
    }

    /// <summary>
    /// Invoked immediately before the gameplay scene stops being the foreground scene.
    /// </summary>
    public static class HSPEvent_SCENEDEACTIVATE_GAMEPLAY
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".gameplayscene.deactivate";
    }

    /// <summary>
    /// A Manager whose responsibility is to invoke the events relating to creation/destruction of the `gameplay` scene.
    /// </summary>
    public sealed class GameplaySceneManager : HSPSceneManager<GameplaySceneManager>
    {
        public static new string UNITY_SCENE_NAME => "Testing And Shit"; // TODO - swap out for "Gameplay" when the part with creating and loading rockets is done.

        public static GameplaySceneManager Instance => instance;
        public static GameObject GameObject => instance.gameObject;

        //void Awake()
        //{
        //    HSPEvent.EventManager.TryInvoke( HSPEvent_STARTUP_GAMEPLAY.ID );
        //}

        protected override void OnLoad()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_SCENELOAD_GAMEPLAY.ID );
        }

        protected override void OnUnload()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_SCENEUNLOAD_GAMEPLAY.ID );
        }

        protected override void OnActivate()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_SCENEACTIVATE_GAMEPLAY.ID );
        }

        protected override void OnDeactivate()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_SCENEDEACTIVATE_GAMEPLAY.ID );
        }
    }
}