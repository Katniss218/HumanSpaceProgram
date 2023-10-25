using KSS.Core.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core.DesignScene.Tools
{
    /// <summary>
    /// Allows to detach and attach parts.
    /// </summary>
    public class PickTool : MonoBehaviour
    {
        [SerializeField]
        Transform _heldPart;

        [SerializeField]
        Camera _camera;

        void Update()
        {
            if( UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() )
                return;

            if( _heldPart == null )
            {
                if( Input.GetKeyDown( KeyCode.Mouse0 ) )
                {
                    TryGrabPart();

                    if( _heldPart != null )
                    {
                        PositionHeldPart();
                    }
                }
            }
            else
            {
                PositionHeldPart();

                if( Input.GetKeyDown( KeyCode.Mouse0 ) )
                {
                    TryPlacePart();
                }
            }
        }

        private void TryGrabPart()
        {
            Ray ray = _camera.ScreenPointToRay( Input.mousePosition );
            if( UnityEngine.Physics.Raycast( ray, out RaycastHit hitInfo, 8192, 1 << 24 ) )
            {
                GameObject hitObj = hitInfo.collider.gameObject;

                FClickInteractionRedirect r = hitObj.GetComponent<FClickInteractionRedirect>();
                if( r != null && r.Target != null )
                {
                    hitObj = r.Target;
                }

                _heldPart = hitObj.transform;
                _heldPart.gameObject.SetLayer( 25, true );
                _heldPart.SetParent( null );
                // recalc vessel data.
            }
        }

        private void TryPlacePart()
        {
            Ray ray = _camera.ScreenPointToRay( Input.mousePosition );
            if( UnityEngine.Physics.Raycast( ray, out RaycastHit hitInfo, 8192, 1 << 24 ) )
            {
                GameObject hitObj = hitInfo.collider.gameObject;

                FClickInteractionRedirect r = hitObj.GetComponent<FClickInteractionRedirect>();
                if( r != null && r.Target != null )
                {
                    hitObj = r.Target;
                }

                _heldPart.gameObject.SetLayer( 24, true );
                _heldPart.SetParent( hitObj.transform );
                _heldPart = null;
                // recalc vessel data.
            }
            else
            {
                // place as ghost

                _heldPart.gameObject.SetLayer( 24, true );
                _heldPart = null;
            }
        }

        private void PositionHeldPart()
        {
            // Held part is moved on a plane defined by the normal = camera forward, and through point = current position of picked up part.

            Vector3 point = _heldPart.position;

            Ray ray = _camera.ScreenPointToRay( Input.mousePosition );

            // Snap to surface.
            if( !Input.GetKeyDown( KeyCode.LeftAlt )
             && UnityEngine.Physics.Raycast( ray, out RaycastHit hitInfo, 8192, 1 << 24 ) )
#warning TODO - this reycasts against the held part too, which it shouldn't.
            {
                _heldPart.rotation = Quaternion.LookRotation( hitInfo.normal, Vector3.up );
                _heldPart.position = hitInfo.point; // todo - use surface attach node if available.

                return;
            }

            Plane p = new Plane( _camera.transform.forward, point );
            if( p.Raycast( ray, out float intersectionDistance ) )
            {
                Vector3 intersectionPoint = ray.GetPoint( intersectionDistance );
                _heldPart.position = intersectionPoint;
            }
        }
    }
}