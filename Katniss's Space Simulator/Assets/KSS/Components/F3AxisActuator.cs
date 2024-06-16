using KSS.Control.Controls;
using KSS.Control;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;
using KSS.Components;

namespace Assets.KSS.Components
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
                YActuatorTransform.localRotation = Quaternion.identity;
                YActuatorTransform.localRotation = Quaternion.Euler( 0, 0, clampedY ) * YActuatorTransform.localRotation;
            }
            if( ZActuatorTransform != null )
            {
                float clampedZ = Mathf.Clamp( Z, MinZ, MaxZ );
                ZActuatorTransform.localRotation = Quaternion.identity;
                ZActuatorTransform.localRotation = Quaternion.Euler( 0, clampedZ, 0 ) * ZActuatorTransform.localRotation;
            }
        }

        [SerializationMappingProvider( typeof( F3AxisActuator ) )]
        public static SerializationMapping F3AxisActuatorMapping()
        {
            return new MemberwiseSerializationMapping<F3AxisActuator>()
            {
                ("reference_transform", new Member<F3AxisActuator, Transform>( ObjectContext.Ref, o => o.ReferenceTransform )),
                ("x_actuator_transform", new Member<F3AxisActuator, Transform>( ObjectContext.Ref, o => o.XActuatorTransform )),
                ("y_actuator_transform", new Member<F3AxisActuator, Transform>( ObjectContext.Ref, o => o.YActuatorTransform )),
                ("z_actuator_transform", new Member<F3AxisActuator, Transform>( ObjectContext.Ref, o => o.ZActuatorTransform )),

                ("x", new Member<F3AxisActuator, float>( o => o.X )),
                ("y", new Member<F3AxisActuator, float>( o => o.Y )),
                ("z", new Member<F3AxisActuator, float>( o => o.Z )),
                ("min_x", new Member<F3AxisActuator, float>( o => o.MinX )),
                ("max_x", new Member<F3AxisActuator, float>( o => o.MaxX )),
                ("min_y", new Member<F3AxisActuator, float>( o => o.MinY )),
                ("max_y", new Member<F3AxisActuator, float>( o => o.MaxY )),
                ("min_z", new Member<F3AxisActuator, float>( o => o.MinZ )),
                ("max_z", new Member<F3AxisActuator, float>( o => o.MaxZ )),

                ("get_reference_transform", new Member<F3AxisActuator, ControlParameterOutput<Transform>>( o => o.GetReferenceTransform )),
                ("set_x", new Member<F3AxisActuator, ControlleeInput<float>>( o => o.SetX )),
                ("set_y", new Member<F3AxisActuator, ControlleeInput<float>>( o => o.SetY )),
                ("set_z", new Member<F3AxisActuator, ControlleeInput<float>>( o => o.SetZ )),
                ("set_xyz", new Member<F3AxisActuator, ControlleeInput<Vector3>>( o => o.SetXYZ ))
            };
        }
    }
}