using HSP.Vanilla.Settings;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Settings
{
    /// <summary>
    /// A base class for all settings page UIs.
    /// </summary>
    public abstract class UISettingsPage_Scenario : UISettingsPage<SettingsPage_Scenario>
    {
        /// <summary>
        /// Creates the core/base of the functionality panel.
        /// </summary>
        public static new T Create<T>( IUIElementContainer parent, SettingsPage_Scenario settingsPage ) where T : UISettingsPage_Scenario
        {
            return UISettingsPage<SettingsPage_Scenario>.Create<T>( parent, settingsPage );
        }
    }
}