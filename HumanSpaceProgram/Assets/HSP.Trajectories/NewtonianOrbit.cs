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

        // cache could work as an array of arrays and their dividing time points. Possibly halving the distance.

        private List<Vector3Dbl> _cachedPositions;
        private List<Vector3Dbl> _cachedVelocities;

        public Vector3Dbl AbsolutePosition => throw new NotImplementedException();

        public Vector3Dbl AbsoluteVelocity => throw new NotImplementedException();

        public Vector3Dbl AbsoluteAcceleration => throw new NotImplementedException();

        public double Mass => throw new NotImplementedException();

        public void AddVelocityChange( Vector3Dbl velocityChange )
        {
            // invalidate anything beyond current UT.
            throw new NotImplementedException();
        }

        public void AddVelocityChangeAtUT( Vector3Dbl velocityChange, double ut )
        {
            // invalidate anything beyond current UT.
            throw new NotImplementedException();
        }

        public OrbitalStateVector GetCurrentStateVector()
        {
            throw new NotImplementedException();
        }

        public void SetCurrentStateVector( OrbitalStateVector stateVector )
        {
            throw new NotImplementedException();
        }

        public OrbitalStateVector GetStateVectorAtUT( double ut )
        {
            throw new NotImplementedException();
        }

        public OrbitalStateVector GetStateVector( float t )
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

            OrbitalStateVector stateVector = GetStateVectorAtUT( ut );

            var forward = stateVector.Velocity.NormalizeToVector3();
            var up = Vector3Dbl.Cross( stateVector.GravityDir, stateVector.Velocity ).NormalizeToVector3();

            return new OrbitalFrame( forward, up );
        }

        public bool HasCacheForUT( double ut )
        {
            throw new NotImplementedException();
        }

        public void Step( IEnumerable<ITrajectory> attractors, double dt )
        {
            throw new NotImplementedException();
        }
    }
}