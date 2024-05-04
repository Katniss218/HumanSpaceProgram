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
        public class Actuator2DGroup : ControlGroup, IPersistsObjects
        {
            [NamedControl( "Ref. Transform", "Connect this to the actuator's reference transform parameter." )]
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
                    l.SetObj( getReferenceTransform.AsGuid(), GetReferenceTransform );
                }
                if( data.TryGetValue( "on_set_xy", out var onSetXY ) )
                {
                    OnSetXY = new();
                    l.SetObj( onSetXY.AsGuid(), OnSetXY );
                }
            }
        }

        /// <summary>
        /// The current steering command in vessel-space. The axes of this vector correspond to rotation around the axes of the vessel.
        /// </summary>
        [field: SerializeField]
        public Vector3 AttitudeCommand { get; set; }

        [NamedControl( "2D Actuators", "Connect to the actuators you want this gimbal controller to control." )]
        public Actuator2DGroup[] Actuators2D = new Actuator2DGroup[5];

        [NamedControl( "Attitude", "Connect to the controller's attitude output." )]
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
            Vessel vessel = this.transform.GetVessel();
            if( vessel == null )
            {
                return;
            }

            Vector3 worldSteering = vessel.ReferenceTransform.TransformDirection( AttitudeCommand );

            foreach( var actuator in Actuators2D )
            {
                if( actuator == null )
                    continue;

                if( actuator.GetReferenceTransform.TryGet( out Transform engineReferenceTransform ) )
                {
                    // Pitch and Yaw in engine's local space directly correspond to the x and y axes of the steering command in engine's local space.
                    // Roll is a bit more difficult.
                    // - For roll, take a vector towards the vessel's CoM in engine's local space, and discard the axial component
                    //   (in this case z because reference transform's axes are deflection axes).

                    Vector3 engineSpaceSteeringCmd = engineReferenceTransform.InverseTransformDirection( worldSteering );

                    Vector3 engineSpaceCoM = engineReferenceTransform.InverseTransformPoint( vessel.ReferenceTransform.TransformPoint( vessel.PhysicsObject.LocalCenterOfMass ) );

                    Vector2 rollDeflectionDir = new Vector2( engineSpaceCoM.x, engineSpaceCoM.y ).normalized;
                    rollDeflectionDir *= engineSpaceSteeringCmd.y;

                    // Combine the pitch/yaw and roll steering commands.
                    Vector2 localDeflection = new Vector2(
                        engineSpaceSteeringCmd.x /* pitch */ + rollDeflectionDir.x /* roll */,
                        engineSpaceSteeringCmd.z /* yaw   */ + rollDeflectionDir.y /* roll */ );

                    // Sign flip to make attitude commands have a left-handed screw (positive attitude command rotates in positive direction along its axis).
                    actuator.OnSetXY.TrySendSignal( -localDeflection );
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
                l.SetObj( setAttitude.AsGuid(), SetAttitude );
            }
        }

        public SerializedData GetData( IReverseReferenceMap s )
        {
            SerializedObject ret = (SerializedObject)IPersistent_Behaviour.GetData( this, s );

            SetAttitude ??= new ControlleeInput<Vector3>( SetAttitudeListener );

            ret.AddAll( new SerializedObject()
            {
                { "set_attitude", this.SetAttitude.GetData( s ) }
            } );

            return ret;
        }

        public void SetData( SerializedData data, IForwardReferenceMap l )
        {
            IPersistent_Behaviour.SetData( this, data, l );

            if( data.TryGetValue( "set_attitude", out var setAttitude ) )
                this.SetAttitude.SetData( setAttitude, l );
        }
    }
}