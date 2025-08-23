using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace HSP.Trajectories
{
    public static class VectorErrorUtils
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static double Abs( double value ) => value < 0 ? -value : value;

        static double Max( double a, double b, double c, double d )
        {
            double m = a;
            if( b > m ) m = b;
            if( c > m ) m = c;
            if( d > m ) m = d;
            return m;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static double SymRelativeVec( in Vector3Dbl a, in Vector3Dbl b, in double eps )
        {
            double den = a.magnitude + b.magnitude + eps;
            return (a - b).magnitude / den;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static double SymRelativeScalar( in double a, in double b, in double eps )
        {
            double d = Abs( a - b );
            double den = Abs( a ) + Abs( b ) + eps;
            return d / den;
        }

        // Epsilon values. Prevent divide by 0 and numerical instability near 0.
        const double epsP = 1e-1;
        const double epsV = 1e-3;
        const double epsA = 1e-6;
        const double epsM = 1e-1;

        /// <summary>
        /// Returns a value in [0..1]. 0 == identical. 1 == maximally different.
        /// </summary>
        public static double Error( in TrajectoryStateVector A, in TrajectoryStateVector B )
        {
            double ep = SymRelativeVec( A.AbsolutePosition, B.AbsolutePosition, epsP );
            double ev = SymRelativeVec( A.AbsoluteVelocity, B.AbsoluteVelocity, epsV );
            double ea = SymRelativeVec( A.AbsoluteAcceleration, B.AbsoluteAcceleration, epsA );
            double em = SymRelativeScalar( A.Mass, B.Mass, epsM );

            return Max( ep, ev, ea, em );
        }
    }

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

        private readonly struct Sample
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

            public static TrajectoryStateVector Lerp( Sample s1, Sample s2, double ut )
            {
                double t = (ut - s1.ut) / (s2.ut - s1.ut);
                return TrajectoryStateVector.Lerp( s1.state, s2.state, t );
            }

            public static TrajectoryStateVector Hermite( Sample s0, Sample s1, Sample s2, Sample s3, double ut )
            {
                // ... cubic hermite on the neighboring samples.
                throw new NotImplementedException();
            }
        }

        public enum SampleType : byte
        {
            Continuous,
            /// <summary>
            /// Sample is discontinuous, i.e. it has a jump in the trajectory.
            /// </summary>
            InstantChange
        }

        public int Count => _count;
        public int Capacity => _samples.Length;
        public double HighUT => _headUT;
        public double LowUT => _tailUT;
        public double Duration => _headUT - _tailUT;
        /// <summary>
        /// Maximum error for insertion/deletion of samples.
        /// </summary>
        public double MaxError { get; set; }

        Sample[] _samples;
        double _headUT;
        double _tailUT;
        int _headIndex;
        int _tailIndex;
        int _count;

        public Ephemeris2( int capacity, double maxError = 0.2 )
        {
            if( capacity <= 2 )
                throw new ArgumentOutOfRangeException( nameof( capacity ), "The ephemeris must hold at least 2 samples." );
            MaxError = maxError;
            SetCapacity( capacity );
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

#warning TODO - inserting discontinuous samples requires you to pass in 2 state vectors (before/after) with the same UT. Of course.

        /*public void Insert( double ut, TrajectoryStateVector state )
        {
            // always straight just insert, unless sample with the time already exists.
            if( _samples.Count == 0 )
            {
                _samples.Add( new Sample( ut, state, false, SampleType.Continuous ) );
                _headUT = ut;
                _tailUT = ut;
                return;
            }

            int index = FindIndex( ut, out var sample );
        }*/

        public static double CalculateError( TrajectoryStateVector a, TrajectoryStateVector b )
        {
            return VectorErrorUtils.Error( a, b );
        }

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
#warning TODO - simulation needs all ephemerides to have the same max length, otherwise it can't simulate far enough. either we now input the simulation length as the sample count, or the simulator has variable length.
#warning TODO - maybe set the max duration in the ephemeris itself and let the buffer resize to fit?

            Sample s1;
            Sample s2;
            TrajectoryStateVector interpolated;
            double error;
            int index;
            if( ut > _headUT ) // Append the new sample to the head.
            {
                index = _headIndex - 1;
                if( index < 0 )
                    index += Capacity;
                s1 = _samples[index]; // 2nd to last sample.
                s2 = _samples[_headIndex];

                //interpolated = Sample.Lerp( s1, newSample, s2.ut );
                //var interpolated = Sample.Hermite( s0, s1, newSample, newSample, s1.ut );

                //error = VectorErrorUtils.Error( interpolated, s2.state );
                error = VectorErrorUtils.Error( state, s1.state );

                if( error < MaxError ) // replace the head sample with the new one.
                {
                    _samples[_headIndex] = newSample;
                    _headUT = ut;
                    return false;
                }

                // append new.
                _headUT = ut;
                _headIndex++;
                if( _headIndex >= Capacity )
                    _headIndex -= Capacity;
                if( _headIndex == _tailIndex ) // Slide tail forward
                {
                    _tailIndex++;
                    if( _tailIndex >= Capacity )
                        _tailIndex -= Capacity;
                    _tailUT = _samples[_tailIndex].ut;
                }
                else
                {
                    _count++;
                }
                _samples[_headIndex] = newSample;
                return true;
            }
            if( ut < _tailUT )
            {
                throw new ArgumentOutOfRangeException( nameof( ut ), $"Inserting to the back not supported yet." );
            }
            if( ut == _headUT ) // replace the head sample with the new one.
            {
                _samples[_headIndex] = newSample;
                _headUT = ut;
                return false;
            }
            if( ut == _tailUT ) // replace the tail sample with the new one.
            {
                _samples[_tailIndex] = newSample;
                _tailUT = ut;
                return false;
            }

            // sample is within the ephemeris.
            throw new InvalidOperationException( "Can't insert in the middle of an ephemeris." );
            /*
            index = FindIndex( ut, out s1 );
            index += 1;
            if( index >= Capacity )
                index -= Capacity;
            s2 = _samples[index];
            interpolated = Sample.Lerp( s1, s2, newSample.ut );
            error = Sample.Error( interpolated, newSample.state );

            if( error < MaxError )
            {
                return false;
            }

            //_samples.Insert( index + 1, newSample );
            return true;*/
        }

        public void Clear()
        {
            _headUT = double.MinValue;
            _tailUT = double.MaxValue;
            _count = 0;
            _headIndex = 0;
            _tailIndex = 0;
        }

        public void SetCapacity( int capacity )
        {
            this._samples = new Sample[capacity];
            Clear();
        }

        public TrajectoryStateVector Evaluate( double ut )
        {
            return Evaluate( ut, Side.IncreasingUT );
        }

        public TrajectoryStateVector Evaluate( double ut, Side side = Side.IncreasingUT )
        {
            // evaluate as if approaching from the given side.
            // Center will average over discontinuities.
            if( _count == 0 )
            {
                throw new InvalidOperationException( "Cannot evaluate empty ephemeris." );
            }
            if( ut > _headUT || ut < _tailUT )
            {
                throw new ArgumentOutOfRangeException( nameof( ut ), $"Time '{ut}' is out of the range of this ephemeris: [{_headUT}, {_tailUT}]." );
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
                        return _samples[index + 1].state; // return [s2] - the upper discontinuous sample.
                    else
                        return TrajectoryStateVector.Lerp( s1.state, _samples[index + 1].state, 0.5 ); // mix both [s1] and [s2] equally.
                }

                return s1.state;
            }

            // Get the 4 samples for hermite interpolation...
            index += 1;
            if( index >= Capacity )
                index -= Capacity;
            Sample s2 = _samples[index]; // Sample [s2]
            index -= 2;
            if( index < _tailIndex )
                index = _tailIndex;
            if( index < 0 )
                index += Capacity;
            Sample s0 = _samples[index]; // Sample [s0]
            index += 3;
            if( index > _headIndex )
                index = _headIndex;
            if( index >= Capacity )
                index -= Capacity;
            Sample s3 = _samples[index]; // Sample [s3]
            //if( s0.sampleType == SampleType.InstantChange ) // we don't care, s0 is 'after' the discontinuity anyway. FindIndex ensures that.
            //if( s3.sampleType == SampleType.InstantChange ) // we don't care, s3 is 'before' the discontinuity anyway.
            if( s2.sampleType == SampleType.InstantChange )
                s3 = s2;
            //if( s1.sampleType == SampleType.InstantChange ) should only be the case if s1.ut = ut, and already caught above.

            return Sample.Lerp( s1, s2, ut );
            //return Sample.Hermite( s0, s1, s2, s3, ut );
        }
    }
}