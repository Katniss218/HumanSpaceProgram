using HSP.UI;
using HSP.Vanilla.Components;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.UILib;
using UnityPlus.UILib.Layout;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Components
{
    public class UISequenceElement : UIPanel
    {
        // dropdown that selects the sequencer at the top.

        // dropdown position clamped to the top of the screen.

        private SequenceElement _sequenceElement;

        private IUIElementContainer _list;
        private UIText _uiHeader;

        public string Header { get => _uiHeader.Text; set => _uiHeader.Text = value; }

        private void RefreshActions()
        {
            foreach( var child in _list.Children.ToArray() )
            {
                child.Destroy();
            }

            foreach( var action in _sequenceElement.Actions )
            {
                _list.AddSequenceAction( new UILayoutInfo( UIAnchor.TopLeft, 0, (20, 20) ) );
            }

            UILayoutManager.ForceLayoutUpdate( this );
        }

        protected internal static T Create<T>( IUIElementContainer parent, UILayoutInfo layout, SequenceElement element, string header ) where T : UISequenceElement
        {
            T uiSequenceElement = UIPanel.Create<T>( parent, layout, AssetRegistry.Get<Sprite>( "builtin::Resources/Sprites/UI/sequencer_element" ) );

            UIText headerText = uiSequenceElement.AddStdText( new UILayoutInfo( UIFill.Horizontal(), UIAnchor.Top, 0, 20 ), header );

            UIPanel list = uiSequenceElement.AddPanel( new UILayoutInfo( UIFill.Horizontal( 3, 3 ), UIAnchor.Top, -22, 20 ), null );

            list.LayoutDriver = new BidirectionalLayoutDriver()
            {
                FreeAxis = BidirectionalLayoutDriver.Axis2D.Y,
                FitToSize = true,
                DirX = BidirectionalLayoutDriver.DirectionX.LeftToRight,
                DirY = BidirectionalLayoutDriver.DirectionY.TopToBottom,
                Spacing = new Vector2( 4f, 4f )
            };

            uiSequenceElement.LayoutDriver = new VerticalFitToSizeLayoutDriver() // kinda janky. the entire layout system needs improvement tbh.
            {
                TargetElement = list,
                MarginTop = 22,
                MarginBottom = 3,
            };

            uiSequenceElement._list = list;
            uiSequenceElement._uiHeader = headerText;
            uiSequenceElement._sequenceElement = element;

            uiSequenceElement.RefreshActions();

            return uiSequenceElement;
        }
    }

    public static class UISequenceElement_Ex
    {
        public static UISequenceElement AddSequenceElement( this IUIElementContainer parent, UILayoutInfo layout, SequenceElement element, string header )
        {
            return UISequenceElement.Create<UISequenceElement>( parent, layout, element, header );
        }
    }
}