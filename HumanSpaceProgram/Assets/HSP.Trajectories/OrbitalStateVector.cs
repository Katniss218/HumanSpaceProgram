using UnityEngine;

namespace HSP.Trajectories
{
    /// <summary>
    /// Represents the position and velocity of an object in a gravitational field at a specified point in time. <br/>
    /// This struct is immutable.
    /// </summary>
    public readonly struct OrbitalStateVector
    {
        /// <summary>
        /// The time at which this state vector was captured.
        /// </summary>
        public double UT { get; }

        /// <summary>
        /// The position of the object, in absolute space.
        /// </summary>
        public Vector3Dbl AbsolutePosition { get; }

        /// <summary>
        /// The velocity of the object, in absolute space.
        /// </summary>
        public Vector3Dbl AbsoluteVelocity { get; }

        public OrbitalStateVector( double ut, Vector3Dbl absolutePosition, Vector3Dbl absoluteVelocity )
        {
            this.UT = ut;
            this.AbsolutePosition = absolutePosition;
            this.AbsoluteVelocity = absoluteVelocity;
        }
    }
}