using HSP.UI.Canvases;
using HSP.Vanilla.Scenes.GameplayScene;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace HSP.Vanilla.UI.Components
{
    /// <summary>
    /// Manages the HUD UI elements displayed for FSequencer in the gameplay scene.
    /// </summary>
    public static class GameplaySequencerPanelFactory
    {
        private static UISequencerPanel _sequencerPanel;
        
        public const string CREATE_SEQUENCER_PANEL = HSPEvent.NAMESPACE_HSP + ".ui.fsequencer";

        [HSPEventListener( HSPEvent_AFTER_ACTIVE_VESSEL_CHANGED.ID, CREATE_SEQUENCER_PANEL )]
        public static void CreateUI()
        {
            UICanvas canvas = GameplaySceneM.Instance.GetStaticCanvas();

            if( !_sequencerPanel.IsNullOrDestroyed() )
            {
                _sequencerPanel.Destroy();
            }

            if( ActiveVesselManager.ActiveObject != null )
            {
                _sequencerPanel = canvas.AddSequencerPanel( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (50, 100) ) );
            }
        }
    }
}