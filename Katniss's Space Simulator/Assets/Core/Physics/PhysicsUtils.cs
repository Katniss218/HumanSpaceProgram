using UnityEngine;

namespace KatnisssSpaceSimulator.Core.Physics
{
    public static class PhysicsUtils
    {
        /// <summary>
        /// Gravitational constant G [N * kg^-2 * m^2].
        /// </summary>
        public const double G = 6.67430e-11;

        public static Vector3Dbl GetGravityForce( double vesselMass, Vector3Dbl airfPosition )
        {
            CelestialBody cb = CelestialBodyManager.Bodies[0]; // temporary.

            Vector3Dbl toBody = cb.AIRFPosition - airfPosition;

            double distanceSq = toBody.sqrMagnitude;
            if( distanceSq == 0.0 )
            {
                return Vector3Dbl.zero;
            }

            double forceMagn = G * ((vesselMass * cb.Mass) / distanceSq);

            return toBody.normalized * forceMagn;
        }
    }
}