using HSP.Effects.Audio;
using HSP.Effects.Particles;
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
        public const string ADD_AUDIO_EFFECT_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_audio_effect_manager";
        public const string ADD_PARTICLE_EFFECT_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_particle_effect_manager";
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

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, ADD_AUDIO_EFFECT_MANAGER )]
        private static void AddAudioEffectManager()
        {
            AlwaysLoadedManager.Instance.gameObject.AddComponent<AudioEffectManager>();
        }

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, ADD_PARTICLE_EFFECT_MANAGER )]
        private static void AddParticleEffectManager()
        {
            AlwaysLoadedManager.Instance.gameObject.AddComponent<ParticleEffectManager>();
        }

        [HSPEventListener( HSPEvent_STARTUP_EARLY.ID, LOAD_MAIN_MENU )]
        private static void LoadMainMenu()
        {
            HSPSceneLoader.LoadSceneAsync<MainMenuSceneManager>();
        }
    }
}