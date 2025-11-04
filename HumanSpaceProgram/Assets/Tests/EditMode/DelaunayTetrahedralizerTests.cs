using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HSP.ResourceFlow.Tests
{
    [TestFixture]
    public class DelaunayTetrahedralizerTests
    {
        const float EPS = 1e-5f;

        static bool VecAlmostEqual( Vector3 a, Vector3 b, float eps = EPS )
        {
            return Mathf.Abs( a.x - b.x ) <= eps &&
                   Mathf.Abs( a.y - b.y ) <= eps &&
                   Mathf.Abs( a.z - b.z ) <= eps;
        }

        [Test]
        public void SimpleNonCoplanar_ProducesSingleTetra_AndSixEdges()
        {
            // Arrange: four points that form a non-degenerate tetra (origin + 3 basis points)
            var input = new List<Vector3>
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(1f, 0f, 0f),
                new Vector3(0f, 1f, 0f),
                new Vector3(0f, 0f, 1f)
            };

            // Act
            var (nodes, edges, tets) = DelaunayTetrahedralizer.ComputeDelaunayTetrahedralization( input );

            // Assert basic counts
            // Note: current implementation appends 4 super nodes to the nodes list.
            Assert.That( nodes, Is.Not.Null );
            Assert.That( edges, Is.Not.Null );
            Assert.That( tets, Is.Not.Null );

            Assert.That( tets.Count, Is.EqualTo( 1 ), "Expected exactly 1 tetra for 4 non-coplanar input points." );
            Assert.That( edges.Count, Is.EqualTo( 6 ), "A tetra with 4 vertices should produce 6 unique edges." );
            Assert.That( nodes.Count, Is.EqualTo( input.Count ), "Expected 4 output points." );

            // Verify that tetra's node positions correspond to some permutation of the input points
            var tetra = tets[0];
            var tetraVertices = new[] { tetra.v0.pos, tetra.v1.pos, tetra.v2.pos, tetra.v3.pos };

            int matched = 0;
            foreach( var p in input )
            {
                if( tetraVertices.Any( tp => VecAlmostEqual( tp, p ) ) )
                    matched++;
            }

            Assert.That( matched, Is.EqualTo( 4 ), $"All 4 input points should appear as vertices of the single returned tetra. Matched={matched}/4" );
        }

        [Test]
        public void CoplanarPoints_ProduceNoTetra()
        {
            // Arrange: 4 coplanar points (z = 0)
            var input = new List<Vector3>
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(1f, 0f, 0f),
                new Vector3(0f, 1f, 0f),
                new Vector3(1f, 1f, 0f)
            };

            // Act
            var (nodes, edges, tets) = DelaunayTetrahedralizer.ComputeDelaunayTetrahedralization( input );

            // Assert
            Assert.That( nodes, Is.Not.Null );
            Assert.That( edges, Is.Not.Null );
            Assert.That( tets, Is.Not.Null );

            // The provided implementation is expected to bail on degenerate/coplanar input.
            Assert.That( tets.Count, Is.EqualTo( 0 ), "Coplanar input should not produce valid tetrahedra in this implementation (degenerate)." );
        }

        [Test]
        public void DuplicatePoints_ProduceNoTetra_OrHandleGracefully()
        {
            // Arrange: duplicate points (3 unique + 1 duplicate)
            var input = new List<Vector3>
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(1f, 0f, 0f),
                new Vector3(0f, 1f, 0f),
                new Vector3(0f, 1f, 0f) // duplicate of previous
            };

            // Act
            var (nodes, edges, tets) = DelaunayTetrahedralizer.ComputeDelaunayTetrahedralization( input );

            // Assert: algorithm should treat duplicates as degenerate; expect 0 tets
            Assert.That( nodes, Is.Not.Null );
            Assert.That( edges, Is.Not.Null );
            Assert.That( tets, Is.Not.Null );

            Assert.That( nodes.Count, Is.EqualTo( 0 ), "Duplicate input points are degenerate — implementation is expected to not produce tetrahedra." );
            Assert.That( edges.Count, Is.EqualTo( 0 ), "Duplicate input points are degenerate — implementation is expected to not produce tetrahedra." );
            Assert.That( tets.Count, Is.EqualTo( 0 ), "Duplicate input points are degenerate — implementation is expected to not produce tetrahedra." );
        }

        [Test]
        public void CoplanarPoints_ProduceNoTetra_OrHandleGracefully()
        {
            // Arrange
            var input = new List<Vector3>
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(1f, 0f, 0f),
                new Vector3(0f, 0f, 1f),
                new Vector3(1f, 0f, 1f)
            };

            // Act
            var (nodes, edges, tets) = DelaunayTetrahedralizer.ComputeDelaunayTetrahedralization( input );

            // Assert: algorithm should treat duplicates as degenerate; expect 0 tets
            Assert.That( nodes, Is.Not.Null );
            Assert.That( edges, Is.Not.Null );
            Assert.That( tets, Is.Not.Null );

            Assert.That( nodes.Count, Is.EqualTo( 0 ), "Duplicate input points are degenerate — implementation is expected to not produce tetrahedra." );
            Assert.That( edges.Count, Is.EqualTo( 0 ), "Duplicate input points are degenerate — implementation is expected to not produce tetrahedra." );
            Assert.That( tets.Count, Is.EqualTo( 0 ), "Duplicate input points are degenerate — implementation is expected to not produce tetrahedra." );
        }

        [Test]
        public void CoplanarPoints_InsideABigTetra()
        {
            // Arrange
            var input = new List<Vector3>
            {
                new Vector3(0f, 1f, 0f),
                new Vector3(-0.866025f, -0.5f, 0f),
                new Vector3(0.866025f, -0.5f, 0f),
                new Vector3(0f, 0f, 0f),

                new Vector3(0f, 5f, -5f),
                new Vector3(-5f, -2.5f,-5f),
                new Vector3(5f, -2.5f, -5f),
                new Vector3(0f, 0f, 5f)
            };

            // Act
            var (nodes, edges, tets) = DelaunayTetrahedralizer.ComputeDelaunayTetrahedralization( input );

            // Assert: algorithm should treat duplicates as degenerate; expect 0 tets
            Assert.That( nodes, Is.Not.Null );
            Assert.That( edges, Is.Not.Null );
            Assert.That( tets, Is.Not.Null );

            Assert.That( nodes.Count, Is.EqualTo( 8 ), "Duplicate input points are degenerate — implementation is expected to not produce tetrahedra." );
            Assert.That( edges.Count, Is.EqualTo( 0 ), "Duplicate input points are degenerate — implementation is expected to not produce tetrahedra." );
            Assert.That( tets.Count, Is.EqualTo( 0 ), "Duplicate input points are degenerate — implementation is expected to not produce tetrahedra." );
        }

        [Test]
        public void Edges_Reference_ReturnedNodes()
        {
            // Arrange: standard tetra points
            var input = new List<Vector3>
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(1f, 0f, 0f),
                new Vector3(0f, 1f, 0f),
                new Vector3(0f, 0f, 1f)
            };

            // Act
            var (nodes, edges, tets) = DelaunayTetrahedralizer.ComputeDelaunayTetrahedralization( input );

            // Assert
            Assert.That( edges.Count, Is.GreaterThanOrEqualTo( 0 ) );

            // Each FlowEdge should reference FlowNode instances that exist in the returned nodes list.
            // This verifies we do not create edges referencing non-returned nodes.
            foreach( var e in edges )
            {
                Assert.That( nodes, Does.Contain( e.end1 ), "Edge.a must be a reference present in returned nodes list." );
                Assert.That( nodes, Does.Contain( e.end2 ), "Edge.b must be a reference present in returned nodes list." );
                Assert.That( e.end1, Is.Not.Null );
                Assert.That( e.end2, Is.Not.Null );
            }
        }
    }
}