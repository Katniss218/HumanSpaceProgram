using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.Trajectories.AccelerationProviders
{
    public class ManeuverNode : IAccelerationProvider
    {
        // not a singleton

        public Vector3Dbl GetAcceleration( double ut )
        {
            throw new NotImplementedException();
        }

        // serialization method.
    }

    public class NBodyAccelerationProvider : IAccelerationProvider
    {
        // pseudo-singleton. One per system really.

        IReadonlyTrajectorySimulator _simulator;
        ITrajectoryTransform _referenceBody;

        public Vector3Dbl GetAcceleration( double ut )
        {
            throw new NotImplementedException();
        }

        // serialization method.
    }

    [Obsolete( "Not implemented" )]
    public class FMMNBodyAccelerationProvider : IAccelerationProvider
    {
        static Dictionary<IReadonlyTrajectorySimulator, Cache> _cachedSystem = new();

        IReadonlyTrajectorySimulator _simulator;

        public Vector3Dbl GetAcceleration( double ut )
        {
            // get the cached system for this simulator.

            // update if necessary (for a given UT - it would need to store the last update probably too).

            throw new NotImplementedException();
        }

        // serialization method.
    }
}