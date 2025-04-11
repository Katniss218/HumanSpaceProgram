using HSP.Vanilla.Settings;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Settings
{
    /// <summary>
    /// A base class for all settings page UIs.
    /// </summary>
    public abstract class UISettingsPage_Construction : UISettingsPage<SettingsPage_Construction>
    {
        /// <summary>
        /// Creates the core/base of the functionality panel.
        /// </summary>
        public static new T Create<T>( IUIElementContainer parent, SettingsPage_Construction settingsPage ) where T : UISettingsPage_Construction
        {
            return UISettingsPage<SettingsPage_Construction>.Create<T>( parent, settingsPage );
        }
    }
}