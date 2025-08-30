using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace HSP.Trajectories
{
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
        private ReadOnlySpan<TrajectoryStateVector> _attractors { get; } // No followers because we're not allowed to use them in calculations
                                                                       // (they can't influence the simulation on purpose).
        public int AttractorCount => _attractors.Length;
        /// <summary>
        /// -1 if not attractor.
        /// </summary>
        public int SelfAttractorIndex { get; }
        

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
        /// Returns a new context that will extrapolate the attractors to the specified UT, and uses the specified self state.
        /// </summary>
        /// <param name="newUT">The UT to which the data will be extrapolated when retrieved.</param>
        public TrajectorySimulationContext Substep( double newUT, in TrajectoryStateVector newSelf )
        {
            // New self is important and very often provided explicitly, so we also set it explicitly.
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