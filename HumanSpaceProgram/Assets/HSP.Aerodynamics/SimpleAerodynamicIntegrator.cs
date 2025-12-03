using HSP.ReferenceFrames;
using HSP.Time;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Aerodynamics
{
    /// <summary>
    /// Add this to a GameObject to enable simple aerodynamic force calculations.
    /// </summary>
    public class SimpleAerodynamicIntegrator : MonoBehaviour
    {
        public double DragCoefficient { get; set; }

        public double ReferenceArea { get; set; }

        private IPhysicsTransform _physicsTransform;
        /// <summary>
        /// Gets the physics transform associated with this game object.
        /// </summary>
        public IPhysicsTransform PhysicsTransform
        {
            get
            {
                if( _physicsTransform.IsUnityNull() )
                {
                    _physicsTransform = this.GetComponent<IPhysicsTransform>();
                }
                return _physicsTransform;
            }
        }

        private IReferenceFrameTransform _referenceFrameTransform;
        /// <summary>
        /// Gets the reference frame transform associated with this game object.
        /// </summary>
        public IReferenceFrameTransform ReferenceFrameTransform
        {
            get
            {
                if( _referenceFrameTransform.IsUnityNull() )
                {
                    _referenceFrameTransform = this.GetComponent<IReferenceFrameTransform>();
                }
                return _referenceFrameTransform;
            }
        }

        void FixedUpdate()
        {
            bool inAtmosphere = HSP.Spatial.SpatialAtmosphere.EvaluatePoint( ReferenceFrameTransform.Position, out HSP.Spatial.AtmosphereData atmosphereData );
            if( inAtmosphere )
            {
                Vector3 sceneForce = CalculateNetAerodynamicForce( atmosphereData );
                PhysicsTransform.AddForce( sceneForce );
                if( this.gameObject.name == "a" )
                {
                    Debug.Log( TimeManager.UT + " : " + atmosphereData.Pressure + " : " + atmosphereData.Density + " : " + this.gameObject.name );
                }
            }
        }

        Vector3 CalculateNetAerodynamicForce( HSP.Spatial.AtmosphereData atmosphereData )
        {
            Vector3 velocity = ReferenceFrameTransform.Velocity;
            Vector3 relativeWind = velocity - atmosphereData.WindVelocity;
            float speed = relativeWind.magnitude;
            if( speed < 0.01 )
                return Vector3.zero;
#warning TODO it jolts at times. Possibly due to large opposing forces/numerical precision / rounding errors.
            float dragMagnitude = (float)(0.5 * atmosphereData.Density * speed * speed * DragCoefficient * ReferenceArea);

            Vector3 dragDirection = -relativeWind.normalized;
            Vector3 dragForce = dragDirection * dragMagnitude;

            return dragForce;
        }


        [MapsInheritingFrom( typeof( SimpleAerodynamicIntegrator ) )]
        public static SerializationMapping SimpleAerodynamicIntegratorMapping()
        {
            return new MemberwiseSerializationMapping<SimpleAerodynamicIntegrator>()
                .WithMember( "drag_coefficient", o => o.DragCoefficient )
                .WithMember( "reference_area", o => o.ReferenceArea );
        }
    }
}