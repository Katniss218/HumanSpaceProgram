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
        Transform _heldPart = null;
        /// <summary>
        /// Gets or sets the part that's currently "held" by the cursor.
        /// </summary>
        public Transform HeldPart
        {
            get => _heldPart;
            set
            {
                if( _heldPart != null )
                {
                    Destroy( _heldPart.gameObject );
                }
                _heldPart = value;
                _heldOffset = Vector3.zero;
            }
        }

        Vector3 _heldOffset;

        FAttachNode[] _nodes;

        public bool SnappingEnabled { get; set; }
        public float SnapAngle { get; set; }

        Camera _camera;
        FAttachNode snappedToNode = null;

        void Awake()
        {
            _camera = GameObject.Find( "Near camera" ).GetComponent<Camera>();
        }

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

        private void TryGrabPart()
        {
            Ray ray = _camera.ScreenPointToRay( Input.mousePosition );
            if( UnityEngine.Physics.Raycast( ray, out RaycastHit hitInfo, 8192, int.MaxValue ) )
            {
                Transform clickedObj = hitInfo.collider.transform;

                FClickInteractionRedirect r = clickedObj.GetComponent<FClickInteractionRedirect>();
                if( r != null && r.Target != null )
                {
                    clickedObj = r.Target.transform;
                }

                if( DesignVesselManager.IsActionable( clickedObj ) )
                {
                    DesignVesselManager.PickUp( clickedObj );
                    HeldPart = clickedObj;
                    _heldOffset = hitInfo.point - clickedObj.position;
                }
            }
        }

        private void PlacePart()
        {
            if( snappedToNode != null )
            {
                _heldPart.SetParent( snappedToNode.transform.parent );
                _heldPart = null;

                return;
            }

            Ray ray = _camera.ScreenPointToRay( Input.mousePosition );

            IEnumerable<RaycastHit> hits = UnityEngine.Physics.RaycastAll( ray, 8192, int.MaxValue ).OrderBy( h => h.distance );
            foreach( var hit in hits )
            {
                Transform hitObj = hit.collider.transform;

                FClickInteractionRedirect r = hitObj.GetComponent<FClickInteractionRedirect>();
                if( r != null && r.Target != null )
                {
                    hitObj = r.Target.transform;
                }

                if( hitObj.root == _heldPart )
                    continue;

                if( DesignVesselManager.IsAttachedToDesignObj( hitObj ) )
                {
                    DesignVesselManager.Place( _heldPart, hitObj );
                    _heldPart = null;
                    // recalc vessel data.
                    return;
                }
            }

            // KSP would place as ghost here

            DesignVesselManager.Place( _heldPart, null );
            _heldPart = null;
        }

        private void PositionHeldPart()
        {
            // Held part is moved on a plane defined by the normal = camera forward, and through point = current position of picked up part.

            Vector3 point = _heldPart.position + _heldOffset;

            Ray ray = _camera.ScreenPointToRay( Input.mousePosition );

            // Snap to surface.
            if( !Input.GetKey( KeyCode.LeftAlt ) )
            {
                IEnumerable<RaycastHit> hits = UnityEngine.Physics.RaycastAll( ray, 8192, int.MaxValue ).OrderBy( h => h.distance );
                foreach( var hit in hits )
                {
                    if( DesignVesselManager.IsAttachedToDesignObj( hit.collider.transform ) )
                    {
                        // TODO - angle snap like in KSP.

                        _heldPart.rotation = Quaternion.LookRotation( hit.normal, Vector3.up );
                        _heldPart.position = hit.point; // todo - use surface attach node if available.

                        return;
                    }
                }
            }

            Plane p = new Plane( _camera.transform.forward, point );
            if( p.Raycast( ray, out float intersectionDistance ) )
            {
                Vector3 intersectionPoint = ray.GetPoint( intersectionDistance );

                _heldPart.position = intersectionPoint - _heldOffset;

                // snap to node, if available.
#warning TODO - snapping sometimes snaps to the wrong node.
                FAttachNode[] heldNodes = _heldPart.GetComponentsInChildren<FAttachNode>();
                FAttachNode[] targetNodes = FindObjectsOfType<FAttachNode>().Where( n => n.transform.root != HeldPart ).ToArray();

                var tuple = FAttachNode.TrySnap( _heldPart, heldNodes, targetNodes );
                if( tuple != null )
                {
                    snappedToNode = tuple.Value.tgt;
                }
            }
        }
    }
}