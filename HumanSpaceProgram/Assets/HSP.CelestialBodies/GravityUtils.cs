using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.CelestialBodies
{
    public static class GravityUtils
    {
        public static IEnumerable<(CelestialBody, Vector3Dbl)> GetNBodyGravityAccelerations( Vector3Dbl absolutePosition )
        {
            // method to get the gravitational contribution of every CB individually, mostly for analysis and for auxiliary functionality.

            (CelestialBody, Vector3Dbl)[] acc = new (CelestialBody, Vector3Dbl)[CelestialBodyManager.CelestialBodyCount];
            int i = 0;
            foreach( var body in CelestialBodyManager.CelestialBodies )
            {
                Vector3Dbl toBody = body.ReferenceFrameTransform.AbsolutePosition - absolutePosition;

                double distanceSq = toBody.sqrMagnitude;
                if( distanceSq == 0.0 )
                {
                    continue;
                }

                double accelerationMagnitude = PhysicalConstants.G * (body.Mass / distanceSq);
                acc[i] = (body, toBody.normalized * accelerationMagnitude);
                i++;
            }
            return acc;
        }

        /// <summary>
        /// Calculates the gravitational acceleration at a point in space.
        /// </summary>
        /// <param name="absolutePosition">The position of the point in absolute inertial reference frame.</param>
        /// <returns>The calculated acceleration, in [m/s^2].</returns>
        public static Vector3Dbl GetNBodyGravityAcceleration( Vector3Dbl absolutePosition )
        {
            Vector3Dbl accSum = Vector3Dbl.zero;
            foreach( var body in CelestialBodyManager.CelestialBodies )
            {
                Vector3Dbl toBody = body.ReferenceFrameTransform.AbsolutePosition - absolutePosition;

                double distanceSq = toBody.sqrMagnitude;
                if( distanceSq == 0.0 )
                {
                    continue;
                }

                double accelerationMagnitude = PhysicalConstants.G * (body.Mass / distanceSq);
                accSum += toBody.normalized * accelerationMagnitude;
            }

            return accSum;
        }

        /// <summary>
        /// Calculates the gravitational force at a point in space.
        /// </summary>
        /// <param name="absolutePosition">The position of the point in absolute inertial reference frame.</param>
        /// <returns>The calculated force, in [N].</returns>
        public static Vector3Dbl GetNBodyGravityForce( Vector3Dbl absolutePosition, double objectMass )
        {
            return objectMass * GetNBodyGravityAcceleration( absolutePosition );
        }
    }
}