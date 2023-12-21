using KSS.Components;
using KSS.Core;
using KSS.Core.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization.ReferenceMaps;

namespace KSS.GameplayScene.Tools
{
    /// <summary>
    /// Allows to detach and attach parts.
    /// </summary>
    public class ConstructTool : GameplaySceneToolBase
    {
        Transform _heldPart = null;
        Dictionary<FConstructible, ConstructionSite.ConstructibleData> _de;
        BidirectionalReferenceStore _refMap;

        public void SetGhostPart( Transform root, Dictionary<FConstructible, ConstructionSite.ConstructibleData> de, BidirectionalReferenceStore refMap, Vector3 heldOffset )
        {
            if( this._heldPart == root )
                return;

            if( this._heldPart != null )
            {
                Destroy( this._heldPart.gameObject );
            }

            this._heldPart = root;
            this._heldPart.gameObject.SetLayer( (int)Layer.VESSEL_DESIGN_HELD, true );
            this._de = de;
            this._refMap = refMap;
            this._heldOffset = heldOffset;
        }

        Vector3 _heldOffset;

        FAttachNode[] _nodes;

        public bool AngleSnappingEnabled { get; set; }
        public float AngleSnappingInterval { get; set; }

        Camera _camera;
        FAttachNode.SnappingCandidate? _currentSnap = null;

        // construct tool gets assigned a ghost, and its job is to place it. So it's basically a "place ghost" tool.
        // adjusting the placed ghost can be done by the move/rotate tools.

        // placed ghost can be adjusted, this resets all build points (or can't be adjusted if it has any build points accumulated)

        // adjustment is done by a different tool.

        private Ray _currentFrameCursorRay;
        private Transform _currentFrameHitObject;
        private RaycastHit _currentFrameHit;

        void Awake()
        {
            _camera = GameObject.Find( "Near Camera" ).GetComponent<Camera>();
        }

        void Update()
        {
            if( UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() )
                return;

            _currentFrameCursorRay = _camera.ScreenPointToRay( Input.mousePosition );

            if( Physics.Raycast( _currentFrameCursorRay, out _currentFrameHit, 8192, 1 << (int)Layer.PART_OBJECT ) )
            {
                _currentFrameHitObject = FClickInteractionRedirect.TryRedirect( _currentFrameHit.collider.transform );
            }
            else
            {
                _currentFrameHitObject = null;
            }

            PositionHeldPart();

            if( Input.GetKeyUp( KeyCode.Mouse0 ) )
            {
                PlacePart();
            }
        }

        void OnDisable() // if tool switched while trying to place new construction ghost
        {
            if( _heldPart != null )
            {
                Destroy( _heldPart.gameObject );
                _heldPart = null;
                _refMap = null;
                _de = null;
            }
        }

        // ksp - press AND release - pick up
        // release - place
        // release - select move/rotate

        private void PlacePart()
        {
            if( _currentSnap == null )
            {
                if( _currentFrameHitObject == null )
                {
                    return;
                }

                Vessel hitVessel = _currentFrameHitObject.GetVessel();
                if( hitVessel == null )
                {
                    return;
                }

                ConstructionSite.AddGhostToConstruction( _heldPart, _de, hitVessel.RootPart, _refMap );
                _heldPart = null;
                _refMap = null;
                _de = null;
                _currentSnap = null;
            }
            else
            {
                Transform parent = _currentSnap.Value.targetNode.transform.parent;
                Vessel hitVessel = parent.GetVessel();
                if( hitVessel == null )
                {
                    return;
                }

                Transform newRoot = VesselHierarchyUtils.ReRoot( parent );
                _heldPart = newRoot;
                // Node-attach (object is already positioned).
                ConstructionSite.AddGhostToConstruction( _heldPart, _de, parent, _refMap );
                _heldPart = null;
                _refMap = null;
                _de = null;
                _currentSnap = null;
            }
            GameplaySceneToolManager.UseTool<DefaultTool>();
        }

        private void PositionHeldPart()
        {
            if( !Input.GetKey( KeyCode.LeftAlt ) )
            {
                // Snap to surface of other parts.

                if( _currentFrameHitObject != null )
                {
                    Vessel hitVessel = _currentFrameHitObject.GetVessel();
                    if( hitVessel == null )
                    {
                        return;
                    }

                    Vector3 newPos = _currentFrameHit.point;
                    if( AngleSnappingEnabled )
                    {
                        Vector3 projectedPoint = Vector3.ProjectOnPlane( (_currentFrameHitObject.position - _currentFrameHit.point), _currentFrameHitObject.up ).normalized;
                        float angle = Vector3.SignedAngle( _currentFrameHitObject.right, projectedPoint, _currentFrameHitObject.up );

                        float roundedAngle = AngleSnappingInterval * Mathf.Round( angle / AngleSnappingInterval );

                        Quaternion rotation = Quaternion.AngleAxis( roundedAngle + 180, _currentFrameHitObject.up ); // angle + 180 appears to be needed, for some reason.

                        newPos = rotation * (_currentFrameHitObject.right * Vector3.Distance( _currentFrameHit.point, _currentFrameHitObject.position )) // position relative to (0,0,0)
                            + _currentFrameHitObject.position                                                                                            // translate from (0,0,0) to the part
                            + new Vector3( 0, (_currentFrameHit.point.y - _currentFrameHitObject.position.y), 0 );                                       // translate vertically from the part to to the cursor
                    }

                    _heldPart.rotation = Quaternion.LookRotation( _currentFrameHit.normal, _currentFrameHitObject.up );
                    _heldPart.position = newPos; // todo - use surface attach node when available.
                    return;
                }
            }

            Plane viewPlane = new Plane( _camera.transform.forward, (_heldPart.position + _heldOffset) );
            if( viewPlane.Raycast( _currentFrameCursorRay, out float intersectionDistance ) )
            {
                Vector3 planePoint = _currentFrameCursorRay.GetPoint( intersectionDistance );

                // Reset the position/rotation before snapping to prevent the previous snapping from affecting what nodes will snap.
                // It should always snap "as if the part is at the cursor", not wherever it was snapped to previously.
                _heldPart.position = planePoint - _heldOffset;
                _heldPart.rotation = Quaternion.identity;

                TrySnappingHeldPartToAttachmentNode( viewPlane.normal );
            }
        }

        private void TrySnappingHeldPartToAttachmentNode( Vector3 viewDirection )
        {
            FAttachNode[] heldNodes = _heldPart.GetComponentsInChildren<FAttachNode>();
            FAttachNode[] targetNodes = VesselManager.GetLoadedVessels().GetComponentsInChildren<FAttachNode>().ToArray();

            FAttachNode.SnappingCandidate? nodePair = FAttachNode.GetBestSnappingNodePair( heldNodes, targetNodes, viewDirection );
            if( nodePair != null )
            {
                FAttachNode.SnapTo( _heldPart, nodePair.Value.snappedNode, nodePair.Value.targetNode );
                _currentSnap = nodePair;
            }
            else
            {
                _currentSnap = null;
            }
        }
    }
}