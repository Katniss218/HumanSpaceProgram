﻿using HSP.Input;
using HSP.ViewportTools;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.Input;

namespace HSP.Vanilla.Scenes.DesignScene.Tools
{
    /// <summary>
    /// Allows to move a selected part after placing.
    /// </summary>
    public class RotateTool : DesignSceneTool
	{
		private bool _snappingEnabled;
		public bool SnappingEnabled
		{
			get => _snappingEnabled;
			set
			{
				_snappingEnabled = value;
				foreach( var handle in _handles.GetHandles<TranslationTransformHandle>() )
				{
					handle.SnappingEnabled = value;
				}
			}
		}

		private float _snappingInterval = 22.5f;
		public float SnappingInterval
		{
			get => _snappingInterval;
			set
			{
				_snappingInterval = value;
				foreach( var handle in _handles.GetHandles<TranslationTransformHandle>() )
				{
					handle.SnappingInterval = value;
				}
			}
		}

		private TransformHandleSet _handles;

		void Update()
		{
			if( _handles == null )
			{
				return;
			}

			if( UnityEngine.Input.GetKeyDown( KeyCode.LeftShift ) )
			{
				SnappingEnabled = true;
			}
			if( UnityEngine.Input.GetKeyUp( KeyCode.LeftShift ) )
			{
				SnappingEnabled = false;
			}
		}

		void OnEnable()
		{
            HierarchicalInputManager.AddAction( Input.InputChannel.PRIMARY_DOWN, InputChannelPriority.MEDIUM, Input_MouseDown );
			if( DesignVesselManager.DesignObject != null )
			{
				CreateHandles();
				var target = DesignVesselManager.DesignObject.RootPart;
				_handles.Target = target;
				_handles.transform.position = target.position;
			}
		}

		void OnDisable()
		{
            HierarchicalInputManager.RemoveAction( Input.InputChannel.PRIMARY_DOWN, Input_MouseDown );
			if( _handles != null )
			{
				_handles.Destroy();
				_handles = null;
			}
		}

		private bool Input_MouseDown( float value )
		{
			Ray ray = _handles.RaycastCamera.ScreenPointToRay( UnityEngine.Input.mousePosition );
			if( Physics.Raycast( ray, out RaycastHit hitInfo, 8192, int.MaxValue ) )
			{
				Transform clickedObj = hitInfo.collider.transform;

				TransformRedirect r = clickedObj.GetComponent<TransformRedirect>();
				if( r != null && r.Target != null )
				{
					clickedObj = r.Target;
				}

				if( DesignVesselManager.IsLooseOrPartOfDesignObject( clickedObj ) )
				{
					if( _handles == null )
						CreateHandles();

					_handles.Target = clickedObj;
					_handles.transform.position = clickedObj.position;
					return true;
				}
			}
			return false;
		}

		void CreateHandles()
		{
			_handles = TransformHandleSet.Create( Vector3.zero, Quaternion.identity, null, null );
			_handles.CreateXYZHandles<RotationTransformHandle>(
				AssetRegistry.Get<Mesh>( $"builtin::Resources/Meshes/rotate_handle_1d" ),
				AssetRegistry.Get<Material>( $"builtin::Resources/Materials/axis" ),
				go =>
				{
					BoxCollider c = go.AddComponent<BoxCollider>();
					c.size = new Vector3( 3.5f, 3.5f, 0.1f );
				} );
			_handles.RaycastCamera = SceneCamera.Camera.GetComponent<Camera>();
		}
	}
}