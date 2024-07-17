using HSP.Components;
using HSP.Construction;
using HSP.Core;
using HSP.Core.Components;
using HSP.Core.Mods;
using HSP.Core.Physics;
using HSP.Core.ReferenceFrames;
using HSP.DesignScene;
using HSP.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Input;
using UnityPlus.Serialization;
using UnityPlus.Serialization.ReferenceMaps;

namespace HSP.GameplayScene.Tools
{
    /// <summary>
    /// Allows to detach and attach parts.
    /// </summary>
    public class ConstructTool : GameplaySceneToolBase
    {
        Transform _heldPart = null;

        Vector3 _heldOffset;
        Quaternion _heldRotation = Quaternion.identity;

        FAttachNode[] _nodes;

        public bool AngleSnappingEnabled { get; set; }
        public float AngleSnappingInterval { get; set; }

        FAttachNode.SnappingCandidate? _currentSnap = null;

        private Ray _cursorRay;
        private Transform _hitObject;
        private RaycastHit _hit;

        IForwardReferenceMap refMap; // ref map used to spawn the object.

        public void SpawnVesselAndSetGhost( string vesselId )
        {
            ForwardReferenceStore refStore = new ForwardReferenceStore();
            GameObject spawnedGameObject = PartRegistry.Load( new NamespacedIdentifier( "Vessels", vesselId ), refStore );
            if( spawnedGameObject == null )
            {
                GameplaySceneToolManager.UseTool<DefaultTool>();
                return;
            }

            foreach( var fc in spawnedGameObject.GetComponentsInChildren<FConstructible>() )
            {
                fc.BuildPoints = 0.0f; // This ghosts the new object.
            }

            this.refMap = refStore;
            this._heldPart = spawnedGameObject.transform;
            this._heldPart.gameObject.SetLayer( (int)Layer.VESSEL_DESIGN_HELD, true );
            this._heldOffset = Vector3.zero;
        }

        void Update()
        {
            if( _heldPart == null )
            {
                GameplaySceneToolManager.UseTool<DefaultTool>();
                return;
            }

            if( UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() )
                return;

            _cursorRay = SceneCamera.Camera.ScreenPointToRay( UnityEngine.Input.mousePosition );

            if( Physics.Raycast( _cursorRay, out _hit, 8192, 1 << (int)Layer.PART_OBJECT ) )
            {
                _hitObject = FClickInteractionRedirect.TryRedirect( _hit.collider.transform );
            }
            else
            {
                _hitObject = null;
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
            if( _currentSnap == null )
            {
                if( _hitObject == null )
                {
                    return;
                }

                if( UnityEngine.Input.GetKey( KeyCode.LeftAlt ) )
                {
                    return;
                }

                Vessel hitVessel = _hitObject.GetVessel();
                if( hitVessel == null )
                {
                    return;
                }

                FConstructionSite.CreateOrAppend( _heldPart, _hitObject );
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
                FConstructionSite.CreateOrAppend( _heldPart, parent );
            }
            _heldPart = null;
            _currentSnap = null;
            GameplaySceneToolManager.UseTool<DefaultTool>();
        }

        private void PositionHeldPart()
        {
            // Surface attachment.
            if( !UnityEngine.Input.GetKey( KeyCode.LeftAlt ) )
            {
                if( _hitObject != null )
                {
                    Vessel hitVessel = _hitObject.GetVessel();
                    if( hitVessel == null )
                    {
                        return;
                    }

                    Vector3 newHeldPosition = _hit.point;
                    if( AngleSnappingEnabled )
                    {
                        Vector3 projectedPoint = Vector3.ProjectOnPlane( (_hitObject.position - _hit.point), _hitObject.up ).normalized;
                        float angle = Vector3.SignedAngle( _hitObject.right, projectedPoint, _hitObject.up );

                        float roundedAngle = AngleSnappingInterval * Mathf.Round( angle / AngleSnappingInterval );

                        Quaternion rotation = Quaternion.AngleAxis( roundedAngle + 180, _hitObject.up ); // `angle + 180` appears to be needed, for some reason.

                        newHeldPosition = rotation * (_hitObject.right * Vector3.Distance( _hit.point, _hitObject.position )) // Position relative to (0,0,0).
                            + _hitObject.position                                                                             // Translate from (0,0,0) to the part.
                            + new Vector3( 0, (_hit.point.y - _hitObject.position.y), 0 );                                    // Translate vertically from the part origin to to the cursor.
                    }

                    _heldPart.rotation = Quaternion.LookRotation( _hit.normal, _hitObject.up ) * _heldRotation;
                    _heldPart.position = newHeldPosition; // TODO - Use surface attach node when available.
                    return;
                }
            }

            // Node attachment.
            Plane viewPlane = new Plane( SceneCamera.Camera.transform.forward, (_heldPart.position + _heldOffset) );
            if( viewPlane.Raycast( _cursorRay, out float intersectionDistance ) )
            {
                Vector3 planePoint = _cursorRay.GetPoint( intersectionDistance );

                // Reset the position/rotation before snapping to prevent the previous snapping from affecting what nodes will snap.
                // It should always snap "as if the part is at the cursor", not wherever it was snapped to previously.
                _heldPart.position = planePoint - _heldOffset;

                var closestVessel = VesselManager.LoadedVessels.OrderBy( v => Vector3.Distance( v.transform.position, planePoint ) ).First();

                // Rotation should take into account the orientation of the vessel we are most likely trying to snap to.
                _heldPart.rotation = closestVessel.transform.rotation * _heldRotation;

                TrySnappingHeldPartToAttachmentNode( viewPlane.normal );
            }
        }

        private void TrySnappingHeldPartToAttachmentNode( Vector3 viewDirection )
        {
            FAttachNode[] heldNodes = _heldPart.GetComponentsInChildren<FAttachNode>();
            FAttachNode[] targetNodes = VesselManager.LoadedVessels.GetComponentsInChildren<FAttachNode>().ToArray();

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