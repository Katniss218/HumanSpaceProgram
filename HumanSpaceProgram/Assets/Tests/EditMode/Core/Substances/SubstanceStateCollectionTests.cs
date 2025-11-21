using HSP.ResourceFlow;
using NUnit.Framework;

namespace HSP_Tests_EditMode
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
            ISubstance sbs = new Substance( "test" );
            ISubstanceStateCollection collection1 = new SubstanceStateCollection()
            {
                { sbs, 50f }
            };
            ISubstanceStateCollection collection2 = new SubstanceStateCollection()
            {
                { sbs, 50f }
            };

            // Act
            collection1.Add( collection2, 1 );

            // Assert
            Assert.IsTrue( collection1.Count == 1 && collection1[0].Item2 == 100f );
        }

        [Test]
        public void Add___NegativeAmount___Subtracts()
        {
            // Arrange
            ISubstance sbs = new Substance( "test" );
            ISubstanceStateCollection collection1 = new SubstanceStateCollection()
            {
                { sbs, 50f }
            };
            ISubstanceStateCollection collection2 = new SubstanceStateCollection()
            {
                { sbs, -50f }
            };

            // Act
            collection1.Add( collection2, 1 );

            // Assert
            Assert.IsTrue( collection1.Count == 1 && collection1[0].Item2 == 0f );
        }

        [Test]
        public void Add___SingleSubstance_NegativeDt___RemovesFromExisting()
        {
            // Arrange
            ISubstance sbs = new Substance( "test" );
            ISubstanceStateCollection collection1 = new SubstanceStateCollection()
            {
                { sbs, 100f }
            };
            ISubstanceStateCollection collection2 = new SubstanceStateCollection()
            {
                { sbs, 50f }
            };

            // Act
            collection1.Add( collection2, -1 );

            // Assert
            Assert.IsTrue( collection1.Count == 1 && collection1[0].Item2 == 50f );
        }

        [Test]
        public void Add___MultipleSubstances___AddsToCorresponding()
        {
            // Arrange
            Substance sbs1 = new Substance( "t1" );
            Substance sbs2 = new Substance( "t2" );
            Substance sbs3 = new Substance( "t3" );
            Substance sbs4 = new Substance( "t4" );

            SubstanceStateCollection collection1 = new SubstanceStateCollection()
            {
                { sbs1, 50f },
                { sbs2, 30f },
                { sbs3, 20f },
                { sbs4, 10f }
            };

            SubstanceStateCollection collection2 = new SubstanceStateCollection()
            {
                { sbs4, 10f }, // different order of elements on purpose.
                { sbs3, 20f },
                { sbs2, 30f },
                { sbs1, 50f }
            };

            // Act
            collection1.Add( collection2, 1 );

            // Assert
            Assert.IsTrue( collection1.Count == 4 &&
                collection1[0].Item2 == 100f
             && collection1[1].Item2 == 60f
             && collection1[2].Item2 == 40f
             && collection1[3].Item2 == 20f );
        }

        [Test]
        public void Add___ToEmpty___AddsNewSubstance()
        {
            // Arrange
            ISubstance sbs = new Substance( "test" );
            ISubstanceStateCollection collection1 = SubstanceStateCollection.Empty;
            ISubstanceStateCollection collection2 = new SubstanceStateCollection()
            {
                { sbs, 50f }
            };

            // Act
            collection1.Add( collection2, 1 );

            // Assert
            Assert.IsTrue( collection1.Count == 1 && collection1[0].Item2 == 50f );
        }

        [Test]
        public void Add___ToEmpty_NegativeDt___AddsNewSubstanceWithNegativeAmount()
        {
            // Arrange
            ISubstance sbs = new Substance( "test" );
            ISubstanceStateCollection collection1 = SubstanceStateCollection.Empty;
            ISubstanceStateCollection collection2 = new SubstanceStateCollection()
            {
                { sbs, 50f }
            };

            // Act
            collection1.Add( collection2, -1 );

            // Assert
            Assert.IsTrue( collection1.Count == 1 && collection1[0].Item2 == -50f );
        }
    }
}