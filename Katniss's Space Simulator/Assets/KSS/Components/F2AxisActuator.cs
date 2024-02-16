using KSS.Control;
using KSS.Control.Controls;
using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Components
{
	public class F2AxisActuator : MonoBehaviour, IPersistent
	{
		// 2-axis actuator.

		// reference direction = parent direction.

		/// <summary>
		/// The transform used as a reference (0,0) orientation.
		/// </summary>
		[field: SerializeField]
		public Transform ReferenceTransform { get; set; }

		[NamedControl( "Coordinate Space Transform" )]
		public ControlParameterOutput<Transform> GetReferenceTransform;
		public Transform GetTransform()
		{
			return ReferenceTransform;
		}

		[field: SerializeField]
		public Transform XActuatorTransform { get; set; }
		
		[field: SerializeField]
		public Transform YActuatorTransform { get; set; }

		public float X { get; set; }
		public float Y { get; set; }

		[field: SerializeField]
		public float MinX { get; set; } = -5f;
		[field: SerializeField]
		public float MaxX { get; set; } = 5f;
		[field: SerializeField]
		public float MinY { get; set; } = -5f;
		[field: SerializeField]
		public float MaxY { get; set; } = 5f;

		[NamedControl( "Deflection (X)" )]
		public ControlleeInput<float> SetX;
		private void SetXListener( float x )
		{
			this.X = x;
		}

		[NamedControl( "Deflection (Y)" )]
		public ControlleeInput<float> SetY;
		private void SetYListener( float y )
		{
			this.Y = y;
		}

		[NamedControl( "Deflection (XY)" )]
		public ControlleeInput<Vector2> SetXY;
		private void SetXYListener( Vector2 xy )
		{
			this.X = xy.x;
			this.Y = xy.y;
		}

		void Awake()
		{
			GetReferenceTransform = new ControlParameterOutput<Transform>( GetTransform );
			SetX = new ControlleeInput<float>( SetXListener );
			SetY = new ControlleeInput<float>( SetYListener );
			SetXY = new ControlleeInput<Vector2>( SetXYListener );
		}

		void FixedUpdate()
		{
			float clampedX = Mathf.Clamp( X, MinX, MaxX );
			float clampedY = Mathf.Clamp( Y, MinY, MaxY );

			XActuatorTransform.localRotation = Quaternion.identity;
			YActuatorTransform.localRotation = Quaternion.identity;
			XActuatorTransform.localRotation = Quaternion.Euler( clampedX, 0, 0 ) * XActuatorTransform.localRotation;
			YActuatorTransform.localRotation = Quaternion.Euler( 0, 0, clampedY ) * YActuatorTransform.localRotation;
		}

		public SerializedData GetData( IReverseReferenceMap s )
		{
			throw new NotImplementedException();
		}

		public void SetData( IForwardReferenceMap l, SerializedData data )
		{
			throw new NotImplementedException();
		}
	}
}