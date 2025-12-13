using HSP.ResourceFlow;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HSP_Tests_EditMode.ResourceFlow
{
    [TestFixture]
    public class DelaunayTetrahedralizerTests
    {
        private const float EPSILON = 1e-4f;

        [Test]
        public void SimpleNonCoplanar___ProducesSingleTetrahedron()
        {
            // Arrange
            var input = new List<Vector3>
            {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 0, 1)
            };

            // Act
            var (nodes, edges, tets) = DelaunayTetrahedralizer.ComputeTetrahedralization( input );

            // Assert
            Assert.That( nodes, Is.Not.Null );
            Assert.That( edges, Is.Not.Null );
            Assert.That( tets, Is.Not.Null );

            Assert.That( nodes.Count, Is.EqualTo( 4 ), "Should have 4 nodes for 4 unique inputs." );
            Assert.That( tets.Count, Is.EqualTo( 1 ), "Expected exactly 1 tetrahedron for 4 non-coplanar points." );
            Assert.That( edges.Count, Is.EqualTo( 6 ), "A tetrahedron should have 6 edges." );

            double expectedVolume = 1.0 / 6.0; // Volume of this specific tetrahedron
            Assert.That( tets.Sum( t => t.GetVolume() ), Is.EqualTo( expectedVolume ).Within( EPSILON ), "Tetrahedron volume is incorrect." );
        }

        [Test]
        public void Cube___ProducesMultipleTetrahedra_WithCorrectVolume()
        {
            // Arrange
            var input = new List<Vector3>
            {
                new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0),
                new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 1)
            };

            // Act
            var (nodes, edges, tets) = DelaunayTetrahedralizer.ComputeTetrahedralization( input );

            // Assert
            Assert.That( nodes.Count, Is.EqualTo( 8 ) );
            Assert.That( tets.Count, Is.GreaterThanOrEqualTo( 5 ), "A cube should be divided into at least 5 tetrahedra." );

            double totalVolume = tets.Sum( t => t.GetVolume() );
            Assert.That( totalVolume, Is.EqualTo( 1.0 ).Within( EPSILON ), "Total volume of tetrahedra should equal the volume of the unit cube." );
        }

        [TestCase( 2, 2, 2 )]
        [TestCase( 3, 2, 2 )]
        [TestCase( 3, 4, 5 )]
        [TestCase( 6, 6, 6 )]
        [TestCase( 5, 6, 7 )]
        [TestCase( 7, 7, 7 )]
        [TestCase( 3, 12, 3 )]
        [Test]
        public void GridPoints___GeneratesValidTetrahedralization( int xCount, int yCount, int zCount )
        {
            // Arrange
            var input = new List<Vector3>();
            for( int x = 0; x < xCount; x++ )
                for( int y = 0; y < yCount; y++ )
                    for( int z = 0; z < zCount; z++ )
                    {
                        input.Add( new Vector3( x, y, z ) );
                    }

            // Act
            var (nodes, edges, tets) = DelaunayTetrahedralizer.ComputeTetrahedralization( input );

            // Assert
            Assert.That( nodes.Count, Is.EqualTo( xCount * yCount * zCount ) );
            Assert.That( tets.Count, Is.GreaterThan( 0 ), "A grid of points should produce a valid tetrahedralization." );

            double totalVolume = tets.Sum( t => t.GetVolume() );
            double convexHullVolume = (xCount - 1) * (yCount - 1) * (zCount - 1); // Volume of the bounding box
            Assert.That( totalVolume, Is.EqualTo( convexHullVolume ).Within( EPSILON ), "Total volume should match the volume of the grid's convex hull." );
        }

        [Test]
        public void CoplanarPoints___HandledGracefully()
        {
            // Arrange
            var input = new List<Vector3>
            {
                new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0)
            };

            // Act
            var (nodes, edges, tets) = DelaunayTetrahedralizer.ComputeTetrahedralization( input );

            // Assert
            // The noise should make the points non-coplanar, allowing tetrahedralization.
            Assert.That( tets.Count, Is.GreaterThan( 0 ), "Tetrahedralization should succeed due to internal noise injection." );

            // The volume should be very small, close to zero, as the shape is nearly flat.
            double totalVolume = tets.Sum( t => t.GetVolume() );
            Assert.That( totalVolume, Is.LessThan( 0.1 ), "Total volume of a nearly-flat tetrahedralization should be close to zero." );
        }

        [Test]
        public void CollinearPoints___HandledGracefully()
        {
            // Arrange
            var input = new List<Vector3>
            {
                new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(2, 0, 0), // Collinear
                new Vector3(0, 1, 0) // Fourth point to make it 3D
            };

            // Act
            var (nodes, edges, tets) = DelaunayTetrahedralizer.ComputeTetrahedralization( input );

            // Assert
            Assert.That( tets.Count, Is.GreaterThan( 0 ), "Should produce tetrahedra, even with collinear points, due to noise." );
            double totalVolume = tets.Sum( t => t.GetVolume() );
            Assert.That( totalVolume, Is.LessThan( 0.1 ), "Total volume should be near zero for a degenerate shape." );
        }

        [Test]
        public void DuplicatePoints___HandledGracefully()
        {
            // Arrange
            var input = new List<Vector3>
            {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 0, 1),
                new Vector3(0, 0, 0) // Duplicate
            };

            // Act
            var (nodes, edges, tets) = DelaunayTetrahedralizer.ComputeTetrahedralization( input );

            // Assert
            // The algorithm should effectively use the unique set of points.
            Assert.That( nodes.Count, Is.EqualTo( 4 ), "Node count should reflect unique input points." );
            Assert.That( tets.Count, Is.EqualTo( 1 ) );
            Assert.That( edges.Count, Is.EqualTo( 6 ) );
        }

        [Test]
        public void UnitCube___ProducesConvexHull()
        {
            // Arrange: A unit cube with an extra point outside one face, creating a larger convex hull.
            var input = new List<Vector3>
            {
                new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0),
                new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 1)
            };

            // Act
            var (nodes, edges, tets) = DelaunayTetrahedralizer.ComputeTetrahedralization( input );

            // Assert
            // The total volume should be that of the convex hull. This is the volume of the original
            // unit cube (1.0) plus the volume of a pyramid on one face (base area 1, height 0.5),
            // which is 1/3 * 1 * 0.5 = 1/6.
            double expectedVolume = 1.0;
            double totalVolume = tets.Sum( t => t.GetVolume() );

            Assert.That( totalVolume, Is.EqualTo( expectedVolume ).Within( 0.01 ), "Total volume of tetrahedra should match the volume of the convex hull." );
        }


        [Test]
        public void UnitCube_WithPointInside___ProducesConvexHull()
        {
            // Arrange: A unit cube with an extra point outside one face, creating a larger convex hull.
            var input = new List<Vector3>
            {
                new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0),
                new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 1),
                new Vector3(0.5f, 0.5f, 0.5f)
            };

            // Act
            var (nodes, edges, tets) = DelaunayTetrahedralizer.ComputeTetrahedralization( input );

            // Assert
            // The total volume should be that of the convex hull. This is the volume of the original
            // unit cube (1.0) plus the volume of a pyramid on one face (base area 1, height 0.5),
            // which is 1/3 * 1 * 0.5 = 1/6.
            double expectedVolume = 1.0;
            double totalVolume = tets.Sum( t => t.GetVolume() );

            Assert.That( totalVolume, Is.EqualTo( expectedVolume ).Within( 0.01 ), "Total volume of tetrahedra should match the volume of the convex hull." );
        }

        [Test]
        public void UnitCube_WithPointOutside___ProducesConvexHull()
        {
            // Arrange: A unit cube with an extra point outside one face, creating a larger convex hull.
            var input = new List<Vector3>
            {
                new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0),
                new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 1),
                // Add an external point to extend the convex hull.
                new Vector3(0.5f, 0.5f, -0.5f)
            };

            // Act
            var (nodes, edges, tets) = DelaunayTetrahedralizer.ComputeTetrahedralization( input );

            // Assert
            // The total volume should be that of the convex hull. This is the volume of the original
            // unit cube (1.0) plus the volume of a pyramid on one face (base area 1, height 0.5),
            // which is 1/3 * 1 * 0.5 = 1/6.
            double expectedVolume = 1.0 + (1.0 / 6.0);
            double totalVolume = tets.Sum( t => t.GetVolume() );

            Assert.That( totalVolume, Is.EqualTo( expectedVolume ).Within( 0.01 ), "Total volume of tetrahedra should match the volume of the convex hull." );
        }
    }
}