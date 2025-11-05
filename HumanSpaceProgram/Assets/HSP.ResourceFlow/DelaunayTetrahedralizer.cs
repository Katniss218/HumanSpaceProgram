using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.ResourceFlow
{
    public static class DelaunayTetrahedralizer
    {
        /// <summary>
        /// A triangular face used for boundary extraction. Order-independent (deduplication) via sorted indices.
        /// </summary>
        private readonly struct Face : IEquatable<Face>
        {
            public readonly int a, b, c;

            public Face( int a, int b, int c )
            {
                // Sort 3 values without allocations (by swapping).
                if( b < a ) (b, a) = (a, b);
                if( c < a ) (c, a) = (a, c);
                if( c < b ) (c, b) = (b, c);

                this.a = a;
                this.b = b;
                this.c = c;
            }

            public bool Equals( Face other ) => a == other.a && b == other.b && c == other.c;

            public override bool Equals( object obj ) => obj is Face f && Equals( f );

            public override int GetHashCode() => HashCode.Combine( a, b, c );
        }

        /// <summary>
        /// Internal tetra struct for math and caching circumsphere.
        /// Tracks degeneracy and supports circumsphere queries.
        /// </summary>
        private sealed class Tetra
        {
            public readonly int i0, i1, i2, i3;

            private Vector3 _sphereCenterCache;
            private float _radiusSqCache;
            private bool _circumsphereComputed;
            private readonly List<FlowNode> _nodesRef;

            public bool IsDegenerate { get; private set; }

            public Tetra( int i0, int i1, int i2, int i3, List<FlowNode> nodesRef )
            {
                this.i0 = i0;
                this.i1 = i1;
                this.i2 = i2;
                this.i3 = i3;
                this._nodesRef = nodesRef;
                this._circumsphereComputed = false;
                this.IsDegenerate = false;

                Vector3 p0 = nodesRef[i0].pos;
                Vector3 p1 = nodesRef[i1].pos;
                Vector3 p2 = nodesRef[i2].pos;
                Vector3 p3 = nodesRef[i3].pos;

                float absVolume = FlowTetrahedron.GetVolume( p0, p1, p2, p3 );

                // Choose characteristic scale: max edge length among the tetra edges.
                float maxEdgeLengthSq = 0f;

                void CheckEdge( Vector3 a, Vector3 b )
                {
                    float lengthSq = (a - b).sqrMagnitude;
                    if( lengthSq > maxEdgeLengthSq )
                        maxEdgeLengthSq = lengthSq;
                }

                CheckEdge( p0, p1 );
                CheckEdge( p0, p2 );
                CheckEdge( p0, p3 );
                CheckEdge( p1, p2 );
                CheckEdge( p1, p3 );
                CheckEdge( p2, p3 );

                float maxEdgeLength = (maxEdgeLengthSq > 0f) ? Mathf.Sqrt( maxEdgeLengthSq ) : 0f;

                // threshold (scale-aware): scalarTriple scales as edge^3.
                const float EPSILON = 1e-7f;
                float volumeEps = EPSILON * Math.Max( 1.0f, maxEdgeLength * maxEdgeLength * maxEdgeLength );

                if( absVolume < volumeEps )
                {
                    IsDegenerate = true;
                }
            }

            public bool HasVertexIndex( int idx ) => i0 == idx || i1 == idx || i2 == idx || i3 == idx;

            public void ComputeCircumsphere()
            {
                if( IsDegenerate )
                {
                    _circumsphereComputed = true;
                    return;
                }

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
                catch( InvalidOperationException )
                {
                    // Singular matrix -> degenerate tetra (coplanar / nearly-coplanar)
                    IsDegenerate = true;
                    return;
                }

                // Build b = [|x1|^2 - |x0|^2, |x2|^2 - |x0|^2, |x3|^2 - |x0|^2]
                float norm0 = p0.sqrMagnitude;
                float norm1 = p1.sqrMagnitude;
                float norm2 = p2.sqrMagnitude;
                float norm3 = p3.sqrMagnitude;
                Vector3 b = new Vector3( (norm1 - norm0), (norm2 - norm0), (norm3 - norm0) );

                Vector3 center = invA * b;

                _radiusSqCache = (center - p0).sqrMagnitude;
                // Sanity check.
                if( float.IsNaN( _radiusSqCache ) || float.IsInfinity( _radiusSqCache ) )
                {
                    IsDegenerate = true;
                    return;
                }
                _sphereCenterCache = center;
                _circumsphereComputed = true;
            }

            public bool PointInsideCircumsphere( Vector3 p )
            {
                if( !_circumsphereComputed )
                    ComputeCircumsphere();

                if( IsDegenerate )
                    return false; // degenerate case treat as not containing

                float distanceSq = (_sphereCenterCache - p).sqrMagnitude;

                const float REL_EPS = 1e-3f;
                float tolerance = REL_EPS * Math.Max( 1f, _radiusSqCache );
                return distanceSq <= _radiusSqCache + tolerance;
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

            tetraList.RemoveAll( t => t.HasVertexIndex( super0 ) || t.HasVertexIndex( super1 ) || t.HasVertexIndex( super2 ) || t.HasVertexIndex( super3 ) );
            tetraList.RemoveAll( t => t.IsDegenerate );

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
            float minX = nodes[0].pos.x;
            float minY = nodes[0].pos.y;
            float minZ = nodes[0].pos.z;
            float maxX = minX;
            float maxY = minY;
            float maxZ = minZ;
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

            // 1 - Find all tetrahedra whose circumsphere contains the point ('bad' tetra) - need to be removed/split.
            HashSet<Tetra> badTetra = new();
            foreach( var tetra in tetraList )
            {
                if( tetra.PointInsideCircumsphere( pointPos ) )
                {
                    badTetra.Add( tetra );
                }
            }

            if( badTetra.Count == 0 )
                return;

            // 2 - Find boundary faces (faces that are shared by exactly one bad tetra)
            Dictionary<Face, (int count, (int a, int b, int c) origin)> faceInfo = new();

            foreach( var bad in badTetra )
            {
                var faces = new[]
                {
                    (face: new Face(bad.i0, bad.i1, bad.i2), orig: (bad.i0, bad.i1, bad.i2)),
                    (face: new Face(bad.i0, bad.i1, bad.i3), orig: (bad.i0, bad.i1, bad.i3)),
                    (face: new Face(bad.i0, bad.i2, bad.i3), orig: (bad.i0, bad.i2, bad.i3)),
                    (face: new Face(bad.i1, bad.i2, bad.i3), orig: (bad.i1, bad.i2, bad.i3))
                };

                foreach( var (face, orig) in faces )
                {
                    if( faceInfo.TryGetValue( face, out var val ) )
                        faceInfo[face] = (val.count + 1, val.origin);
                    else
                        faceInfo[face] = (1, orig);
                }
            }

            List<(int a, int b, int c)> boundaryFaces = new();
            foreach( var kv in faceInfo )
            {
                if( kv.Value.count == 1 )
                    boundaryFaces.Add( kv.Value.origin );
            }

            // 3 - Remove bad tetrahedra from tetraList efficiently.
            tetraList.RemoveAll( t => badTetra.Contains( t ) );

            // 4 - Create new tetrahedra by connecting point to each boundary face.
            foreach( var (a, b, c) in boundaryFaces )
            {
                int d = pointIndex;

                Tetra newTetra = new Tetra( a, b, c, d, nodes );

                // If newTetra is degenerate or has negative orientation, attempt to fix by swapping vertices.
                if( newTetra.IsDegenerate )
                {
                    newTetra = new Tetra( b, a, c, d, nodes );
                }

                // Still degenerate.
                if( newTetra.IsDegenerate )
                {
                    continue;
                }

                float signedVol = FlowTetrahedron.GetSignedVolume( nodes[newTetra.i0].pos, nodes[newTetra.i1].pos, nodes[newTetra.i2].pos, nodes[newTetra.i3].pos );
                if( signedVol < 0f )
                {
                    // Flip winding: swap i1 and i2 (create a new tetra with flipped vertices).
                    newTetra = new Tetra( newTetra.i0, newTetra.i2, newTetra.i1, newTetra.i3, nodes );
                    if( newTetra.IsDegenerate )
                        continue; // Flipping made it degenerate.
                }

                tetraList.Add( newTetra );
            }
        }
    }
}