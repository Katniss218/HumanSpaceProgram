using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.ResourceFlow
{
    public static class DelaunayTetrahedralizer
    {
        // A triangular face used for boundary extraction. order-independent dedup using sorted indices.
        private readonly struct Face : IEquatable<Face>
        {
            public readonly int i0, i1, i2;

            public Face( int a, int b, int c )
            {
                // Sort 3 values without allocations (by swapping).
                if( b < a ) (b, a) = (a, b);
                if( c < a ) (c, a) = (a, c);
                if( c < b ) (c, b) = (b, c);

                this.i0 = a;
                this.i1 = b;
                this.i2 = c;
            }

            public bool Equals( Face other ) => i0 == other.i0 && i1 == other.i1 && i2 == other.i2;

            public override int GetHashCode() => HashCode.Combine( i0, i1, i2 );
        }

        /// <summary>
        /// Internal tetra struct for math and caching circumsphere.
        /// </summary>
        private sealed class Tetra
        {
            public readonly int i0, i1, i2, i3;

            private Vector3 _sphereCenterCache;
            private float _radiusSqCache;
            private bool _circumsphereComputed;
            private readonly List<FlowNode> _nodesRef;

            public Tetra( int i0, int i1, int i2, int i3, List<FlowNode> nodesRef )
            {
                Vector3 p0 = nodesRef[i0].pos;
                Vector3 p1 = nodesRef[i1].pos;
                Vector3 p2 = nodesRef[i2].pos;
                Vector3 p3 = nodesRef[i3].pos;

                if( FlowTetrahedron.GetVolume( p0, p3, p2, p1 ) <= 1e-6f )
                {
                    throw new InvalidOperationException( "Degenerate tetrahedron with zero or negative volume encountered when computing circumsphere." );
                }

                this.i0 = i0;
                this.i1 = i1;
                this.i2 = i2;
                this.i3 = i3;
                this._nodesRef = nodesRef;
                _circumsphereComputed = false;
            }

            public bool HasVertexIndex( int idx ) => i0 == idx || i1 == idx || i2 == idx || i3 == idx;

            private void SetAsDegenerate()
            {
                _sphereCenterCache = new Vector3( float.MaxValue, float.MaxValue, float.MaxValue );
                _radiusSqCache = float.PositiveInfinity;
                _circumsphereComputed = true;
            }

            public void ComputeCircumsphere()
            {
                Vector3 p0 = _nodesRef[i0].pos;
                Vector3 p1 = _nodesRef[i1].pos;
                Vector3 p2 = _nodesRef[i2].pos;
                Vector3 p3 = _nodesRef[i3].pos;

                // Build Matrix A = [ 2*(x1-x0) ; 2*(x2-x0) ; 2*(x3-x0) ]
                Matrix3x3 A = new Matrix3x3(
                    2f * (p1.x - p0.x), 2f * (p1.y - p0.y), 2f * (p1.z - p0.z),
                    2f * (p2.x - p0.x), 2f * (p2.y - p0.y), 2f * (p2.z - p0.z),
                    2f * (p3.x - p0.x), 2f * (p3.y - p0.y), 2f * (p3.z - p0.z)
                );

                Matrix3x3 invA;
                try
                {
                    invA = A.inverse;
                }
                catch( InvalidOperationException ex ) // Singular matrix.
                {
                    SetAsDegenerate();
                    return;
                }

                if( float.IsNaN( invA.m00 ) || float.IsNaN( invA.m01 ) || float.IsNaN( invA.m02 ) ||
                    float.IsNaN( invA.m10 ) || float.IsNaN( invA.m11 ) || float.IsNaN( invA.m12 ) ||
                    float.IsNaN( invA.m20 ) || float.IsNaN( invA.m21 ) || float.IsNaN( invA.m22 ) ||
                    float.IsInfinity( invA.m00 ) || float.IsInfinity( invA.m01 ) || float.IsInfinity( invA.m02 ) ||
                    float.IsInfinity( invA.m10 ) || float.IsInfinity( invA.m11 ) || float.IsInfinity( invA.m12 ) ||
                    float.IsInfinity( invA.m20 ) || float.IsInfinity( invA.m21 ) || float.IsInfinity( invA.m22 ) )
                {
                    SetAsDegenerate();
                    return;
                }

                // Build b = [|x1|^2 - |x0|^2, |x2|^2 - |x0|^2, |x3|^2 - |x0|^2]
                float norm0 = p0.x * p0.x + p0.y * p0.y + p0.z * p0.z;
                float norm1 = p1.x * p1.x + p1.y * p1.y + p1.z * p1.z;
                float norm2 = p2.x * p2.x + p2.y * p2.y + p2.z * p2.z;
                float norm3 = p3.x * p3.x + p3.y * p3.y + p3.z * p3.z;
                Vector3 b = new Vector3( (norm1 - norm0), (norm2 - norm0), (norm3 - norm0) );

                Vector3 center = invA * b;

                _sphereCenterCache = center;

                float dx = center.x - p0.x;
                float dy = center.y - p0.y;
                float dz = center.z - p0.z;
                _radiusSqCache = (dx * dx) + (dy * dy) + (dz * dz);
                _circumsphereComputed = true;
            }

            public bool PointInsideCircumsphere( Vector3 p )
            {
                if( !_circumsphereComputed )
                    ComputeCircumsphere();

                if( float.IsInfinity( _radiusSqCache ) )
                    return false; // degenerate case treat as not containing

                float dx = _sphereCenterCache.x - p.x;
                float dy = _sphereCenterCache.y - p.y;
                float dz = _sphereCenterCache.z - p.z;
                float distSq = dx * dx + dy * dy + dz * dz;

                // Use a tiny epsilon to avoid floating point issues: consider point inside if distSq <= radiusSq*(1 + eps)
                const float eps = 1e-6f;
                return distSq <= _radiusSqCache * (1.0 + eps);
            }
        }

        /// <summary>
        /// Compute the Delaunay tetrahedralization of the given set of 3D points.
        /// </summary>
        public static (List<FlowNode> nodes, List<FlowEdge> edges, List<FlowTetrahedron> tets) ComputeDelaunayTetrahedralization( IList<Vector3> inputPoints )
        {
            // Bowyer-Watson incremental tetrahedralization.

            if( inputPoints == null )
                throw new ArgumentNullException( nameof( inputPoints ) );

            List<FlowNode> nodes = new( inputPoints.Count + 4 );

            foreach( var p in inputPoints )
            {
                nodes.Add( new FlowNode( p ) );
            }

            if( inputPoints.Count < 4 )
            {
                return (nodes, new List<FlowEdge>(), new List<FlowTetrahedron>());
            }

            BuildSuperTetrahedron( nodes, out int super0, out int super1, out int super2, out int super3 );

            // Start with single large super-tetrahedron containing the entire set.
            List<Tetra> tetraList = new()
            {
                new Tetra( super0, super1, super2, super3, nodes )
            };

            // Insert each real point incrementally, dividing the containing tetra.
            for( int i = 0; i < inputPoints.Count; ++i )
            {
                InsertPoint( i, nodes, tetraList );
            }

            // Remove any tetrahedron that references a super vertex. Also removes degenerate tetras.
            tetraList.RemoveAll( t => t.HasVertexIndex( super0 ) || t.HasVertexIndex( super1 ) || t.HasVertexIndex( super2 ) || t.HasVertexIndex( super3 ) );
            if( tetraList.Count == 0 )
            {
                return (new List<FlowNode>(), new List<FlowEdge>(), new List<FlowTetrahedron>());
            }
            nodes.RemoveRange( nodes.Count - 4, 4 );

            // Build FlowTetrahedron list and edges.
            List<FlowTetrahedron> flowTets = new( tetraList.Count );
            HashSet<(int, int)> edgeSet = new();

            foreach( var t in tetraList )
            {
                FlowNode a = nodes[t.i0];
                FlowNode b = nodes[t.i1];
                FlowNode c = nodes[t.i2];
                FlowNode d = nodes[t.i3];

                flowTets.Add( new FlowTetrahedron( a, b, c, d ) );

                AddEdgeToSet( edgeSet, t.i0, t.i1 );
                AddEdgeToSet( edgeSet, t.i0, t.i2 );
                AddEdgeToSet( edgeSet, t.i0, t.i3 );
                AddEdgeToSet( edgeSet, t.i1, t.i2 );
                AddEdgeToSet( edgeSet, t.i1, t.i3 );
                AddEdgeToSet( edgeSet, t.i2, t.i3 );
            }

            List<FlowEdge> edges = new( edgeSet.Count );
            foreach( (int u, int v) in edgeSet )
            {
                edges.Add( new FlowEdge( nodes[u], nodes[v], -1 ) );
            }

            return (nodes, edges, flowTets);
        }

        private static void AddEdgeToSet( HashSet<(int, int)> set, int vert1, int vert2 )
        {
            if( vert1 == vert2 )
                return;

            if( vert1 < vert2 )
                set.Add( (vert1, vert2) );
            else
                set.Add( (vert2, vert1) );
        }

        /// <summary>
        /// Build a super tetra that encloses all existing nodes (appends 4 nodes and returns their indices).
        /// </summary>
        private static void BuildSuperTetrahedron( List<FlowNode> nodes, out int idx0, out int idx1, out int idx2, out int idx3 )
        {
            // Find the max/min bounds of existing points.
            float minX = nodes[0].pos.x, minY = nodes[0].pos.y, minZ = nodes[0].pos.z;
            float maxX = minX, maxY = minY, maxZ = minZ;
            foreach( var node in nodes )
            {
                Vector3 pos = node.pos;

                if( pos.x < minX )
                    minX = pos.x;
                if( pos.y < minY )
                    minY = pos.y;
                if( pos.z < minZ )
                    minZ = pos.z;

                if( pos.x > maxX )
                    maxX = pos.x;
                if( pos.y > maxY )
                    maxY = pos.y;
                if( pos.z > maxZ )
                    maxZ = pos.z;
            }

            Vector3 center = new Vector3( (minX + maxX) * 0.5f, (minY + maxY) * 0.5f, (minZ + maxZ) * 0.5f );
            float maxSpan = Math.Max( maxX - minX, Math.Max( maxY - minY, maxZ - minZ ) );
            if( maxSpan <= 0 )
            {
                maxSpan = 1.0f;
            }

            float scale = maxSpan * 10.0f;

            // Create 4 extra points forming a non-degenerate starting tetra enclosing all points.
            Vector3 s0 = center + new Vector3( 0, 0, 3f * scale );
            Vector3 s1 = center + new Vector3( 2f * scale, 0, -scale );
            Vector3 s2 = center + new Vector3( -scale, 2f * scale, -scale );
            Vector3 s3 = center + new Vector3( -scale, -scale, -scale );

            idx0 = nodes.Count;
            nodes.Add( new FlowNode( s0 ) );
            idx1 = nodes.Count;
            nodes.Add( new FlowNode( s1 ) );
            idx2 = nodes.Count;
            nodes.Add( new FlowNode( s2 ) );
            idx3 = nodes.Count;
            nodes.Add( new FlowNode( s3 ) );
        }

        private static void InsertPoint( int pointIndex, List<FlowNode> nodes, List<Tetra> tetraList )
        {
            Vector3 pointPos = nodes[pointIndex].pos;

            // 1) Find all tetrahedra whose circumsphere contains the point ('bad' tetra) - need to be removed/split.
            List<Tetra> tetrahedraWithPointInCircumsphere = new();
            foreach( var tetra in tetraList )
            {
                if( tetra.PointInsideCircumsphere( pointPos ) )
                {
                    tetrahedraWithPointInCircumsphere.Add( tetra );
                }
            }

            // This will never be true because the super tetra is made big enough (unless the points are literally near the float limit).
            if( tetrahedraWithPointInCircumsphere.Count == 0 )
                return;

            // 2) Find boundary faces (faces that are shared by exactly one bad tetra)
            Dictionary<Face, int> faceCounts = new();
            Dictionary<Face, (int, int, int)> faceToOrigin = new(); // store original indices (not sorted) for deterministic new tetra creation if needed
            foreach( var badTetra in tetrahedraWithPointInCircumsphere )
            {
                Face[] faces = new[]
                {
                    new Face( badTetra.i0, badTetra.i1, badTetra.i2 ),
                    new Face( badTetra.i0, badTetra.i1, badTetra.i3 ),
                    new Face( badTetra.i0, badTetra.i2, badTetra.i3 ),
                    new Face( badTetra.i1, badTetra.i2, badTetra.i3 )
                };

                (int, int, int)[] originals = new[]
                {
                    (badTetra.i0, badTetra.i1, badTetra.i2),
                    (badTetra.i0, badTetra.i1, badTetra.i3),
                    (badTetra.i0, badTetra.i2, badTetra.i3),
                    (badTetra.i1, badTetra.i2, badTetra.i3)
                };

                for( int i = 0; i < faces.Length; ++i )
                {
                    Face face = faces[i];
                    faceCounts.TryGetValue( face, out int faceCount );
                    faceCounts[face] = faceCount + 1;

                    if( !faceToOrigin.ContainsKey( face ) )
                    {
                        faceToOrigin[face] = originals[i];
                    }
                }
            }

            List<(int, int, int)> boundaryFaces = new();
            foreach( (Face face, int count) in faceCounts )
            {
                if( count == 1 )
                {
                    (int, int, int) orig = faceToOrigin[face];
                    boundaryFaces.Add( orig );
                }
            }

            // 3) Remove bad tetrahedra from tetraList
            foreach( var badTetra in tetrahedraWithPointInCircumsphere )
            {
                tetraList.Remove( badTetra );
            }

            // 4) Create new tetrahedra by connecting point to each boundary face
            foreach( var boundaryFace in boundaryFaces )
            {
                Tetra newTetra = new Tetra( boundaryFace.Item1, boundaryFace.Item2, boundaryFace.Item3, pointIndex, nodes );
                tetraList.Add( newTetra );
            }
        }
    }
}