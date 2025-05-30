using HSP.Audio;
using HSP.Input;
using HSP.SceneManagement;
using HSP.ScreenCapturing;
using HSP.Time;
using HSP.Timelines;
using HSP.Vanilla.Scenes.MainMenuScene;
using UnityPlus.Input;

namespace HSP.Vanilla.Scenes.AlwaysLoadedScene
{
    public static class OnStartup
    {
        public const string ADD_TIME_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_time_manager";
        public const string ADD_TIMELINE_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_timeline_manager";
        public const string ADD_SCREENSHOT_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_screenshot_manager";
        public const string ADD_AUDIO_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_audio_manager";
        public const string ADD_AUDIO_EFFECT_PLAYER = HSPEvent.NAMESPACE_HSP + ".add_audio_effect_player";
        public const string LOAD_MAIN_MENU = HSPEvent.NAMESPACE_HSP + ".load_main_menu";

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, ADD_TIME_MANAGER )]
        private static void AddTimeManager()
        {
            AlwaysLoadedManager.Instance.gameObject.AddComponent<TimeManager>();
        }

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, ADD_TIMELINE_MANAGER )]
        private static void AddTimelineManager()
        {
            AlwaysLoadedManager.Instance.gameObject.AddComponent<TimelineManager>();
        }

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, ADD_SCREENSHOT_MANAGER )]
        private static void AddScreenshotManager()
        {
            ScreenshotManager sm = AlwaysLoadedManager.GameObject.AddComponent<ScreenshotManager>();
            HierarchicalInputManager.AddAction( InputChannel.SCREENSHOT, InputChannelPriority.MEDIUM, ( _ ) => sm.TakeScreenshot() );
        }

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, ADD_AUDIO_MANAGER )]
        private static void AddAudioManager()
        {
            AlwaysLoadedManager.Instance.gameObject.AddComponent<AudioManager>();
        }
        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, ADD_AUDIO_EFFECT_PLAYER )]
        private static void AddAudioEffectPlayer()
        {
            AlwaysLoadedManager.Instance.gameObject.AddComponent<AudioEffectPlayer>();
        }
        
        [HSPEventListener( HSPEvent_STARTUP_EARLY.ID, LOAD_MAIN_MENU )]
        private static void LoadMainMenu()
        {
            SceneLoader.LoadSceneAsync( MainMenuSceneManager.SCENE_NAME, true, false, null );
        }
    }
}