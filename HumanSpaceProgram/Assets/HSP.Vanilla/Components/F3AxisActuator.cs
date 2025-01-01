using HSP.ControlSystems;
using HSP.ControlSystems.Controls;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Components
{
    public class F3AxisActuator : MonoBehaviour
    {
        /// <summary>
        /// The transform used as a reference (0,0,0) orientation.
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

        [field: SerializeField]
        public Transform ZActuatorTransform { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        [field: SerializeField]
        public float MinX { get; set; } = -5f;
        [field: SerializeField]
        public float MaxX { get; set; } = 5f;
        [field: SerializeField]
        public float MinY { get; set; } = -5f;
        [field: SerializeField]
        public float MaxY { get; set; } = 5f;
        [field: SerializeField]
        public float MinZ { get; set; } = -5f;
        [field: SerializeField]
        public float MaxZ { get; set; } = 5f;

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

        [NamedControl( "Deflection (Z)" )]
        public ControlleeInput<float> SetZ;
        private void SetZListener( float z )
        {
            this.Z = z;
        }

        [NamedControl( "Deflection (XYZ)" )]
        public ControlleeInput<Vector3> SetXYZ;
        private void SetXYZListener( Vector3 xyz )
        {
            this.X = xyz.x;
            this.Y = xyz.y;
            this.Z = xyz.z;
        }

        void Awake()
        {
            GetReferenceTransform = new ControlParameterOutput<Transform>( GetTransform );
            SetX = new ControlleeInput<float>( SetXListener );
            SetY = new ControlleeInput<float>( SetYListener );
            SetZ = new ControlleeInput<float>( SetZListener );
            SetXYZ = new ControlleeInput<Vector3>( SetXYZListener );
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
                if( YActuatorTransform != XActuatorTransform )
                    YActuatorTransform.localRotation = Quaternion.identity;
                YActuatorTransform.localRotation = Quaternion.Euler( 0, 0, clampedY ) * YActuatorTransform.localRotation;
            }
            if( ZActuatorTransform != null )
            {
                float clampedZ = Mathf.Clamp( Z, MinZ, MaxZ );
                if( ZActuatorTransform != YActuatorTransform && ZActuatorTransform != XActuatorTransform )
                    ZActuatorTransform.localRotation = Quaternion.identity;
                ZActuatorTransform.localRotation = Quaternion.Euler( 0, clampedZ, 0 ) * ZActuatorTransform.localRotation;
            }
        }

        [MapsInheritingFrom( typeof( F3AxisActuator ) )]
        public static SerializationMapping F3AxisActuatorMapping()
        {
            return new MemberwiseSerializationMapping<F3AxisActuator>()
                .WithMember( "reference_transform", ObjectContext.Ref, o => o.ReferenceTransform )
                .WithMember( "x_actuator_transform", ObjectContext.Ref, o => o.XActuatorTransform )
                .WithMember( "y_actuator_transform", ObjectContext.Ref, o => o.YActuatorTransform )
                .WithMember( "z_actuator_transform", ObjectContext.Ref, o => o.ZActuatorTransform )

                .WithMember( "x", o => o.X )
                .WithMember( "y", o => o.Y )
                .WithMember( "z", o => o.Z )
                .WithMember( "min_x", o => o.MinX )
                .WithMember( "max_x", o => o.MaxX )
                .WithMember( "min_y", o => o.MinY )
                .WithMember( "max_y", o => o.MaxY )
                .WithMember( "min_z", o => o.MinZ )
                .WithMember( "max_z", o => o.MaxZ )

                .WithMember( "get_reference_transform", o => o.GetReferenceTransform )
                .WithMember( "set_x", o => o.SetX )
                .WithMember( "set_y", o => o.SetY )
                .WithMember( "set_z", o => o.SetZ )
                .WithMember( "set_xyz", o => o.SetXYZ );
        }
    }
}