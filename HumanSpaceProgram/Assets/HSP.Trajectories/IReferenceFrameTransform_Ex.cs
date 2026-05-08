using HSP.Time;

namespace HSP.Trajectories
{
    public static class IReferenceFrameTransform_Ex
    {
        public static TrajectoryStateVector GetBodyState( this ITrajectoryTransform self )
        {
            var state = self.ReferenceFrameTransform.GetState( null ); // Get state in absolute frame

            return new TrajectoryStateVector(
                state.Position,
                state.Velocity,
                state.Acceleration,
                self.PhysicsTransform.Mass
                );
        }
    }
}