using HSP.SceneManagement;
using HSP.Timelines;
using HSP.UI.Windows;
using HSP.Vanilla.Scenes.GameplayScene;
using HSP.Vanilla.Scenes.MainMenuScene;
using HSP.Vanilla.UI.Components;
using System.Linq;
using UnityEngine;
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

        public const string SHOW_GAMEPLAY_LOAD_MESSAGE = HSPEvent.NAMESPACE_HSP + ".4be3224b-466f-48a5-8639-7dc572fe7cee";

        [HSPEventListener( HSPEvent_ON_TIMELINE_LOAD_ERROR.ID, SHOW_GAMEPLAY_LOAD_MESSAGE )]
        private static void error( HSPEvent_ON_TIMELINE_LOAD_ERROR.EventData e )
        {
            HSPSceneManager.ReplaceForegroundScene<MainMenuSceneM>( onAfterLoaded: () =>
            {
                UICanvas canvas = MainMenuSceneM.Instance.GetStaticCanvas();
                var errorMessages = e.data.GetMessages( LogType.Error );
                canvas.AddConfirmWindow( "Error", $"An error occurred while loading the timeline:\n\n{errorMessages.FirstOrDefault()}\n See the log file for the full list.", null );
            } );
        }

        [HSPEventListener( HSPEvent_ON_TIMELINE_LOAD_SUCCESS.ID, SHOW_GAMEPLAY_LOAD_MESSAGE )]
        private static void success( HSPEvent_ON_TIMELINE_LOAD_SUCCESS.EventData e )
        {
            var infoMessages = e.data.GetMessages( LogType.Log );
            var warningMessages = e.data.GetMessages( LogType.Warning );
            if( infoMessages.Any() || warningMessages.Any() )
            {
                UICanvas canvas = HSPSceneManager.ForegroundScene.GetStaticCanvas();
                canvas.AddConfirmWindow( "Info", $"A number of warnings/infos occurred:\n\n{infoMessages.First()}\n", null );
            }
        }
    }
}