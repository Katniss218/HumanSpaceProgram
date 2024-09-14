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

        public double UT { get; private set; }

        public double Mass => throw new NotImplementedException();

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

            //OrbitalStateVector stateVector = GetStateVectorAtUT( ut );

            //var forward = stateVector.AbsoluteVelocity.NormalizeToVector3();
            //var up = Vector3Dbl.Cross( GravityDir, stateVector.AbsoluteVelocity ).NormalizeToVector3();

            //return new OrbitalFrame( forward, up );
            throw new NotImplementedException();
        }

        public bool HasCacheForUT( double ut )
        {
            throw new NotImplementedException();
        }

        public void Step( IEnumerable<TrajectoryBodyState> attractors, double dt )
        {
            Vector3Dbl selfAbsolutePosition = this.GetCurrentStateVector().AbsolutePosition;

            Vector3Dbl accSum = Vector3Dbl.zero;
            foreach( var body in attractors )
            {
#warning TODO - the trajectory might've been updated (stepped before, we need to get the position it had at the current time).
                // instead of passing in a trajectory, maybe pass in the state vectors collected earlier?

                Vector3Dbl toBody = body.AbsolutePosition - selfAbsolutePosition;

                double distanceSq = toBody.sqrMagnitude;
                if( distanceSq == 0.0 )
                {
                    continue;
                }

                double accelerationMagnitude = PhysicalConstants.G * (body.Mass / distanceSq);
                accSum += toBody.normalized * accelerationMagnitude;
            }

            return accSum;
        }
    }
}