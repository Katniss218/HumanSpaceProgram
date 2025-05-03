using HSP.UI;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.Layout;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Components
{
    /// <summary>
    /// A base class for all components that are displayed in the <see cref="UIPartWindow"/>.
    /// </summary>
    /// <typeparam name="TComponent">The type of the component being displayed.</typeparam>
    public abstract class UIPartWindowComponent<TComponent> : UIPanel
    {
        protected UIRectMask contentPanel;

        private UIText _title;

        private UIButton _toggleButton;

        /// <summary>
        /// The object that is referenced by this UI element.
        /// </summary>
        public TComponent ReferenceComponent { get; protected set; }

        public string Title { get => _title.Text; set => _title.Text = value; }

        public float OpenHeight { get; set; }

        private bool _isOpen = true;

        private void ToggleOpen()
        {
            var size = contentPanel.rectTransform.sizeDelta;
            if( _isOpen )
            {
                size.y = 0f;
                _toggleButton.Background = AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_down" );
            }
            else
            {
                size.y = OpenHeight;
                _toggleButton.Background = AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_up" );
            }
            contentPanel.rectTransform.sizeDelta = size;
            _isOpen = !_isOpen;
            UILayoutManager.ForceLayoutUpdate( contentPanel );
        }

        /// <summary>
        /// Creates the core/base of the functionality panel.
        /// </summary>
        protected static T Create<T>( IUIElementContainer parent, TComponent referenceComponent ) where T : UIPartWindowComponent<TComponent>
        {
            T uiPaw = UIPanel.Create<T>( parent, new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Top, 0, default ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/functionality_panel" ) );

            UIText title = uiPaw.AddStdText( new UILayoutInfo( UIFill.Horizontal( 25, 60 ), UIAnchor.Top, 0, 15 ), "" );

            UIButton collapseToggleButton = uiPaw.AddButton( new UILayoutInfo( UIAnchor.TopRight, (-20, 0), (15, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_up" ), uiPaw.ToggleOpen );
            
            UIRectMask contentMask = uiPaw.AddRectMask( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Top, -20, 15 ) );
            uiPaw.ReferenceComponent = referenceComponent;
            uiPaw.contentPanel = contentMask;
            uiPaw._title = title;
            uiPaw._toggleButton = collapseToggleButton;

            uiPaw.LayoutDriver = new VerticalFitToSizeLayoutDriver()
            {
                MarginTop = 20f,
                TargetElement = contentMask
            };

            return uiPaw;
        }
    }
}