using HSP.Content;
using HSP.Content.Vessels;
using HSP.Input;
using HSP.Vanilla.Components;
using HSP.Vanilla.Scenes.DesignScene;
using HSP.Vessels;
using HSP.Vessels.Construction;
using System.Linq;
using UnityEngine;
using UnityPlus.Input;
using UnityPlus.Serialization;
using UnityPlus.Serialization.ReferenceMaps;

namespace HSP.Vanilla.Scenes.GameplayScene.Tools
{
    /// <summary>
    /// Allows to detach and attach parts.
    /// </summary>
    public class ConstructTool : GameplaySceneTool
    {
        public bool AngleSnappingEnabled { get; set; }
        public float AngleSnappingInterval { get; set; }

        Vessel _heldPartGraph = null;

        Vector3 _heldOffset;
        Quaternion _heldRotation = Quaternion.identity;

        FAttachNode[] _nodes;
        FAttachNode.SnappingCandidate? _currentSnap = null;

        Ray _currentFrameCursorRay;
        VesselPart _currentFrameHitPart;
        RaycastHit _currentFrameHit;
        IForwardReferenceMap _refMap; // ref map used to spawn the object.

        public void SpawnVesselAndSetGhost( string vesselId )
        {
            ForwardReferenceStore refStore = new ForwardReferenceStore();

            if( !PartRegistry.TryLoad( new NamespacedID( "Vessels", vesselId ), refStore, out Vessel spawnedVessel ) )
            {
                GameplaySceneToolManager.UseTool<DefaultTool>();
                return;
            }

            //foreach( var fc in spawnedVessel /* get FConstructibles */ )
            //{
            //    fc.BuildPoints = 0.0f; // This ghosts the new object.
            //}

            this._refMap = refStore;
            this._heldPartGraph = spawnedVessel;
            this._heldPartGraph.gameObject.SetLayer( (int)Layer.VESSEL_DESIGN_HELD, true );
            this._heldOffset = Vector3.zero;
        }

        void Update()
        {
            if( _heldPartGraph == null )
            {
                GameplaySceneToolManager.UseTool<DefaultTool>();
                return;
            }

            if( UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() )
                return;

            _currentFrameCursorRay = SceneCamera.GetCamera<GameplaySceneM>().ScreenPointToRay( UnityEngine.Input.mousePosition );

            if( Physics.Raycast( _currentFrameCursorRay, out _currentFrameHit, 8192, 1 << (int)Layer.PART_OBJECT )
             && DesignVesselManager.TryGetPart( _currentFrameHit.collider.transform, out var part ) )
            {
                _currentFrameHitPart = part;
            }
            else
            {
                _currentFrameHitPart = null;
            }

            PositionHeldPart();
        }

        void OnEnable()
        {
            HierarchicalInputManager.AddAction( Input.InputChannel.PRIMARY_UP, InputChannelPriority.MEDIUM, Input_MouseClick );
            HierarchicalInputManager.AddAction( InputChannel.CONSTRUCT_PART_ROTATE_XP, InputChannelPriority.MEDIUM, Input_RotateXp );
            HierarchicalInputManager.AddAction( InputChannel.CONSTRUCT_PART_ROTATE_XN, InputChannelPriority.MEDIUM, Input_RotateXn );
            HierarchicalInputManager.AddAction( InputChannel.CONSTRUCT_PART_ROTATE_YP, InputChannelPriority.MEDIUM, Input_RotateYp );
            HierarchicalInputManager.AddAction( InputChannel.CONSTRUCT_PART_ROTATE_YN, InputChannelPriority.MEDIUM, Input_RotateYn );
            HierarchicalInputManager.AddAction( InputChannel.CONSTRUCT_PART_ROTATE_ZP, InputChannelPriority.MEDIUM, Input_RotateZp );
            HierarchicalInputManager.AddAction( InputChannel.CONSTRUCT_PART_ROTATE_ZN, InputChannelPriority.MEDIUM, Input_RotateZn );
        }

        void OnDisable()
        {
            HierarchicalInputManager.RemoveAction( Input.InputChannel.PRIMARY_UP, Input_MouseClick );
            HierarchicalInputManager.RemoveAction( InputChannel.CONSTRUCT_PART_ROTATE_XP, Input_RotateXp );
            HierarchicalInputManager.RemoveAction( InputChannel.CONSTRUCT_PART_ROTATE_XN, Input_RotateXn );
            HierarchicalInputManager.RemoveAction( InputChannel.CONSTRUCT_PART_ROTATE_YP, Input_RotateYp );
            HierarchicalInputManager.RemoveAction( InputChannel.CONSTRUCT_PART_ROTATE_YN, Input_RotateYn );
            HierarchicalInputManager.RemoveAction( InputChannel.CONSTRUCT_PART_ROTATE_ZP, Input_RotateZp );
            HierarchicalInputManager.RemoveAction( InputChannel.CONSTRUCT_PART_ROTATE_ZN, Input_RotateZn );
            if( _heldPartGraph != null )
            {
                Destroy( _heldPartGraph.gameObject );
                _heldPartGraph = null;
            }
        }

        private bool Input_MouseClick( float value )
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

        private bool Input_RotateXp( float value )
        {
            return RotateHeldPart( Vector3.right, 45f );
        }

        private bool Input_RotateXn( float value )
        {
            return RotateHeldPart( Vector3.left, 45f );
        }

        private bool Input_RotateYp( float value )
        {
            return RotateHeldPart( Vector3.up, 45f );
        }

        private bool Input_RotateYn( float value )
        {
            return RotateHeldPart( Vector3.down, 45f );
        }

        private bool Input_RotateZp( float value )
        {
            return RotateHeldPart( Vector3.forward, 45f );
        }

        private bool Input_RotateZn( float value )
        {
            return RotateHeldPart( Vector3.back, 45f );
        }

        // ksp - press AND release - pick up
        // release - place
        // release - select move/rotate

        private void PlacePart()
        {
            if( _currentSnap != null )
            {
                // Node-attach (object is already positioned).
                _heldPartGraph.gameObject.SetLayer( (int)Layer.PART_OBJECT, true );

                var targetVessel = _currentSnap.Value.targetNode.Part.Vessel;
                VesselHierarchyUtils.Attach( _currentSnap.Value );

                TryStartConstructing( targetVessel, _currentSnap.Value.snappedNode.Part );

                _heldPartGraph = null;
                _currentSnap = null;
                GameplaySceneToolManager.UseTool<DefaultTool>();
                return;
            }

            // Surface-attach (object is already positioned).
            if( _currentFrameHitPart != null )
            {
                _heldPartGraph.gameObject.SetLayer( (int)Layer.PART_OBJECT, true );

                var targetVessel = _currentFrameHitPart.Vessel;
                VesselHierarchyUtils.SurfaceAttach( _heldPartGraph.Parts.First(), _currentFrameHitPart );

                TryStartConstructing( targetVessel, _currentFrameHitPart );

                _heldPartGraph = null;
                _currentSnap = null;
                GameplaySceneToolManager.UseTool<DefaultTool>();
                return;
            }

            // No valid placement found, can not drop/place the part 'loosely'.
            // @@todo - buildings will be placed here in the future.
        }

        private static void TryStartConstructing( Vessel targetVessel, VesselPart snappedPart )
        {
            var site = targetVessel.GetConstructionSite();
            if( site != null )
            {
                var parts = snappedPart.GetComponentsInChildren<VesselPart>( true );
                site.SetUnderConstruction( parts, true );
                if( site.State == ConstructionState.NotStarted )
                {
                    site.StartConstructing();
                }
            }
        }

        private void PositionHeldPart()
        {
            // Surface attachment.
            if( !UnityEngine.Input.GetKey( KeyCode.LeftAlt ) )
            {
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