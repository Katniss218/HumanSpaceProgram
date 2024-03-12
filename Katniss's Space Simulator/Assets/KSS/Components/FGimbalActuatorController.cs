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
	/// Controls a number of gimbal actuators.
	/// </summary>
	public class FGimbalActuatorController : MonoBehaviour, IPersistent
	{
		public class Actuator2DGroup : ControlGroup
		{
			[NamedControl( "Transform", "The object to use as the coordinate frame of the actuator." )]
			public ControlParameterInput<Transform> GetReferenceTransform = new();

			[NamedControl( "Deflection (XY)" )]
			public ControllerOutput<Vector2> OnSetXY = new();

			public Actuator2DGroup() : base() 
			{ }
		}

		// TODO - make work for both 1-axis, 2-axis, and 3-axis actuators.

		// TODO - certain controllers should be able to be disabled (stop responding to signals)

		/// <summary>
		/// The current steering command in vessel-space. The axes of this vector correspond to rotation around the axes of the vessel.
		/// </summary>
		[field: SerializeField]
		public Vector3 AttitudeCommand { get; set; }

		[NamedControl( "2D Actuators", "Connect to the actuators you want this gimbal controller to control." )]
		public Actuator2DGroup[] Actuators2D = new Actuator2DGroup[5];

		[NamedControl( "Attitude", "Connect to the avionics." )]
		public ControlleeInput<Vector3> SetAttitude;
		private void SetAttitudeListener( Vector3 attitude )
		{
			AttitudeCommand = attitude;
		}

		void Awake()
		{
			SetAttitude = new ControlleeInput<Vector3>( SetAttitudeListener );
		}

		void FixedUpdate()
		{
			IPartObject partObject = this.transform.GetPartObject();

			Vector3 worldSteering = partObject.ReferenceTransform.TransformDirection( AttitudeCommand );

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
			SerializedArray array = new SerializedArray();
			foreach( var act in Actuators2D )
			{
				SerializedObject elemData = new SerializedObject()
				{
					{ "on_set_xy", act.OnSetXY.GetData( s ) },
					{ "get_reference_transform", act.GetReferenceTransform.GetData( s ) }
				};
				array.Add( elemData );
			}

			return new SerializedObject()
			{
				{ "actuators_2d", array }
			};
		}

		public void SetData( IForwardReferenceMap l, SerializedData data )
		{
			if( data.TryGetValue("actuators_2d", out var actuators2D ))
			{
				// This is a bit too verbose imo.
				// needs an extension method to make serializing/deserializing arrays/lists of any type cleaner.
				this.Actuators2D = new Actuator2DGroup[((SerializedArray)actuators2D).Count];
				int i = 0;
				foreach( var elemData in (SerializedArray)actuators2D )
				{
					this.Actuators2D[i] = new Actuator2DGroup();
					this.Actuators2D[i].SetData( l, elemData );
					i++;
				}
			}
		}
	}
}