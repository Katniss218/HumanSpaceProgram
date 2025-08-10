using System;

namespace HSP.Trajectories
{
    public readonly ref struct TrajectorySimulationContext
    {
        public double UT { get; }
        public double Step { get; }
        public TrajectoryStateVector Self { get; }
        public ReadOnlySpan<TrajectoryStateVector> CurrentAttractors { get; }
        // No followers because we're not allowed to use them in calculations (they don't influence the simulation, on purpose).

        public TrajectorySimulationContext( double ut, double step, TrajectoryStateVector self, ReadOnlySpan<TrajectoryStateVector> currentAttractors )
        {
            this.UT = ut;
            this.Step = step;
            this.Self = self;
            this.CurrentAttractors = currentAttractors;
        }
    }
}