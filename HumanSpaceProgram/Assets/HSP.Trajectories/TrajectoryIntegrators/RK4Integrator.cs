using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Trajectories.TrajectoryIntegrators
{
    public sealed class RK4Integrator : ITrajectoryIntegrator
    {
        public double Step( TrajectorySimulationContext context, ReadOnlySpan<ITrajectoryStepProvider> accelerationProviders, out TrajectoryStateVector nextSelf )
        {
            var pos0 = context.Self.AbsolutePosition;
            var vel0 = context.Self.AbsoluteVelocity;
            var dt = context.Step;

            // k1
            Vector3Dbl acc0 = context.SumAccelerations( accelerationProviders );

            // k2 (at dt/2 using k1)
            Vector3Dbl pos_k2 = pos0 + (vel0 * (0.5 * dt));
            Vector3Dbl vel_k2 = vel0 + (acc0 * (0.5 * dt));
            var state_k2 = new TrajectoryStateVector( pos_k2, vel_k2, acc0, context.Self.Mass );
            var tempContext = context.Substep( context.UT + (dt * 0.5), state_k2 );
            Vector3Dbl acc2 = tempContext.SumAccelerations( accelerationProviders );

            // k3 (at dt/2 using k2)
            Vector3Dbl pos_k3 = pos0 + (vel_k2 * (0.5 * dt));
            Vector3Dbl vel_k3 = vel0 + (acc2 * (0.5 * dt));
            var state_k3 = new TrajectoryStateVector( pos_k3, vel_k3, acc2, context.Self.Mass );
            tempContext = context.Substep( context.UT + (dt * 0.5), state_k3 );
            Vector3Dbl acc3 = tempContext.SumAccelerations( accelerationProviders );

            // k4 (at dt using k3)
            Vector3Dbl pos_k4 = pos0 + (vel_k3 * dt);
            Vector3Dbl vel_k4 = vel0 + (acc3 * dt);
            var state_k4 = new TrajectoryStateVector( pos_k4, vel_k4, acc3, context.Self.Mass );
            tempContext = context.Substep( context.UT + dt, state_k4 );
            Vector3Dbl acc4 = tempContext.SumAccelerations( accelerationProviders );

            // combine increments:
            Vector3Dbl deltaPos = (vel0 + (vel_k2 * 2.0) + (vel_k3 * 2.0) + vel_k4) * (dt / 6.0);
            Vector3Dbl deltaVel = (acc0 + (acc2 * 2.0) + (acc3 * 2.0) + acc4) * (dt / 6.0);

            Vector3Dbl posFinal = pos0 + deltaPos;
            Vector3Dbl velFinal = vel0 + deltaVel;

            nextSelf = new TrajectoryStateVector( posFinal, velFinal, acc4, context.Self.Mass );
            return dt;
        }

        [MapsInheritingFrom( typeof( RK4Integrator ) )]
        public static SerializationMapping RK4IntegratorMapping()
        {
            return new MemberwiseSerializationMapping<RK4Integrator>();
        }
    }
}