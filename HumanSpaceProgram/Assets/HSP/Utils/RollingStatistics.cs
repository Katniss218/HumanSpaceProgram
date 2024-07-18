using System;
using System.Linq;

namespace HSP
{
    /// <summary>
    /// A class for doing statistics on a rolling number of samples.
    /// </summary>
    public class RollingStatistics
    {
        private float[] _samples;

        private int _current = 0;
        private int _sampleCount = 0;

        public RollingStatistics( int size )
        {
            if( size < 2 )
            {
                throw new ArgumentOutOfRangeException( nameof( size ), $"Size must be at least 2." );
            }

            _samples = new float[size];
        }

        public float GetMean()
        {
            float sum = 0;
            for( int i = 0; i < _sampleCount; i++ )
            {
                sum += _samples[i];
            }

            return _sampleCount == 0
                ? 0
                : sum / _sampleCount;
        }

        public float GetMedian()
        {
            if( _sampleCount == 0 )
                return _sampleCount;

            float[] sortedSamples = _samples.Take( _sampleCount ).OrderBy( x => x ).ToArray();

            return _sampleCount % 2 == 0
                ? (sortedSamples[_sampleCount / 2 - 1] + sortedSamples[_sampleCount / 2]) / 2
                : sortedSamples[_sampleCount / 2];

        }

        /// <summary>
        /// Returns an array of values for the given corresponding percentiles.
        /// </summary>
        public float[] GetPercentiles( params float[] percentiles )
        {
            if( _sampleCount == 0 )
                return new float[] { };

            float[] sortedSamples = _samples.Take( _sampleCount ).OrderBy( x => x ).ToArray();

            float[] result = new float[percentiles.Length];
            for( int i = 0; i < percentiles.Length; i++ )
            {
                float percentile = percentiles[i] * 100;
                int index = (int)Math.Ceiling( (percentile / 100) * _sampleCount ) - 1;
                result[i] = sortedSamples[index];
            }

            return result;
        }

        /// <summary>
        /// Adds a sample of given value to the rolling array. Replaces the oldest sample if full.
        /// </summary>
        public void AddSample( float value )
        {
            _samples[_current] = value;

            _current = (_current + 1) % _samples.Length;

            if( _sampleCount < _samples.Length ) // Prevents non-initialized samples from being used when calculating the average.
            {
                _sampleCount++;
            }
        }
    }
}