using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.Trajectories
{
    /// <summary>
    /// A trajectory that follows a newtonian gravitational field.
    /// </summary>
    public class NewtonianOrbit : ITrajectory
    {
        private double _cachedUpToUT;

        private List<Vector3Dbl> _cachedPositions;
        private List<Vector3Dbl> _cachedVelocities;

        public void AddAcceleration( Vector3Dbl acceleration )
        {
            // invalidate anything beyond current UT.
            throw new NotImplementedException();
        }

        public void AddAccelerationAtUT( Vector3Dbl acceleration, double ut )
        {
            // invalidate anything beyond specified UT.
            throw new NotImplementedException();
        }

        public StateVector GetCurrentStateVector()
        {
            throw new NotImplementedException();
        }

        public void SetCurrentStateVector( StateVector stateVector )
        {
            throw new NotImplementedException();
        }

        public StateVector GetStateVectorAtUT( double ut )
        {
            throw new NotImplementedException();
        }

        public StateVector GetStateVector( float t )
        {
            throw new NotImplementedException();
        }

        public OrbitalFrame GetCurrentOrbitalFrame()
        {
            throw new NotImplementedException();
        }

        public OrbitalFrame GetOrbitalFrameAtUT( double ut )
        {
            // Prograde -> towards velocity.
            // Antiradial -> "towards" gravity, but projected onto a plane whose normal is velocity, such that it's orthogonal to Prograde.

            StateVector stateVector = GetStateVectorAtUT( ut );

            var forward = stateVector.Velocity.NormalizeToVector3();
            var up = Vector3Dbl.Cross( stateVector.GravityDir, stateVector.Velocity ).NormalizeToVector3();

            return new OrbitalFrame( forward, up );
        }
    }
}