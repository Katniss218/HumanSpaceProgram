using HSP.ReferenceFrames;
using HSP_Tests.NUnit;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP_Tests_EditMode
{
    public class CenteredInertialReferenceFrameTests
    {
        static IEqualityComparer<Vector3Dbl> vector3DblApproxComparer = new Vector3DblApproximateComparer( 1e-12 );
        static IEqualityComparer<QuaternionDbl> quaternionDblApproxComparer = new QuaternionDblApproximateComparer( 1e-12 );

        [Test]
        public void AtUt___WorksCorrectly()
        {
            CenteredInertialReferenceFrame sut = new CenteredInertialReferenceFrame( 0, Vector3Dbl.zero, Vector3Dbl.one );

            var sut2 = sut.AtUT( 1 );

            Assert.That( sut2.TransformPosition( Vector3Dbl.zero ), Is.EqualTo( Vector3Dbl.one ) );
        }

        [Test]
        public void AtUtBackwards___WorksCorrectly()
        {
            CenteredInertialReferenceFrame sut = new CenteredInertialReferenceFrame( 0, Vector3Dbl.zero, Vector3Dbl.one );

            var sut2 = sut.AtUT( -1 );

            Assert.That( sut2.TransformPosition( Vector3Dbl.zero ), Is.EqualTo( -Vector3Dbl.one ) );
        }

        [Test]
        public void Constructor___SetsCorrectValues()
        {
            var position = new Vector3Dbl( 10.0, 20.0, 30.0 );
            var velocity = new Vector3Dbl( 1.0, 2.0, 3.0 );
            var referenceUT = 1000.0;
            var frame = new CenteredInertialReferenceFrame( referenceUT, position, velocity );

            Assert.That( frame.ReferenceUT, Is.EqualTo( referenceUT ) );
            Assert.That( frame.Position, Is.EqualTo( position ) );
            Assert.That( frame.Velocity, Is.EqualTo( velocity ) );
        }

        [Test]
        public void AtUT___CalculatesCorrectPosition()
        {
            var frame = new CenteredInertialReferenceFrame( 0.0, Vector3Dbl.zero, new Vector3Dbl( 1.0, 2.0, 3.0 ) );
            var futureFrame = (CenteredInertialReferenceFrame)frame.AtUT( 10.0 );

            var expectedPosition = new Vector3Dbl( 10.0, 20.0, 30.0 );
            Assert.That( futureFrame.Position, Is.EqualTo( expectedPosition ) );
        }

        [Test]
        public void TransformVelocity___AddsFrameVelocity()
        {
            var frame = new CenteredInertialReferenceFrame( 0.0, Vector3Dbl.zero, new Vector3Dbl( 5.0, 10.0, 15.0 ) );
            var localVel = new Vector3Dbl( 1.0, 2.0, 3.0 );

            var globalVel = frame.TransformVelocity( localVel );
            var expectedVel = new Vector3Dbl( 6.0, 12.0, 18.0 );

            Assert.That( globalVel, Is.EqualTo( expectedVel ) );
        }

        [Test]
        public void AllTransforms___RoundTripAccuracy()
        {
            var frame = new CenteredInertialReferenceFrame( 0.0, new Vector3Dbl( 1.0, 2.0, 3.0 ), new Vector3Dbl( 0.1, 0.2, 0.3 ) );
            ReferenceFrameTestHelpers.AssertRoundTripAccuracy( frame, "CenteredInertialReferenceFrame" );
        }

        [Test]
        public void TimeEvolution___PreservesProperties()
        {
            var frame = new CenteredInertialReferenceFrame( 0.0, Vector3Dbl.zero, new Vector3Dbl( 1.0, 2.0, 3.0 ) );
            ReferenceFrameTestHelpers.AssertTimeEvolution( frame, 100.0, "CenteredInertialReferenceFrame" );
        }

        [Test]
        public void Equals___WorksCorrectly()
        {
            var frame1 = new CenteredInertialReferenceFrame( 0.0, Vector3Dbl.zero, new Vector3Dbl( 1.0, 2.0, 3.0 ) );
            var frame2 = new CenteredInertialReferenceFrame( 0.0, Vector3Dbl.zero, new Vector3Dbl( 1.0, 2.0, 3.0 ) );
            var frame3 = new CenteredInertialReferenceFrame( 0.0, Vector3Dbl.zero, new Vector3Dbl( 1.0, 2.0, 4.0 ) );

            ReferenceFrameTestHelpers.AssertEqualityMethods( frame1, frame2, true, "CenteredInertialReferenceFrame" );
            ReferenceFrameTestHelpers.AssertEqualityMethods( frame1, frame3, false, "CenteredInertialReferenceFrame" );
        }

        [Test]
        public void TIME()
        {/// <summary>
         /// Calculates burn time to achieve a given delta-v using Tsiolkovsky rocket equation.
         /// </summary>
         /// <param name="deltaV">Desired change in velocity (m/s)</param>
         /// <param name="initialMass">Initial mass of the rocket (kg)</param>
         /// <param name="thrust">Constant engine thrust (N)</param>
         /// <param name="exhaustVelocity">Effective exhaust velocity (m/s)</param>
         /// <returns>Burn time in seconds</returns>
            double CalculateBurnTime( double deltaV, double initialMass, double thrust, double exhaustVelocity )
            {
                if( initialMass <= 0 )
                    throw new ArgumentException( "Initial mass must be positive." );
                if( thrust <= 0 )
                    throw new ArgumentException( "Thrust must be positive." );
                if( exhaustVelocity <= 0 )
                    throw new ArgumentException( "Exhaust velocity must be positive." );
                if( deltaV < 0 )
                    throw new ArgumentException( "Delta-v cannot be negative." );

                // Compute final mass after burn using the rocket equation
                double finalMass = initialMass * Math.Exp( -deltaV / exhaustVelocity );

                // Mass flow rate (kg/s)
                double massFlowRate = thrust / exhaustVelocity;

                // Burn time = mass used / mass flow rate
                double burnTime = (initialMass - finalMass) / massFlowRate;

                return burnTime / (31556952); // to years
            }

            //time = CalculateBurnTime( 5000, 1_000_000, 5_000_000, 4_000 );

            Debug.Log( CalculateBurnTime( 5000, 1600, 0.1, 40_000 ) );
            Debug.Log( CalculateBurnTime( 7500, 1600, 0.1, 40_000 ) );
            Debug.Log( CalculateBurnTime( 10000, 1600, 0.1, 40_000 ) );
            Debug.Log( CalculateBurnTime( 10000, 1600, 0.1, 40_000 ) );
        }
    }
}