﻿using HSP.ReferenceFrames;
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
        /// The reference time.
        /// </summary>
        public double UT { get; }

        public Vector3Dbl Position { get; }
        public Vector3Dbl Velocity { get; }
        public Vector3 GravityDir { get; }

        public OrbitalStateVector( double ut, Vector3Dbl position, Vector3Dbl velocity, Vector3 gravityDir )
        {
            this.UT = ut;
            this.Position = position;
            this.Velocity = velocity;
            this.GravityDir = gravityDir.normalized;
        }

        /// <summary>
        /// Calculates the orbital frame of this state vector.
        /// </summary>
        public OrbitalFrame GetOrbitalFrame()
        {
            Vector3 forward = Velocity.NormalizeToVector3();
            Vector3 up = Vector3.ProjectOnPlane( -GravityDir, forward );
            return new OrbitalFrame( forward, up );
        }

        /// <summary>
        /// Calculates the reference frame of this state vector.
        /// </summary>
        public IReferenceFrame GetReferenceFrame()
        {
            Vector3 forward = Velocity.NormalizeToVector3();
            Vector3 up = Vector3.ProjectOnPlane( -GravityDir, forward );

            return new OrientedReferenceFrame( UT, Position, Quaternion.LookRotation( forward, up ) );
        }
    }
}