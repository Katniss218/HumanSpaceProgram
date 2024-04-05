using KSS.Control;
using KSS.Control.Controls;
using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Components
{
	public class F2AxisActuator : MonoBehaviour, IPersistsObjects, IPersistsData
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

        public SerializedObject GetObjects( IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { "set_x", s.GetID( SetX ).GetData() },
                { "set_y", s.GetID( SetY ).GetData() },
                { "set_xy", s.GetID( SetXY ).GetData() },
            };
        }

        public void SetObjects( SerializedObject data, IForwardReferenceMap l )
        {
            if( data.TryGetValue( "set_x", out var setX ) )
            {
                SetX = new( SetXListener );
                l.SetObj( setX.ToGuid(), SetX );
            }

            if( data.TryGetValue( "set_y", out var setY ) )
            {
                SetY = new( SetYListener );
                l.SetObj( setY.ToGuid(), SetY );
            }

            if( data.TryGetValue( "set_xy", out var setXY ) )
            {
                SetXY = new( SetXYListener );
                l.SetObj( setXY.ToGuid(), SetXY );
            }
        }

        public SerializedData GetData( IReverseReferenceMap s )
        {
            SerializedObject ret = (SerializedObject)IPersistent_Behaviour.GetData( this, s );

            ret.AddAll( new SerializedObject()
            {
				{ "reference_transform", s.WriteObjectReference( this.ReferenceTransform ) },
				{ "x_actuator_transform", s.WriteObjectReference( this.XActuatorTransform ) },
				{ "y_actuator_transform", s.WriteObjectReference( this.YActuatorTransform ) },
				{ "min_x", this.MinX },
				{ "min_y", this.MinY },
				{ "max_x", this.MaxX },
				{ "max_y", this.MaxY }
			} );

			return ret;
		}

		public void SetData( SerializedData data, IForwardReferenceMap l )
		{
			IPersistent_Behaviour.SetData( this, data, l );

			if( data.TryGetValue("reference_transform", out var referenceTransform ) )
				this.ReferenceTransform = l.ReadObjectReference( referenceTransform ) as Transform;

			if( data.TryGetValue("x_actuator_transform", out var xActuatorTransform ) )
				this.XActuatorTransform = l.ReadObjectReference( xActuatorTransform ) as Transform;
			if( data.TryGetValue("y_actuator_transform", out var yActuatorTransform ) )
				this.YActuatorTransform = l.ReadObjectReference( yActuatorTransform ) as Transform;

			if( data.TryGetValue( "min_x", out var minX ) )
				this.MinX = (float)minX;
			if( data.TryGetValue( "min_y", out var minY ) )
				this.MinY = (float)minY;
			if( data.TryGetValue( "max_x", out var maxX ) )
				this.MaxX = (float)maxX;
			if( data.TryGetValue( "max_y", out var maxY ) )
				this.MaxY = (float)maxY;
		}
	}
}