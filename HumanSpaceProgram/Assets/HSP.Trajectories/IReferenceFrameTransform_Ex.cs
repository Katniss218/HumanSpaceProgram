using HSP.Time;

namespace HSP.Trajectories
{
    public static class IReferenceFrameTransform_Ex
    {
        public static TrajectoryStateVector GetBodyState( this ITrajectoryTransform self )
        {
            return new TrajectoryStateVector(
                self.ReferenceFrameTransform.AbsolutePosition,
                self.ReferenceFrameTransform.AbsoluteVelocity,
                self.ReferenceFrameTransform.AbsoluteAcceleration,
                self.PhysicsTransform.Mass
                );
        }
    }
}