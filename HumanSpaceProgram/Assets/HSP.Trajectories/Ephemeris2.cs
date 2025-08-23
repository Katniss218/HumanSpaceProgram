using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.Trajectories
{
    public sealed class Ephemeris2
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

            /// <summary>
            /// Computes how 'similar' the two state vectors are.
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns>A value in range of [0..1], where 0 is totally equal, and 1 = totally different.</returns>
            public static double Error( TrajectoryStateVector a, TrajectoryStateVector b )
            {
                // If they're different enough, the leading samples are considered significant and will not be discarded on insertion of new samples.
                const double wp = 1.0;
                const double wv = 0.5;
                const double wa = 0.25;
                const double wm = 0.5;
                double characteristicLengthPosSqr = (a.AbsolutePosition.sqrMagnitude + b.AbsolutePosition.sqrMagnitude) / 2.0;
                double characteristicLengthVelSqr = (a.AbsoluteVelocity.sqrMagnitude + b.AbsoluteVelocity.sqrMagnitude) / 2.0;
                double characteristicLengthAccSqr = (a.AbsoluteAcceleration.sqrMagnitude + b.AbsoluteAcceleration.sqrMagnitude) / 2.0;
                double characteristicLengthMassSqr = (a.Mass * a.Mass + b.Mass * b.Mass) / 2.0;

                double deltaM = a.Mass - b.Mass;
                return wp * ((a.AbsolutePosition - b.AbsolutePosition).sqrMagnitude / characteristicLengthPosSqr)
                     + wv * ((a.AbsoluteVelocity - b.AbsoluteVelocity).sqrMagnitude / characteristicLengthVelSqr)
                     + wa * ((a.AbsoluteAcceleration - b.AbsoluteAcceleration).sqrMagnitude / characteristicLengthAccSqr)
                     + wm * ((deltaM * deltaM) / characteristicLengthMassSqr);
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
        public double HeadUT => _headUT;
        public double TailUT => _tailUT;
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
            _samples = new Sample[capacity];
            _headUT = double.MinValue;
            _tailUT = double.MaxValue;
            _count = 0;
            _headIndex = 0;
            _tailIndex = 0;
            MaxError = maxError;
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
            if( ut > _headUT )
            {
                sample = _samples[_headIndex];
                return _headIndex;
            }
            if( ut < _tailUT )
            {
                sample = _samples[_tailIndex];
                return _tailIndex;
            }

            // TODO - Optimization, we can assume that the distribution of samples in time is roughly uniform to speed up the search.

            // Binary search for now...
            // Return index of lower sample in case of discontinuities.
            int lower = _tailIndex;
            int upper = _headIndex;
            if( lower > upper )
                upper += Capacity; // Lower and upper search bounds are in a linear 'de-wrapped' range.
            int index = (lower + upper) / 2;
            if( index > Capacity ) // Index is then wrapped back.
                index -= Capacity;
            sample = _samples[index];
            while( lower < upper )
            {
                int mid = (lower + upper) / 2; // integer division auto-rounds down to 'lower' sample.
                                               // [s0] - [s1] - ut - [s2] - [s3]        ut is the parameter, [sN] are the hermite samples, [s1] is the sample it should return.
                index = mid;
                if( index > Capacity )
                    index -= Capacity;
                sample = _samples[index];

                if( sample.ut < ut )
                {
                    lower = mid;
                }
                else if( sample.ut > ut )
                {
                    upper = mid;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ut"></param>
        /// <param name="state"></param>
        /// <param name="tolerance"></param>
        /// <returns>True if a new sample was inserted, false when an existing sample was replaced/moved.</returns>
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
                    return true;
                }
                else
                {
                    _headUT = ut;
                    _headIndex++;
                    if( _headIndex >= Capacity )
                        _headIndex -= Capacity;
                    _samples[_headIndex] = newSample;
                    return true;
                }
            }
#warning TODO - simulation needs all ephemerides to have the same max length, otherwise it can't simulate far enough. either we now input the simulation length as the sample count, or the simulator has variable length.
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

                interpolated = Sample.Lerp( s1, newSample, s2.ut );
                //var interpolated = Sample.Hermite( s0, s1, newSample, newSample, s1.ut );

                error = Sample.Error( interpolated, s2.state );

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