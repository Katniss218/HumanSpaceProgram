using HSP.Settings;
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
    /// <typeparam name="TSettingsPage">The type of the settings page being displayed.</typeparam>
    public abstract class UISettingsPage<TSettingsPage> : UIPanel where TSettingsPage : ISettingsPage
    {
        protected UIRectMask contentPanel;

        /// <summary>
        /// The object that is referenced by this UI element.
        /// </summary>
        public TSettingsPage SettingsPage { get; private set; }

        /// <summary>
        /// Creates the core/base of the functionality panel.
        /// </summary>
        protected static T Create<T>( IUIElementContainer parent, TSettingsPage settingsPage ) where T : UISettingsPage<TSettingsPage>
        {
            T uiPage = UIPanel.Create<T>( parent, new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Top, 0, default ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/functionality_panel" ) );

            UIRectMask contentMask = uiPage.AddRectMask( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Top, -20, 15 ) );
            uiPage.SettingsPage = settingsPage;
            uiPage.contentPanel = contentMask;

            uiPage.LayoutDriver = new VerticalFitToSizeLayoutDriver()
            {
                MarginTop = 20f,
                TargetElement = contentMask
            };

            return uiPage;
        }
    }
}