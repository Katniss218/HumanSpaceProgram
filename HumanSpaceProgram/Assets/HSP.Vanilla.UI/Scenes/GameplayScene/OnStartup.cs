using HSP.Vanilla.Scenes.GameplayScene;
using HSP.Vanilla.UI.Components;

namespace HSP.Vanilla.UI.Scenes.GameplayScene
{
    public static class OnStartup
    {
        public const string ADD_VIEWPORT_CLICK_CONTROLLER = HSPEvent.NAMESPACE_HSP + ".add_click_controller";
        public const string ADD_NAVBALL_RENDER_TEXTURE_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_navball_render_texture_manager";

        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_LOAD.ID, ADD_VIEWPORT_CLICK_CONTROLLER )]
        private static void AddViewportClickController()
        {
            Vanilla.Scenes.GameplayScene.GameplaySceneM.Instance.gameObject.AddComponent<GameplayViewportClickController>();
        }

        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_LOAD.ID, ADD_NAVBALL_RENDER_TEXTURE_MANAGER )]
        private static void AddNavballRenderTextureManager()
        {
            var manager = Vanilla.Scenes.GameplayScene.GameplaySceneM.GameObject.AddComponent<NavballRenderTextureManager>();

            NavballRenderTextureManager.ResetAttitudeIndicatorRT();
            NavballRenderTextureManager.CreateNavball();
            NavballRenderTextureManager.CreateNavballCamera();
        }
    }
}