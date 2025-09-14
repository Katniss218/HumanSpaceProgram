using System;
using UnityEngine;

namespace HSP.Trajectories
{
    /// <summary>
    /// Represents a fixed-duration ephemeris.
    /// </summary>
    /// <remarks>
    /// Implemented as a resizeable circular buffer with adaptive error-based sample insertion.
    /// </remarks>
    public sealed class Ephemeris2 : IReadonlyEphemeris
    {
        /// <summary>
        /// Side semantics at a given time instant that may have a discontinuity (jump).
        /// IncreasingUT = value approached from ut < sample.ut, DecreasingUT = value approached from ut > sample.ut.
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
#warning TODO - slide when set to smaller?

        Sample[] _samples;
        double _headUT;
        double _tailUT;
        int _headIndex;
        int _tailIndex;
        int _count;

        public Ephemeris2( double maxError = 0.01, double maxDuration = double.PositiveInfinity )
        {
            MaxError = maxError;
            MaxDuration = maxDuration;
            Clear( 64 );
        }
        
        public Ephemeris2( int capacity, double maxError = 0.01, double maxDuration = double.PositiveInfinity )
        {
            if( capacity <= 2 )
                throw new ArgumentOutOfRangeException( nameof( capacity ), "The ephemeris must hold at least 2 samples." );
            MaxError = maxError;
            MaxDuration = maxDuration;
            Clear( capacity );
        }

        /// <summary>
        /// Gets the index of the sample 'before' the specified UT, and the sample itself. <br/>
        /// In case, the UT matches the sample exactly, and the sample is discontinuous, it will return the upper sample (after the discontinuity). <br/>
        /// </summary>
        /// <param name="sample">The sample corresponding to the returned index.</param>
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

            // TODO - Optimization, we can assume that the distribution of samples in time is roughly uniform to speed up the search.

            // Binary search for now...
            // Return index of lower sample in case of discontinuities.
            // [s0] - [s1] - ut - [s2] - [s3]        ut is the parameter, [sN] are the hermite samples, [s1] is the sample it should return.
            int lower = _tailIndex;
            int upper = _headIndex;
            int index;
            if( lower > upper )
                upper += Capacity; // Lower and upper search bounds are in a linear 'de-wrapped' range.

            while( lower < upper )
            {
                int mid = (lower + upper + 1) / 2; // Bias upwards, corresponds to the bias down in `upper = mid - 1;`
                index = mid;
                if( index >= Capacity )
                    index -= Capacity;
                sample = _samples[index];

                if( sample.ut <= ut )
                {
                    lower = mid;
                }
                else if( sample.ut > ut )
                {
                    upper = mid - 1;
                }
                else // Exact UT match.
                {
                    // If mid found the 'upper' discontinuous sample (after the discontinuity), and the sample's UT is exactly equal to our UT, return the lower sample.
                    if( sample.sampleType == SampleType.InstantChange && sample.afterDiscontinuity )
                    {
                        index -= 1;
                        if( index < 0 )
                            index += Capacity;
                        sample = _samples[index];
                        return mid - 1;
                    }
                    return mid;
                }
            }

            index = lower;
            if( index >= Capacity )
                index -= Capacity;
            sample = _samples[index];
            return lower;
        }

        public static double CalculateError( TrajectoryStateVector a, TrajectoryStateVector b )
        {
            return VectorSimilarityUtils.Error( a, b );
        }

        const double TOLERANCE = 1e-10;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ut"></param>
        /// <param name="state"></param>
        /// <param name="tolerance"></param>
        /// <returns>True if a new sample was inserted, false when an existing sample was replaced/moved (not counting sliding the entire ephemeris).</returns>
        public bool InsertAdaptive( double ut, TrajectoryStateVector state )
        {
            Sample newSample = new Sample( ut, state, false, SampleType.Continuous );

            // Insert discontinuous always,
            // continuous:
            // - if sample is further ahead - check how it interpolates with the 2nd-to-head sample. if the tolerance is close enough - replace the head sample with the new one.
            // - if sample is in the middle - evaluate at the UT, and only insert if error(eval, new) > tolerance. else do nothing.
            if( _count == 0 )
            {
                _samples[_headIndex] = newSample;
                _count++;
                _headUT = ut;
                _tailUT = ut;
                return true;
            }

            if( _count == 1 )
            {
                if( ut < _tailUT )
                {
                    _tailUT = ut;
                    _tailIndex--;
                    if( _tailIndex < 0 )
                        _tailIndex += Capacity;
                    _samples[_tailIndex] = newSample;
                    _count++;
                    return true;
                }
                else
                {
                    _headUT = ut;
                    _headIndex++;
                    if( _headIndex >= Capacity )
                        _headIndex -= Capacity;
                    _samples[_headIndex] = newSample;
                    _count++;
                    return true;
                }
            }

            Sample s1;
            double error;
            int index;
            // ---------------------------------------------
            // Option 1: Append the new sample to the head.
            if( ut > _headUT )
            {
                index = _headIndex - 1;
                if( index < 0 )
                    index += Capacity;
                s1 = _samples[index];

                // Compare the 2nd-to-last sample with the new sample (and not the last, because the last is always updated and will not show much difference).
                error = VectorSimilarityUtils.Error( state, s1.state );

                if( error < MaxError ) // Only replace the existing head sample.
                {
                    _samples[_headIndex] = newSample;
                    _headUT = ut;
                    return false;
                }

                // Resize first, if the insertion requires it. This will also de-wrap the array and change indices.
                if( _count == Capacity )
                {
                    ResizeArray( Math.Max( Capacity * 2, 16 ), 0 );
                }

                _headUT = ut;
                _headIndex++;
                if( _headIndex >= Capacity )
                    _headIndex -= Capacity;

                if( _count < Capacity )
                {
                    _count++;
                }

                _samples[_headIndex] = newSample;

                if( (Duration) > MaxDuration )
                {
                    SlideForward();
                }

                return true;
            }
            // ---------------------------------------------
            // Option 2: Append the new sample to the tail.
            if( ut < _tailUT )
            {
                index = _tailIndex + 1;
                if( index >= Capacity )
                    index -= Capacity;
                s1 = _samples[index];

                error = VectorSimilarityUtils.Error( state, s1.state );

                if( error < MaxError ) // Only replace the existing tail sample.
                {
                    _samples[_tailIndex] = newSample;
                    _tailUT = ut;
                    return false;
                }

                // Resize first, if the insertion requires it. This will also de-wrap the array and change indices.
                if( _count == Capacity )
                {
                    int size = Math.Max( Capacity * 2, 16 );
                    ResizeArray( size, size - _count );
                }

                _tailUT = ut;
                _tailIndex--;
                if( _tailIndex < 0 )
                    _tailIndex += Capacity;

                if( _count < Capacity )
                {
                    _count++;
                }

                _samples[_tailIndex] = newSample;

                if( (Duration) > MaxDuration )
                {
                    SlideBackward();
                }
                return true;
            }
            // ---------------------------------------------
            // Option 3: Replace the head.
            if( ut == _headUT ) // replace the head sample with the new one.
            {
                _samples[_headIndex] = newSample;
                _headUT = ut;
                return false;
            }
            // ---------------------------------------------
            // Option 4: Replace the tail.
            if( ut == _tailUT ) // replace the tail sample with the new one.
            {
                _samples[_tailIndex] = newSample;
                _tailUT = ut;
                return false;
            }
            // ---------------------------------------------
            // Option 5: Replace the interior samples - not allowed.
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
            this._samples = new Sample[newCapacity];
            Clear();
        }

        private void ResizeArray( int newCapacity, int startIndex )
        {
            // forward = true - the existing array should be placed at array.length, false - existing array placed at 0.
            // should also de-wrap the circular array.
            Sample[] newSamples = new Sample[newCapacity];

            if( _count > 0 )
            {
                int firstLen = Math.Min( _samples.Length - _tailIndex, _count );
                int secondLen = _count - firstLen;

                if( firstLen > 0 )
                    Array.Copy( _samples, _tailIndex, newSamples, startIndex, firstLen );
                if( secondLen > 0 )
                    Array.Copy( _samples, 0, newSamples, startIndex + firstLen, secondLen );

                _tailIndex = startIndex;
                _headIndex = startIndex + _count - 1;
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
            // Only 1 sample was inserted, but the samples near the tail might be denser, so we need a loop.

            // Advance until we find a sample with ut >= targetTailUT or reach the head.
            //   (this can happen if the head sample is inserted very far into the future from any other samples).
            // When such sample is found, the previous sample is the new tail sample, and somewhere between the two is the new tailUT.
            double targetTailUT = _headUT - MaxDuration;

            int newTailIndex = _tailIndex;
            while( newTailIndex != _headIndex && _samples[newTailIndex].ut < targetTailUT )
            {
                newTailIndex++;
                if( newTailIndex >= _samples.Length )
                    newTailIndex -= _samples.Length;
            }

            newTailIndex--; // Take previous.
            if( newTailIndex < 0 )
                newTailIndex += _samples.Length;

            _tailIndex = newTailIndex;
            _tailUT = targetTailUT; // UT detached from the sample, but within the range of samples. Do not re-sample because precision.

            if( _headIndex >= _tailIndex )
                _count = _headIndex - _tailIndex + 1;
            else
                _count = (_headIndex + _samples.Length) - _tailIndex + 1; // De-wrapped head index.
        }

        private void SlideBackward()
        {
            // Analogous to SlideForward(), but in reverse.
            double targetHeadUT = _tailUT + MaxDuration;

            int newHeadIndex = _headIndex;
            while( newHeadIndex != _tailIndex && _samples[newHeadIndex].ut > targetHeadUT )
            {
                newHeadIndex--;
                if( newHeadIndex < 0 )
                    newHeadIndex += _samples.Length;
            }

            newHeadIndex++;
            if( newHeadIndex >= _samples.Length )
                newHeadIndex -= _samples.Length;

            _headIndex = newHeadIndex;
            _headUT = targetHeadUT; // UT detached from the sample, but within the range of samples. Do not re-sample because precision.

            if( _headIndex >= _tailIndex )
                _count = _headIndex - _tailIndex + 1;
            else
                _count = (_headIndex + _samples.Length) - _tailIndex + 1;
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

            int index = FindIndex( ut, out var s1 );

            if( s1.ut == ut )
            {
                // If discontinuity, select side
                if( s1.sampleType == SampleType.InstantChange )
                {
                    if( side == Side.IncreasingUT )
                        return s1.state;
                    else if( side == Side.DecreasingUT )
                    {
                        index++;
                        if( index >= Capacity )
                            index -= Capacity;
                        return _samples[index + 1].state; // return [s2] - the upper discontinuous sample.
                    }
                    else
                    {
                        index++;
                        if( index >= Capacity )
                            index -= Capacity;
                        return TrajectoryStateVector.Lerp( s1.state, _samples[index].state, 0.5 ); // mix both [s1] and [s2] equally.
                    }
                }

                return s1.state;
            }

            index++;
            if( index >= Capacity )
                index -= Capacity;
            Sample s2 = _samples[index];
            //if( s1.sampleType == SampleType.InstantChange ) should only be the case if s1.ut = ut, and already caught above.

            return VectorInterpolationUtils.CubicHermite( s1, s2, ut );
        }
    }
}