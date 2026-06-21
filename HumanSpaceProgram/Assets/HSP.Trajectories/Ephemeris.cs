using System;
using System.Runtime.CompilerServices;

namespace HSP.Trajectories
{
    /// <summary>
    /// Represents a fixed-duration ephemeris.
    /// </summary>
    /// <remarks>
    /// Implemented as a resizeable circular buffer with adaptive error-based sample insertion.
    /// </remarks>
    public sealed class Ephemeris : IReadonlyEphemeris
    {
        /// <summary>
        /// Side semantics at a given time instant that may have a discontinuity (jump).
        /// IncreasingUT = value approached from ut &lt; sample.ut (left-hand limit, pre-jump).
        /// DecreasingUT = value approached from ut &gt; sample.ut (right-hand limit, post-jump).
        /// </summary>
        public enum Side : sbyte
        {
            IncreasingUT = -1,
            Middle = 0,   // for continuous samples (no discontinuity flags)
            DecreasingUT = 1
        }

        public readonly struct Sample
        {
            public readonly double ut;
            public readonly TrajectoryStateVector state;

            // Whether each sample is immediately before/after a discontinuity.
            // - Continuous samples share with the segment before and after.
            // - Discontinuities store 2 samples with the same UT at the point of discontinuity.
            public readonly bool afterDiscontinuity; // if true, the discontinuous sample represents the 'end' sample (e.g. after a discontinuous impulse).
            public readonly bool beforeDiscontinuity => !afterDiscontinuity;

            public readonly SampleType sampleType;

            public Sample( double ut, TrajectoryStateVector state, bool afterDiscontinuity, SampleType sampleType )
            {
                this.ut = ut;
                this.state = state;
                this.afterDiscontinuity = afterDiscontinuity;
                this.sampleType = sampleType;
            }
        }

        public enum SampleType : byte
        {
            /// <summary>
            /// Smooth sample.
            /// </summary>
            Continuous,
            /// <summary>
            /// Sample is discontinuous, i.e. it has a jump in the trajectory.
            /// </summary>
            InstantChange
        }

        /// <summary>
        /// The number of samples in the ephemeris.
        /// </summary>
        public int Count => _count;
        /// <summary>
        /// The maximum number of samples that this ephemeris can hold.
        /// </summary>
        public int Capacity => _samples.Length;
        /// <summary>
        /// The UT of the first sample in the ephemeris, in [s] since epoch - see <see cref="HSP.Time.TimeManager.UT"/>.
        /// </summary>
        public double HighUT => _headUT;
        /// <summary>
        /// The UT of the last sample in the ephemeris, in [s] since epoch - see <see cref="HSP.Time.TimeManager.UT"/>.
        /// </summary>
        public double LowUT => _tailUT;
        /// <summary>
        /// The duration of the ephemeris, in [s].
        /// </summary>
        public double Duration => _samples.Length == 0 ? 0 : (_headUT - _tailUT);

        /// <summary>
        /// Maximum difference allowed between two consecutive samples when using adaptive insertion.
        /// </summary>
        public double MaxError { get; set; }

        /// <summary>
        /// The maximum duration of the ephemeris, in [s]. When exceeded, the ephemeris will slide to maintain this duration.
        /// </summary>
        public double MaxDuration { get; set; }

        private Sample[] _samples;
        private double _headUT;
        private double _tailUT;
        private int _headIndex;
        private int _tailIndex;
        private int _count;

        private const double TOLERANCE = 1e-10;

        public Ephemeris( double maxError = 0.01, double maxDuration = double.PositiveInfinity )
        {
            MaxError = maxError;
            MaxDuration = maxDuration;
            Clear( 64 );
        }

        public Ephemeris( int capacity, double maxError = 0.01, double maxDuration = double.PositiveInfinity )
        {
            if( capacity <= 2 )
                throw new ArgumentOutOfRangeException( nameof( capacity ), "The ephemeris must hold at least 2 samples." );
            MaxError = maxError;
            MaxDuration = maxDuration;
            Clear( capacity );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private int WrapIndex( int index )
        {
            int len = _samples.Length;
            if( index >= len ) 
                return index - len;
            if( index < 0 ) 
                return index + len;
            return index;
        }

        /// <summary>
        /// Gets the index of the sample 'before' or exactly at the specified UT, and the sample itself. <br/>
        /// Deals correctly with circular index wrap-around using linearized binary search bounds.
        /// </summary>
        private int FindIndex( double ut, out Sample sample )
        {
            if( _count == 0 )
            {
                sample = default;
                return -1;
            }
            if( ut >= _headUT )
            {
                sample = _samples[_headIndex];
                return _headIndex;
            }
            if( ut <= _tailUT )
            {
                sample = _samples[_tailIndex];
                return _tailIndex;
            }

            int lower = _tailIndex;
            int upper = _headIndex;
            if( lower > upper )
                upper += Capacity; // De-wrap search space bounds linearly

            while( lower < upper )
            {
                // Bias upwards to ensure progress (especially when upper - lower == 1)
                int mid = (lower + upper + 1) / 2;
                int realIndex = mid;
                if( realIndex >= Capacity )
                    realIndex -= Capacity;

                if( _samples[realIndex].ut <= ut )
                {
                    lower = mid;
                }
                else
                {
                    upper = mid - 1;
                }
            }

            int finalIndex = lower;
            if( finalIndex >= Capacity )
                finalIndex -= Capacity;

            sample = _samples[finalIndex];
            return finalIndex;
        }

        public static double CalculateError( TrajectoryStateVector a, TrajectoryStateVector b )
        {
            return VectorSimilarityUtils.Error( a, b );
        }

        /// <summary>
        /// Inserts a new sample adaptively. Only stores samples when they diverge sufficiently from trends.
        /// </summary>
        /// <returns>True if a new sample was inserted, false when an existing sample was replaced/moved.</returns>
        public bool InsertAdaptive( double ut, TrajectoryStateVector state )
        {
            Sample newSample = new Sample( ut, state, false, SampleType.Continuous );

            // Guard against edge additions or duplicate updates on empty or single-item ephemerides
            if( _count == 0 )
            {
                _samples[_headIndex] = newSample;
                _count++;
                _headUT = ut;
                _tailUT = ut;
                return true;
            }

            // Universal edge duplication guard: if we are exactly updating physical endpoints, replace in-place
            if( ut == _headUT )
            {
                _samples[_headIndex] = newSample;
                return false;
            }
            if( ut == _tailUT )
            {
                _samples[_tailIndex] = newSample;
                return false;
            }

            if( _count == 1 )
            {
                if( ut < _tailUT )
                {
                    _tailUT = ut;
                    _tailIndex = WrapIndex( _tailIndex - 1 );
                    _samples[_tailIndex] = newSample;
                    _count++;
                    return true;
                }
                else
                {
                    _headUT = ut;
                    _headIndex = WrapIndex( _headIndex + 1 );
                    _samples[_headIndex] = newSample;
                    _count++;
                    return true;
                }
            }

            // ---------------------------------------------
            // Option 1: Append the new sample to the head.
            if( ut > _headUT )
            {
                int prevHead = WrapIndex( _headIndex - 1 );
                Sample s1 = _samples[prevHead];

                // Check trend curvature similarity with 2nd-to-last sample
                double error = VectorSimilarityUtils.Error( state, s1.state );
                if( error < MaxError )
                {
                    // Error is negligible: extend interval by just updating the current head position with new data
                    _samples[_headIndex] = newSample;
                    _headUT = ut;
                    return false;
                }

                // Grow if we're out of capacity
                if( _count == Capacity )
                {
                    ResizeArray( Math.Max( Capacity * 2, 16 ) );
                }

                _headUT = ut;
                _headIndex = WrapIndex( _headIndex + 1 );
                _samples[_headIndex] = newSample;
                _count++;

                if( Duration > MaxDuration )
                {
                    SlideForward();
                }

                return true;
            }

            // ---------------------------------------------
            // Option 2: Append the new sample to the tail.
            if( ut < _tailUT )
            {
                int nextTail = WrapIndex( _tailIndex + 1 );
                Sample s1 = _samples[nextTail];

                double error = VectorSimilarityUtils.Error( state, s1.state );
                if( error < MaxError )
                {
                    _samples[_tailIndex] = newSample;
                    _tailUT = ut;
                    return false;
                }

                if( _count == Capacity )
                {
                    ResizeArray( Math.Max( Capacity * 2, 16 ) );
                }

                _tailUT = ut;
                _tailIndex = WrapIndex( _tailIndex - 1 );
                _samples[_tailIndex] = newSample;
                _count++;

                if( Duration > MaxDuration )
                {
                    SlideBackward();
                }

                return true;
            }

            // Option 5: Replace/Insert in middle - not allowed by design
            throw new InvalidOperationException( "Can't insert in the middle of an ephemeris." );
        }

        public void Clear()
        {
            _headUT = 0;
            _tailUT = 0;
            _count = 0;
            _headIndex = 0;
            _tailIndex = 0;
        }

        public void Clear( int newCapacity )
        {
            _samples = new Sample[newCapacity];
            Clear();
        }

        /// <summary>
        /// Resizes the array, de-wrapping the circular structure to start cleanly from index 0.
        /// </summary>
        private void ResizeArray( int newCapacity )
        {
            Sample[] newSamples = new Sample[newCapacity];

            if( _count > 0 )
            {
                int firstLen = Math.Min( _samples.Length - _tailIndex, _count );
                int secondLen = _count - firstLen;

                if( firstLen > 0 )
                    Array.Copy( _samples, _tailIndex, newSamples, 0, firstLen );
                if( secondLen > 0 )
                    Array.Copy( _samples, 0, newSamples, firstLen, secondLen );

                _tailIndex = 0;
                _headIndex = _count - 1;
            }
            else
            {
                _tailIndex = 0;
                _headIndex = 0;
            }

            _samples = newSamples;
        }

        private void SlideForward()
        {
            double targetTailUT = _headUT - MaxDuration;
            int newTailIndex = _tailIndex;

            while( newTailIndex != _headIndex && _samples[newTailIndex].ut < targetTailUT )
            {
                newTailIndex = WrapIndex( newTailIndex + 1 );
            }

            // Step back one sample to keep the interval boundary covered for interpolation
            newTailIndex = WrapIndex( newTailIndex - 1 );

            _tailIndex = newTailIndex;
            _tailUT = targetTailUT;

            _count = (_headIndex >= _tailIndex)
                ? (_headIndex - _tailIndex + 1)
                : (_headIndex + _samples.Length - _tailIndex + 1);
        }

        private void SlideBackward()
        {
            double targetHeadUT = _tailUT + MaxDuration;
            int newHeadIndex = _headIndex;

            while( newHeadIndex != _tailIndex && _samples[newHeadIndex].ut > targetHeadUT )
            {
                newHeadIndex = WrapIndex( newHeadIndex - 1 );
            }

            // Step forward one sample to keep the boundary covered for interpolation
            newHeadIndex = WrapIndex( newHeadIndex + 1 );

            _headIndex = newHeadIndex;
            _headUT = targetHeadUT;

            _count = (_headIndex >= _tailIndex)
                ? (_headIndex - _tailIndex + 1)
                : (_headIndex + _samples.Length - _tailIndex + 1);
        }

        public TrajectoryStateVector Evaluate( double ut )
        {
            return Evaluate( ut, Side.IncreasingUT );
        }

        public TrajectoryStateVector Evaluate( double ut, Side side = Side.IncreasingUT )
        {
            if( _count == 0 )
            {
                throw new InvalidOperationException( "Cannot evaluate empty ephemeris." );
            }
            if( ut > _headUT + TOLERANCE || ut < _tailUT - TOLERANCE )
            {
                throw new ArgumentOutOfRangeException( nameof( ut ), $"Time '{ut:R}' is out of the range of this ephemeris: [{_headUT:R}, {_tailUT:R}]." );
            }
            if( ut >= _headUT - TOLERANCE )
            {
                return _samples[_headIndex].state;
            }
            if( ut <= _tailUT + TOLERANCE )
            {
                return _samples[_tailIndex].state;
            }

            int index = FindIndex( ut, out Sample s1 );

            if( s1.ut == ut )
            {
                if( s1.sampleType == SampleType.InstantChange )
                {
                    if( side == Side.IncreasingUT )
                    {
                        // Approaching from left: we want the pre-jump state (beforeDiscontinuity)
                        if( s1.afterDiscontinuity )
                        {
                            return _samples[WrapIndex( index - 1 )].state;
                        }
                        return s1.state;
                    }
                    else if( side == Side.DecreasingUT )
                    {
                        // Approaching from right: we want the post-jump state (afterDiscontinuity)
                        if( !s1.afterDiscontinuity )
                        {
                            return _samples[WrapIndex( index + 1 )].state;
                        }
                        return s1.state;
                    }
                    else
                    {
                        int otherIndex = s1.afterDiscontinuity ? WrapIndex( index - 1 ) : WrapIndex( index + 1 );
                        return TrajectoryStateVector.Lerp( s1.state, _samples[otherIndex].state, 0.5 );
                    }
                }
                return s1.state;
            }

            int nextIndex = WrapIndex( index + 1 );
            Sample s2 = _samples[nextIndex];

            return VectorInterpolationUtils.CubicHermite( s1, s2, ut );
        }
    }
}