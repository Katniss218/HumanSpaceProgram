using KSS.Control;
using KSS.Control.Controls;
using KSS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Components
{
	/// <summary>
	/// Tries to achieve the desired angular accelerations using gimbal actuators.
	/// </summary>
	public class FGimbalActuatorController : MonoBehaviour, IPersistent
	{
		public class Actuator2DGroup : ControlGroup
		{
			[NamedControl( "Transform", "The object to use as the coordinate frame of the actuator." )]
			public ControlParameterInput<Transform> GetReferenceTransform = new();

			[NamedControl( "Deflection (XY)" )]
			public ControllerOutput<Vector2> OnSetXY = new();
		}

		// TODO - make work for both 1-axis, 2-axis, and 3-axis actuators.

		// TODO - certain controllers should be able to be disabled (stop responding to signals)

		/// <summary>
		/// The current steering command in vessel-space. The axes of this vector correspond to rotation around the axes of the vessel.
		/// </summary>
		[field: SerializeField]
		public Vector3 CurrentSteeringCommand { get; set; }

		[NamedControl( "2D Actuators", "Connect to the actuators you want this gimbal controller to control." )]
		public Actuator2DGroup[] Actuators2D = new Actuator2DGroup[5];

		[NamedControl( "Steering Command", "Connect to the avionics." )]
		public ControlleeInput<Vector3> SetSteeringCommand;
		private void OnSetSteeringCommand( Vector3 targetSteering )
		{
			CurrentSteeringCommand = targetSteering;
		}

		void Awake()
		{
			SetSteeringCommand = new ControlleeInput<Vector3>( OnSetSteeringCommand );
		}

		void FixedUpdate()
		{
			IPartObject partObject = this.transform.GetPartObject();

			Vector3 worldSteering = partObject.ReferenceTransform.TransformDirection( CurrentSteeringCommand );

			foreach( var actuator in Actuators2D )
			{
				if( actuator == null )
					continue;

				if( actuator.GetReferenceTransform.TryGet( out Transform transform ) )
				{
					Vector3 steeringLocal = transform.InverseTransformDirection( worldSteering );
					Vector2 localDeflection = new Vector2( steeringLocal.x /* pitch */, steeringLocal.z /* yaw */ ); // TODO - support roll in the future.
					actuator.OnSetXY.TrySendSignal( localDeflection );
				}
			}
		}

		public SerializedData GetData( IReverseReferenceMap s )
		{
			throw new NotImplementedException();
			// serialize the controlled array, otherwise the stuff is not gonna be set.
		}

		public void SetData( IForwardReferenceMap l, SerializedData data )
		{
			throw new NotImplementedException();
			// serialize the controlled array, otherwise the stuff is not gonna be set.
		}
	}
}