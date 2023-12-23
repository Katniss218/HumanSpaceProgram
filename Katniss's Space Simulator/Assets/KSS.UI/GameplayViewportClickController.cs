using KSS.Cameras;
using KSS.Core;
using KSS.Core.Components;
using KSS.Core.Serialization;
using KSS.Input;
using KSS.UI.Windows;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityPlus.Input;
using UnityPlus.Serialization;

namespace KSS.UI
{
    /// <summary>
    /// Controls clicking in the physical world of the gameplay scene.
    /// </summary>
    [RequireComponent( typeof( PreexistingReference ) )]
    public class GameplayViewportClickController : SingletonMonoBehaviour<GameplayViewportClickController>
    {
        [HSPEventListener( HSPEvent.STARTUP_GAMEPLAY, HSPEvent.NAMESPACE_VANILLA + ".add_click_controller" )]
        private static void CreateInstanceInScene()
        {
            GameplaySceneManager.Instance.gameObject.AddComponent<GameplayViewportClickController>();
        }

        void OnEnable()
        {
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.VIEWPORT_LEFT_MOUSE_DOWN, HierarchicalInputPriority.MEDIUM, Input_MouseDown );
        }

        void OnDisable()
        {
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.VIEWPORT_LEFT_MOUSE_DOWN, Input_MouseDown );
        }

        private bool Input_MouseDown()
        {
            if( UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() )
                return false;

            if( !Physics.Raycast( GameplayCameraController.MainCamera.ScreenPointToRay( HierarchicalInputManager.CurrentState.MousePosition ), out RaycastHit hit ) )
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

            if( !PartWindow.ExistsFor( clickedPart ) )
            {
                PartWindow window = PartWindow.Create( clickedPart );
                return true;
            }
            return false;
        }
    }
}