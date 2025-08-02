using System;

namespace HSP.Trajectories
{
    public sealed class Ephemeris
    {
        public TrajectoryTransform Body { get; }

        public double TimeResolution { get; }
        public double StartUT { get; }
        public double EndUT { get; }

        readonly double _inverseTimeResolution; // Number of data points per second.
        readonly double _offset;
        readonly TrajectoryBodyState[] _points;

        /// <param name="timeResolution">Number of seconds between every data point.</param>
        public Ephemeris( TrajectoryTransform body, double timeResolution, double startUT, double endUT )
        {
            if( body == null )
                throw new ArgumentNullException( nameof( body ), "Body cannot be null." );

            if( timeResolution <= 0 )
                throw new ArgumentOutOfRangeException( nameof( timeResolution ), "Time resolution must be greater than zero." );

            if( startUT >= endUT )
                throw new ArgumentOutOfRangeException( nameof( startUT ), "Start UT must be less than End UT." );

            this.Body = body;
            this.TimeResolution = timeResolution;
            this.StartUT = startUT;
            this.EndUT = endUT;

            int length = (int)Math.Ceiling( (endUT - startUT) / timeResolution );

            _inverseTimeResolution = 1.0 / timeResolution;
            _offset = -startUT * _inverseTimeResolution;
            this._points = new TrajectoryBodyState[length];
        }

        public void SetClosestPoint( double ut, TrajectoryBodyState stateVector )
        {
            if( ut < StartUT || ut > EndUT )
                throw new ArgumentOutOfRangeException( $"Time '{ut}' is out of the range of this ephemeris: [{StartUT}, {EndUT}]." );

            int index = (int)Math.Round( (ut - StartUT) / TimeResolution );
            this._points[index] = stateVector;
        }

        /// <summary>
        /// Evaluates the ephemeris at the given time.
        /// </summary>
        /// <returns>The state vector representing the body at the specified time.</returns>
        public TrajectoryBodyState Evaluate( double ut )
        {
            if( ut < StartUT || ut > EndUT )
                throw new ArgumentOutOfRangeException( $"Time '{ut}' is out of the range of this ephemeris: [{StartUT}, {EndUT}]." );

            // Floating point 'index' remapped from [StartUT, EndUT] to [0, _points.Length - 1].
            double time = ut * _inverseTimeResolution + _offset;

            int index1 = (int)time;
            if( index1 >= _points.Length - 1 )
            {
                return _points[_points.Length - 1];
            }

            var v1 = _points[index1];
            var v2 = _points[index1 + 1];
            double t = time - (double)index1;
            return TrajectoryBodyState.Lerp( v1, v2, t );
        }
    }
}