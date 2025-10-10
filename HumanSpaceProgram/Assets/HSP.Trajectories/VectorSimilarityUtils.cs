using System.Runtime.CompilerServices;
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
        public static double SymRelativeVec( Vector3Dbl a, Vector3Dbl b, double eps )
        {
            double den = a.magnitude + b.magnitude + eps;
            return (a - b).magnitude / den;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static double SymRelativeScalar( double a, double b, double eps )
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
}