using KSS.Components;
using KSS.Core;
using KSS.Core.Components;
using KSS.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Input;
using UnityPlus.Serialization.ReferenceMaps;

namespace KSS.GameplayScene.Tools
{
    /// <summary>
    /// Allows to detach and attach parts.
    /// </summary>
    public class ConstructTool : GameplaySceneToolBase
    {
        Transform _heldPart = null;

        // Additional data that needs to be passed to the c-site.
        (FConstructible, BidirectionalGhostPatch)[] _ghostPatches;
        BidirectionalReferenceStore _ghostRefMap;

        Vector3 _heldOffset;
        Quaternion _heldRotation = Quaternion.identity;

        FAttachNode[] _nodes;

        public bool AngleSnappingEnabled { get; set; }
        public float AngleSnappingInterval { get; set; }

        Camera _camera;
        FAttachNode.SnappingCandidate? _currentSnap = null;

        private Ray _currentFrameCursorRay;
        private Transform _currentFrameHitObject;
        private RaycastHit _currentFrameHit;

        /// <summary>
        /// sets or resets the currently held ghost hierarchy.
        /// </summary>
        /// <param name="root">The root object of the hierarchy.</param>
        /// <param name="ghostPatches">The patches applied to the hierarchy.</param>
        /// <param name="refMap"></param>
        /// <param name="heldOffset"></param>
        public void SetGhostPart( Transform root, (FConstructible, BidirectionalGhostPatch)[] ghostPatches, BidirectionalReferenceStore refMap, Vector3 heldOffset )
        {
            if( this._heldPart == root )
                return;

            if( this._heldPart != null )
            {
                Destroy( this._heldPart.gameObject );
            }

            this._heldPart = root;
            this._heldPart.gameObject.SetLayer( (int)Layer.VESSEL_DESIGN_HELD, true );
            this._ghostPatches = ghostPatches;
            this._ghostRefMap = refMap;
            this._heldOffset = heldOffset;
        }

        void Awake()
        {
            _camera = GameObject.Find( "Near Camera" ).GetComponent<Camera>();
        }

        void Update()
        {
            if( UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() )
                return;

            _currentFrameCursorRay = _camera.ScreenPointToRay( UnityEngine.Input.mousePosition );

            if( Physics.Raycast( _currentFrameCursorRay, out _currentFrameHit, 8192, 1 << (int)Layer.PART_OBJECT ) )
            {
                _currentFrameHitObject = FClickInteractionRedirect.TryRedirect( _currentFrameHit.collider.transform );
            }
            else
            {
                _currentFrameHitObject = null;
            }

            PositionHeldPart();
        }
        void OnEnable()
        {
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.VIEWPORT_PRIMARY_UP, HierarchicalInputPriority.MEDIUM, Input_MouseClick );
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.DESIGN_PART_ROTATE_XP, HierarchicalInputPriority.MEDIUM, Input_RotateXp );
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.DESIGN_PART_ROTATE_XN, HierarchicalInputPriority.MEDIUM, Input_RotateXn );
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.DESIGN_PART_ROTATE_YP, HierarchicalInputPriority.MEDIUM, Input_RotateYp );
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.DESIGN_PART_ROTATE_YN, HierarchicalInputPriority.MEDIUM, Input_RotateYn );
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.DESIGN_PART_ROTATE_ZP, HierarchicalInputPriority.MEDIUM, Input_RotateZp );
            HierarchicalInputManager.AddAction( HierarchicalInputChannel.DESIGN_PART_ROTATE_ZN, HierarchicalInputPriority.MEDIUM, Input_RotateZn );
        }

        void OnDisable()
        {
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.VIEWPORT_PRIMARY_UP, Input_MouseClick );
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.DESIGN_PART_ROTATE_XP, Input_RotateXp );
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.DESIGN_PART_ROTATE_XN, Input_RotateXn );
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.DESIGN_PART_ROTATE_YP, Input_RotateYp );
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.DESIGN_PART_ROTATE_YN, Input_RotateYn );
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.DESIGN_PART_ROTATE_ZP, Input_RotateZp );
            HierarchicalInputManager.RemoveAction( HierarchicalInputChannel.DESIGN_PART_ROTATE_ZN, Input_RotateZn );
            if( _heldPart != null )
            {
                Destroy( _heldPart.gameObject );
                _heldPart = null;
                _ghostRefMap = null;
                _ghostPatches = null;
            }
        }

        private bool Input_MouseClick()
        {
            if( UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() )
                return false;

            PlacePart();
            return true;
        }

        private bool RotateHeldPart( Vector3 worldAxis, float angle )
        {
            if( UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() )
                return false;

            Debug.Log( "rotated by " + worldAxis );
            _heldRotation *= Quaternion.AngleAxis( angle, worldAxis );
            return true;
        }

        private bool Input_RotateXp()
        {
            return RotateHeldPart( Vector3.right, 45f );
        }

        private bool Input_RotateXn()
        {
            return RotateHeldPart( Vector3.left, 45f );
        }

        private bool Input_RotateYp()
        {
            return RotateHeldPart( Vector3.up, 45f );
        }

        private bool Input_RotateYn()
        {
            return RotateHeldPart( Vector3.down, 45f );
        }

        private bool Input_RotateZp()
        {
            return RotateHeldPart( Vector3.forward, 45f );
        }

        private bool Input_RotateZn()
        {
            return RotateHeldPart( Vector3.back, 45f );
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

                if( UnityEngine.Input.GetKey( KeyCode.LeftAlt ) )
                {
                    return;
                }

                Vessel hitVessel = _currentFrameHitObject.GetVessel();
                if( hitVessel == null )
                {
                    return;
                }

                FConstructionSite.TryAddPart( _heldPart, hitVessel.RootPart, _ghostPatches, _ghostRefMap );
            }
            else
            {
                Transform parent = _currentSnap.Value.targetNode.transform.parent;
                Vessel hitVessel = parent.GetVessel();
                if( hitVessel == null )
                {
                    return;
                }

                Transform newRoot = VesselHierarchyUtils.ReRoot( _currentSnap.Value.snappedNode.transform.parent );
                _heldPart = newRoot;
                // Node-attach (object is already positioned).
                FConstructionSite.TryAddPart( _heldPart, parent, _ghostPatches, _ghostRefMap );
            }
            _heldPart = null;
            _ghostRefMap = null;
            _ghostPatches = null;
            _currentSnap = null;
            GameplaySceneToolManager.UseTool<DefaultTool>();
        }

        private void PositionHeldPart()
        {
            if( !UnityEngine.Input.GetKey( KeyCode.LeftAlt ) )
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

                    _heldPart.rotation = Quaternion.LookRotation( _currentFrameHit.normal, _currentFrameHitObject.up ) * _heldRotation;
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
                _heldPart.rotation = _heldRotation;

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