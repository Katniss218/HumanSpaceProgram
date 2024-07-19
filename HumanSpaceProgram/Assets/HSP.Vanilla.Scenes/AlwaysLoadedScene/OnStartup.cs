using HSP.Content.AssetLoaders;
using HSP.Content.Vessels.AssetLoaders;
using HSP.Input;
using HSP.ScreenCapturing;
using HSP.Time;
using HSP.Timelines;
using UnityPlus.Input;

namespace HSP.Vanilla.Scenes.AlwaysLoadedScene
{
    public static class OnStartup
    {
        public const string BIND_INPUT_CHANNELS = HSPEvent.NAMESPACE_HSP + ".bind_input_channels";

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, BIND_INPUT_CHANNELS )]
        private static void BindInputs()
        {
            HierarchicalInputChannel.BindInputs();
        }


        public const string ADD_TIME_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_time_manager";
        public const string ADD_TIMELINE_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_timeline_manager";
        public const string ADD_SCREENSHOT_MANAGER = HSPEvent.NAMESPACE_HSP + ".add_screenshot_manager";


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
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.COMMON_SCREENSHOT, HierarchicalInputPriority.MEDIUM, ( _ ) => sm.TakeScreenshot() );
        }

        public const string RELOAD_TEXTURES = HSPEvent.NAMESPACE_HSP + ".reload_textures";
        public const string RELOAD_PARTS = HSPEvent.NAMESPACE_HSP + ".reload_parts";
        public const string RELOAD_VESSELS = HSPEvent.NAMESPACE_HSP + ".reload_vessels";

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, RELOAD_TEXTURES )]
        public static void ReloadTextures()
        {
            GameDataTextureLoader.ReloadTextures();
        }

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, RELOAD_PARTS )]
        public static void ReloadParts()
        {
            GameDataJsonPartFactory.ReloadParts();
        }

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, RELOAD_VESSELS )]
        public static void ReloadVessels()
        {
            GameDataJsonVesselFactory.ReloadVesselsAsParts();
        }
    }
}