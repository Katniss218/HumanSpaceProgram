using HSP.ReferenceFrames;
using UnityEngine;

namespace HSP.Trajectories
{
    /// <summary>
    /// Represents the position and velocity of an object in a gravitational field.
    /// </summary>
    public struct StateVector
    {
        private readonly Vector3Dbl _position;
        private readonly Vector3Dbl _velocity;

        private readonly Vector3 _gravityDir;

        public Vector3Dbl Position => _position;
        public Vector3Dbl Velocity => _velocity;
        public Vector3 GravityDir => _gravityDir;

        public StateVector( Vector3Dbl position, Vector3Dbl velocity, Vector3 gravityDir )
        {
            this._position = position;
            this._velocity = velocity;
            this._gravityDir = gravityDir.normalized;
        }

        public OrbitalFrame GetOrbitalFrame()
        {
            Vector3 forward = _velocity.NormalizeToVector3();
            Vector3 up = Vector3.ProjectOnPlane( -_gravityDir, forward );
            return new OrbitalFrame( forward, up );
        }

        public IReferenceFrame GetReferenceFrame()
        {
            Vector3 forward = _velocity.NormalizeToVector3();
            Vector3 up = Vector3.ProjectOnPlane( -_gravityDir, forward );

            return new OrientedReferenceFrame( _position, Quaternion.LookRotation( forward, up ) );
        }
    }
}