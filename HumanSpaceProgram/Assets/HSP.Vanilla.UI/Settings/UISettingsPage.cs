using HSP.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.Layout;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Settings
{
    public interface IUISettingsPage
    {
        /// <summary>
        /// Applies the settings of the settings page associated with this UI.
        /// </summary>
        void Apply();


        //

        private static Dictionary<Type, MethodInfo> _factoryCache = new();

        /// <summary>
        /// Creates an <see cref="IUISettingsPage"/> for the given settings page instance.
        /// </summary>
        /// <param name="parent">The UI element to use as the parent.</param>
        /// <param name="page">The settings page to create the UI for.</param>
        /// <returns>The instantiated UI element.</returns>
        /// <exception cref="InvalidOperationException">Thrown when an appropriate factory method could not be found.</exception>
        public static IUISettingsPage Create( IUIElementContainer parent, ISettingsPage page )
        {
            // find the UI for the page using reflection
            Type pageType = page.GetType();
            if( !_factoryCache.TryGetValue( page.GetType(), out MethodInfo factoryMethod ) )
            {
                Type uiBaseType = typeof( UISettingsPage<> ).MakeGenericType( pageType );

                IEnumerable<Type> uiTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany( a => a.GetTypes() )
                    .Where( t => !t.IsAbstract && uiBaseType.IsAssignableFrom( t ) );

                Type uiType = uiTypes.FirstOrDefault();
                if( uiType == null )
                {
                    throw new InvalidOperationException( $"No UI found for the settings page '{pageType.Name}'." );
                }
                if( uiTypes.Count() > 1 )
                {
                    throw new InvalidOperationException( $"Multiple UIs found for the settings page '{pageType.Name}'." );
                }

                // Finds the standard 'Create' factory method and uses that.

                factoryMethod = uiType.GetMethod( "Create", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static );
                factoryMethod = factoryMethod.MakeGenericMethod( uiType );
                _factoryCache.Add( pageType, factoryMethod );
            }

            return (IUISettingsPage)factoryMethod.Invoke( null, new object[] { parent, page } );
        }
    }

    /// <summary>
    /// A base class for all settings page UIs.
    /// </summary>
    /// <typeparam name="TSettingsPage">The type of the settings page being displayed.</typeparam>
    public abstract class UISettingsPage<TSettingsPage> : UIPanel, IUISettingsPage where TSettingsPage : ISettingsPage
    {
        /// <summary>
        /// The object that is referenced by this UI element.
        /// </summary>
        public TSettingsPage SettingsPage { get; private set; }

        protected UIRectMask contentPanel;

        public void Apply()
        {
            SettingsPage.Apply();
        }

        /// <summary>
        /// Creates the core/base of the functionality panel.
        /// </summary>
        protected static T Create<T>( IUIElementContainer parent, float height, TSettingsPage settingsPage ) where T : UISettingsPage<TSettingsPage>
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