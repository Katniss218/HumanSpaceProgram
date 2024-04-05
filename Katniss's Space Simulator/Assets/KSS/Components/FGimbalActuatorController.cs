using KSS.Control;
using KSS.Control.Controls;
using KSS.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Components
{
    /// <summary>
    /// Controls a number of gimbal actuators.
    /// </summary>
    public class FGimbalActuatorController : MonoBehaviour, IPersistsObjects, IPersistsData
    {
        public class Actuator2DGroup : ControlGroup, IPersistsObjects, IPersistsData
        {
            [NamedControl( "Transform", "The object to use as the coordinate frame of the actuator." )]
            public ControlParameterInput<Transform> GetReferenceTransform = new();

            [NamedControl( "Deflection (XY)" )]
            public ControllerOutput<Vector2> OnSetXY = new();

            public Actuator2DGroup() : base()
            { }

            public SerializedObject GetObjects( IReverseReferenceMap s )
            {
                return new SerializedObject()
                {
                    { "get_reference_transform", s.GetID( GetReferenceTransform ).GetData() },
                    { "on_set_xy", s.GetID( OnSetXY ).GetData() }
                };
            }

            public void SetObjects( SerializedObject data, IForwardReferenceMap l )
            {
                if( data.TryGetValue( "get_reference_transform", out var getReferenceTransform ) )
                {
                    GetReferenceTransform = new();
                    l.SetObj( getReferenceTransform.ToGuid(), GetReferenceTransform );
                }
                if( data.TryGetValue( "on_set_xy", out var onSetXY ) )
                {
                    OnSetXY = new();
                    l.SetObj( onSetXY.ToGuid(), OnSetXY );
                }
            }

            public SerializedData GetData( IReverseReferenceMap s )
            {
                return new SerializedObject()
                {
                    { "on_set_xy", OnSetXY.GetData( s ) }
                };
            }

            public void SetData( SerializedData data, IForwardReferenceMap l )
            {
                if( data.TryGetValue( "on_set_xy", out var onSetXY ) )
                {
                    this.OnSetXY.SetData( onSetXY, l );
                }
            }
        }

        // TODO - make work for both 1-axis, 2-axis, and 3-axis actuators.

        // TODO - certain controllers should be able to be disabled (stop responding to signals)

        /// <summary>
        /// The current steering command in vessel-space. The axes of this vector correspond to rotation around the axes of the vessel.
        /// </summary>
        [field: SerializeField]
        public Vector3 AttitudeCommand { get; set; }

        [NamedControl( "2D Actuators", "Connect to the actuators you want this gimbal controller to control." )]
        public Actuator2DGroup[] Actuators2D = new Actuator2DGroup[5];

        [NamedControl( "Attitude", "Connect to the avionics." )]
        public ControlleeInput<Vector3> SetAttitude;
        private void SetAttitudeListener( Vector3 attitude )
        {
            AttitudeCommand = attitude;
        }

        void Awake()
        {
            SetAttitude = new ControlleeInput<Vector3>( SetAttitudeListener );
        }

        void FixedUpdate()
        {
            IPartObject partObject = this.transform.GetPartObject();
            if( partObject == null )
            {
                return;
            }

            Vector3 worldSteering = partObject.ReferenceTransform.TransformDirection( AttitudeCommand );

            foreach( var actuator in Actuators2D )
            {
                if( actuator == null )
                    continue;

                if( actuator.GetReferenceTransform.TryGet( out Transform transform ) )
                {
                    Vector3 steeringLocal = transform.InverseTransformDirection( worldSteering );
                    Vector2 localDeflection = new Vector2( steeringLocal.x /* pitch */, steeringLocal.z /* yaw */ ); // TODO - support roll in the future.
                    actuator.OnSetXY.TrySendSignal( localDeflection );
                }
            }
        }

        public SerializedObject GetObjects( IReverseReferenceMap s )
        {
            SerializedArray array = new SerializedArray();
            foreach( var act in Actuators2D )
            {
                array.Add( act == null ? null : act.GetObjects( s ) );
            }

            return new SerializedObject()
            {
                { "actuators_2d", array },
                { "set_attitude", s.GetID( SetAttitude ).GetData() }
            };
        }

        public void SetObjects( SerializedObject data, IForwardReferenceMap l )
        {
            if( data.TryGetValue<SerializedArray>( "actuators_2d", out var actuators2D ) )
            {
                Actuators2D = actuators2D.Cast<SerializedObject>().Select( act =>
                {
                    if( act == null )
                        return null;

                    var ret = new Actuator2DGroup();
                    ret.SetObjects( act, l );
                    return ret;

                } ).ToArray();
            }

            if( data.TryGetValue( "set_attitude", out var setAttitude ) )
            {
                SetAttitude = new( SetAttitudeListener );
                l.SetObj( setAttitude.ToGuid(), SetAttitude );
            }
        }

        public SerializedData GetData( IReverseReferenceMap s )
        {
            SerializedObject ret = (SerializedObject)IPersistent_Behaviour.GetData( this, s );

            SerializedArray array = new SerializedArray();
            foreach( var act in Actuators2D )
            {
                SerializedObject elemData = act == null
                    ? null
                    : new SerializedObject()
                {
                    { "on_set_xy", act.OnSetXY.GetData( s ) },
                };
                array.Add( elemData );
            }

            ret.AddAll( new SerializedObject()
            {
                { "actuators_2d", array },
            } );

            return ret;
        }

        public void SetData( SerializedData data, IForwardReferenceMap l )
        {
            IPersistent_Behaviour.SetData( this, data, l );

            if( data.TryGetValue( "actuators_2d", out var actuators2D ) )
            {
                // This is a bit too verbose imo.
                // needs an extension method to make serializing/deserializing arrays/lists of any type cleaner.
                int i = 0;
                foreach( var elemData in (SerializedArray)actuators2D )
                {
                    if( elemData != null )
                    {
                        this.Actuators2D[i]?.SetData( elemData, l );
                    }
                    i++;
                }
            }
        }
    }
}