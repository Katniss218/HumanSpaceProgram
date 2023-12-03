using KSS.Cameras;
using KSS.Core;
using KSS.Core.Components;
using KSS.Core.Serialization;
using KSS.UI.Windows;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.UI
{
    /// <summary>
    /// Manages clicking in the physical world of the gameplay scene.
    /// </summary>
    [RequireComponent( typeof( PreexistingReference ) )]
    public class GameplayClickInteractionManager : SingletonMonoBehaviour<GameplayClickInteractionManager>
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

                if( !Physics.Raycast( GameplayCameraController.MainCamera.ScreenPointToRay( Input.mousePosition ), out RaycastHit hit ) )
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

                if( FPart.GetPart( clickedPart ) == null )
                {
                    return;
                }

                if( !PartWindow.ExistsFor( clickedPart ) )
                {
                    PartWindow window = PartWindow.Create( clickedPart );
                }
            }
        }
    }
}