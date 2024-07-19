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
        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, HSPEvent.NAMESPACE_HSP + ".bind_input_channels" )]
        private static void BindInputs()
        {
            HierarchicalInputChannel.BindInputs();
        }

        
        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, HSPEvent.NAMESPACE_HSP + ".add_time_manager" )]
        private static void AddTimeManager()
        {
            AlwaysLoadedManager.Instance.gameObject.AddComponent<TimeManager>();
        }

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, HSPEvent.NAMESPACE_HSP + ".add_timeline_manager" )]
        private static void AddTimelineManager()
        {
            AlwaysLoadedManager.Instance.gameObject.AddComponent<TimelineManager>();
        }

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, HSPEvent.NAMESPACE_HSP + ".add_screenshot_manager" )]
        private static void AddScreenshotManager()
        {
            ScreenshotManager sm = AlwaysLoadedManager.GameObject.AddComponent<ScreenshotManager>();
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.COMMON_SCREENSHOT, HierarchicalInputPriority.MEDIUM, ( _ ) => sm.TakeScreenshot() );
        }

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, HSPEvent.NAMESPACE_HSP + ".reload_textures" )]
        public static void ReloadTextures()
        {
            GameDataTextureLoader.ReloadTextures();
        }

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, HSPEvent.NAMESPACE_HSP + ".reload_parts" )]
        public static void ReloadParts()
        {
            GameDataJsonPartFactory.ReloadParts();
        }

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, HSPEvent.NAMESPACE_HSP + ".reload_vessels" )]
        public static void ReloadVessels()
        {
            GameDataJsonVesselFactory.ReloadVesselsAsParts();
        }
    }
}