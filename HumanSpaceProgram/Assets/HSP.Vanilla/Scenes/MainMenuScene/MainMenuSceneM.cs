using HSP.SceneManagement;
using UnityEngine;

namespace HSP.Vanilla.Scenes.MainMenuScene
{
    /// <summary>
    /// Invoked immediately after loading the main menu scene.
    /// </summary>
    public static class HSPEvent_MAIN_MENU_SCENE_LOAD
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".mainmenu_scene.load";
    }

    /// <summary>
    /// Invoked immediately before unloading the main menu scene.
    /// </summary>
    public static class HSPEvent_MAIN_MENU_SCENE_UNLOAD
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".mainmenu_scene.unload";
    }

    /// <summary>
    /// Invoked immediately after the main menu scene becomes the foreground scene.
    /// </summary>
    public static class HSPEvent_MAIN_MENU_SCENE_ACTIVATE
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".mainmenu_scene.activate";
    }

    /// <summary>
    /// Invoked immediately before the main menu scene stops being the foreground scene.
    /// </summary>
    public static class HSPEvent_MAIN_MENU_SCENE_DEACTIVATE
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".mainmenu_scene.deactivate";
    }

    /// <summary>
    /// A Manager whose responsibility is to invoke the events relating to creation/destruction of the `main menu` scene.
    /// </summary>
    public sealed class MainMenuSceneM : HSPScene<MainMenuSceneM>
    {
        public static new string UNITY_SCENE_NAME => "MainMenu";

        public static MainMenuSceneM Instance => instance;
        public static GameObject GameObject => instance.gameObject;

        protected override void OnActivate()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_MAIN_MENU_SCENE_LOAD.ID );
        }

        protected override void OnDeactivate()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_MAIN_MENU_SCENE_UNLOAD.ID );
        }

        protected override void OnLoad()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_MAIN_MENU_SCENE_ACTIVATE.ID );
        }

        protected override void OnUnload()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_MAIN_MENU_SCENE_DEACTIVATE.ID );
        }
    }
}