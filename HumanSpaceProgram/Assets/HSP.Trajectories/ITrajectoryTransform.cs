using HSP.ReferenceFrames;
using System.Collections.Generic;

namespace HSP.Trajectories
{
    public interface ITrajectoryTransform
    {
        public IReferenceFrameTransform ReferenceFrameTransform { get; }

        public IPhysicsTransform PhysicsTransform { get; }

        public ITrajectoryIntegrator TrajectoryIntegrator { get; }

        public IReadOnlyList<IAccelerationProvider> AccelerationProviders { get; }

        public bool IsAttractor { get; }

        public bool TrajectoryDoesntNeedUpdating();
    }
}