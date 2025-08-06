using System;

namespace HSP.Trajectories
{
    /// <summary>
    /// Represents an arbitrary trajectory in space and time.
    /// </summary>
    /// <remarks>
    /// This is implemented using a circular buffer of state vector data points, interpolated on evaluation.
    /// </remarks>
    public sealed class Ephemeris
    {
        /// <summary>
        /// The time between adjacent data points, in [s].
        /// </summary>
        public double TimeResolution { get; }

        /// <summary>
        /// The time at the last (highest UT) data point.
        /// </summary>
        public double EndUT { get; private set; }

        /// <summary>
        /// The time at the first (lowest UT) data point.
        /// </summary>
        public double StartUT { get; private set; }

        /// <summary>
        /// Gets the time between the start and end of the ephemeris, in [s].
        /// </summary>
        public double Duration
        {
            get => EndUT - StartUT; 
        }

        /// <summary>
        /// Sets the time interval of the ephemeris, keeping any data points that overlap with the new interval.
        /// </summary>
        public void SetDuration( double startUT, double endUT )
        {
            double duration = endUT - startUT;

            int bufferSize = (int)Math.Ceiling( duration / TimeResolution ) + 1;
            var newBuffer = new TrajectoryStateVector[bufferSize];

            double overlapStart = Math.Max( startUT, StartUT );
            double overlapEnd = Math.Min( endUT, EndUT );
            int newCount = 0;

            if( overlapEnd >= overlapStart && _count > 0 )
            {
                double firstPseudoIndex = (overlapStart - StartUT) * _inverseTimeResolution;
                double lastPseudoIndex = (overlapEnd - StartUT) * _inverseTimeResolution;

                int firstIdx = (int)Math.Ceiling( firstPseudoIndex );
                int lastIdx = (int)Math.Floor( lastPseudoIndex );

                newCount = lastIdx - firstIdx + 1;
                if( newCount < 0 )
                    newCount = 0;

                for( int i = 0; i < newCount; i++ )
                {
                    int oldIndex = (_startIndex + firstIdx + i) % Capacity;
                    newBuffer[i] = _buffer[oldIndex];
                }
            }

            this._buffer = newBuffer;
            this._startIndex = 0;
            this._count = newCount;
        }

        /// <summary>
        /// The number of data points currently in this ephemeris.
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// The maximum number of data points that can be stored in this ephemeris.
        /// </summary>
        public int Capacity => _buffer.Length;

        readonly double _inverseTimeResolution;
        TrajectoryStateVector[] _buffer;
        int _startIndex;    // Index of the first (oldest) data point in the circular buffer.
        int _count;         // Current number of data points.

        /// <summary>
        /// Constructs an ephemeris with a fixed capacity.
        /// </summary>
        /// <param name="ut">The initial universal time (UT) of the first data point.</param>
        /// <param name="timeResolution">The time between adjacent data points, in [s].</param>
        /// <param name="duration">The maximum duration of the ephemeris, in [s].</param>
        public Ephemeris( double ut, double timeResolution, double duration )
        {
            if( timeResolution <= 0 )
                throw new ArgumentOutOfRangeException( nameof( timeResolution ) );
            if( duration < timeResolution )
                throw new ArgumentOutOfRangeException( nameof( duration ) );

            TimeResolution = timeResolution;
            EndUT = ut;
            StartUT = ut;
            _inverseTimeResolution = 1.0 / timeResolution;

            int bufferSize = (int)Math.Ceiling( duration / timeResolution ) + 1;
            _buffer = new TrajectoryStateVector[bufferSize];
            _startIndex = 0;
            _count = 0;
        }

        /// <summary>
        /// Adds a new sample to the front (highest UT). If full, drops the newest.
        /// </summary>
        public void AppendToFront( TrajectoryStateVector state )
        {
            double newEndUT = (_count == 0)
                ? StartUT
                : EndUT + TimeResolution;

            int index = (_count + _startIndex) % Capacity;

            if( _count == Capacity )
                _startIndex = (_startIndex + 1) % Capacity; // Slide the window forward.
            else
                _count++;

            _buffer[index] = state;

            if( _count == 1 )
                EndUT = newEndUT;
        }

        /// <summary>
        /// Adds a new sample to the back (lowest UT). If full, drops the oldest.
        /// </summary>
        public void AppendToBack( TrajectoryStateVector state )
        {
            double newStartUT = (_count == 0)
                ? StartUT
                : StartUT - TimeResolution;

            int index = (_startIndex - 1 + Capacity) % Capacity;

            if( _count != Capacity )
                _count++;

            _buffer[index] = state;
            _startIndex = index; // Slide the window backward.

            StartUT = newStartUT;
        }

        /// <summary>
        /// Evaluates the ephemeris at the given time.
        /// </summary>
        /// <returns>The state vector representing the body at the specified time.</returns>
        public TrajectoryStateVector Evaluate( double ut )
        {
            if( _count == 0 )
                throw new InvalidOperationException( "The ephemeris to evaluate must have at least 1 data point." );

            if( ut < StartUT || ut > EndUT )
                throw new ArgumentOutOfRangeException( $"Time '{ut}' is out of the range of this ephemeris: [{StartUT}, {EndUT}]." );

            double pseudoIndex = (ut - StartUT) * _inverseTimeResolution;
            int i0 = (int)pseudoIndex; // Does a floor operation.
            int i1 = i0 + 1;

            double t = pseudoIndex - (double)i0;
            int index1 = (i0 + _startIndex) % Capacity;
            if( t == 0 )
                return _buffer[index1];
            int index2 = (i1 + _startIndex) % Capacity;
            return TrajectoryStateVector.Lerp( _buffer[index1], _buffer[index2], t );
        }
    }
}