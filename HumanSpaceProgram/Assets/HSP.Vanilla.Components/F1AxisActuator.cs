using HSP.ControlSystems;
using HSP.ControlSystems.Controls;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Components
{
    public class F1AxisActuator : MonoBehaviour
    {
        /// <summary>
        /// The transform used as a reference (0) orientation.
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

        public float X { get; set; }
        
		[field: SerializeField]
        public float MinX { get; set; } = -5f;
		[field: SerializeField]
        public float MaxX { get; set; } = 5f;
        
        [NamedControl( "Deflection (X)" )]
        public ControlleeInput<float> SetX;
        private void SetXListener( float x )
        {
            this.X = x;
        }

        void Awake()
        {
            GetReferenceTransform = new ControlParameterOutput<Transform>( GetTransform );
            SetX = new ControlleeInput<float>( SetXListener );
        }

        void FixedUpdate()
        {
            if( XActuatorTransform != null )
            {
                float clampedX = Mathf.Clamp( X, MinX, MaxX );
                XActuatorTransform.localRotation = Quaternion.identity;
                XActuatorTransform.localRotation = Quaternion.Euler( clampedX, 0, 0 ) * XActuatorTransform.localRotation;
            }
        }

        [MapsInheritingFrom( typeof( F1AxisActuator ) )]
        public static SerializationMapping F1AxisActuatorMapping()
        {
            return new MemberwiseSerializationMapping<F1AxisActuator>()
            {
                ("reference_transform", new Member<F1AxisActuator, Transform>( ObjectContext.Ref, o => o.ReferenceTransform )),
                ("x_actuator_transform", new Member<F1AxisActuator, Transform>( ObjectContext.Ref, o => o.XActuatorTransform )),

                ("x", new Member<F1AxisActuator, float>( o => o.X )),
                ("min_x", new Member<F1AxisActuator, float>( o => o.MinX )),
                ("max_x", new Member<F1AxisActuator, float>( o => o.MaxX )),

                ("get_reference_transform", new Member<F1AxisActuator, ControlParameterOutput<Transform>>( o => o.GetReferenceTransform )),
                ("set_x", new Member<F1AxisActuator, ControlleeInput<float>>( o => o.SetX ))
            };
        }
    }
}