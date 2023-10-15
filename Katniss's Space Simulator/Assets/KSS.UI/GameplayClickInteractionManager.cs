using KSS.Cameras;
using KSS.Components;
using KSS.Core;
using KSS.UI.Windows;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KSS.UI
{
    /// <summary>
    /// Manages clicking in the physical world of the gameplay scene.
    /// </summary>
    public class GameplayClickInteractionManager : MonoBehaviour
    {
        /*
        
        This class is solely for the gameplay scene. The design scene should have its own interaction controller/manager class.

        */


        private void Update()
        {
            if( UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() )
            {
                return;
            }

            if( Input.GetKeyDown( KeyCode.Mouse0 ) )
            {
                if( !Physics.Raycast( CameraController.Instance.MainCamera.ScreenPointToRay( Input.mousePosition ), out RaycastHit hit ) )
                {
                    return;
                }

                Transform clickedPart = hit.collider.transform;
                if( clickedPart.GetVessel() == null )
                {
                    return;
                }

                FClickInteractionRedirect redirectComponent = clickedPart.GetComponent<FClickInteractionRedirect>();
                if( redirectComponent != null )
                {
                    clickedPart = redirectComponent.Target;
                }

                PartWindow.ExistsFor( clickedPart );

                PartWindow window = PartWindow.Create( clickedPart );
            }
        }
    }
}