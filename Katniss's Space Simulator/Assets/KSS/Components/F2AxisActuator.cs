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
        public Transform ActuatorTransform { get; set; }

        public float X { get; set; }
        public float Y { get; set; }

        public float MinX { get; set; }
        public float MaxX { get; set; }
        public float MinY { get; set; }
        public float MaxY { get; set; }

        [NamedControl( "Set X" )]
        private ControlleeInput<float> SetX;
        public void OnSetX( float x )
        {
            this.X = x;
        }

        [NamedControl( "Set Y" )]
        private ControlleeInput<float> SetY;
        public void OnSetY( float y )
        {
            this.Y = y;
        }
        
        [NamedControl( "Set XY" )]
        private ControlleeInput<Vector2> SetXY;
        public void OnSetXY( Vector2 xy )
        {
            this.X = xy.x;
            this.Y = xy.y;
        }

        void Awake()
        {
            GetReferenceTransform = new ControlParameterOutput<Transform>( GetTransform );
            SetX = new ControlleeInput<float>( OnSetX );
            SetY = new ControlleeInput<float>( OnSetY );
        }

        void FixedUpdate()
        {
            this.X = Mathf.Clamp( X, MinX, MaxX );
            this.Y = Mathf.Clamp( Y, MinY, MaxY );
            ActuatorTransform.rotation = Quaternion.Euler( X, Y, 0 );
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