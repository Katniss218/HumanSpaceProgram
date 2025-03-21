﻿using HSP.ReferenceFrames;
using HSP.Vanilla.Components;
using HSP.Vessels;
using UnityPlus.UILib;
using UnityPlus.UILib.Layout;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Components
{
    public class UISequencerPanel : UIPanel
    {
        // dropdown that selects the sequencer at the top.

        // dropdown position clamped to the top of the screen.

        private Vessel _activeVessel;

        private FSequencer _activeSequencer;

        private void RefreshVessel()
        {
            if( _activeSequencer != null )
                _activeSequencer.OnAfterInvoked -= RefreshSequencer;
            _activeSequencer = _activeVessel.gameObject.GetComponentInChildren<FSequencer>();
            if( _activeSequencer != null )
                _activeSequencer.OnAfterInvoked += RefreshSequencer;

            RefreshSequencer();
        }
        
        private void RefreshSequencer()
        {
            foreach( var seqElement in this.Children.ToArray() )
            {
                seqElement.Destroy();
            }

            int num = 0;
            if( _activeSequencer != null )
            {
                foreach( var element in _activeSequencer.Sequence.RemainingElements )
                {
                    this.AddSequenceElement( new UILayoutInfo( UIAnchor.BottomLeft, 0, (50, 100) ), element, $" #{num} " );
                    num++;
                }
            }

            UILayoutManager.ForceLayoutUpdate( this );
        }

        private void OnDestroy()
        {
            if( _activeSequencer != null )
                _activeSequencer.OnAfterInvoked -= RefreshSequencer;
        }

        protected internal static T Create<T>( IUIElementContainer parent, UILayoutInfo layout ) where T : UISequencerPanel
        {
            T uiSequencerPanel = UIPanel.Create<T>( parent, layout, null );

            uiSequencerPanel._activeVessel = ActiveVesselManager.ActiveObject.GetVessel();

            uiSequencerPanel.LayoutDriver = new VerticalLayoutDriver()
            {
                Dir = VerticalLayoutDriver.Direction.BottomToTop,
                FitToSize = true,
                Spacing = 2f
            };

            uiSequencerPanel.RefreshVessel();

            return uiSequencerPanel;
        }
    }

    public static class UISequencerPanel_Ex
    {
        public static UISequencerPanel AddSequencerPanel( this IUIElementContainer parent, UILayoutInfo layout )
        {
            return UISequencerPanel.Create<UISequencerPanel>( parent, layout );
        }
    }
}