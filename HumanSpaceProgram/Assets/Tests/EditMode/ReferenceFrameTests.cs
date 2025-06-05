using System;
using System.Collections;
using System.Collections.Generic;
using HSP.ReferenceFrames;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HSP_Tests_EditMode
{
    public class CenteredInertialReferenceFrameTests
    {
        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

        // Tests use a fresh, clean scene.

        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###

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