using HSP.SceneManagement;
using UnityEngine;

namespace HSP.Vanilla.Scenes.GameplayScene
{
    /// <summary>
    /// Invoked immediately after loading the gameplay scene.
    /// </summary>
    public static class HSPEvent_STARTUP_GAMEPLAY
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".startup.gameplay";
    }

    /// <summary>
    /// A Manager whose responsibility is to invoke the events relating to creation/destruction of the `gameplay` scene.
    /// </summary>
    public class GameplaySceneManager : SceneManager<GameplaySceneManager>
    {
        public override string UNITY_SCENE_NAME => "Testing And Shit"; // TODO - swap out for "Gameplay" when the part with creating and loading rockets is done.

        public static GameplaySceneManager Instance => instance;
        public static GameObject GameObject => instance.gameObject;

        void Awake()
        {
            //HSPEvent.EventManager.TryInvoke( HSPEvent_STARTUP_GAMEPLAY.ID );
        }

        protected override void OnLoad()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_STARTUP_GAMEPLAY.ID );
        }

        protected override void OnUnload()
        {
        }

        protected override void OnActivate()
        {
        }

        protected override void OnDeactivate()
        {
        }
    }
}