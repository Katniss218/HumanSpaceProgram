# Map Scene
Map scene is used to draw a 'map' of the celestial system.

The map scene is intended to me loaded on top of the Gameplay scene.




different render modes
- Natural - lifelike
- Other - topographical, land/water, etc.









centralize the trajectory stuff. cache is within the simulator, not within each orbit.


```csharp
public readonly struct TrajectoryStepData
{
    public readonly Vector3Dbl position;
    public readonly Vector3Dbl velocity;
    public readonly Vector3Dbl acceleration;
}

public class Ephemeris
{
    public TrajectoryTransform Body { get; }

    public double TimeResolution { get; }
    public double StartUT { get; }
    public double EndUT { get; }

    readonly double _invDt;
    readonly double _offset;
    TrajectoryStepData[] _points;

    public Ephemeris( TrajectoryTransform body, double timeResolution, double startUT, double endUT )
    {
        // ...
    }

    public void SetClosestPoint( double ut, StateVector2 stateVector )
    {
        // ...
    }

    public StateVector2 Evaluate( double ut )
    {
        // ...
    }
}

/// Simulates the trajectories
public class TrajectorySimulator
{
    // Timestepper (simulator):
    private List<TrajectoryStepData> _currentAttractors;
    private List<TrajectoryStepData> _nextAttractors;
    private List<double> _massesAttractors;
    private List<TrajectoryStepData> _currentFollowers;
    private List<TrajectoryStepData> _nextFollowers;
    private double _currentUT;

    private readonly double _timeResolution;

    // Calculated ephemerides:
    List<Ephemeris> _attractorEphemerides = new();
    List<Ephemeris> _followerEphemerides = new();

    private void Reset( TrajectorySimulator other )
    {
        
    }

    public Run( double endUT )
    {
    #warning TODO - variable timestep.

        // run forward or backward, depending on endUT
        // old ephemeris data should be discarded.
        // theoretical max length of the ephemeris is fixed

        while( _currentUT < endUT )
        {
            // prolong

            // when ran far enough, store the points as ephemerides in the corresponding ephemeris structs.
            // the trajectory integrator can "tell you" that it's closed form and not numerical
        }
    }
}

public class TrajectoryTransformManager : SingletonMonoBehaviour<TrajectoryTransformManager>
{
    static TrajectorySimulator _groundTruthSimulator; // simulates 'real' steps, ephemeris stays close to current TimeManager.UT
    static TrajectorySimulator _flightplanSimulator;
    static Dictionary<ITrajectory, TrajectoryTransform> _map;

    // store its own cache of pos/vel/mass, compare with the list of simulated things and update only ones that changed without resetting cache
    // only reset if whatever moved has nonzero mass and is attractor
    // all trajectory transforms need to be processed at once, then sim, and then assign to all again. So this has to stay

    static void Simulate(); // called by the update loop
}

public class TrajectoryTransform : MonoBehaviour
{
    ITrajectoryIntegrator trajectory;
    IAccelerationProvider[] providers;

    double Mass => ;
    Vector3Dbl AbsolutePosition => ;
    Vector3Dbl AbsoluteVelocity => ;
    Vector3Dbl AbsoluteAcceleration => ;

    // updates to follow trajectory in scene reference frame
    // add this monobeh to a vessel/cb to automagically register an object with the trajectory system
}

public interface ITrajectoryIntegrator
{
    public double? Step( double dt, TrajectoryStepData self, IEnumerable<IAccelerationProvider> accelerationProviders, out TrajectoryStepData nextSelf );
}

public interface IAccelerationProvider
{
    public Vector3Dbl GetAcceleration(); // returns acceleration at *current* ut.
}

//
//
//

public class NBodyAccelerationProvider : IAccelerationProvider
{
    // needs to have the state of the system
    TrajectorySimulator _sim;
    
    // ...
    Vector3Dbl GetAcceleration()
    {
        _sim.
    }
}

```




