using HSP.SceneManagement;
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

        public const string ADD_SEQUENCER_PANEL = HSPEvent.NAMESPACE_HSP + ".ui.fsequencer.add";
        public const string REMOVE_SEQUENCER_PANEL = HSPEvent.NAMESPACE_HSP + ".ui.fsequencer.remove";
        public const string UPDATE_SEQUENCER_PANEL = HSPEvent.NAMESPACE_HSP + ".ui.fsequencer";

        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_ACTIVATE.ID, ADD_SEQUENCER_PANEL )]
        private static void AddViewportClickController()
        {
            if( !_sequencerPanel.IsNullOrDestroyed() )
            {
                _sequencerPanel.Destroy();
            }

            if( ActiveVesselManager.ActiveObject != null )
            {
                UICanvas canvas = GameplaySceneM.Instance.GetStaticCanvas();
                _sequencerPanel = canvas.AddSequencerPanel( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (50, 100) ) );
            }
        }

        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_DEACTIVATE.ID, REMOVE_SEQUENCER_PANEL )]
        private static void RemoveViewportClickController()
        {
            if( !_sequencerPanel.IsNullOrDestroyed() )
            {
                _sequencerPanel.Destroy();
            }
        }

        [HSPEventListener( HSPEvent_AFTER_ACTIVE_VESSEL_CHANGED.ID, UPDATE_SEQUENCER_PANEL )]
        public static void CreateUI()
        {
            if( !HSPSceneManager.IsForeground<GameplaySceneM>() )
                return;

            if( !_sequencerPanel.IsNullOrDestroyed() )
            {
                _sequencerPanel.Destroy();
            }

            if( ActiveVesselManager.ActiveObject != null )
            {
                UICanvas canvas = GameplaySceneM.Instance.GetStaticCanvas();
                _sequencerPanel = canvas.AddSequencerPanel( new UILayoutInfo( UIAnchor.BottomLeft, (0, 0), (50, 100) ) );
            }
        }
    }
}