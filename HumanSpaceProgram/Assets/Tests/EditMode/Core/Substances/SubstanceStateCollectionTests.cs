using System.Collections;
using System.Collections.Generic;
using HSP.Core;
using HSP.Core.ResourceFlowSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace KSS_Tests_EditMode
{
    public class SubstanceStateCollectionTests
    {
        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###
        // Tests use a fresh, clean scene.
        // ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ### ###
        
        [Test]
        public void Empty___ShouldBeEmpty()
        {
            // Arrange
            SubstanceStateCollection collection1 = SubstanceStateCollection.Empty;

            // Assert
            Assert.IsTrue( collection1.IsEmpty() );
        }

        [Test]
        public void Add___SingleSubstance___AddsToExisting()
        {
            // Arrange
            Substance sbs = new Substance();
            SubstanceStateCollection collection1 = new SubstanceStateCollection( new SubstanceState( 50f, sbs ) );
            SubstanceStateCollection collection2 = new SubstanceStateCollection( new SubstanceState( 50f, sbs ) );

            // Act
            collection1.Add( collection2, 1 );

            // Assert
            Assert.IsTrue( collection1.SubstanceCount == 1 && collection1[0].MassAmount == 100f );
        }

        [Test]
        public void Add___NegativeAmount___Subtracts()
        {
            // Arrange
            Substance sbs = new Substance();
            SubstanceStateCollection collection1 = new SubstanceStateCollection( new SubstanceState( 50f, sbs ) );
            SubstanceStateCollection collection2 = new SubstanceStateCollection( new SubstanceState( -50f, sbs ) );

            // Act
            collection1.Add( collection2, 1 );

            // Assert
            Assert.IsTrue( collection1.SubstanceCount == 1 && collection1[0].MassAmount == 0f );
        }

        [Test]
        public void Add___SingleSubstance_NegativeDt___RemovesFromExisting()
        {
            // Arrange
            Substance sbs = new Substance();
            SubstanceStateCollection collection1 = new SubstanceStateCollection( new SubstanceState( 100f, sbs ) );
            SubstanceStateCollection collection2 = new SubstanceStateCollection( new SubstanceState( 50f, sbs ) );

            // Act
            collection1.Add( collection2, -1 );

            // Assert
            Assert.IsTrue( collection1.SubstanceCount == 1 && collection1[0].MassAmount == 50f );
        }

        [Test]
        public void Add___MultipleSubstances___AddsToCorresponding()
        {
            // Arrange
            Substance sbs1 = new Substance();
            Substance sbs2 = new Substance();
            Substance sbs3 = new Substance();
            Substance sbs4 = new Substance();

            SubstanceStateCollection collection1 = new SubstanceStateCollection( 
                new SubstanceState( 50f, sbs1 ), 
                new SubstanceState( 30f, sbs2 ),
                new SubstanceState( 20f, sbs3 ) ,
                new SubstanceState( 10f, sbs4 ) );

            SubstanceStateCollection collection2 = new SubstanceStateCollection( 
                new SubstanceState( 10f, sbs4 ), 
                new SubstanceState( 20f, sbs3 ), 
                new SubstanceState( 30f, sbs2 ), 
                new SubstanceState( 50f, sbs1 ) ); // different order of elements on purpose.

            // Act
            collection1.Add( collection2, 1 );

            // Assert
            Assert.IsTrue( collection1.SubstanceCount == 4 && 
                collection1[0].MassAmount == 100f
             && collection1[1].MassAmount == 60f
             && collection1[2].MassAmount == 40f
             && collection1[3].MassAmount == 20f );
        }

        [Test]
        public void Add___ToEmpty___AddsNewSubstance()
        {
            // Arrange
            Substance sbs = new Substance();
            SubstanceStateCollection collection1 = SubstanceStateCollection.Empty;
            SubstanceStateCollection collection2 = new SubstanceStateCollection( new SubstanceState( 50f, sbs ) );

            // Act
            collection1.Add( collection2, 1 );

            // Assert
            Assert.IsTrue( collection1.SubstanceCount == 1 && collection1[0].MassAmount == 50f );
        }

        [Test]
        public void Add___ToEmpty_NegativeDt___AddsNewSubstanceWithNegativeAmount()
        {
            // Arrange
            Substance sbs = new Substance();
            SubstanceStateCollection collection1 = new SubstanceStateCollection();
            SubstanceStateCollection collection2 = new SubstanceStateCollection( new SubstanceState( 50f, sbs ) );

            // Act
            collection1.Add( collection2, -1 );

            // Assert
            Assert.IsTrue( collection1.SubstanceCount == 1 && collection1[0].MassAmount == -50f );
        }
    }
}