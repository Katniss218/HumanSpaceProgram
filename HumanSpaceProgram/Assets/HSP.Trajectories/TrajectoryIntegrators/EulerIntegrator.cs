using System.Collections.Generic;
using UnityEngine;

namespace HSP.Trajectories.TrajectoryIntegrators
{
    public sealed class EulerIntegrator : ITrajectoryIntegrator
    {
        public double Step( double ut, double step, TrajectoryStateVector self, IEnumerable<IAccelerationProvider> accelerationProviders, out TrajectoryStateVector nextSelf )
        {
            Vector3Dbl _currentAcceleration = Vector3Dbl.zero;
            foreach( var attractor in accelerationProviders )
            {
                _currentAcceleration += attractor.GetAcceleration( ut );
            }

            Vector3Dbl _currentVelocity = self.AbsoluteVelocity + _currentAcceleration * step;
            Vector3Dbl _currentPosition = self.AbsolutePosition + _currentVelocity * step;
            nextSelf = new TrajectoryStateVector( _currentPosition, _currentVelocity, _currentAcceleration, self.Mass );
            return step;
        }
    }
}