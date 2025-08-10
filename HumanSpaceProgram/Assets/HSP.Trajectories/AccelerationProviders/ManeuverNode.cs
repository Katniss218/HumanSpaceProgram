using System;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Trajectories.AccelerationProviders
{
    public class ManeuverNode : ITrajectoryStepProvider
    {
        public Vector3Dbl maneuver; // deltaV to acceleration. we need to know engine thrust etc.
        public double startUT;
        public double duration = 1.0; // in [s].

        public ITrajectoryStepProvider Clone( ITrajectoryTransform self, IReadonlyTrajectorySimulator simulator )
        {
            throw new NotImplementedException();
        }

        public Vector3Dbl GetAcceleration( TrajectorySimulationContext context )
        {
            throw new NotImplementedException();

            if( context.UT < startUT || context.UT > startUT + duration )
            {
                return Vector3Dbl.zero; // No acceleration outside the maneuver time window.
            }

            
        }

        public double? GetMass( TrajectorySimulationContext context )
        {
            throw new NotImplementedException();
        }

        // serialization method.
    }

    public class TwoBodyAccelerationProvider : ITrajectoryStepProvider
    {
        public ITrajectoryTransform _parentBody; // Switch parent when crossing SOI boundaries?
                                                 // Would need some caching or something of the body graph, if any exists.
        int _parentBodyIndex;

        public ITrajectoryStepProvider Clone( ITrajectoryTransform self, IReadonlyTrajectorySimulator simulator )
        {
            if( _parentBody == self )
                throw new InvalidOperationException( $"[{this.GetType().Name}] The body can't be its own parent." );

            return new TwoBodyAccelerationProvider()
            {
                _parentBody = this._parentBody,
                _parentBodyIndex = simulator.GetAttractorIndex( this._parentBody )
            };
        }

        public Vector3Dbl GetAcceleration( TrajectorySimulationContext context )
        {
            if( _parentBodyIndex == -1 )
                return Vector3Dbl.zero; // no parent body, no acceleration.

            Vector3Dbl selfPos = context.Self.AbsolutePosition;
            var parent = context.CurrentAttractors[_parentBodyIndex];

            Vector3Dbl toParent = parent.AbsolutePosition - selfPos;
            double accelerationMagnitude = PhysicalConstants.G * (parent.Mass / toParent.sqrMagnitude);
            return toParent.normalized * accelerationMagnitude;
        }

        public double? GetMass( TrajectorySimulationContext context )
        {
            return null;
        }


        [MapsInheritingFrom( typeof( TwoBodyAccelerationProvider ) )]
        public static SerializationMapping TwoBodyAccelerationProviderMapping()
        {
            return new MemberwiseSerializationMapping<TwoBodyAccelerationProvider>();
        }
    }

    public class NBodyAccelerationProvider : ITrajectoryStepProvider
    {

        public ITrajectoryStepProvider Clone( ITrajectoryTransform self, IReadonlyTrajectorySimulator simulator )
        {
            return this; // Can return this, since there is no internal state.
        }

        public Vector3Dbl GetAcceleration( TrajectorySimulationContext context )
        {
            Vector3Dbl selfPos = context.Self.AbsolutePosition;

            Vector3Dbl accSum = Vector3Dbl.zero;
            foreach( var attractor in context.CurrentAttractors )
            {
                Vector3Dbl toBody = attractor.AbsolutePosition - selfPos;

                double distanceSq = toBody.sqrMagnitude;
                if( distanceSq == 0.0 ) // Skip itself.
                {
                    continue;
                }

                double accelerationMagnitude = PhysicalConstants.G * (attractor.Mass / distanceSq);
                accSum += toBody.normalized * accelerationMagnitude;
            }
            return accSum;
        }

        public double? GetMass( TrajectorySimulationContext context )
        {
            return null;
        }


        [MapsInheritingFrom( typeof( NBodyAccelerationProvider ) )]
        public static SerializationMapping NBodyAccelerationProviderMapping()
        {
            return new MemberwiseSerializationMapping<NBodyAccelerationProvider>();
        }
    }

    [Obsolete( "Not implemented" )]
    public class FMMNBodyAccelerationProvider : ITrajectoryStepProvider
    {
        // pseudo-singleton. One per simulator really.

        static Dictionary<IReadonlyTrajectorySimulator, Cache> _cachedSystem = new();

        IReadonlyTrajectorySimulator _simulator;

        public ITrajectoryStepProvider Clone( ITrajectoryTransform self, IReadonlyTrajectorySimulator simulator )
        {
            throw new NotImplementedException();
        }

        public Vector3Dbl GetAcceleration( TrajectorySimulationContext context )
        {
            // get the cached system for this simulator.

            // update if necessary (for a given UT - it would need to store the last update probably too).

            throw new NotImplementedException();
        }

        public double? GetMass( TrajectorySimulationContext context )
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