namespace HSP.Trajectories
{
    public interface IReadonlyEphemeris
    {
        double HighUT { get; }
        double LowUT { get; }
        double Duration { get; }
        int Count { get; }
        int Capacity { get; }
        TrajectoryStateVector Evaluate( double ut );
    }
}