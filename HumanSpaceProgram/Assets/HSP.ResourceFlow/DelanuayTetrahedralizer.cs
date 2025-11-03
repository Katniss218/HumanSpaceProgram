using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.ResourceFlow
{
    [Obsolete( "untested, needs rigorous testing, WIP" )]
    public static class DelanuayTetrahedralizer
    {
        // A triangular face used for boundary extraction; order-independent dedup using sorted indices.
        private struct Face : IEquatable<Face>
        {
            public int a, b, c;
            public Face( int a, int b, int c )
            {
                // store sorted so equality/dedup is simple
                int[] arr = new[] { a, b, c };
                Array.Sort( arr );
                this.a = arr[0]; this.b = arr[1]; this.c = arr[2];
            }
            public bool Equals( Face other ) => a == other.a && b == other.b && c == other.c;
            public override int GetHashCode() => HashCode.Combine( a, b, c );
        }

        // Internal tetra struct for math and caching circumsphere
        private class Tetra
        {
            public int i0, i1, i2, i3;
            private Vector3 centerCache; // circumsphere center
            private double radiusSqCache;
            private bool circumsphereComputed;
            private readonly List<FlowNode> nodesRef;

            public Tetra( int i0, int i1, int i2, int i3, List<FlowNode> nodesRef )
            {
                this.i0 = i0; this.i1 = i1; this.i2 = i2; this.i3 = i3;
                this.nodesRef = nodesRef;
                circumsphereComputed = false;
            }

            public bool HasVertexIndex( int idx ) => i0 == idx || i1 == idx || i2 == idx || i3 == idx;

            public void ComputeCircumsphere()
            {
                var p0 = nodesRef[i0].pos;
                var p1 = nodesRef[i1].pos;
                var p2 = nodesRef[i2].pos;
                var p3 = nodesRef[i3].pos;

                // convert to double only for norm computation accuracy; A will be float Matrix3x3
                double[] x0 = { p0.x, p0.y, p0.z };
                double[] x1 = { p1.x, p1.y, p1.z };
                double[] x2 = { p2.x, p2.y, p2.z };
                double[] x3 = { p3.x, p3.y, p3.z };

                // Build Matrix A rows = 2*(xi - x0) for i = 1..3, using float Matrix3x3
                // A = [ 2*(x1-x0) ; 2*(x2-x0) ; 2*(x3-x0) ]
                var A = new Matrix3x3(
                    2f * (p1.x - p0.x), 2f * (p1.y - p0.y), 2f * (p1.z - p0.z),
                    2f * (p2.x - p0.x), 2f * (p2.y - p0.y), 2f * (p2.z - p0.z),
                    2f * (p3.x - p0.x), 2f * (p3.y - p0.y), 2f * (p3.z - p0.z)
                );

                // Build b = [|x1|^2 - |x0|^2, |x2|^2 - |x0|^2, |x3|^2 - |x0|^2]
                double norm0 = x0[0] * x0[0] + x0[1] * x0[1] + x0[2] * x0[2];
                double norm1 = x1[0] * x1[0] + x1[1] * x1[1] + x1[2] * x1[2];
                double norm2 = x2[0] * x2[0] + x2[1] * x2[1] + x2[2] * x2[2];
                double norm3 = x3[0] * x3[0] + x3[1] * x3[1] + x3[2] * x3[2];

                var b = new Vector3( (float)(norm1 - norm0), (float)(norm2 - norm0), (float)(norm3 - norm0) );

                // Attempt to invert A using the Matrix3x3.inverse property
                Matrix3x3 inv;
                try
                {
                    inv = A.inverse; // uses your struct's inverse implementation
                }
                catch
                {
                    // If inverse property throws for singular matrix, treat as degenerate
                    centerCache = new Vector3( float.MaxValue, float.MaxValue, float.MaxValue );
                    radiusSqCache = double.PositiveInfinity;
                    circumsphereComputed = true;
                    return;
                }

                // Quick check: if inverse contains NaN or Infinity then treat as degenerate
                if( float.IsNaN( inv.m00 ) || float.IsNaN( inv.m01 ) || float.IsNaN( inv.m02 ) ||
                    float.IsNaN( inv.m10 ) || float.IsNaN( inv.m11 ) || float.IsNaN( inv.m12 ) ||
                    float.IsNaN( inv.m20 ) || float.IsNaN( inv.m21 ) || float.IsNaN( inv.m22 ) ||
                    float.IsInfinity( inv.m00 ) || float.IsInfinity( inv.m01 ) || float.IsInfinity( inv.m02 ) ||
                    float.IsInfinity( inv.m10 ) || float.IsInfinity( inv.m11 ) || float.IsInfinity( inv.m12 ) ||
                    float.IsInfinity( inv.m20 ) || float.IsInfinity( inv.m21 ) || float.IsInfinity( inv.m22 ) )
                {
                    centerCache = new Vector3( float.MaxValue, float.MaxValue, float.MaxValue );
                    radiusSqCache = double.PositiveInfinity;
                    circumsphereComputed = true;
                    return;
                }

                // Multiply inv * b to get center c (float Vector3)
                Vector3 c = inv * b;

                centerCache = c;

                // compute radius squared in double precision for better accuracy:
                double dx = (double)c.x - x0[0];
                double dy = (double)c.y - x0[1];
                double dz = (double)c.z - x0[2];
                radiusSqCache = dx * dx + dy * dy + dz * dz;
                circumsphereComputed = true;
            }

            public bool PointInsideCircumsphere( Vector3 p )
            {
                if( !circumsphereComputed ) ComputeCircumsphere();
                if( double.IsInfinity( radiusSqCache ) ) return false; // degenerate case treat as not containing
                double dx = centerCache.x - p.x;
                double dy = centerCache.y - p.y;
                double dz = centerCache.z - p.z;
                double distSq = dx * dx + dy * dy + dz * dz;
                // Use a tiny epsilon to avoid floating point issues: consider point inside if distSq <= radiusSq*(1 + eps)
                const double eps = 1e-12;
                return distSq <= radiusSqCache * (1.0 + eps);
            }
        }

        public static (List<FlowNode> nodes, List<FlowEdge> edges, List<FlowTetrahedron> tets) ComputeDelaunayTetrahedralization( IList<Vector3> inputPoints )
        {
            if( inputPoints == null )
                throw new ArgumentNullException( nameof( inputPoints ) );

            // Convert to nodes
            var nodes = new List<FlowNode>( inputPoints.Count + 4 );
            foreach( var p in inputPoints )
                nodes.Add( new FlowNode( p ) );

            if( inputPoints.Count < 4 )
            {
                // Not enough points to form tetrahedra
                return (nodes, new List<FlowEdge>(), new List<FlowTetrahedron>());
            }

            BuildSuperTetrahedron( nodes, out int super0, out int super1, out int super2, out int super3 );

            // Start with single super-tetrahedron
            List<Tetra> tetraList = new()
            {
                new Tetra( super0, super1, super2, super3, nodes )
            };

            // Insert each real point incrementally
            for( int i = 0; i < inputPoints.Count; ++i )
            {
                InsertPoint( i, nodes, tetraList );
            }

            // Remove any tetrahedron that references a super vertex
            tetraList.RemoveAll( t => t.HasVertexIndex( super0 ) || t.HasVertexIndex( super1 ) || t.HasVertexIndex( super2 ) || t.HasVertexIndex( super3 ) );

            // Build FlowTetrahedron list and edges
            var flowTets = new List<FlowTetrahedron>( tetraList.Count );
            var edgeSet = new HashSet<(int, int)>();

            foreach( var t in tetraList )
            {
                var a = nodes[t.i0];
                var b = nodes[t.i1];
                var c = nodes[t.i2];
                var d = nodes[t.i3];

                flowTets.Add( new FlowTetrahedron( a, b, c, d ) );

                AddEdgeToSet( edgeSet, t.i0, t.i1 );
                AddEdgeToSet( edgeSet, t.i0, t.i2 );
                AddEdgeToSet( edgeSet, t.i0, t.i3 );
                AddEdgeToSet( edgeSet, t.i1, t.i2 );
                AddEdgeToSet( edgeSet, t.i1, t.i3 );
                AddEdgeToSet( edgeSet, t.i2, t.i3 );
            }

            var edges = new List<FlowEdge>( edgeSet.Count );
            foreach( var (u, v) in edgeSet )
            {
                edges.Add( new FlowEdge( nodes[u], nodes[v] ) );
            }

            return (nodes, edges, flowTets);
        }

        // --- Helper methods and internal types below ---

        private static void AddEdgeToSet( HashSet<(int, int)> set, int a, int b )
        {
            if( a == b ) return;
            if( a < b ) set.Add( (a, b) );
            else set.Add( (b, a) );
        }


        // Build a super tetra that encloses all existing nodes (appends 4 nodes and returns their indices)
        private static void BuildSuperTetrahedron( List<FlowNode> nodes, out int idx0, out int idx1, out int idx2, out int idx3 )
        {
            // compute bounding box of current nodes
            if( nodes.Count == 0 )
            {
                // Degenerate: create a default big tetra
                nodes.Add( new FlowNode( new Vector3( 0, 0, 1000 ) ) );
                nodes.Add( new FlowNode( new Vector3( 1000, 0, -1000 ) ) );
                nodes.Add( new FlowNode( new Vector3( -1000, 1000, -1000 ) ) );
                nodes.Add( new FlowNode( new Vector3( -1000, -1000, -1000 ) ) );
                idx0 = 0; idx1 = 1; idx2 = 2; idx3 = 3;
                return;
            }

            float minX = nodes[0].pos.x, minY = nodes[0].pos.y, minZ = nodes[0].pos.z;
            float maxX = minX, maxY = minY, maxZ = minZ;
            foreach( var n in nodes )
            {
                var p = n.pos;
                if( p.x < minX ) minX = p.x; if( p.y < minY ) minY = p.y; if( p.z < minZ ) minZ = p.z;
                if( p.x > maxX ) maxX = p.x; if( p.y > maxY ) maxY = p.y; if( p.z > maxZ ) maxZ = p.z;
            }

            var center = new Vector3( (minX + maxX) * 0.5f, (minY + maxY) * 0.5f, (minZ + maxZ) * 0.5f );
            float maxSpan = Math.Max( maxX - minX, Math.Max( maxY - minY, maxZ - minZ ) );
            if( maxSpan <= 0 ) maxSpan = 1.0f;
            float scale = maxSpan * 10.0f; // make super tetra big enough

            // create 4 points forming a non-degenerate tetra around center
            var s0 = center + new Vector3( 0, 0, 3f * scale );
            var s1 = center + new Vector3( 2f * scale, 0, -scale );
            var s2 = center + new Vector3( -scale, 2f * scale, -scale );
            var s3 = center + new Vector3( -scale, -scale, -scale );

            idx0 = nodes.Count;
            nodes.Add( new FlowNode( s0 ) );
            idx1 = nodes.Count;
            nodes.Add( new FlowNode( s1 ) );
            idx2 = nodes.Count;
            nodes.Add( new FlowNode( s2 ) );
            idx3 = nodes.Count;
            nodes.Add( new FlowNode( s3 ) );
        }

        // Insert a single point (index `pointIndex`) into the tetrahedralization
        private static void InsertPoint( int pointIndex, List<FlowNode> nodes, List<Tetra> tetraList )
        {
            var p = nodes[pointIndex].pos;

            // 1) Find all tetrahedra whose circumsphere contains p (bad tetrahedra)
            var bad = new List<Tetra>();
            foreach( var t in tetraList )
            {
                if( t.PointInsideCircumsphere( p ) ) bad.Add( t );
            }

            if( bad.Count == 0 )
            {
                // No containing tetra -> point lies outside current triangulation (should not happen if super tetra encloses)
                // We'll skip insertion, but more robust code would handle this.
                return;
            }

            // 2) Find boundary faces (faces that are shared by exactly one bad tetra)
            var faceCounts = new Dictionary<Face, int>();
            var faceToOrigin = new Dictionary<Face, (int, int, int)>(); // store original indices (not sorted) for deterministic new tetra creation if needed

            foreach( var t in bad )
            {
                var faces = new[]
                {
                    new Face(t.i0, t.i1, t.i2),
                    new Face(t.i0, t.i1, t.i3),
                    new Face(t.i0, t.i2, t.i3),
                    new Face(t.i1, t.i2, t.i3)
                };
                var originals = new[]
                {
                    (t.i0, t.i1, t.i2),
                    (t.i0, t.i1, t.i3),
                    (t.i0, t.i2, t.i3),
                    (t.i1, t.i2, t.i3)
                };

                for( int k = 0; k < faces.Length; ++k )
                {
                    var f = faces[k];
                    faceCounts.TryGetValue( f, out int cnt );
                    faceCounts[f] = cnt + 1;
                    if( !faceToOrigin.ContainsKey( f ) ) faceToOrigin[f] = originals[k];
                }
            }

            var boundaryFaces = new List<(int, int, int)>();
            foreach( var kv in faceCounts )
            {
                if( kv.Value == 1 )
                {
                    // unique face -> boundary
                    var orig = faceToOrigin[kv.Key];
                    boundaryFaces.Add( orig );
                }
            }

            // 3) Remove bad tetrahedra from tetraList
            foreach( var t in bad )
            {
                tetraList.Remove( t );
            }

            // 4) Create new tetrahedra by connecting point to each boundary face
            foreach( var bf in boundaryFaces )
            {
                var newT = new Tetra( bf.Item1, bf.Item2, bf.Item3, pointIndex, nodes );
                tetraList.Add( newT );
            }
        }
    }
}
