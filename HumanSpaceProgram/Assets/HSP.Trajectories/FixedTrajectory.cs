﻿using System;
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

        public void AddAcceleration( Vector3Dbl acceleration )
        {
            throw new NotImplementedException();
        }

        public void AddAccelerationAtUT( Vector3Dbl acceleration, double ut )
        {
            throw new NotImplementedException();
        }

        public OrbitalFrame GetCurrentOrbitalFrame()
        {
            throw new NotImplementedException();
        }

        public StateVector GetCurrentStateVector()
        {
            throw new NotImplementedException();
        }

        public OrbitalFrame GetOrbitalFrameAtUT( double ut )
        {
            throw new NotImplementedException();
        }

        public StateVector GetStateVector( float t )
        {
            throw new NotImplementedException();
        }

        public StateVector GetStateVectorAtUT( double ut )
        {
            throw new NotImplementedException();
        }

        public void SetCurrentStateVector( StateVector stateVector )
        {
            throw new NotImplementedException();
        }
    }
}