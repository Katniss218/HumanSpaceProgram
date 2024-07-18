using HSP.ControlSystems;
using HSP.ControlSystems.Controls;
using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Components
{
    public class F2AxisActuator : MonoBehaviour
    {
        /// <summary>
        /// The transform used as a reference (0,0) orientation.
        /// </summary>
        [field: SerializeField]
        public Transform ReferenceTransform { get; set; }

        [NamedControl( "Ref. Transform" )]
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
            if( XActuatorTransform != null )
            {
                float clampedX = Mathf.Clamp( X, MinX, MaxX );
                XActuatorTransform.localRotation = Quaternion.identity;
                XActuatorTransform.localRotation = Quaternion.Euler( clampedX, 0, 0 ) * XActuatorTransform.localRotation;
            }
            if( YActuatorTransform != null )
            {
                float clampedY = Mathf.Clamp( Y, MinY, MaxY );
                if( XActuatorTransform != YActuatorTransform )
                    YActuatorTransform.localRotation = Quaternion.identity;
                YActuatorTransform.localRotation = Quaternion.Euler( 0, 0, clampedY ) * YActuatorTransform.localRotation;
            }
        }

        [MapsInheritingFrom( typeof( F2AxisActuator ) )]
        public static SerializationMapping F2AxisActuatorMapping()
        {
            return new MemberwiseSerializationMapping<F2AxisActuator>()
            {
                ("reference_transform", new Member<F2AxisActuator, Transform>( ObjectContext.Ref, o => o.ReferenceTransform )),
                ("x_actuator_transform", new Member<F2AxisActuator, Transform>( ObjectContext.Ref, o => o.XActuatorTransform )),
                ("y_actuator_transform", new Member<F2AxisActuator, Transform>( ObjectContext.Ref, o => o.YActuatorTransform )),

                ("x", new Member<F2AxisActuator, float>( o => o.X )),
                ("y", new Member<F2AxisActuator, float>( o => o.Y )),
                ("min_x", new Member<F2AxisActuator, float>( o => o.MinX )),
                ("max_x", new Member<F2AxisActuator, float>( o => o.MaxX )),
                ("min_y", new Member<F2AxisActuator, float>( o => o.MinY )),
                ("max_y", new Member<F2AxisActuator, float>( o => o.MaxY )),

                ("get_reference_transform", new Member<F2AxisActuator, ControlParameterOutput<Transform>>( o => o.GetReferenceTransform )),
                ("set_x", new Member<F2AxisActuator, ControlleeInput<float>>( o => o.SetX )),
                ("set_y", new Member<F2AxisActuator, ControlleeInput<float>>( o => o.SetY )),
                ("set_xy", new Member<F2AxisActuator, ControlleeInput<Vector2>>( o => o.SetXY ))
            };
        }
    }
}