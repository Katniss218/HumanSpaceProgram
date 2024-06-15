using KSS.Control.Controls;
using KSS.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Components
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

        [SerializationMappingProvider( typeof( F1AxisActuator ) )]
        public static SerializationMapping F1AxisActuatorMapping()
        {
            return new MemberwiseSerializationMapping<F1AxisActuator>()
            {
                ("reference_transform", new Member<F1AxisActuator, Transform>( ObjectContext.Ref, o => o.ReferenceTransform )),
                ("x_actuator_transform", new Member<F1AxisActuator, Transform>( ObjectContext.Ref, o => o.XActuatorTransform )),

                ("x", new Member<F1AxisActuator, float>( o => o.X )),
                ("min_x", new Member<F1AxisActuator, float>( o => o.MinX )),
                ("max_x", new Member<F1AxisActuator, float>( o => o.MaxX )),

                ("set_x", new Member<F1AxisActuator, ControlleeInput<float>>( o => o.SetX ))
            };
        }

#warning TODO - finish.
        /*
        public SerializedObject GetObjects( IReverseReferenceMap s )
        {
            return new SerializedObject()
            {
                { "set_x", s.GetID( SetX ).GetData() },
                { "get_reference_transform", s.GetID( GetReferenceTransform ).GetData() }
            };
        }

        public void SetObjects( SerializedObject data, IForwardReferenceMap l )
        {
            if( data.TryGetValue( "set_x", out var setX ) )
            {
                SetX = new( SetXListener );
                l.SetObj( setX.AsGuid(), SetX );
            }

            if( data.TryGetValue( "get_reference_transform", out var getReferenceTransform ) )
            {
                GetReferenceTransform = new( GetTransform );
                l.SetObj( getReferenceTransform.AsGuid(), GetReferenceTransform );
            }
        }

        public SerializedData GetData( IReverseReferenceMap s )
        {
            SerializedObject ret = (SerializedObject)IPersistent_Behaviour.GetData( this, s );

            SetX ??= new ControlleeInput<float>( SetXListener );

            ret.AddAll( new SerializedObject()
            {
                { "reference_transform", s.WriteObjectReference( this.ReferenceTransform ) },
                { "x_actuator_transform", s.WriteObjectReference( this.XActuatorTransform ) },
                { "min_x", this.MinX.AsSerialized() },
                { "max_x", this.MaxX.AsSerialized() },
                { "get_reference_transform", this.GetReferenceTransform.GetData( s ) },
                { "set_x", this.SetX.GetData( s ) }
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

            if( data.TryGetValue( "min_x", out var minX ) )
                this.MinX = minX.AsFloat();
            if( data.TryGetValue( "max_x", out var maxX ) )
                this.MaxX = maxX.AsFloat();

            if( data.TryGetValue( "get_reference_transform", out var getReferenceTransform ) )
                this.GetReferenceTransform.SetData( getReferenceTransform, l );

            if( data.TryGetValue( "set_x", out var setX ) )
                this.SetX.SetData( setX, l );
        }*/
    }
}