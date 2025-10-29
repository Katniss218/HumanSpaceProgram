using HSP.ReferenceFrames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.Aerodynamics
{
    /// <summary>
    /// Add this to a GameObject to enable simple aerodynamic force calculations.
    /// </summary>
    public class SimpleAerodynamicIntegrator : MonoBehaviour
    {
        public double DragCoefficient { get; set; }

        public double ReferenceArea { get; set; }

        IReferenceFrameTransform referenceFrameTransform;
        IPhysicsTransform physicsTransform;

        void Awake()
        {
            referenceFrameTransform = this.GetComponent<IReferenceFrameTransform>();
            physicsTransform = this.GetComponent<IPhysicsTransform>();
        }

        void FixedUpdate()
        {
            bool inAtmosphere = HSP.Spatial.SpatialAtmosphere.EvaluatePoint( referenceFrameTransform.Position, out HSP.Spatial.AtmosphereData atmosphereData );
            if( inAtmosphere )
            {
                Vector3 sceneForce = CalculateNetAerodynamicForce( atmosphereData );
                physicsTransform.AddForce( sceneForce );
            }
        }

        Vector3 CalculateNetAerodynamicForce( HSP.Spatial.AtmosphereData atmosphereData )
        {
            Vector3 velocity = referenceFrameTransform.Velocity;
            Vector3 relativeWind = velocity - atmosphereData.WindVelocity;

            float speed = relativeWind.magnitude;
            if( speed < 0.01 )
                return Vector3.zero;

            // Drag force: Fd = 0.5 * rho * v^2 * Cd * A
            float dragMagnitude = (float)(0.5 * atmosphereData.Density * speed * speed * DragCoefficient * ReferenceArea);

            Vector3 dragDirection = -relativeWind.normalized;
            Vector3 dragForce = dragDirection * dragMagnitude;

            return dragForce;
        }
    }
}