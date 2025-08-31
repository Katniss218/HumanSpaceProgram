using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Trajectories.TrajectoryIntegrators
{
    public sealed class RK45Integrator : ITrajectoryIntegrator
    {
        public double AbsTolerance { get; set; } = 1e-3;
        public double RelTolerance { get; set; } = 1e-6;
        public double SafetyFactor { get; set; } = 0.9;

        // Coefficients.
        const double c2 = 1.0 / 5.0;
        const double c3 = 3.0 / 10.0;
        const double c4 = 4.0 / 5.0;
        const double c5 = 8.0 / 9.0;
        const double c6 = 1.0;
        const double c7 = 1.0;

        const double a21 = 1.0 / 5.0;
        const double a31 = 3.0 / 40.0, a32 = 9.0 / 40.0;
        const double a41 = 44.0 / 45.0, a42 = -56.0 / 15.0, a43 = 32.0 / 9.0;
        const double a51 = 19372.0 / 6561.0, a52 = -25360.0 / 2187.0, a53 = 64448.0 / 6561.0, a54 = -212.0 / 729.0;
        const double a61 = 9017.0 / 3168.0, a62 = -355.0 / 33.0, a63 = 46732.0 / 5247.0, a64 = 49.0 / 176.0, a65 = -5103.0 / 18656.0;
        const double a71 = 35.0 / 384.0, a72 = 0.0, a73 = 500.0 / 1113.0, a74 = 125.0 / 192.0, a75 = -2187.0 / 6784.0, a76 = 11.0 / 84.0;

        // 5th order solution.
        const double b1 = 35.0 / 384.0, b2 = 0.0, b3 = 500.0 / 1113.0, b4 = 125.0 / 192.0, b5 = -2187.0 / 6784.0, b6 = 11.0 / 84.0, b7 = 0.0;
        // 4th order embedded solution.
        const double bs1 = 5179.0 / 57600.0, bs2 = 0.0, bs3 = 7571.0 / 16695.0, bs4 = 393.0 / 640.0, bs5 = -92097.0 / 339200.0, bs6 = 187.0 / 2100.0, bs7 = 1.0 / 40.0;

        public double Step( TrajectorySimulationContext context, ReadOnlySpan<ITrajectoryStepProvider> accelerationProviders, out TrajectoryStateVector nextSelf )
        {
            var pos0 = context.Self.AbsolutePosition;
            var vel0 = context.Self.AbsoluteVelocity;
            double dt = context.Step;

            // k1
            Vector3Dbl acc1 = context.SumAccelerations( accelerationProviders );
            Vector3Dbl vel_k1 = vel0;
            Vector3Dbl acc_k1 = acc1;

            // k2
            Vector3Dbl pos_k2 = pos0 + (vel_k1 * (a21 * dt));
            Vector3Dbl vel_k2 = vel0 + (acc_k1 * (a21 * dt));
            var state_k2 = new TrajectoryStateVector( pos_k2, vel_k2, acc_k1, context.Self.Mass );
            var tempContext = context.Substep( context.UT + c2 * dt, state_k2 );
            Vector3Dbl acc2 = tempContext.SumAccelerations( accelerationProviders );

            // k3
            Vector3Dbl pos_k3 = pos0 + (vel_k1 * (a31 * dt)) + (vel_k2 * (a32 * dt));
            Vector3Dbl vel_k3 = vel0 + (acc_k1 * (a31 * dt)) + (acc2 * (a32 * dt));
            var state_k3 = new TrajectoryStateVector( pos_k3, vel_k3, acc2, context.Self.Mass );
            tempContext = context.Substep( context.UT + c3 * dt, state_k3 );
            Vector3Dbl acc3 = tempContext.SumAccelerations( accelerationProviders );

            // k4
            Vector3Dbl pos_k4 = pos0 + (vel_k1 * (a41 * dt)) + (vel_k2 * (a42 * dt)) + (vel_k3 * (a43 * dt));
            Vector3Dbl vel_k4 = vel0 + (acc_k1 * (a41 * dt)) + (acc2 * (a42 * dt)) + (acc3 * (a43 * dt));
            var state_k4 = new TrajectoryStateVector( pos_k4, vel_k4, acc3, context.Self.Mass );
            tempContext = context.Substep( context.UT + c4 * dt, state_k4 );
            Vector3Dbl acc4 = tempContext.SumAccelerations( accelerationProviders );

            // k5
            Vector3Dbl pos_k5 = pos0
                + (vel_k1 * (a51 * dt))
                + (vel_k2 * (a52 * dt))
                + (vel_k3 * (a53 * dt))
                + (vel_k4 * (a54 * dt));
            Vector3Dbl vel_k5 = vel0
                + (acc_k1 * (a51 * dt))
                + (acc2 * (a52 * dt))
                + (acc3 * (a53 * dt))
                + (acc4 * (a54 * dt));
            var state_k5 = new TrajectoryStateVector( pos_k5, vel_k5, acc4, context.Self.Mass );
            tempContext = context.Substep( context.UT + c5 * dt, state_k5 );
            Vector3Dbl acc5 = tempContext.SumAccelerations( accelerationProviders );

            // k6
            Vector3Dbl pos_k6 = pos0
                + (vel_k1 * (a61 * dt))
                + (vel_k2 * (a62 * dt))
                + (vel_k3 * (a63 * dt))
                + (vel_k4 * (a64 * dt))
                + (vel_k5 * (a65 * dt));
            Vector3Dbl vel_k6 = vel0
                + (acc_k1 * (a61 * dt))
                + (acc2 * (a62 * dt))
                + (acc3 * (a63 * dt))
                + (acc4 * (a64 * dt))
                + (acc5 * (a65 * dt));
            var state_k6 = new TrajectoryStateVector( pos_k6, vel_k6, acc5, context.Self.Mass );
            tempContext = context.Substep( context.UT + c6 * dt, state_k6 );
            Vector3Dbl acc6 = tempContext.SumAccelerations( accelerationProviders );

            // k7
            Vector3Dbl pos_k7 = pos0
                + (vel_k1 * (a71 * dt))
                + (vel_k2 * (a72 * dt))
                + (vel_k3 * (a73 * dt))
                + (vel_k4 * (a74 * dt))
                + (vel_k5 * (a75 * dt))
                + (vel_k6 * (a76 * dt));
            Vector3Dbl vel_k7 = vel0
                + (acc_k1 * (a71 * dt))
                + (acc2 * (a72 * dt))
                + (acc3 * (a73 * dt))
                + (acc4 * (a74 * dt))
                + (acc5 * (a75 * dt))
                + (acc6 * (a76 * dt));
            var state_k7 = new TrajectoryStateVector( pos_k7, vel_k7, acc6, context.Self.Mass );
            tempContext = context.Substep( context.UT + c7 * dt, state_k7 );
            Vector3Dbl acc7 = tempContext.SumAccelerations( accelerationProviders );

            // 5th-order (high-order) solution
            Vector3Dbl deltaPos5 = (vel_k1 * b1 + vel_k2 * b2 + vel_k3 * b3 + vel_k4 * b4 + vel_k5 * b5 + vel_k6 * b6 + vel_k7 * b7) * dt;
            Vector3Dbl deltaVel5 = (acc_k1 * b1 + acc2 * b2 + acc3 * b3 + acc4 * b4 + acc5 * b5 + acc6 * b6 + acc7 * b7) * dt;

            Vector3Dbl pos5 = pos0 + deltaPos5;
            Vector3Dbl vel5 = vel0 + deltaVel5;

            // 4th-order (embedded) solution
            Vector3Dbl deltaPos4 = (vel_k1 * bs1 + vel_k2 * bs2 + vel_k3 * bs3 + vel_k4 * bs4 + vel_k5 * bs5 + vel_k6 * bs6 + vel_k7 * bs7) * dt;
            Vector3Dbl deltaVel4 = (acc_k1 * bs1 + acc2 * bs2 + acc3 * bs3 + acc4 * bs4 + acc5 * bs5 + acc6 * bs6 + acc7 * bs7) * dt;

            Vector3Dbl pos4 = pos0 + deltaPos4;
            Vector3Dbl vel4 = vel0 + deltaVel4;

            // Different error estimate
            Vector3Dbl errPosVec = pos5 - pos4;
            Vector3Dbl errVelVec = vel5 - vel4;

            double scaleX = AbsTolerance + RelTolerance * Math.Max( Math.Abs( pos0.x ), Math.Abs( pos5.x ) );
            double scaleY = AbsTolerance + RelTolerance * Math.Max( Math.Abs( pos0.y ), Math.Abs( pos5.y ) );
            double scaleZ = AbsTolerance + RelTolerance * Math.Max( Math.Abs( pos0.z ), Math.Abs( pos5.z ) );

            double scaleVX = AbsTolerance + RelTolerance * Math.Max( Math.Abs( vel0.x ), Math.Abs( vel5.x ) );
            double scaleVY = AbsTolerance + RelTolerance * Math.Max( Math.Abs( vel0.y ), Math.Abs( vel5.y ) );
            double scaleVZ = AbsTolerance + RelTolerance * Math.Max( Math.Abs( vel0.z ), Math.Abs( vel5.z ) );

            // Normalize squared errors
            double ex = errPosVec.x / scaleX;
            double ey = errPosVec.y / scaleY;
            double ez = errPosVec.z / scaleZ;

            double evx = errVelVec.x / scaleVX;
            double evy = errVelVec.y / scaleVY;
            double evz = errVelVec.z / scaleVZ;

            // Weighted RMS norm
            double err = Math.Sqrt( (ex * ex + ey * ey + ez * ez + evx * evx + evy * evy + evz * evz) / 6.0 );

            const double exponent = 1.0 / 5.0;
            double scale = SafetyFactor * Math.Pow( 1.0 / err, exponent );
            scale = Math.Clamp( scale, 0.1, 10.0 );
            double suggestedDt = dt * scale;

            nextSelf = new TrajectoryStateVector( pos5, vel5, acc7, context.Self.Mass );
            return suggestedDt;
        }

        [MapsInheritingFrom( typeof( RK45Integrator ) )]
        public static SerializationMapping RK45IntegratorMapping()
        {
            return new MemberwiseSerializationMapping<RK45Integrator>();
        }
    }
}