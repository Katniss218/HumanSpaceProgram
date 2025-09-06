using System;
using UnityEngine;

namespace HSP.Trajectories.TrajectoryIntegrators
{
    public sealed class Yoshida4Integrator : ITrajectoryIntegrator
    {
        // Yoshida coefficients
        const double pow2_1_3 = 1.25992104989; // 2^(1/3)
        const double alpha = 1.0 / (2.0 - pow2_1_3);
        const double beta = 1.0 - 2.0 * alpha;

        public double Step( TrajectorySimulationContext context, ReadOnlySpan<ITrajectoryStepProvider> accelerationProviders, out TrajectoryStateVector nextSelf )
        {
            Vector3Dbl pos0 = context.Self.AbsolutePosition;
            Vector3Dbl vel0 = context.Self.AbsoluteVelocity;
            Vector3Dbl acc0 = context.SumAccelerations( accelerationProviders );
            double ut = context.UT;

            // Substep 1
            double dt = alpha * context.Step;
            ut += dt;
            double halfDtSq = 0.5 * (dt * dt);
            Vector3Dbl pos1 = pos0 + (vel0 * dt) + (acc0 * halfDtSq);

            var tempContext = context.Substep( ut, new TrajectoryStateVector( pos1, vel0, acc0, context.Self.Mass ) );
            Vector3Dbl acc1 = tempContext.SumAccelerations( accelerationProviders );
            Vector3Dbl vel1 = vel0 + ((acc0 + acc1) * (0.5 * dt));

            // Substep 2
            dt = beta * context.Step;
            ut += dt;
            halfDtSq = 0.5 * (dt * dt);
            Vector3Dbl pos2 = pos1 + (vel1 * dt) + (acc1 * halfDtSq);

            tempContext = context.Substep( ut, new TrajectoryStateVector( pos2, vel1, acc1, context.Self.Mass ) );
            Vector3Dbl acc2 = tempContext.SumAccelerations( accelerationProviders );
            Vector3Dbl vel2 = vel1 + ((acc1 + acc2) * (0.5 * dt));

            // Substep 3
            dt = alpha * context.Step;
            ut += dt;
            halfDtSq = 0.5 * (dt * dt);
            Vector3Dbl pos3 = pos2 + (vel2 * dt) + (acc2 * halfDtSq);

            tempContext = context.Substep( ut, new TrajectoryStateVector( pos3, vel2, acc2, context.Self.Mass ) );
            Vector3Dbl acc3 = tempContext.SumAccelerations( accelerationProviders );
            Vector3Dbl vel3 = vel2 + ((acc2 + acc3) * (0.5 * dt));

            nextSelf = new TrajectoryStateVector( pos3, vel3, acc3, context.Self.Mass );
            return context.Step;
        }
    }
}