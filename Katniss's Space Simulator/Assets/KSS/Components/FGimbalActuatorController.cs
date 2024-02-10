using KSS.Control;
using KSS.Control.Controls;
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
		public class ActuatorGroup : ControlGroup
		{
			[NamedControl( "Transform", "Specifies the object to use as the current coordinate frame of the controlled engine.\nPick the corresponding engine." )]
			public ControlParameterInput<Transform> GetReferenceTransform;
			
			[NamedControl( "Thruster", "Pick the corresponding engine." )]
			public ControlParameterInput<IThruster> GetThruster;

			[NamedControl( "Deflection" )]
			public ControllerOutput<Vector2> OnSetDeflection; // the value of this is going to be assigned to the control input on the engine.
		}

		// TODO - make work for both 1-axis, 2-axis, and 3-axis actuators.

		// TODO - certain controllers should be able to be disabled (stop responding to signals)

		public Vector3 TargetSteering { get; set; }

		[NamedControl( "Actuators", "Connect to the actuators you want this gimbal controller to control." )]
		private ActuatorGroup[] Actuators = new ActuatorGroup[5];

		[NamedControl( "Steering Command", "Connect to the avionics." )]
		private ControlleeInput<Vector3> SteeringCommand;
		private void OnSteeringCommand( Vector3 targetSteering )
		{
			TargetSteering = targetSteering;
		}

		void Awake()
		{
			SteeringCommand = new ControlleeInput<Vector3>( OnSteeringCommand );
		}

		void FixedUpdate()
		{
			// the parameter is a vector that represents the desired steering command around each of the 3 principal axes of the vessel.
			// TODO - calculate deflections and move.
			foreach( var actuator in Actuators )
			{
				// TODO - we need to know the current thrust, and current deflection
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