using HSP.Input;
using HSP.Vessels;
using System.Linq;
using UnityEngine;
using UnityPlus.Input;

namespace HSP.Vanilla.Scenes.DesignScene.Tools
{
    /// <summary>
    /// Allows to detach and attach parts.
    /// </summary>
    public class PickTool : DesignSceneTool
    {
        Vessel _heldPartGraph = null; // @@INFO - In-world part graphs don't exist 'outside' vessels, thus this is a vessel.

        Vector3 _heldOffset;
        Quaternion _heldRotation;

        FAttachNode.SnappingCandidate? _currentSnap = null;

        public bool AngleSnappingEnabled = true;
        public float AngleSnappingInterval = 22.5f;

        private Ray _currentFrameCursorRay;
        private VesselPart _currentFrameHitPart;
        private RaycastHit _currentFrameHit;

        /// <summary>
        /// Sets the held part graph, destroys the previously held part graph (if any).
        /// </summary>
        public void SetHeldPart( Vessel value, Vector3 clickOffset, Quaternion clickRotation )
        {
            if( _heldPartGraph == value )
                return;
            if( _heldPartGraph != null )
                VesselFactory.Destroy( _heldPartGraph );

            _heldPartGraph = value;
            _heldPartGraph.gameObject.SetLayer( (int)Layer.VESSEL_DESIGN_HELD, true );

            _heldOffset = clickOffset;
            _heldRotation = clickRotation; // KSP takes into account whether the orientation was changed using the WASDQE keys.
        }

        void Update()
        {
            if( UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() )
                return;

            _currentFrameCursorRay = SceneCamera.GetCamera<DesignSceneM>().ScreenPointToRay( UnityEngine.Input.mousePosition );

            if( Physics.Raycast( _currentFrameCursorRay, out _currentFrameHit, 8192, 1 << (int)Layer.PART_OBJECT )
             && DesignVesselManager.TryGetPart( _currentFrameHit.collider.transform, out var part ) )
            {
                _currentFrameHitPart = part;
            }
            else
            {
                _currentFrameHitPart = null;
            }

            if( _heldPartGraph != null )
            {
                PositionHeldPart();
            }
        }

        void OnEnable()
        {
            HierarchicalInputManager.AddAction( Input.InputChannel.PRIMARY_UP, InputChannelPriority.MEDIUM, Input_MouseClick );
        }

        void OnDisable() // if tool switched while action is performed.
        {
            HierarchicalInputManager.RemoveAction( Input.InputChannel.PRIMARY_UP, Input_MouseClick );
            if( _heldPartGraph != null )
            {
                PlacePart();
            }
        }

        private bool Input_MouseClick( float value )
        {
            if( UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() )
                return false;

            if( _heldPartGraph == null )
            {
                TryPickUpPart();

                if( _heldPartGraph != null )
                {
                    PositionHeldPart();
                    return true;
                }
            }
            else
            {
                PositionHeldPart();
                PlacePart();
                return true;
            }
            return false;
        }

        private void TryPickUpPart()
        {
            if( _currentFrameHitPart != null )
            {
                var oldVessel = _currentFrameHitPart.Vessel;
                bool wasDesignObject = (oldVessel == DesignVesselManager.DesignObject);
                if( VesselHierarchyUtils.TryDetach( _currentFrameHitPart ) )
                {
                    // _currentFrameHitPart.Vessel is now a (new) loose vessel, we can hold it.
                    var newVessel = _currentFrameHitPart.Vessel;
                    if( oldVessel == newVessel )
                    {
                        // The whole vessel was picked up.
                        if( wasDesignObject )
                        {
                            DesignVesselManager.ClearDesignObject();
                        }
                        else
                        {
                            DesignVesselManager.RemoveLoosePart( newVessel );
                        }
                    }

                    SetHeldPart( newVessel, _currentFrameHit.point - _currentFrameHitPart.transform.position, _currentFrameHitPart.transform.rotation );
                }
            }
        }

        private void PlacePart()
        {
            if( _currentSnap != null )
            {
                // Node-attach (object is already positioned).
                _heldPartGraph.gameObject.SetLayer( (int)Layer.PART_OBJECT, true );

                var targetVessel = _currentSnap.Value.targetNode.Part.Vessel;
                VesselHierarchyUtils.Attach( _currentSnap.Value );

                if( DesignVesselManager.DesignObject == null )
                {
                    DesignVesselManager.SetDesignObject( targetVessel );
                    DesignVesselManager.RemoveLoosePart( targetVessel );
                }

                _heldPartGraph = null;
                _currentSnap = null;
                return;
            }

            // Surface-attach (object is already positioned).
            if( _currentFrameHitPart != null )
            {
                _heldPartGraph.gameObject.SetLayer( (int)Layer.PART_OBJECT, true );

                var targetVessel = _currentFrameHitPart.Vessel;
                VesselHierarchyUtils.SurfaceAttach( _heldPartGraph.Parts.First(), _currentFrameHitPart );

                if( DesignVesselManager.DesignObject == null )
                {
                    DesignVesselManager.SetDesignObject( targetVessel );
                    DesignVesselManager.RemoveLoosePart( targetVessel );
                }

                _heldPartGraph = null;
                _currentSnap = null;
                return;
            }

            // Place as a ghost loose part (object is already positioned).
            _heldPartGraph.gameObject.SetLayer( (int)Layer.PART_OBJECT, true );

            if( DesignVesselManager.DesignObject == null )
            {
                DesignVesselManager.SetDesignObject( _heldPartGraph );
            }
            else
            {
                DesignVesselManager.AddLoosePart( _heldPartGraph );
            }

            _heldPartGraph = null;
            _currentSnap = null;
        }

        private void PositionHeldPart()
        {
            if( !UnityEngine.Input.GetKey( KeyCode.LeftAlt ) )
            {
                // Snap to surface of other parts.

                if( _currentFrameHitPart != null )
                {
                    Transform currentFrameHitTransform = _currentFrameHitPart.transform;
                    Vector3 newPos = _currentFrameHit.point;
                    if( AngleSnappingEnabled )
                    {
                        Vector3 projectedPoint = Vector3.ProjectOnPlane( (currentFrameHitTransform.position - _currentFrameHit.point), currentFrameHitTransform.up ).normalized;
                        float angle = Vector3.SignedAngle( currentFrameHitTransform.right, projectedPoint, currentFrameHitTransform.up );

                        float roundedAngle = AngleSnappingInterval * Mathf.Round( angle / AngleSnappingInterval );

                        Quaternion rotation = Quaternion.AngleAxis( roundedAngle + 180, currentFrameHitTransform.up ); // angle + 180 appears to be needed, for some reason.

                        newPos = rotation * (currentFrameHitTransform.right * Vector3.Distance( _currentFrameHit.point, currentFrameHitTransform.position )) // position relative to (0,0,0)
                            + currentFrameHitTransform.position                                                                                            // translate from (0,0,0) to the part
                            + new Vector3( 0, (_currentFrameHit.point.y - currentFrameHitTransform.position.y), 0 );                                       // translate vertically from the part to to the cursor
                    }

                    _heldPartGraph.transform.rotation = Quaternion.LookRotation( _currentFrameHit.normal, currentFrameHitTransform.up ) * _heldRotation;
                    _heldPartGraph.transform.position = newPos; // todo - use surface attach node when available.
                    return;
                }
            }

            Plane viewPlane = new Plane( SceneCamera.GetCamera<DesignSceneM>().transform.forward, (_heldPartGraph.transform.position + _heldOffset) );
            if( viewPlane.Raycast( _currentFrameCursorRay, out float intersectionDistance ) )
            {
                Vector3 planePoint = _currentFrameCursorRay.GetPoint( intersectionDistance );

                // Reset the position/rotation before snapping to prevent the previous snapping from affecting what nodes will snap.
                // It should always snap "as if the part is at the cursor", not wherever it was snapped to previously.
                _heldPartGraph.transform.position = planePoint - _heldOffset;
                _heldPartGraph.transform.rotation = _heldRotation;

                TrySnappingHeldPartToAttachmentNode( viewPlane.normal );
            }
        }

        private void TrySnappingHeldPartToAttachmentNode( Vector3 viewDirection )
        {
            FAttachNode[] heldNodes = _heldPartGraph.GetComponentsInChildren<FAttachNode>();
            FAttachNode[] targetNodes = FAttachNode.GetAttachNodes( DesignVesselManager.DesignObject ).ToArray();

            FAttachNode.SnappingCandidate? nodePair = FAttachNode.GetBestSnappingNodePair( heldNodes, targetNodes, viewDirection );
            if( nodePair != null )
            {
                FAttachNode.SnapTo( _heldPartGraph.transform, nodePair.Value.snappedNode, nodePair.Value.targetNode );
                _currentSnap = nodePair;
            }
            else
            {
                _currentSnap = null;
            }
        }
    }
}