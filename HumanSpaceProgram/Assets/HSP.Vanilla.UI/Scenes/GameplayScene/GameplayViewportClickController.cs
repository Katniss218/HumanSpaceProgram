using HSP.Input;
using HSP.Vanilla.Scenes.GameplayScene;
using HSP.Vanilla.UI.Components;
using HSP.Vessels;
using HSP.Vessels.Components;
using UnityEngine;
using UnityPlus.Input;
using UnityPlus.UILib;

namespace HSP.Vanilla.UI.Scenes.GameplayScene
{
    /// <summary>
    /// Controls clicking in the physical world of the gameplay scene.
    /// </summary>
    public class GameplayViewportClickController : SingletonMonoBehaviour<GameplayViewportClickController>
    {
        public const string ADD_VIEWPORT_CLICK_CONTROLLER = HSPEvent.NAMESPACE_HSP + ".click_controller.add";
        public const string REMOVE_VIEWPORT_CLICK_CONTROLLER = HSPEvent.NAMESPACE_HSP + ".click_controller.remove";

        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_ACTIVATE.ID, ADD_VIEWPORT_CLICK_CONTROLLER )]
        private static void AddViewportClickController()
        {
            GameplaySceneM.Instance.gameObject.AddComponent<GameplayViewportClickController>();
        }
        [HSPEventListener( HSPEvent_GAMEPLAY_SCENE_DEACTIVATE.ID, REMOVE_VIEWPORT_CLICK_CONTROLLER )]
        private static void RemoveViewportClickController()
        {
            var comp = GameplaySceneM.Instance.gameObject.GetComponent<GameplayViewportClickController>();
            UnityEngine.Object.Destroy( comp );
        }

        void OnEnable()
        {
            HierarchicalInputManager.AddAction( Input.InputChannel.PRIMARY_DOWN, InputChannelPriority.MEDIUM, Input_MouseDown );
        }

        void OnDisable()
        {
            HierarchicalInputManager.RemoveAction( Input.InputChannel.PRIMARY_DOWN, Input_MouseDown );
        }

        private bool Input_MouseDown( float value )
        {
            if( UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() )
                return false;

            if( !Physics.Raycast( SceneCamera.GetCamera<GameplaySceneM>().ScreenPointToRay( HierarchicalInputManager.CurrentState.MousePosition ), out RaycastHit hit ) )
            {
                return false;
            }

            Transform clickedPart = hit.collider.transform;
            if( clickedPart.GetVessel() == null )
            {
                return false;
            }

            TransformRedirect redirectComponent = clickedPart.GetComponent<TransformRedirect>();
            if( redirectComponent != null && redirectComponent.Target != null )
            {
                clickedPart = redirectComponent.Target;
            }

            if( FPart.GetPart( clickedPart ) == null )
            {
                return false;
            }

            if( !UIPartWindow.ExistsFor( clickedPart ) )
            {
                UIPartWindow partWindow = GameplaySceneM.Instance.GetWindowCanvas().AddPartWindow( new UILayoutInfo( UIAnchor.Center, (0, 0), (300f, 300f) ), clickedPart );
                return true;
            }
            return false;
        }
    }
}