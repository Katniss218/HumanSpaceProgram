using HSP.Settings;
using HSP.Timelines;
using HSP.Vanilla.Scenes.AlwaysLoadedScene;

namespace HSP.Vanilla.Settings
{
    public class ReloadSettingsListener
    {
        public const string RELOAD_SETTINGS = HSPEvent.NAMESPACE_HSP + ".settings.load";

        // Reload the settings when the providers become active or start using a different source.
        [HSPEventListener( HSPEvent_STARTUP_EARLY.ID, RELOAD_SETTINGS )]
        [HSPEventListener( HSPEvent_AFTER_TIMELINE_NEW.ID, RELOAD_SETTINGS )]
        [HSPEventListener( HSPEvent_AFTER_TIMELINE_LOAD.ID, RELOAD_SETTINGS )]
        private static void ReloadSettings_OnStartup()
        {
            SettingsManager.ReloadSettings();
        }
    }
}