using HSP.ReferenceFrames;
using UnityEngine;

namespace HSP.Trajectories
{
    /// <summary>
    /// Represents the position and velocity of an object in a gravitational field. <br/>
    /// This struct is immutable.
    /// </summary>
    public struct OrbitalStateVector
    {
        /// <summary>
        /// The time at which this state vector was captured.
        /// </summary>
        public double UT { get; }

        public Vector3Dbl AbsolutePosition { get; }

        public Vector3Dbl AbsoluteVelocity { get; }

        /// <summary>
        /// The direction of the gravitational acceleration at <see cref="AbsolutePosition"/>.
        /// </summary>
        public Vector3 GravityDir { get; }

        public OrbitalStateVector( double ut, Vector3Dbl absolutePosition, Vector3Dbl absoluteVelocity, Vector3 gravityDir )
        {
            this.UT = ut;
            this.AbsolutePosition = absolutePosition;
            this.AbsoluteVelocity = absoluteVelocity;
            this.GravityDir = gravityDir.normalized;
        }

        /// <summary>
        /// Calculates the orbital frame of this state vector.
        /// </summary>
        public OrbitalFrame GetOrbitalFrame()
        {
            Vector3 forward = AbsoluteVelocity.NormalizeToVector3();
            Vector3 up = Vector3.ProjectOnPlane( -GravityDir, forward );
            return new OrbitalFrame( forward, up );
        }

        /// <summary>
        /// Calculates the reference frame of this state vector.
        /// </summary>
        public IReferenceFrame GetReferenceFrame()
        {
            Vector3 forward = AbsoluteVelocity.NormalizeToVector3();
            Vector3 up = Vector3.ProjectOnPlane( -GravityDir, forward );

            return new OrientedReferenceFrame( UT, AbsolutePosition, Quaternion.LookRotation( forward, up ) );
        }
    }
}