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
            _substanceA = new Substance("A");
            _substanceB = new Substance("B");
        }

        [Test]
        public void Constructor___Default___IsEmpty()
        {
            var collection = new SubstanceStateCollection();
            Assert.IsTrue(collection.IsEmpty());
            Assert.AreEqual(0, collection.Count);
        }

        [Test]
        public void Indexer_Set___NewSubstance___AddsSubstance()
        {
            var collection = new SubstanceStateCollection();
            collection[_substanceA] = 50.0;

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(50.0, collection[_substanceA]);
            Assert.AreEqual(50.0, collection.GetMass());
        }

        [Test]
        public void Indexer_Set___ExistingSubstance___UpdatesMass()
        {
            var collection = new SubstanceStateCollection();
            collection[_substanceA] = 50.0;
            collection[_substanceA] = 100.0;

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(100.0, collection[_substanceA]);
            Assert.AreEqual(100.0, collection.GetMass());
        }

        [Test]
        public void Indexer_Set___ToZero___RemovesSubstance()
        {
            var collection = new SubstanceStateCollection();
            collection[_substanceA] = 50.0;
            collection[_substanceB] = 25.0;
            
            collection[_substanceA] = 0.0;

            Assert.AreEqual(1, collection.Count);
            Assert.IsFalse(collection.Contains(_substanceA));
            Assert.AreEqual(25.0, collection.GetMass());
        }

        [Test]
        public void Add_Single___NewSubstance___AddsCorrectly()
        {
            var collection = new SubstanceStateCollection();
            collection.Add(_substanceA, 75.0);

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(75.0, collection[_substanceA]);
        }

        [Test]
        public void Add_Single___ExistingSubstance___UpdatesCorrectly()
        {
            var collection = new SubstanceStateCollection();
            collection.Add(_substanceA, 50.0);
            collection.Add(_substanceA, 25.0);

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(75.0, collection[_substanceA]);
        }
        
        [Test]
        public void Add_Single___ToZero___RemovesSubstance()
        {
            var collection = new SubstanceStateCollection();
            collection.Add(_substanceA, 50.0);
            collection.Add(_substanceA, -50.0);

            Assert.IsTrue(collection.IsEmpty());
        }
        
        [Test]
        public void Add_Collection___AddsAllSubstances()
        {
            var collection1 = new SubstanceStateCollection();
            collection1.Add(_substanceA, 10.0);

            var collection2 = new SubstanceStateCollection();
            collection2.Add(_substanceA, 20.0);
            collection2.Add(_substanceB, 30.0);

            collection1.Add(collection2);

            Assert.AreEqual(2, collection1.Count);
            Assert.AreEqual(30.0, collection1[_substanceA]);
            Assert.AreEqual(30.0, collection1[_substanceB]);
            Assert.AreEqual(60.0, collection1.GetMass());
        }

        [Test]
        public void Scale___ScalesAllMasses()
        {
            var collection = new SubstanceStateCollection();
            collection.Add(_substanceA, 10.0);
            collection.Add(_substanceB, 20.0);

            collection.Scale(2.0);

            Assert.AreEqual(20.0, collection[_substanceA]);
            Assert.AreEqual(40.0, collection[_substanceB]);
            Assert.AreEqual(60.0, collection.GetMass());
        }

        [Test]
        public void Clear___RemovesAllSubstances()
        {
            var collection = new SubstanceStateCollection();
            collection.Add(_substanceA, 10.0);
            collection.Add(_substanceB, 20.0);

            collection.Clear();

            Assert.IsTrue(collection.IsEmpty());
            Assert.AreEqual(0, collection.Count);
            Assert.AreEqual(0, collection.GetMass());
        }
        
        [Test]
        public void Clone___CreatesIdenticalIndependentCopy()
        {
            var original = new SubstanceStateCollection();
            original.Add(_substanceA, 100.0);

            var clone = (SubstanceStateCollection)original.Clone();
            
            Assert.AreEqual(1, clone.Count);
            Assert.AreEqual(100.0, clone[_substanceA]);
            
            // Modify original and ensure clone is unaffected
            original.Add(_substanceA, 50.0);
            
            Assert.AreEqual(150.0, original[_substanceA]);
            Assert.AreEqual(100.0, clone[_substanceA]);
        }
    }
}
