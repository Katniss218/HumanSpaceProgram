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
            GetReferenceTransform ??= new ControlParameterOutput<Transform>( GetTransform );
            SetX ??= new ControlleeInput<float>( SetXListener );
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
                .WithMember( "reference_transform", ObjectContext.Ref, o => o.ReferenceTransform )
                .WithMember( "x_actuator_transform", ObjectContext.Ref, o => o.XActuatorTransform )

                .WithMember( "x", o => o.X )
                .WithMember( "min_x", o => o.MinX )
                .WithMember( "max_x", o => o.MaxX )

                .WithMember( "get_reference_transform", o => o.GetReferenceTransform )
                .WithMember( "set_x", o => o.SetX );
        }
    }
}