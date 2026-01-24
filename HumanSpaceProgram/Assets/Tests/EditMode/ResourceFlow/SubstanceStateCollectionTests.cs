using HSP.ResourceFlow;
using NUnit.Framework;

namespace HSP_Tests_EditMode.ResourceFlow
{
    [TestFixture]
    public class SubstanceStateCollectionTests
    {
        private ISubstance _substanceA;
        private ISubstance _substanceB;

        [SetUp]
        public void SetUp()
        {
            _substanceA = new Substance( "A" ) { MolarMass = 1 };
            _substanceB = new Substance( "B" ) { MolarMass = 1 };
        }

        [Test, Description( "Tests basic add, set, and retrieve functionality." )]
        public void AddSetRetrieve_BehavesCorrectly()
        {
            // Arrange
            var collection = new SubstanceStateCollection();

            // Act & Assert (Add via Add method)
            collection.Add( _substanceA, 50.0 );
            Assert.That( collection.Count, Is.EqualTo( 1 ) );
            Assert.That( collection[_substanceA], Is.EqualTo( 50.0 ) );
            Assert.That( collection.TryGet( _substanceA, out double mass ), Is.True );
            Assert.That( mass, Is.EqualTo( 50.0 ) );

            // Act & Assert (Update via setter)
            collection[_substanceA] = 75.0;
            Assert.That( collection.Count, Is.EqualTo( 1 ) );
            Assert.That( collection[_substanceA], Is.EqualTo( 75.0 ) );

            // Act & Assert (Retrieve non-existent)
            Assert.That( collection[_substanceB], Is.EqualTo( 0.0 ) );
            Assert.That( collection.TryGet( _substanceB, out _ ), Is.False );
        }

        [Test, Description( "Tests that adding a negative mass correctly subtracts and removes the substance if its mass becomes zero or less." )]
        public void Add_WithNegativeMass_RemovesSubstance()
        {
            // Arrange
            var collection = new SubstanceStateCollection();
            collection.Add( _substanceA, 50.0 );
            collection.Add( _substanceB, 30.0 );

            // Act
            collection.Add( _substanceA, -50.0 );

            // Assert
            Assert.That( collection.Count, Is.EqualTo( 1 ), "Substance A should have been removed." );
            Assert.That( collection.Contains( _substanceA ), Is.False );
            Assert.That( collection.Contains( _substanceB ), Is.True );
            Assert.That( collection[_substanceB], Is.EqualTo( 30.0 ) );
        }

        [Test, Description( "Tests that adding one collection to another correctly sums the masses of common substances." )]
        public void Add_Collection_SumsCorrectly()
        {
            // Arrange
            var collection1 = new SubstanceStateCollection();
            collection1.Add( _substanceA, 10.0 );
            collection1.Add( _substanceB, 20.0 );

            var collection2 = new SubstanceStateCollection();
            collection2.Add( _substanceB, 30.0 );

            // Act
            collection1.Add( collection2 );

            // Assert
            Assert.That( collection1.Count, Is.EqualTo( 2 ) );
            Assert.That( collection1[_substanceA], Is.EqualTo( 10.0 ) );
            Assert.That( collection1[_substanceB], Is.EqualTo( 50.0 ) );
        }

        [Test, Description( "Tests that scaling the collection correctly multiplies all substance masses." )]
        public void Scale_MultipliesAllMasses()
        {
            // Arrange
            var collection = new SubstanceStateCollection();
            collection.Add( _substanceA, 10.0 );
            collection.Add( _substanceB, 20.0 );

            // Act
            collection.Scale( 2.5 );

            // Assert
            Assert.That( collection[_substanceA], Is.EqualTo( 25.0 ) );
            Assert.That( collection[_substanceB], Is.EqualTo( 50.0 ) );
        }

        [Test, Description( "Tests that cloning a collection creates a deep, independent copy." )]
        public void Clone_CreatesIndependentCopy()
        {
            // Arrange
            var original = new SubstanceStateCollection();
            original.Add( _substanceA, 100.0 );

            // Act
            var clone = original.Clone() as SubstanceStateCollection;
            Assert.That( clone, Is.Not.Null );

            // Modify original
            original.Add( _substanceA, 50.0 );

            // Assert
            Assert.That( original[_substanceA], Is.EqualTo( 150.0 ), "Original should have been modified." );
            Assert.That( clone[_substanceA], Is.EqualTo( 100.0 ), "Clone should not have been modified." );
        }

        [Test, Description( "Tests that GetMass() returns a consistent total mass after various operations." )]
        public void GetMass_IsConsistent()
        {
            // Arrange
            var collection = new SubstanceStateCollection();
            Assert.That( collection.GetMass(), Is.EqualTo( 0.0 ) );

            // Act & Assert: Add
            collection.Add( _substanceA, 50.0 );
            Assert.That( collection.GetMass(), Is.EqualTo( 50.0 ) );

            // Act & Assert: Add another substance
            collection.Add( _substanceB, 30.0 );
            Assert.That( collection.GetMass(), Is.EqualTo( 80.0 ) );

            // Act & Assert: Remove a substance by setting to 0
            collection[_substanceB] = 0.0;
            Assert.That( collection.GetMass(), Is.EqualTo( 50.0 ) );

            // Act & Assert: Scale
            collection.Scale( 0.5 );
            Assert.That( collection.GetMass(), Is.EqualTo( 25.0 ) );

            // Act & Assert: Clear
            collection.Clear();
            Assert.That( collection.GetMass(), Is.EqualTo( 0.0 ) );
        }
    }
}
