using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.Layout;
using UnityPlus.UILib.UIElements;

namespace HSP.UI.Windows.PartWindowComponents
{
    public abstract class UIPartWindowComponent : UIPanel
    {
        protected UIRectMask contentPanel;

        private UIText _title;

        private UIButton _toggleButton;

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
        protected static T Create<T>( IUIElementContainer parent ) where T : UIPartWindowComponent
        {
            T uiPaw = UIPanel.Create<T>( parent, new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Top, 0, default ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/functionality_panel" ) );

            UIText title = uiPaw.AddStdText( new UILayoutInfo( UIFill.Horizontal( 25, 60 ), UIAnchor.Top, 0, 15 ), "" );

            UIButton collapseToggleButton = uiPaw.AddButton( new UILayoutInfo( UIAnchor.TopRight, (-20, 0), (15, 15) ), AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/button_up" ), uiPaw.ToggleOpen );
            
            UIRectMask contentMask = uiPaw.AddRectMask( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Top, -20, 15 ) );
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