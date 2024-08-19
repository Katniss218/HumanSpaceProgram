using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.Trajectories
{
    /// <summary>
    /// A trajectory that follows of a bunch of predefined points.
    /// </summary>
    public class FixedTrajectory : ITrajectory
    {
        private double _upToUT;

        private List<Vector3Dbl> _positions;
        private List<QuaternionDbl> _rotations;

        public Vector3Dbl AbsolutePosition => throw new NotImplementedException();

        public Vector3Dbl AbsoluteVelocity => throw new NotImplementedException();

        public Vector3Dbl AbsoluteAcceleration => throw new NotImplementedException();

        public double Mass => throw new NotImplementedException();

        public void AddAcceleration( Vector3Dbl acceleration )
        {
            return;
        }

        public void AddAccelerationAtUT( Vector3Dbl acceleration, double ut )
        {
            return;
        }

        public OrbitalStateVector GetCurrentStateVector()
        {
            throw new NotImplementedException();
        }

        public void SetCurrentStateVector( OrbitalStateVector stateVector )
        {
            return;
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
            throw new NotImplementedException();
        }

        public void AddVelocityChange( Vector3Dbl velocityChange )
        {
            throw new NotImplementedException();
        }

        public void AddVelocityChangeAtUT( Vector3Dbl velocityChange, double ut )
        {
            throw new NotImplementedException();
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