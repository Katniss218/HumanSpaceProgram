using System;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Trajectories.AccelerationProviders
{
    public class ManeuverNode : IAccelerationProvider
    {
        // not a singleton

        public Vector3Dbl GetAcceleration( double ut )
        {
            throw new NotImplementedException();
        }

        public double? GetMass( double ut )
        {
            throw new NotImplementedException();
        }

        // serialization method.
    }

    public class TwoBodyAccelerationProvider : IAccelerationProvider
    {
        // pseudo-singleton. One per simulator really.

        IReadonlyTrajectorySimulator _simulator;
        ITrajectoryTransform _referenceBody;
        ITrajectoryTransform _parentBody; // change when crossing SOI boundaries.

#warning TODO - how to initialize these? they need to be copied, not just assigned, from the trajectory transform - per simulator.

        public Vector3Dbl GetAcceleration( double ut )
        {
            throw new NotImplementedException();
        }
        public double? GetMass( double ut )
        {
            throw new NotImplementedException();
        }


        [MapsInheritingFrom( typeof( TwoBodyAccelerationProvider ) )]
        public static SerializationMapping TwoBodyAccelerationProviderMapping()
        {
            return new MemberwiseSerializationMapping<TwoBodyAccelerationProvider>();
        }
    }

    public class NBodyAccelerationProvider : IAccelerationProvider
    {
        // pseudo-singleton. One per simulator really.

        IReadonlyTrajectorySimulator _simulator;
        ITrajectoryTransform _referenceBody;

        public Vector3Dbl GetAcceleration( double ut )
        {
            throw new NotImplementedException();
        }
        public double? GetMass( double ut )
        {
            throw new NotImplementedException();
        }


        [MapsInheritingFrom( typeof( NBodyAccelerationProvider ) )]
        public static SerializationMapping NBodyAccelerationProviderMapping()
        {
            return new MemberwiseSerializationMapping<NBodyAccelerationProvider>();
        }
    }

    [Obsolete( "Not implemented" )]
    public class FMMNBodyAccelerationProvider : IAccelerationProvider
    {
        // pseudo-singleton. One per simulator really.

        static Dictionary<IReadonlyTrajectorySimulator, Cache> _cachedSystem = new();

        IReadonlyTrajectorySimulator _simulator;

        public Vector3Dbl GetAcceleration( double ut )
        {
            // get the cached system for this simulator.

            // update if necessary (for a given UT - it would need to store the last update probably too).

            throw new NotImplementedException();
        }
        public double? GetMass( double ut )
        {
            throw new NotImplementedException();
        }


        [MapsInheritingFrom( typeof( FMMNBodyAccelerationProvider ) )]
        public static SerializationMapping FMMNBodyAccelerationProviderMapping()
        {
            return new MemberwiseSerializationMapping<FMMNBodyAccelerationProvider>();
        }
    }
}