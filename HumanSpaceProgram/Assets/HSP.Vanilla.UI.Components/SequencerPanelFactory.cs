using HSP.ReferenceFrames;
using UnityPlus.UILib;
using UnityPlus.UILib.UIElements;

namespace HSP.UI.SceneFactories
{
    /// <summary>
    /// Manages the HUD UI elements displayed for FSequencer in the gameplay scene.
    /// </summary>
    public static class SequencerPanelFactory
    {
        private static UISequencerPanel _sequencerPanel;

        [HSPEventListener( HSPEvent.GAMEPLAY_AFTER_ACTIVE_OBJECT_CHANGE, HSPEvent.NAMESPACE_VANILLA + ".ui.fsequencer" )]
        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, HSPEvent.NAMESPACE_VANILLA + ".ui.fsequencer" )]
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