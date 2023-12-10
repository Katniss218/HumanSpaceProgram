using KSS.Core.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.DesignScene.Tools
{
    /// <summary>
    /// Allows to detach and attach parts.
    /// </summary>
    public class PickTool : MonoBehaviour
    {
        Transform _heldPart = null;

        Vector3 _heldClickOffset;
        Quaternion _heldPartStartRotation;

        Camera _camera;
        FAttachNode _nodeTheHeldPartIsSnappedTo = null;

        public bool AngleSnappingEnabled = false;
        public float AngleSnappingInterval = 22.5f;

        /// <summary>
        /// Sets the held part, destroys the previously held part (if any).
        /// </summary>
        public void SetHeldPart( Transform value, Vector3 clickOffset )
        {
            if( _heldPart == value )
                return;
            if( _heldPart != null )
                Destroy( _heldPart.gameObject );
            _heldPart = value;
            _heldClickOffset = clickOffset;
            _heldPartStartRotation = value.rotation;
        }

        void Awake()
        {
            _camera = GameObject.Find( "Near camera" ).GetComponent<Camera>(); // todo - change the GameObject.Find to something proper.
        }

        void Update()
        {
            if( UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() )
                return;

            if( _heldPart == null )
            {
                if( Input.GetKeyUp( KeyCode.Mouse0 ) )
                {
                    TryPickUpPart();

                    if( _heldPart != null )
                    {
                        PositionHeldPart();
                    }
                }
            }
            else
            {
                PositionHeldPart();

                if( Input.GetKeyUp( KeyCode.Mouse0 ) )
                {
                    PlacePart();
                }
            }
        }

        void OnDisable() // if tool switched while action is performed.
        {
            if( _heldPart != null )
            {
                PlacePart();
            }
        }

        private void TryPickUpPart()
        {
            Ray ray = _camera.ScreenPointToRay( Input.mousePosition );
            if( UnityEngine.Physics.Raycast( ray, out RaycastHit hitInfo, 8192, int.MaxValue ) )
            {
                Transform clickedObj = hitInfo.collider.transform;

                if( clickedObj.HasComponent<FClickInteractionRedirect>( out var redirect ) && redirect.Target != null )
                {
                    clickedObj = redirect.Target.transform;
                }

                if( DesignObjectManager.TryDetach( clickedObj ) )
                {
                    SetHeldPart( clickedObj, hitInfo.point - clickedObj.position );
                }
            }
        }

        private void PlacePart()
        {
            if( _nodeTheHeldPartIsSnappedTo != null )
            {
                if( DesignObjectManager.TryAttach( _heldPart, _nodeTheHeldPartIsSnappedTo.transform.parent ) ) 
                    // TODO - attaching should take into account which object the node belongs to, and re-root accordingly.
                {
                    _heldPart = null;
                    _nodeTheHeldPartIsSnappedTo = null;
                }

                return;
            }

            Ray ray = _camera.ScreenPointToRay( Input.mousePosition );

            IEnumerable<RaycastHit> hits = UnityEngine.Physics.RaycastAll( ray, 8192, int.MaxValue ).OrderBy( h => h.distance );
            foreach( var hit in hits )
            {
                Transform hitObj = hit.collider.transform;

                if( hitObj.root == _heldPart )
                    continue;

                if( hitObj.HasComponent<FClickInteractionRedirect>( out var redirect ) && redirect.Target != null )
                {
                    hitObj = redirect.Target.transform;
                }

                // thing we clicked on is part of the rocket/rover/vessel.
                // thing we are holding is not the design object itself.
                if( DesignObjectManager.TryAttach( _heldPart, hitObj ) )
                {
                    _heldPart = null;
                    _nodeTheHeldPartIsSnappedTo = null;
                    return;
                }
            }

            // KSP would place as ghost here
            if( DesignObjectManager.TryAttach( _heldPart, null ) )
            {
                _heldPart = null;
                _nodeTheHeldPartIsSnappedTo = null;
            }
        }

        private void PositionHeldPart()
        {
            Ray ray = _camera.ScreenPointToRay( Input.mousePosition );

            if( !Input.GetKey( KeyCode.LeftAlt ) )
            {
                // Snap to surface of other parts.
                IEnumerable<RaycastHit> hits = UnityEngine.Physics.RaycastAll( ray, 8192, int.MaxValue ).OrderBy( h => h.distance );
                foreach( var hit in hits )
                {
                    Transform hitObj = hit.collider.transform;

                    if( hitObj.root == _heldPart )
                        continue;

                    if( hitObj.HasComponent<FClickInteractionRedirect>( out var redirect ) && redirect.Target != null )
                    {
                        hitObj = redirect.Target.transform;
                    }

                    if( DesignObjectManager.CanBeAttachedTo( hitObj ) )
                    {
                        Vector3 newPos = hit.point;
                        if( AngleSnappingEnabled )
                        {
                            // Find the angle between the 'up' direction of the collider, the 'right' direction, and the direction to the hit point.
                            // Round that to nearest multiple of snappingangle, and rotate 'right' direction by that angle to get new point.
                            float angle = Vector3.SignedAngle( hitObj.right, Vector3.ProjectOnPlane( (hitObj.position - hit.point), hitObj.up ).normalized, hitObj.up );
                            if( angle < 0 )
                                angle += 360;
                            float roundedAngle = AngleSnappingInterval * Mathf.Round( angle / AngleSnappingInterval );
                            Quaternion rotation = Quaternion.AngleAxis( roundedAngle + 180, hitObj.up ); // angle + 180 appears to be needed, for some reason.
                            newPos = rotation * (hitObj.right * Vector3.Distance( hit.point, hitObj.position ));
                        }

                        _heldPart.rotation = Quaternion.LookRotation( hit.normal, hitObj.up );
                        _heldPart.position = newPos; // todo - use surface attach node if available.

                        return;
                    }
                }
            }

            Vector3 planePoint = _heldPart.position + _heldClickOffset;

            Plane viewPlane = new Plane( _camera.transform.forward, planePoint );
            if( viewPlane.Raycast( ray, out float intersectionDistance ) )
            {
                Vector3 intersectionPoint = ray.GetPoint( intersectionDistance );

                // Reset the position/rotation before snapping to prevent the previous snapping from affecting what nodes will snap.
                // It should always snap "as if the part is at the cursor", not wherever it was snapped to previously.
                _heldPart.position = intersectionPoint - _heldClickOffset;
                _heldPart.rotation = _heldPartStartRotation;

                TrySnappingHeldPartToAttachmentNode( viewPlane.normal );
            }
        }

        private void TrySnappingHeldPartToAttachmentNode( Vector3 viewDirection )
        {
            FAttachNode[] heldNodes = _heldPart.GetComponentsInChildren<FAttachNode>();
            FAttachNode[] targetNodes = DesignObjectManager.GetAttachableRoots().GetComponentsInChildren<FAttachNode>().Where( n => n.transform.root != _heldPart ).ToArray();

            FAttachNode.SnappingCandidate? nodePair = FAttachNode.GetBestSnappingNodePair( heldNodes, targetNodes, viewDirection );
            if( nodePair != null )
            {
                FAttachNode.SnapTo( _heldPart, nodePair.Value.snappedNode, nodePair.Value.targetNode );
                _nodeTheHeldPartIsSnappedTo = nodePair.Value.targetNode;
            }
            else
            {
                _nodeTheHeldPartIsSnappedTo = null;
            }
        }
    }
}