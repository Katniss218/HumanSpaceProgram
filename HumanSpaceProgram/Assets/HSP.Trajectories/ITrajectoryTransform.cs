using HSP.ReferenceFrames;
using System.Collections.Generic;

namespace HSP.Trajectories
{
    public interface ITrajectoryTransform
    {
        public IReferenceFrameTransform ReferenceFrameTransform { get; }

        public IPhysicsTransform PhysicsTransform { get; }

        public ITrajectoryIntegrator Integrator { get; }

        public IReadOnlyList<ITrajectoryStepProvider> AccelerationProviders { get; }

        public bool IsAttractor { get; }

        public bool TrajectoryDoesntNeedUpdating();
    }
}