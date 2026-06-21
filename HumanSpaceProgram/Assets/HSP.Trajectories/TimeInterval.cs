using System;

namespace HSP.Trajectories
{
    public readonly struct TimeInterval
    {
        public readonly double minUT;
        public readonly double maxUT;

        public double duration => maxUT - minUT;

        public TimeInterval( double point )
        {
            this.minUT = point;
            this.maxUT = point;
        }

        public TimeInterval( double minUT, double maxUT )
        {
            if( maxUT < minUT )
                throw new ArgumentException( "maxUT must be greater than or equal to minUT." );

            this.minUT = minUT;
            this.maxUT = maxUT;
        }

        public bool Contains( double ut )
        {
            return ut >= minUT && ut <= maxUT;
        }
    }
}