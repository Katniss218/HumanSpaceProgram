using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace HSP.Trajectories
{
    /// <summary>
    /// Contains the data, inherent to the simulation itself, that's available to trajectory step providers during a simulation step.
    /// </summary>
    public readonly ref struct TrajectorySimulationContext
    {
        /// <summary>
        /// Gets the UT of the current simulation step.
        /// </summary>
        public double UT { get; }

        /// <summary>
        /// Gets the step size of the current simulation step.
        /// </summary>
        public double Step { get; }

        /// <summary>
        /// Gets the state vector of the body currently being processed.
        /// </summary>
        public TrajectoryStateVector Self { get; }

        /// <summary>
        /// Contains the index of 'self' in the attractors array, or -1 if 'self' is not an attractor.
        /// </summary>
        public int SelfAttractorIndex { get; }

        public int AttractorCount => _attractors.Length;

        readonly ReadOnlySpan<TrajectoryStateVector> _attractors; // No followers because they aren't allowed to influence any other body.
        readonly bool _isExtrapolated;
        readonly double _deltaTime;

        public TrajectorySimulationContext( double ut, double step, TrajectoryStateVector self, int selfAttractorIndex, ReadOnlySpan<TrajectoryStateVector> attractors )
        {
            this.UT = ut;
            this.Step = step;
            this.Self = self;
            this.SelfAttractorIndex = selfAttractorIndex;
            this._attractors = attractors;
            this._isExtrapolated = false;
            this._deltaTime = 0.0;
        }

        public TrajectorySimulationContext( double ut, double step, TrajectoryStateVector self, int selfAttractorIndex, ReadOnlySpan<TrajectoryStateVector> attractors, double deltaTime )
        {
            this.UT = ut;
            this.Step = step;
            this.Self = self;
            this.SelfAttractorIndex = selfAttractorIndex;
            this._attractors = attractors;
            this._isExtrapolated = deltaTime != 0;
            this._deltaTime = deltaTime;
        }

        /// <summary>
        /// Returns the state of the attractor with the given index.
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public TrajectoryStateVector GetAttractor( int index )
        {
            if( _isExtrapolated )
                return _attractors[index].Extrapolate( _deltaTime );
            else
                return _attractors[index];
        }

        /// <summary>
        /// Returns a new context that will extrapolate the attractor states to the specified UT, and uses the specified 'self' state.
        /// </summary>
        /// <param name="newUT">The UT to which the data will be extrapolated when retrieved.</param>
        public TrajectorySimulationContext Substep( double newUT, in TrajectoryStateVector newSelf )
        {
            // newSelf is important and usually already calculated by the integrator, so we can have it as a parameter and just set it here.
            return new TrajectorySimulationContext( newUT, Step, newSelf, SelfAttractorIndex, _attractors, newUT - UT );
        }
    }

    public static class TrajectorySimulationContext_Ex
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Vector3Dbl SumAccelerations( this TrajectorySimulationContext context, ReadOnlySpan<ITrajectoryStepProvider> providers )
        {
            Vector3Dbl acc = Vector3Dbl.zero;
            foreach( var provider in providers )
            {
                acc += provider.GetAcceleration( context );
            }
            return acc;
        }
    }
}