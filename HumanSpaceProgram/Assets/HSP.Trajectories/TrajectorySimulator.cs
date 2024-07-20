
using System.Collections.Generic;

namespace HSP.Trajectories
{
    public sealed class TrajectorySimulator
    {
        public List<ITrajectory> Attractors { get; private set; } = new();
        public List<ITrajectory> Followers { get; private set; } = new();

        private double _simulationEndUT;


    }
}