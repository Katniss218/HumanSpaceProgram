using HSP.Vanilla.Scenes.GameplayScene;
using HSP.Vanilla.UI.Components;

namespace HSP.Vanilla.UI.Scenes.GameplayScene
{
    public static class OnStartup
    {
        public const string ADD_NAVBALL_RENDER_TEXTURE_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_navball_render_texture_manager";

        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_LOAD.ID, ADD_NAVBALL_RENDER_TEXTURE_MANAGER )]
        private static void AddNavballRenderTextureManager()
        {
            var manager = GameplaySceneM.GameObject.AddComponent<NavballRenderTextureManager>();

            NavballRenderTextureManager.ResetAttitudeIndicatorRT();
            NavballRenderTextureManager.CreateNavball();
            NavballRenderTextureManager.CreateNavballCamera();
        }
    }
}