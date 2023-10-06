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
        public static Vector3Dbl GetGravityAcceleration( Vector3Dbl airfPosition )
        {
            CelestialBody cb = CelestialBodyManager.CelestialBodies[0]; // temporary.

            Vector3Dbl toBody = cb.AIRFPosition - airfPosition;

            double distanceSq = toBody.sqrMagnitude;
            if( distanceSq == 0.0 )
            {
                return Vector3Dbl.zero;
            }

            double forceMagn = G * (cb.Mass / distanceSq);

            return toBody.normalized * forceMagn;
        }

        /// <summary>
        /// Calculates the gravitational force at a point in space.
        /// </summary>
        /// <param name="airfPosition">The position of the point in absolute inertial reference frame.</param>
        /// <returns>The calculated force, in [N].</returns>
        public static Vector3Dbl GetGravityForce( double objectMass, Vector3Dbl airfPosition )
        {
            return objectMass * GetGravityAcceleration( airfPosition );
        }
    }
}