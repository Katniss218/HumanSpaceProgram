# Map Scene
Map scene is used to draw a 'map' of the celestial system.

The map scene is intended to me loaded on top of the Gameplay scene.




different render modes
- Natural - lifelike
- Other - topographical, land/water, etc.









centralize the trajectory stuff. cache is within the simulator, not within each orbit.


```csharp

public class TrajectorySimulator
{
    // non monobeh, simulates trajectories, caches ephemeris

    EphemerisCache x;
}

public class EphemerisCache
{

}

public interface ITrajectory
{
    // propagates the trajectories
    public double Mass { get; }
}

public class TrajectoryManager : MonoBehaviour
{
    Dictionary<ITrajectory, TrajectoryTransform> _map;

    // store its own cache of pos/vel/mass, compare with the list of simulated things and update only ones that changed without resetting cache
}

public class TrajectoryTransform : MonoBehaviour
{
    ITrajectory trajectory;

    // updates to follow trajectory in scene reference frame
}

```




