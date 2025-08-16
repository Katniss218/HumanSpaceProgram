using System;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace HSP.Trajectories
{
    public interface IReadonlyEphemeris
    {
        double TimeResolution { get; }
        double HighUT { get; }
        double LowUT { get; }
        double Duration { get; }
        double MaxDuration { get; }
        int Count { get; }
        int Capacity { get; }
        TrajectoryStateVector Evaluate( double ut );
    }

    public sealed class Ephemeris : IReadonlyEphemeris
    {
        /// <summary>
        /// Gets the amount of time between the adjacent data points in the ephemeris, in [s].
        /// </summary>
        public double TimeResolution { get; }

        public double HighUT => _floatingHeadUT;
        public double LowUT => _floatingTailUT;
        public double Duration => _floatingHeadUT - _floatingTailUT;
        public double MaxDuration => _buffer.Length * TimeResolution;
        public int Count => _count;
        public int Capacity => _buffer.Length;

        readonly double _inverseTimeResolution; // number of data points in 1 second.
        TrajectoryStateVector[] _buffer;
        int _headIndex;
        int _tailIndex;
        double _headUT;
        double _tailUT;
        int _count;

        // 'Floating' head/tail are 2 'temporary' data points that exist when the start/end of
        //   the ephemeris is not aligned with the regular grid of data points.
        TrajectoryStateVector _floatingHead;
        TrajectoryStateVector _floatingTail;
        double _floatingHeadUT;
        double _floatingTailUT;

        /// <summary>
        /// 
        /// </summary>
        public Ephemeris( double timeResolution, double maxDuration )
        {
            if( timeResolution <= 0 )
                throw new ArgumentOutOfRangeException( nameof( timeResolution ), $"Time resolution must be a positive number." );

            if( maxDuration < timeResolution )
                throw new ArgumentOutOfRangeException( nameof( maxDuration ), $"Duration must be at least equal to the time resolution." );

            this.TimeResolution = timeResolution;
            _inverseTimeResolution = 1.0 / timeResolution;
            int bufferSize = (int)(maxDuration * _inverseTimeResolution) + 1;
            _buffer = new TrajectoryStateVector[bufferSize];
            Clear();
        }

        public void SetDuration( double headUT, double tailUT )
        {
            if( tailUT >= headUT )
                throw new ArgumentOutOfRangeException( nameof( tailUT ), $"Tail UT must be less than head UT." );

            double maxDuration = headUT - tailUT;

            if( maxDuration < TimeResolution )
                throw new ArgumentOutOfRangeException( nameof( headUT ), $"Duration must be at least equal to the time resolution." );

            int bufferSize = (int)(maxDuration * _inverseTimeResolution) + 1;
            var newBuffer = new TrajectoryStateVector[bufferSize];

            double overlapStart = Math.Max( tailUT, _tailUT );
            double overlapEnd = Math.Min( headUT, _headUT );
            double overlapDuration = overlapEnd - overlapStart;
            if( overlapDuration <= 0 )
            {
                Clear();

                return;
            }

            int newCount = (int)(overlapDuration * _inverseTimeResolution);
            int oldIndex = _tailIndex;
            int index = -1;
            for( int i = 0; i < newCount; i++ )
            {
                index++;
                if( index < 0 || index >= _buffer.Length )
                    index %= _buffer.Length; // wrap around the circular buffer.

                newBuffer[index] = _buffer[oldIndex];
            }

            _buffer = newBuffer;
            _headIndex = newCount - 1;
            _tailIndex = 0;
            _headUT = overlapEnd;
            _tailUT = overlapStart;
            _count = newCount;

            _floatingHead = _buffer[_headIndex];
            _floatingTail = _buffer[_tailIndex];
            _floatingHeadUT = _headUT;
            _floatingTailUT = _tailUT;
        }

        public void Clear()
        {
            _headIndex = -1;
            _tailIndex = -1;
            _headUT = 0;
            _tailUT = 0;
            _count = 0;

            _floatingHeadUT = 0;
            _floatingTailUT = 0;
        }

        /// <summary>
        /// Fills the ephemeris with data points, interpolated between the current last data point, and the specified state vector at the specified UT.
        /// </summary>
        public void Append( TrajectoryStateVector stateVector, double ut )
        {
            if( _count == 0 )
            {
                // Initialize the ephemeris with the first data point.

                _buffer[0] = stateVector;
                _headIndex = 0;
                _tailIndex = 0;
                _headUT = ut;
                _tailUT = ut;
                _count = 1;

                _floatingHead = stateVector;
                _floatingTail = stateVector;
                _floatingHeadUT = ut;
                _floatingTailUT = ut;
                return;
            }

            if( ut > _headUT )
            {
                // append to head.
                _floatingHead = stateVector;
                _floatingHeadUT = ut;

                double durationFromHead = ut - _headUT;
                if( durationFromHead < TimeResolution ) // No room for a new point yet.
                    return;

                // Append to the buffer, and slide the window forward, if the new length would be larger than the maximum duration.

                int numSamples = (int)(durationFromHead * _inverseTimeResolution);
                if( numSamples == 0 )
                    return;

                TrajectoryStateVector head = _buffer[_headIndex];
                int index = _headIndex;
                for( int i = 1; i <= numSamples; i++ )
                {
                    index++;
                    if( index < 0 || index >= _buffer.Length )
                        index %= _buffer.Length; // wrap around the circular buffer.

                    double t = (double)i / numSamples;
                    _buffer[index] = TrajectoryStateVector.Lerp( head, stateVector, t );
                }

                _count += numSamples;
                _headIndex = index;
                _headUT += numSamples * TimeResolution;

                // Slide forward and trim floating tail.

                int overflowCount = _count - _buffer.Length;
                if( overflowCount > 0 )
                {
                    _count = _buffer.Length;
                    _tailIndex += overflowCount;
                    if( _tailIndex >= _buffer.Length )
                        _tailIndex %= _buffer.Length; // wrap around the circular buffer.
                    _tailUT += overflowCount * TimeResolution;
                    _floatingTail = _buffer[_tailIndex];
                    _floatingTailUT = _tailUT;
                }

                return;
            }
            if( ut < _tailUT )
            {
                // append to tail.
                _floatingTail = stateVector;
                _floatingTailUT = ut;

                double durationFromTail = _tailUT - ut;
                if( durationFromTail < TimeResolution )
                    return;

                // Append to the buffer, and slide the window backward, if the new length would be larger than the maximum duration.
                int numSamples = (int)(durationFromTail * _inverseTimeResolution);
                if( numSamples == 0 )
                    return;

                TrajectoryStateVector tail = _buffer[_tailIndex];
                int index = _tailIndex;
                for( int i = 1; i <= numSamples; i++ )
                {
                    index--;
                    if( index < 0 )
                        index = (index + _buffer.Length) % _buffer.Length; // wrap around the circular buffer.

                    double t = (double)i / numSamples;
                    _buffer[index] = TrajectoryStateVector.Lerp( tail, stateVector, t );
                }

                _count += numSamples;
                _tailIndex = index;
                _tailUT -= numSamples * TimeResolution;

                // Slide backward and trim floating head.

                int overflowCount = _count - _buffer.Length;
                if( overflowCount > 0 )
                {
                    _count = _buffer.Length;
                    _headIndex -= overflowCount;
                    if( _headIndex < 0 )
                        _headIndex = (_headIndex + _buffer.Length) % _buffer.Length; // wrap around the circular buffer.
                    _headUT -= overflowCount * TimeResolution;
                    _floatingHead = _buffer[_headIndex];
                    _floatingHeadUT = _headUT;
                }

                return;
            }

            throw new ArgumentOutOfRangeException( nameof( ut ), $"Can't append a data point to the ephemeris. The data point must outside the existing ephemeris data." );
        }

        public TrajectoryStateVector Evaluate( double ut )
        {
            if( ut > HighUT || ut < LowUT )
                throw new ArgumentOutOfRangeException( nameof( ut ), $"Time '{ut}' is out of the range of this ephemeris: [{LowUT}, {HighUT}]." );

            double pseudoIndex;
            if( ut > _headUT ) // interpolate between floating head and buffer head.
            {
                pseudoIndex = (ut - _headUT) * _inverseTimeResolution; // should already be in range 0..1. If it's not then the insertion fucked something up.
                return TrajectoryStateVector.Lerp( _buffer[_headIndex], _floatingHead, pseudoIndex );
            }

            if( ut < _tailUT ) // interpolate between floating tail and buffer head.
            {
                pseudoIndex = (_tailUT - ut) * _inverseTimeResolution; // should already be in range 0..1. If it's not then the insertion fucked something up.

                return TrajectoryStateVector.Lerp( _buffer[_tailIndex], _floatingTail, pseudoIndex );
            }
            // interpolate the buffer data points.
            pseudoIndex = (ut - _tailUT) * _inverseTimeResolution;
            int i0 = (int)pseudoIndex;
            int i1 = i0 + 1;


            double t = pseudoIndex - (double)i0;
            int index1 = (i0 + _tailIndex) % Capacity;
            if( t == 0 )
                return _buffer[index1];
            int index2 = (i1 + _tailIndex) % Capacity;
            return TrajectoryStateVector.Lerp( _buffer[index1], _buffer[index2], t );
        }
    }
}