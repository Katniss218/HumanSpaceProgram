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
        Quaternion _heldRotation;

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
                if( Input.GetKeyUp( KeyCode.Mouse0 ) )
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

                if( DesignObjectManager.IsActionable( clickedObj ) )
                {
                    if( !DesignObjectManager.IsRootOfDesignObj( clickedObj ) )
                    {
                        DesignObjectManager.PickUp( clickedObj );
                    }
                    else
                    {
                        clickedObj = DesignObjectManager.DesignObject.transform;
                    }
                    HeldPart = clickedObj;
                    _heldOffset = hitInfo.point - clickedObj.position;
                    _heldRotation = clickedObj.rotation;
                }
            }
        }

        private void PlacePart()
        {
            if( snappedToNode != null )
            {
                if( !DesignObjectManager.IsDesignObj( _heldPart ) )
                {
                    DesignObjectManager.Place( _heldPart, snappedToNode.transform.parent );
                }
                _heldPart = null;
                snappedToNode = null;

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

                if( DesignObjectManager.IsAttachedToDesignObj( hitObj ) )
                {
                    if( !DesignObjectManager.IsDesignObj( _heldPart ) )
                    {
                        DesignObjectManager.Place( _heldPart, hitObj );
                    }
                    _heldPart = null;
                    // recalc vessel data.
                    return;
                }
            }

            // KSP would place as ghost here
            if( !DesignObjectManager.IsAttachedToDesignObj( _heldPart ) )
            {
                DesignObjectManager.Place( _heldPart, null );
            }
            _heldPart = null;
        }

        private void PositionHeldPart()
        {
            // Held part is moved on a plane defined by the normal = camera forward, and through point = current position of picked up part.

            Vector3 mousePoint = _heldPart.position + _heldOffset;

            Ray ray = _camera.ScreenPointToRay( Input.mousePosition );

            // Snap to surface.
            if( !Input.GetKey( KeyCode.LeftAlt ) )
            {
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

                    if( DesignObjectManager.IsActionable( hitObj ) )
                    {
                        // TODO - angle snap like in KSP.

                        _heldPart.rotation = Quaternion.LookRotation( hit.normal, Vector3.up );
                        _heldPart.position = hit.point; // todo - use surface attach node if available.

                        return;
                    }
                }
            }

            Plane viewPlane = new Plane( _camera.transform.forward, mousePoint );
            if( viewPlane.Raycast( ray, out float intersectionDistance ) )
            {
                Vector3 intersectionPoint = ray.GetPoint( intersectionDistance );

                _heldPart.position = intersectionPoint - _heldOffset; // always reset before snapping, to un-snap, and prevent the snapped position taking different nodes.
                _heldPart.rotation = _heldRotation;

                if( DesignObjectManager.IsDesignObj( _heldPart ) )
                {
                    snappedToNode = null;
                }
                else
                {
                    // snap to node, if available.
                    FAttachNode[] heldNodes = _heldPart.GetComponentsInChildren<FAttachNode>();
                    FAttachNode[] targetNodes = FindObjectsOfType<FAttachNode>().Where( n => n.transform.root != HeldPart ).ToArray();

                    FAttachNode.SnappingCandidate? nodePair = FAttachNode.GetSnappingNodePair( heldNodes, targetNodes, viewPlane.normal );
                    if( nodePair != null )
                    {
                        FAttachNode.SnapTo( _heldPart, nodePair.Value.snappedNode, nodePair.Value.targetNode );
                        snappedToNode = nodePair.Value.targetNode;
                    }
                    else
                    {
                        snappedToNode = null;
                    }
                }
            }
        }
    }
}