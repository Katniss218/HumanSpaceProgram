using HSP.ControlSystems;
using HSP.ControlSystems.Controls;
using HSP.Vessels;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Components
{
    /// <summary>
    /// Controls a number of gimbal actuators.
    /// </summary>
    public class FGimbalActuatorController : MonoBehaviour
    {
        public class Actuator2DGroup : ControlGroup
        {
            [NamedControl( "Ref. Transform", "Connect this to the actuator's reference transform parameter." )]
            public ControlParameterInput<Transform> GetReferenceTransform = new();

            [NamedControl( "Deflection (XY)" )]
            public ControllerOutput<Vector2> OnSetXY = new();

            public Actuator2DGroup() : base()
            { }


            [MapsInheritingFrom( typeof( Actuator2DGroup ) )]
            public static SerializationMapping Actuator2DGroupMapping()
            {
                return new MemberwiseSerializationMapping<Actuator2DGroup>()
                {
                    ("get_reference_transform", new Member<Actuator2DGroup, ControlParameterInput<Transform>>( o => o.GetReferenceTransform )),
                    ("on_set_xy", new Member<Actuator2DGroup, ControllerOutput<Vector2>>( o => o.OnSetXY ))
                };
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

                    Vector3 engineSpaceCoM = engineReferenceTransform.InverseTransformPoint( vessel.ReferenceTransform.TransformPoint( vessel.PhysicsTransform.LocalCenterOfMass ) );

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

        [MapsInheritingFrom( typeof( FGimbalActuatorController ) )]
        public static SerializationMapping FGimbalActuatorControllerMapping()
        {
            return new MemberwiseSerializationMapping<FGimbalActuatorController>()
            {
                ("actuators_2d", new Member<FGimbalActuatorController, Actuator2DGroup[]>( o => o.Actuators2D )),
#warning TODO - OR, another possibility is that it's saved correctly, but the prefab has it as null, and when the prefab is saved to save the part, the rest is created by the default initializer, but Awake is never called.
                ("set_attitude", new Member<FGimbalActuatorController, ControlleeInput<Vector3>>( o => o.SetAttitude ))
            };
        }
    }
}