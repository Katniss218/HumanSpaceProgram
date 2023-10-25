using KSS.Cameras;
using KSS.Core;
using KSS.Core.Components;
using KSS.UI.Windows;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KSS.UI
{
    /// <summary>
    /// Manages clicking in the physical world of the gameplay scene.
    /// </summary>
    public class GameplayClickInteractionManager : HSPManager
    {
        /*
        
        This class is solely for the gameplay scene. The design scene should have its own interaction controller/manager class.

        */


        private void Update()
        {
            if( Input.GetKeyDown( KeyCode.Mouse0 ) )
            {
                if( UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() )
                {
                    return;
                }

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
                if( redirectComponent != null && redirectComponent.Target != null )
                {
                    clickedPart = redirectComponent.Target.transform;
                }

                PartWindow.ExistsFor( clickedPart );

                PartWindow window = PartWindow.Create( clickedPart );
            }
        }
    }
}