using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Trajectories.TrajectoryIntegrators
{
    public sealed class RK2Integrator : ITrajectoryIntegrator
    {
        public double Step( TrajectorySimulationContext context, ReadOnlySpan<ITrajectoryStepProvider> accelerationProviders, out TrajectoryStateVector nextSelf )
        {
            Vector3Dbl pos0 = context.Self.AbsolutePosition;
            Vector3Dbl vel0 = context.Self.AbsoluteVelocity;
            double dt = context.Step;

            Vector3Dbl acc0 = context.SumAccelerations( accelerationProviders );

            // estimate mid-state analytically, assuming the acceleration won't change.
            Vector3Dbl pos1 = pos0 + (vel0 * (0.5 * dt));
            Vector3Dbl vel1 = vel0 + (acc0 * (0.5 * dt));

            var context1 = context.Substep( context.UT + (0.5 * dt), new TrajectoryStateVector( pos1, vel1, acc0, context.Self.Mass ) );
            Vector3Dbl acc2 = context1.SumAccelerations( accelerationProviders );

            Vector3Dbl pos2 = pos0 + (vel1 * dt);
            Vector3Dbl vel2 = vel0 + (acc2 * dt);

            nextSelf = new TrajectoryStateVector( pos2, vel2, acc2, context.Self.Mass );
            return dt;
        }

        [MapsInheritingFrom( typeof( RK2Integrator ) )]
        public static SerializationMapping RK2IntegratorMapping()
        {
            return new MemberwiseSerializationMapping<RK2Integrator>();
        }
    }
}
