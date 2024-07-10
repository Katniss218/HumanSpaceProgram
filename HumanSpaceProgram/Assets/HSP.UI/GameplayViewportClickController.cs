using HSP.Cameras;
using HSP.Core;
using HSP.Core.Components;
using HSP.Core.Serialization;
using HSP.Input;
using HSP.UI.Windows;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityPlus.Input;
using UnityPlus.Serialization;
using UnityPlus.UILib;

namespace HSP.UI
{
    /// <summary>
    /// Controls clicking in the physical world of the gameplay scene.
    /// </summary>
    public class GameplayViewportClickController : SingletonMonoBehaviour<GameplayViewportClickController>
    {
        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, HSPEvent.NAMESPACE_VANILLA + ".add_click_controller" )]
        private static void CreateInstanceInScene()
        {
            GameplaySceneManager.Instance.gameObject.AddComponent<GameplayViewportClickController>();
        }

        void OnEnable()
        {
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.VIEWPORT_PRIMARY_DOWN, HierarchicalInputPriority.MEDIUM, Input_MouseDown );
        }

        void OnDisable()
        {
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.VIEWPORT_PRIMARY_DOWN, Input_MouseDown );
        }

        private bool Input_MouseDown( float value )
        {
            if( UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() )
                return false;

            if( !Physics.Raycast( GameplaySceneCamera.MainCamera.ScreenPointToRay( HierarchicalInputManager.CurrentState.MousePosition ), out RaycastHit hit ) )
            {
                return false;
            }

            Transform clickedPart = hit.collider.transform;
            if( clickedPart.GetVessel() == null )
            {
                return false;
            }

            FClickInteractionRedirect redirectComponent = clickedPart.GetComponent<FClickInteractionRedirect>();
            if( redirectComponent != null && redirectComponent.Target != null )
            {
                clickedPart = redirectComponent.Target.transform;
            }

            if( FPart.GetPart( clickedPart ) == null )
            {
                return false;
            }

            if( !UIPartWindow.ExistsFor( clickedPart ) )
            {
                UIPartWindow partWindow = CanvasManager.Get( CanvasName.WINDOWS ).AddPartWindow( new UILayoutInfo( UIAnchor.Center, (0, 0), (300f, 300f) ), clickedPart );
                return true;
            }
            return false;
        }
    }
}