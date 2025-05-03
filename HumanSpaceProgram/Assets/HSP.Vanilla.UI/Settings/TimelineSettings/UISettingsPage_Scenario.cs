using HSP.Vanilla.Settings;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.Layout;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Settings
{
    /// <summary>
    /// A base class for all settings page UIs.
    /// </summary>
    public class UISettingsPage_Construction : UISettingsPage<SettingsPage_Construction>
    {
        /// <summary>
        /// Creates the core/base of the functionality panel.
        /// </summary>
        public static T Create<T>( IUIElementContainer parent, SettingsPage_Construction settingsPage ) where T : UISettingsPage_Construction
        {
            var ui = UISettingsPage<SettingsPage_Construction>.Create<T>( parent, 100, settingsPage );

            var inf = ui.AddFloatInputField( new UILayoutInfo( UIAnchor.TopRight, (0, 0), (100, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/input_field" ) );
            inf.SetValue( settingsPage.ConstructionSpeedMultiplier );
            inf.OnValueChanged += ( e ) => settingsPage.ConstructionSpeedMultiplier = e.NewValue;

            ui.AddText( new UILayoutInfo( UIFill.Horizontal( 0, 105 ), UIAnchor.Top, 0, 15 ), "Construction speed multiplier" );

            return ui;
        }
    }
}