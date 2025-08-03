using UnityEngine;

namespace HSP.Trajectories
{
    public interface IAccelerationProvider
    {
        /// <summary>
        /// Gets the acceleration at the specified UT.
        /// </summary>
        public Vector3Dbl GetAcceleration( double ut );

        public double? GetMass( double ut ); // useful for maneuver nodes I guess. We Also need one for staging events?
    }
}