using HSP.ReferenceFrames;
using HSP.UI;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Components
{
    /// <summary>
    /// Manages the HUD UI elements displayed for FSequencer in the gameplay scene.
    /// </summary>
    public static class SequencerPanelFactory
    {
        private static UISequencerPanel _sequencerPanel;
        
        public const string CREATE_SEQUENCER_PANEL = HSPEvent.NAMESPACE_HSP + ".ui.fsequencer";

        [HSPEventListener( HSPEvent_AFTER_ACTIVE_OBJECT_CHANGED.ID, CREATE_SEQUENCER_PANEL )]
        public static void CreateUI()
        {
            UICanvas canvas = CanvasManager.Get( CanvasName.STATIC );

            if( !_sequencerPanel.IsNullOrDestroyed() )
            {
                _sequencerPanel.Destroy();
            }

            if( ActiveObjectManager.ActiveObject != null )
            {
                _sequencerPanel = canvas.AddSequencerPanel( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (50, 100) ) );
            }
        }
    }
}