using HSP.Trajectories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Trajectories.TrajectoryIntegrators
{
    public sealed class VerletIntegrator : ITrajectoryIntegrator
    {
        public double Step( TrajectorySimulationContext context, ReadOnlySpan<ITrajectoryStepProvider> accelerationProviders, out TrajectoryStateVector nextSelf )
        {
            Vector3Dbl pos0 = context.Self.AbsolutePosition;
            Vector3Dbl vel0 = context.Self.AbsoluteVelocity;
            double dt = context.Step;

            Vector3Dbl acc0 = context.SumAccelerations( accelerationProviders );

            Vector3Dbl pos1 = pos0 + (vel0 * dt) + (acc0 * (0.5 * dt * dt));

            var context1 = context.Substep( context.UT + dt, new TrajectoryStateVector( pos1, vel0, acc0, context.Self.Mass ) );

            Vector3Dbl acc1 = context1.SumAccelerations( accelerationProviders );

            Vector3Dbl vel1 = vel0 + ((acc0 + acc1) * (0.5 * dt));

            nextSelf = new TrajectoryStateVector( pos1, vel1, acc1, context.Self.Mass );
            return dt;
        }

        [MapsInheritingFrom( typeof( VerletIntegrator ) )]
        public static SerializationMapping VerletIntegratorMapping()
        {
            return new MemberwiseSerializationMapping<VerletIntegrator>();
        }
    }
}