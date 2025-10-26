using HSP.SceneManagement;
using HSP.Timelines;
using HSP.UI.Windows;
using HSP.Vanilla.Scenes.GameplayScene;
using HSP.Vanilla.Scenes.MainMenuScene;
using HSP.Vanilla.UI.Components;
using UnityPlus.UILib.UIElements;

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

        [HSPEventListener( HSPEvent_ON_TIMELINE_LOAD_ERROR.ID, "efs" )]
        private static void err( HSPEvent_ON_TIMELINE_LOAD_ERROR.EventData e )
        {
            // load mainmenu.
            // show popup when loaded.

#warning TODO - something to do with scene management, gameplay scene is unloaded too soon and doesn't unload fully (while lod spheres are building and camera is being created).
            // 'there is no foreground scene' means that the scene loading has not finished yet and we're trying to unload it.

            HSPSceneManager.ReplaceForegroundScene<MainMenuSceneM>( onAfterLoaded: () =>
            {
                UICanvas canvas = MainMenuSceneM.Instance.GetStaticCanvas();
                canvas.AddConfirmWindow( "Error", $"An error occurred while loading the timeline:\n\n{e.data.HasErrors}\n\nReturning to Main Menu.", null );
            } );
        }
    }
}