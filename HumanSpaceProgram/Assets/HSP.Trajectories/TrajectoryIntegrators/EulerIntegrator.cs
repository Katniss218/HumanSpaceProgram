using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Trajectories.TrajectoryIntegrators
{
    public sealed class EulerIntegrator : ITrajectoryIntegrator
    {
        public double Step( TrajectorySimulationContext context, ReadOnlySpan<ITrajectoryStepProvider> accelerationProviders, out TrajectoryStateVector nextSelf )
        {
            double dt = context.Step;

            Vector3Dbl acc1 = context.SumAccelerations( accelerationProviders );

            Vector3Dbl _currentVelocity = context.Self.AbsoluteVelocity + (acc1 * dt);
            Vector3Dbl _currentPosition = context.Self.AbsolutePosition + (_currentVelocity * dt);

            nextSelf = new TrajectoryStateVector( _currentPosition, _currentVelocity, acc1, context.Self.Mass );
            return dt;
        }


        [MapsInheritingFrom( typeof( EulerIntegrator ) )]
        public static SerializationMapping EulerIntegratorMapping()
        {
            return new MemberwiseSerializationMapping<EulerIntegrator>();
        }
    }
}