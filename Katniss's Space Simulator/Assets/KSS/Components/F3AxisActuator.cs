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

namespace Assets.KSS.Components
{
	public class F3AxisActuator : MonoBehaviour, IPersistsObjects, IPersistsData
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
            float clampedX = Mathf.Clamp( X, MinX, MaxX );
            float clampedY = Mathf.Clamp( Y, MinY, MaxY );
            float clampedZ = Mathf.Clamp( Z, MinZ, MaxZ );

            XActuatorTransform.localRotation = Quaternion.identity;
            YActuatorTransform.localRotation = Quaternion.identity;
            ZActuatorTransform.localRotation = Quaternion.identity;
            XActuatorTransform.localRotation = Quaternion.Euler( clampedX, 0, 0 ) * XActuatorTransform.localRotation;
            YActuatorTransform.localRotation = Quaternion.Euler( 0, 0, clampedY ) * YActuatorTransform.localRotation;
            ZActuatorTransform.localRotation = Quaternion.Euler( 0, clampedZ, 0 ) * ZActuatorTransform.localRotation;
        }

        public SerializedObject GetObjects( IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { "set_x", s.GetID( SetX ).GetData() },
                { "set_y", s.GetID( SetY ).GetData() },
                { "set_z", s.GetID( SetZ ).GetData() },
                { "set_xyz", s.GetID( SetXYZ ).GetData() },
                { "get_reference_transform", s.GetID( GetReferenceTransform ).GetData() }
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
            
            if( data.TryGetValue( "set_z", out var setZ ) )
            {
                SetZ = new( SetZListener );
                l.SetObj( setZ.ToGuid(), SetZ );
            }

            if( data.TryGetValue( "set_xyz", out var setXYZ ) )
            {
                SetXYZ = new( SetXYZListener );
                l.SetObj( setXYZ.ToGuid(), SetXYZ );
            }

            if( data.TryGetValue( "get_reference_transform", out var getReferenceTransform ) )
            {
                GetReferenceTransform = new( GetTransform );
                l.SetObj( getReferenceTransform.ToGuid(), GetReferenceTransform );
            }
        }

        public SerializedData GetData( IReverseReferenceMap s )
		{
            SerializedObject ret = (SerializedObject)IPersistent_Behaviour.GetData( this, s );

            SetX ??= new ControlleeInput<float>( SetXListener );
            SetY ??= new ControlleeInput<float>( SetYListener );
            SetZ ??= new ControlleeInput<float>( SetZListener );
            SetXYZ ??= new ControlleeInput<Vector3>( SetXYZListener );

            ret.AddAll( new SerializedObject()
            {
                { "reference_transform", s.WriteObjectReference( this.ReferenceTransform ) },
                { "x_actuator_transform", s.WriteObjectReference( this.XActuatorTransform ) },
                { "y_actuator_transform", s.WriteObjectReference( this.YActuatorTransform ) },
                { "z_actuator_transform", s.WriteObjectReference( this.ZActuatorTransform ) },
                { "min_x", this.MinX },
                { "min_y", this.MinY },
                { "min_z", this.MinZ },
                { "max_x", this.MaxX },
                { "max_y", this.MaxY },
                { "max_z", this.MaxZ },
                { "get_reference_transform", this.GetReferenceTransform.GetData( s ) },
                { "set_x", this.SetX.GetData( s ) },
                { "set_y", this.SetY.GetData( s ) },
                { "set_z", this.SetZ.GetData( s ) },
                { "set_xyz", this.SetXYZ.GetData( s ) }
            } );

            return ret;
		}

		public void SetData( SerializedData data, IForwardReferenceMap l )
		{
            IPersistent_Behaviour.SetData( this, data, l );

            if( data.TryGetValue( "reference_transform", out var referenceTransform ) )
                this.ReferenceTransform = l.ReadObjectReference( referenceTransform ) as Transform;

            if( data.TryGetValue( "x_actuator_transform", out var xActuatorTransform ) )
                this.XActuatorTransform = l.ReadObjectReference( xActuatorTransform ) as Transform;
            if( data.TryGetValue( "y_actuator_transform", out var yActuatorTransform ) )
                this.YActuatorTransform = l.ReadObjectReference( yActuatorTransform ) as Transform;
            if( data.TryGetValue( "z_actuator_transform", out var zActuatorTransform ) )
                this.ZActuatorTransform = l.ReadObjectReference( zActuatorTransform ) as Transform;

            if( data.TryGetValue( "min_x", out var minX ) )
                this.MinX = (float)minX;
            if( data.TryGetValue( "min_y", out var minY ) )
                this.MinY = (float)minY;
            if( data.TryGetValue( "min_z", out var minZ ) )
                this.MinZ = (float)minZ;
            if( data.TryGetValue( "max_x", out var maxX ) )
                this.MaxX = (float)maxX;
            if( data.TryGetValue( "max_y", out var maxY ) )
                this.MaxY = (float)maxY;
            if( data.TryGetValue( "max_z", out var maxZ ) )
                this.MaxZ = (float)maxZ;

            if( data.TryGetValue( "get_reference_transform", out var getReferenceTransform ) )
                this.GetReferenceTransform.SetData( getReferenceTransform, l );

            if( data.TryGetValue( "set_x", out var setX ) )
                this.SetX.SetData( setX, l );
            if( data.TryGetValue( "set_y", out var setY ) )
                this.SetY.SetData( setY, l );
            if( data.TryGetValue( "set_z", out var setZ ) )
                this.SetZ.SetData( setZ, l );
            if( data.TryGetValue( "set_xyz", out var setXYZ ) )
                this.SetXYZ.SetData( setXYZ, l );
        }
	}
}