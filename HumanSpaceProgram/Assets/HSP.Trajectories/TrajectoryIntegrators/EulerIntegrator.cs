using System;
using UnityEngine;

namespace HSP.Trajectories.TrajectoryIntegrators
{
    public sealed class EulerIntegrator : ITrajectoryIntegrator
    {
        public double Step( TrajectorySimulationContext context, ReadOnlySpan<ITrajectoryStepProvider> accelerationProviders, out TrajectoryStateVector nextSelf )
        {
            Vector3Dbl _currentAcceleration = Vector3Dbl.zero;
            foreach( var attractor in accelerationProviders )
            {
                _currentAcceleration += attractor.GetAcceleration( context );
            }

            Vector3Dbl _currentVelocity = context.Self.AbsoluteVelocity + _currentAcceleration * context.Step;
            Vector3Dbl _currentPosition = context.Self.AbsolutePosition + _currentVelocity * context.Step;
            nextSelf = new TrajectoryStateVector( _currentPosition, _currentVelocity, _currentAcceleration, context.Self.Mass );
            return context.Step;
        }
    }
}