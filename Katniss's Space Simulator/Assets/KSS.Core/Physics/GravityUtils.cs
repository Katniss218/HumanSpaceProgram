using System;
using System.Linq;
using UnityEngine;

namespace KSS.Core.Physics
{
    public static class GravityUtils
    {
        /// <summary>
        /// Gravitational constant G [N * kg^-2 * m^2].
        /// </summary>
        public const double G = 6.67430e-11;

        /// <summary>
        /// Calculates the gravitational acceleration at a point in space.
        /// </summary>
        /// <param name="airfPosition">The position of the point in absolute inertial reference frame.</param>
        /// <returns>The calculated acceleration, in [m/s^2].</returns>
        public static Vector3Dbl GetNBodyGravityAcceleration( Vector3Dbl airfPosition )
        {
            CelestialBody[] bodies = CelestialBodyManager.GetAll(); // this can be optimized.

            Vector3Dbl accSum = Vector3Dbl.zero;
            foreach( var body in bodies )
            {
                Vector3Dbl toBody = body.AIRFPosition - airfPosition;

                double distanceSq = toBody.sqrMagnitude;
                if( distanceSq == 0.0 )
                {
                    continue;
                }

                double forceMagn = G * (body.Mass / distanceSq);
                accSum += toBody.normalized * forceMagn;
            }

            return accSum;
        }

        /// <summary>
        /// Calculates the gravitational force at a point in space.
        /// </summary>
        /// <param name="airfPosition">The position of the point in absolute inertial reference frame.</param>
        /// <returns>The calculated force, in [N].</returns>
        public static Vector3Dbl GetNBodyGravityForce( Vector3Dbl airfPosition, double objectMass )
        {
            return objectMass * GetNBodyGravityAcceleration( airfPosition );
        }
    }
}