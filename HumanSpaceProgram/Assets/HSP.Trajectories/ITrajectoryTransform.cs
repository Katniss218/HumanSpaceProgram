using HSP.ReferenceFrames;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.Trajectories
{
    public interface ITrajectoryTransform : IComponent
    {
        public IReferenceFrameTransform ReferenceFrameTransform { get; }

        public IPhysicsTransform PhysicsTransform { get; }

        public ITrajectoryIntegrator Integrator { get; }

        public IReadOnlyList<ITrajectoryStepProvider> AccelerationProviders { get; }

        public bool IsAttractor { get; }

        public bool TrajectoryNeedsUpdating();
    }
}