using HSP.Settings;
using HSP.Timelines;
using HSP.Vanilla.Scenes.AlwaysLoadedScene;

namespace HSP.Vanilla.Settings
{
    public class ReloadSettingsListener
    {
        public const string RELOAD_SETTINGS = HSPEvent.NAMESPACE_HSP + ".settings.load";

        [HSPEventListener( HSPEvent_STARTUP_EARLY.ID, RELOAD_SETTINGS )]
        private static void ReloadSettings_OnStartup()
        {
            SettingsManager.ReloadSettings();
        }

        [HSPEventListener( HSPEvent_AFTER_TIMELINE_NEW.ID, RELOAD_SETTINGS )]
        [HSPEventListener( HSPEvent_AFTER_TIMELINE_LOAD.ID, RELOAD_SETTINGS )]
        private static void ReloadSettings_OnTimelineStart()
        {
            SettingsManager.ReloadSettings();
        }
    }
}