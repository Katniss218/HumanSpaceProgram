using HSP.Input;
using HSP.Vanilla.Components;
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
		Transform _heldPart = null;

		Vector3 _heldClickOffset;
		Quaternion _heldRotation;

		FAttachNode.SnappingCandidate? _currentSnap = null;

		public bool AngleSnappingEnabled = true;
		public float AngleSnappingInterval = 22.5f;

		private Ray _currentFrameCursorRay;
		private Transform _currentFrameHitObject;
		private RaycastHit _currentFrameHit;

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
			_heldPart.gameObject.SetLayer( (int)Layer.VESSEL_DESIGN_HELD, true );

			_heldClickOffset = clickOffset;
			_heldRotation = value.rotation; // KSP takes into account whether the orientation was changed using the WASDQE keys.
		}

		void Update()
		{
			if( UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() )
				return;

			_currentFrameCursorRay = SceneCamera.Camera.ScreenPointToRay( UnityEngine.Input.mousePosition );

			if( Physics.Raycast( _currentFrameCursorRay, out _currentFrameHit, 8192, 1 << (int)Layer.PART_OBJECT ) )
			{
				_currentFrameHitObject = TransformRedirect.TryRedirect( _currentFrameHit.collider.transform );
			}
			else
			{
				_currentFrameHitObject = null;
			}

			if( _heldPart != null )
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
			if( _heldPart != null )
			{
				PlacePart();
			}
		}

		private bool Input_MouseClick( float value )
		{
			if( UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() )
				return false;

			if( _heldPart == null )
			{
				TryPickUpPart();

				if( _heldPart != null )
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
			if( _currentFrameHitObject != null )
			{
				if( DesignVesselManager.TryDetach( _currentFrameHitObject ) )
				{
					SetHeldPart( _currentFrameHitObject, _currentFrameHit.point - _currentFrameHitObject.position );
				}
			}
		}

		private void PlacePart()
		{
			if( _currentSnap != null )
			{
				Transform newRoot = VesselHierarchyUtils.ReRoot( _currentSnap.Value.snappedNode.transform.parent );
				_heldPart = newRoot;
				// Node-attach (object is already positioned).
				if( DesignVesselManager.TryAttach( _heldPart, _currentSnap.Value.targetNode.transform.parent ) )
				{
					_heldPart.gameObject.SetLayer( (int)Layer.PART_OBJECT, true );
					_heldPart = null;
					_currentSnap = null;
				}

				return;
			}

			// Surface-attach (object is already positioned).
			if( _currentFrameHitObject != null )
			{
				if( DesignVesselManager.TryAttach( _heldPart, _currentFrameHitObject ) )
				{
					_heldPart.gameObject.SetLayer( (int)Layer.PART_OBJECT, true );
					_heldPart = null;
					_currentSnap = null;
					return;
				}
			}

			// Place as a ghost loose part (object is already positioned).
			if( DesignVesselManager.TryAttach( _heldPart, null ) )
			{
				_heldPart.gameObject.SetLayer( (int)Layer.PART_OBJECT, true );
				_heldPart = null;
				_currentSnap = null;
			}
		}

		private void PositionHeldPart()
		{
			if( !UnityEngine.Input.GetKey( KeyCode.LeftAlt ) )
			{
				// Snap to surface of other parts.

				if( _currentFrameHitObject != null )
				{
					if( DesignVesselManager.CanHaveChildren( _currentFrameHitObject ) )
					{
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
			}

			Plane viewPlane = new Plane( SceneCamera.Camera.transform.forward, (_heldPart.position + _heldClickOffset) );
			if( viewPlane.Raycast( _currentFrameCursorRay, out float intersectionDistance ) )
			{
				Vector3 planePoint = _currentFrameCursorRay.GetPoint( intersectionDistance );

				// Reset the position/rotation before snapping to prevent the previous snapping from affecting what nodes will snap.
				// It should always snap "as if the part is at the cursor", not wherever it was snapped to previously.
				_heldPart.position = planePoint - _heldClickOffset;
				_heldPart.rotation = _heldRotation;

				TrySnappingHeldPartToAttachmentNode( viewPlane.normal );
			}
		}

		private void TrySnappingHeldPartToAttachmentNode( Vector3 viewDirection )
		{
			FAttachNode[] heldNodes = _heldPart.GetComponentsInChildren<FAttachNode>();
			FAttachNode[] targetNodes = DesignVesselManager.GetAttachableRoots().GetComponentsInChildren<FAttachNode>().Where( n => n.transform.root != _heldPart ).ToArray();

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