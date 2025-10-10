using System.Runtime.CompilerServices;
using UnityEngine;

namespace HSP.Trajectories
{
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

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
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

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
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
}