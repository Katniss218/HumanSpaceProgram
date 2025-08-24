using System;
using System.Runtime.CompilerServices;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace HSP.Trajectories
{
    public static class VectorSimilarityUtils
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static double Abs( double value ) => value < 0 ? -value : value;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static double Max( double a, double b, double c, double d )
        {
            double m = a;
            if( b > m ) m = b;
            if( c > m ) m = c;
            if( d > m ) m = d;
            return m;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static double SymRelativeVec( Vector3Dbl a, Vector3Dbl b, double eps )
        {
            double den = a.magnitude + b.magnitude + eps;
            return (a - b).magnitude / den;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static double SymRelativeScalar( double a, double b, double eps )
        {
            double d = Abs( a - b );
            double den = Abs( a ) + Abs( b ) + eps;
            return d / den;
        }

        // Epsilon values. Prevent divide by 0 and numerical instability near 0.
        const double epsP = 1e-3;
        const double epsV = 1e-6;
        const double epsA = 1e-6;
        const double epsM = 1e-3;

        /// <summary>
        /// Calculates how 'similar' the state vectors are.
        /// </summary>
        /// <returns>
        /// A value in [0..1], where 0 = identical, 1 = maximally different.
        /// </returns>
        public static double Error( in TrajectoryStateVector a, in TrajectoryStateVector b )
        {
            double ep = SymRelativeVec( a.AbsolutePosition, b.AbsolutePosition, epsP );
            double ev = SymRelativeVec( a.AbsoluteVelocity, b.AbsoluteVelocity, epsV );
            double ea = SymRelativeVec( a.AbsoluteAcceleration, b.AbsoluteAcceleration, epsA );
            double em = SymRelativeScalar( a.Mass, b.Mass, epsM );

            return Max( ep, ev, ea, em ); // Any one component being 'bad' means that the ephemeris is 'bad',
                                          // even if the other components haven't changed - because it still needs a sample to keep the 'bad' component in check.
        }
    }

    public static class VectorInterpolationUtils
    {
        /// <summary>
        /// Performs linear interpolation of the state vectors inside the two samples. <br/>
        /// Continuous in position.
        /// </summary>
        public static TrajectoryStateVector Lerp( in Ephemeris2.Sample s1, in Ephemeris2.Sample s2, double ut )
        {
            double dt = s2.ut - s1.ut;
            double t = (ut - s1.ut) / dt;

            return TrajectoryStateVector.Lerp( s1.state, s2.state, t );
        }

        /// <summary>
        /// Performs cubic hermite interpolation of the state vectors inside the two samples. <br/>
        /// Continuous in position and velocity.
        /// </summary>
        public static TrajectoryStateVector CubicHermite( in Ephemeris2.Sample s1, in Ephemeris2.Sample s2, double ut )
        {
            double dt = s2.ut - s1.ut;
            double t = (ut - s1.ut) / dt;

            Vector3Dbl m0 = s1.state.AbsoluteVelocity * dt;
            Vector3Dbl m1 = s2.state.AbsoluteVelocity * dt;
            Vector3Dbl n0 = s1.state.AbsoluteAcceleration * dt;
            Vector3Dbl n1 = s2.state.AbsoluteAcceleration * dt;

            return new TrajectoryStateVector(
                CubicHermiteNormalized( s1.state.AbsolutePosition, m0, s2.state.AbsolutePosition, m1, t ),
                CubicHermiteNormalized( s1.state.AbsoluteVelocity, n0, s2.state.AbsoluteVelocity, n1, t ),
                Vector3Dbl.Lerp( s1.state.AbsoluteAcceleration, s2.state.AbsoluteAcceleration, t ),
                MathD.Lerp( s1.state.Mass, s2.state.Mass, t )
                );
        }

        /// <summary>
        /// Performs quintic hermite interpolation of the state vectors inside the two samples. <br/>
        /// Continuous in position, velocity, and acceleration.
        /// </summary>
        public static TrajectoryStateVector QuinticHermite( in Ephemeris2.Sample s1, in Ephemeris2.Sample s2, double ut )
        {
            double dt = s2.ut - s1.ut;
            double t = (ut - s1.ut) / dt;
            double dt2 = dt * dt;

            Vector3Dbl m0 = s1.state.AbsoluteVelocity * dt;
            Vector3Dbl m1 = s2.state.AbsoluteVelocity * dt;
            Vector3Dbl sd0 = s1.state.AbsoluteAcceleration * dt2;
            Vector3Dbl sd1 = s2.state.AbsoluteAcceleration * dt2;
            Vector3Dbl n0 = s1.state.AbsoluteAcceleration * dt;
            Vector3Dbl n1 = s2.state.AbsoluteAcceleration * dt;

            return new TrajectoryStateVector(
                QuinticHermiteNormalized( s1.state.AbsolutePosition, m0, sd0, s2.state.AbsolutePosition, m1, sd1, t ),
                CubicHermiteNormalized( s1.state.AbsoluteVelocity, n0, s2.state.AbsoluteVelocity, n1, t ),
                Vector3Dbl.Lerp( s1.state.AbsoluteAcceleration, s2.state.AbsoluteAcceleration, t ),
                MathD.Lerp( s1.state.Mass, s2.state.Mass, t )
                );
        }

        public static Vector3Dbl CubicHermiteNormalized( Vector3Dbl p0, Vector3Dbl m0, Vector3Dbl p1, Vector3Dbl m1, double t )
        {
            // https://www.rose-hulman.edu/~finn/CCLI/Notes/day09.pdf
            double t2 = t * t;
            double t3 = t2 * t;

            double h00 = 1.0 - (3.0 * t2) + (2.0 * t3);
            double h10 = t - (2.0 * t2) + t3;
            double h01 = (3.0 * t2) - (2.0 * t3);
            double h11 = -t2 + t3;

            return new Vector3Dbl(
                h00 * p0.x + h10 * m0.x + h01 * p1.x + h11 * m1.x,
                h00 * p0.y + h10 * m0.y + h01 * p1.y + h11 * m1.y,
                h00 * p0.z + h10 * m0.z + h01 * p1.z + h11 * m1.z
            );
        }

        public static Vector3Dbl QuinticHermiteNormalized( Vector3Dbl p0, Vector3Dbl m0, Vector3Dbl sd0, Vector3Dbl p1, Vector3Dbl m1, Vector3Dbl sd1, double t )
        {
            // https://www.rose-hulman.edu/~finn/CCLI/Notes/day09.pdf
            double t2 = t * t;
            double t3 = t2 * t;
            double t4 = t3 * t;
            double t5 = t4 * t;

            double h00 = 1.0 - (10.0 * t3) + (15.0 * t4) - (6.0 * t5);
            double h10 = t - (6.0 * t3) + (8.0 * t4) - (3.0 * t5);
            double h20 = (0.5 * t2) - (1.5 * t3) + (1.5 * t4) - (0.5 * t5);
            double h01 = (10.0 * t3) - (15.0 * t4) + (6.0 * t5);
            double h11 = (-4.0 * t3) + (7.0 * t4) - (3.0 * t5);
            double h21 = (0.5 * t3) - t4 + (0.5 * t5);

            return new Vector3Dbl(
                h00 * p0.x + h10 * m0.x + h20 * sd0.x + h01 * p1.x + h11 * m1.x + h21 * sd1.x,
                h00 * p0.y + h10 * m0.y + h20 * sd0.y + h01 * p1.y + h11 * m1.y + h21 * sd1.y,
                h00 * p0.z + h10 * m0.z + h20 * sd0.z + h01 * p1.z + h11 * m1.z + h21 * sd1.z
            );
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
        public double Duration => _headUT - _tailUT;

        /// <summary>
        /// Maximum difference allowed between two consecutive samples when using adaptive insertion.
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
            return VectorSimilarityUtils.Error( a, b );
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

                _headUT = ut;
                _headIndex++;
                if( _headIndex >= Capacity )
                    _headIndex -= Capacity;
                if( _count == Capacity ) // Slide the tail forward when we reach the end of the buffer.
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

                _tailUT = ut;
                _tailIndex--;
                if( _tailIndex < 0 )
                    _tailIndex += Capacity;
                if( _count == Capacity ) // Slide the head back when we reach the end of the buffer.
                {
                    _headIndex--;
                    if( _headIndex < 0 )
                        _headIndex += Capacity;
                    _headUT = _samples[_headIndex].ut;
                }
                else
                {
                    _count++;
                }
                _samples[_tailIndex] = newSample;
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
            // Option 5: Replace the interior samples.
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
                    {
                        index += 1;
                        if( index >= Capacity )
                            index -= Capacity;
                        return _samples[index + 1].state; // return [s2] - the upper discontinuous sample.
                    }
                    else
                    {
                        index += 1;
                        if( index >= Capacity )
                            index -= Capacity;
                        return TrajectoryStateVector.Lerp( s1.state, _samples[index].state, 0.5 ); // mix both [s1] and [s2] equally.
                    }
                }

                return s1.state;
            }

            index += 1;
            if( index >= Capacity )
                index -= Capacity;
            Sample s2 = _samples[index];
            //if( s1.sampleType == SampleType.InstantChange ) should only be the case if s1.ut = ut, and already caught above.

            return VectorInterpolationUtils.CubicHermite( s1, s2, ut );
        }
    }
}