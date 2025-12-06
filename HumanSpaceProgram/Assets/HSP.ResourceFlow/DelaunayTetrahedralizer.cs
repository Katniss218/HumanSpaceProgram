using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// A static utility class that implements the Bowyer-Watson algorithm for generating a 3D Delaunay tetrahedralization from a set of input points. <br/>
    /// This is a crucial part of the resource flow system, as it allows the arbitrary geometry of a tank part to be voxelized into a set of tetrahedra, <br/>
    /// whose edges can then be used to accurately model fluid distribution under acceleration. <br/>
    /// </summary>
    /// <remarks>
    /// To improve robustness and prevent failures with highly structured or near-coplanar input points (common in game assets), a small amount of <br/>
    /// deterministic noise is added to the vertex positions before tetrahedralization. This noise is internal to the algorithm and does not affect <br/>
    /// the final node positions, ensuring that the resulting tetrahedral mesh is geometrically valid and well-conditioned for the flow simulation.
    /// </remarks>
    public static class DelaunayTetrahedralizer
    {
        private const float EPSILON = 1e-5f;
        private const float NOISE_SCALE = 0.05f;

        public class TetrahedronVertex
        {
            public Vector3 Position;
            public int OriginalIndex;
            public FlowNode NodeReference;

            public TetrahedronVertex( Vector3 pos, int index, FlowNode node )
            {
                Position = pos;
                OriginalIndex = index;
                NodeReference = node;
            }
        }

        public class Tetrahedron
        {
            public TetrahedronVertex A, B, C, D;
            public Vector3 Circumcenter;
            public float CircumradiusSqr;

            public bool IsValid { get; private set; }
            public bool IsBad;

            public Tetrahedron( TetrahedronVertex a, TetrahedronVertex b, TetrahedronVertex c, TetrahedronVertex d )
            {
                A = a;
                B = b;
                C = c;
                D = d;

                float volume = GetSignedVolume( A.Position, B.Position, C.Position, D.Position );

                if( Mathf.Abs( volume ) < EPSILON )
                {
                    SetInvalid();
                }
                else
                {
                    IsValid = true;
                    CalculateCircumsphere();
                }
            }

            private void CalculateCircumsphere()
            {
                Vector3 ba = B.Position - A.Position;
                Vector3 ca = C.Position - A.Position;
                Vector3 da = D.Position - A.Position;

                float lenBa = ba.sqrMagnitude;
                float lenCa = ca.sqrMagnitude;
                float lenDa = da.sqrMagnitude;

                // Denominator from triple product.
                float denominator = 2.0f * Vector3.Dot( ba, Vector3.Cross( ca, da ) );

                // If denominator is effectively zero, points are cospherical or coplanar.
                // In either case, treat as invalid.
                if( Mathf.Abs( denominator ) < EPSILON )
                {
                    SetInvalid();
                }

                Vector3 numerator =
                    Vector3.Cross( ca, da ) * lenBa +
                    Vector3.Cross( da, ba ) * lenCa +
                    Vector3.Cross( ba, ca ) * lenDa;

                Vector3 offset = numerator / denominator;

                Circumcenter = A.Position + offset;
                CircumradiusSqr = offset.sqrMagnitude;

                if( CircumradiusSqr > 10_000f * 10_000f )
                {
                    SetInvalid();
                }
            }

            public void SetInvalid()
            {
                IsValid = false;
                Circumcenter = Vector3.zero;
                CircumradiusSqr = 0;
            }

            public bool IsValidAndContains( Vector3 point )
            {
                // Skip invalid or coplanar.
                if( !IsValid )
                    return false;

                float distSqr = (point - Circumcenter).sqrMagnitude;
                return distSqr <= CircumradiusSqr + EPSILON;
            }

            public static float GetSignedVolume( Vector3 a, Vector3 b, Vector3 c, Vector3 d )
            {
                return Vector3.Dot( b - a, Vector3.Cross( c - a, d - a ) ) / 6.0f;
            }
        }

        private readonly struct Face : IEquatable<Face>
        {
            public readonly int i1, i2, i3;
            public readonly TetrahedronVertex v1, v2, v3;

            public Face( TetrahedronVertex a, TetrahedronVertex b, TetrahedronVertex c )
            {
                if( b.OriginalIndex < a.OriginalIndex ) (a, b) = (b, a);
                if( c.OriginalIndex < a.OriginalIndex ) (c, a) = (a, c);
                if( c.OriginalIndex < b.OriginalIndex ) (c, b) = (b, c);

                v1 = a; v2 = b; v3 = c;
                i1 = a.OriginalIndex; i2 = b.OriginalIndex; i3 = c.OriginalIndex;
            }

            public bool Equals( Face other ) => i1 == other.i1 && i2 == other.i2 && i3 == other.i3;
            public override int GetHashCode() => HashCode.Combine( i1, i2, i3 );
        }

        private static float Hash01( int seed )
        {
            unchecked
            {
                uint x = (uint)seed;
                x ^= x >> 16;
                x *= 0x7feb352d;
                x ^= x >> 15;
                x *= 0x846ca68b;
                x ^= x >> 16;
                // Convert to [0..1]
                return (x / (float)uint.MaxValue);
            }
        }

        private static Vector3 DeterministicNoise3( int seed )
        {
            float nx = Hash01( seed * 73856093 );
            float ny = Hash01( seed * 19349663 );
            float nz = Hash01( seed * 83492791 );

            // Map from [0..1] to [-1..1]
            return new Vector3( nx * 2f - 1f, ny * 2f - 1f, nz * 2f - 1f );
        }

        public static (List<FlowNode> nodes, List<FlowEdge> edges, List<FlowTetrahedron> tets) ComputeTetrahedralization( IList<Vector3> inputPoints )
        {
            // Bowyer-Watson Delaunay Tetrahedralization
            if( inputPoints == null || inputPoints.Count < 4 )
                return (new List<FlowNode>(), new List<FlowEdge>(), new List<FlowTetrahedron>());

            // Note - floating point Bowyer-Watson fails on aligned grids/near-coplanar inputs.

            TetrahedronVertex[] vertices = new TetrahedronVertex[inputPoints.Count];
            List<FlowNode> nodes = new List<FlowNode>( inputPoints.Count );

            Vector3 min = inputPoints[0];
            Vector3 max = inputPoints[0];

            for( int i = 0; i < inputPoints.Count; i++ )
            {
                Vector3 point = inputPoints[i];

                FlowNode node = new FlowNode( point );
                nodes.Add( node );
                // Adding a moderate amount of noise *only to the tetrahedralization* to avoid coplanar issues.
                // Prevents overlapping and fucky tetrahedra from being formed.
                // Ensures that the volume summed over all resulting tetrahedra approximately equals the convex hull volume.
                //   Without this, the volume is too high due to coplanar issues and overlapping tetra.
                vertices[i] = new TetrahedronVertex( point + (DeterministicNoise3( i ) * NOISE_SCALE), i, node );

                min = Vector3.Min( min, point );
                max = Vector3.Max( max, point );
            }

            Vector3 inputBounds = max - min;
            float deltaMax = Mathf.Max( inputBounds.x, Mathf.Max( inputBounds.y, inputBounds.z ) );
            Vector3 center = (min + max) * 0.5f;

            // Super-Tetrahedron
            float scale = 20.0f * deltaMax;
            List<Tetrahedron> tetrahedra = new()
            {
                new Tetrahedron(
                    new TetrahedronVertex(center + new Vector3(0, scale, 0), -1, null),
                    new TetrahedronVertex(center + new Vector3(-scale, -scale, scale), -2, null),
                    new TetrahedronVertex(center + new Vector3(scale, -scale, scale), -3, null),
                    new TetrahedronVertex(center + new Vector3(0, -scale, -scale), -4, null))
            };

            Dictionary<Face, int> boundaryFaces = new();

            // Bowyer-Watson Loop
            // - insert the vertex into the containing tetrahedron.
            foreach( var vertex in vertices )
            {
                List<Tetrahedron> badTetrahedra = new();

                // Identify bad tetrahedra
                for( int i = 0; i < tetrahedra.Count; i++ )
                {
                    if( tetrahedra[i].IsValidAndContains( vertex.Position ) )
                    {
                        badTetrahedra.Add( tetrahedra[i] );
                    }
                }

                boundaryFaces.Clear();

                foreach( var badTetra in badTetrahedra )
                {
                    badTetra.IsBad = true;
                    Face[] faces = {
                        new Face( badTetra.A, badTetra.B, badTetra.C ),
                        new Face( badTetra.A, badTetra.B, badTetra.D ),
                        new Face( badTetra.A, badTetra.C, badTetra.D ),
                        new Face( badTetra.B, badTetra.C, badTetra.D )
                    };

                    foreach( var face in faces )
                    {
                        if( !boundaryFaces.ContainsKey( face ) )
                        {
                            boundaryFaces[face] = 0;
                        }
                        boundaryFaces[face]++;
                    }
                }

                tetrahedra.RemoveAll( t => t.IsBad );

                // Re-triangulate
                foreach( var kvp in boundaryFaces )
                {
                    if( kvp.Value == 1 )
                    {
                        Face face = kvp.Key;

                        Tetrahedron newTet = new Tetrahedron( face.v1, face.v2, face.v3, vertex );

                        // Only add if it is geometrically valid (not flat/degenerate).
                        if( newTet.IsValid )
                        {
                            tetrahedra.Add( newTet );
                        }
                    }
                }
            }

            // 4. Cleanup and Output
            List<FlowTetrahedron> finalTets = new();
            List<FlowEdge> finalEdges = new();
            HashSet<long> edgeSet = new();
            Dictionary<FlowNode, int> nodeToIndex = new();

            for( int i = 0; i < nodes.Count; ++i ) nodeToIndex[nodes[i]] = i;

            foreach( var tet in tetrahedra )
            {
                // Ignore super-tet vertices and invalid tets
                if( !tet.IsValid )
                    continue;

                bool isSuper = tet.A.OriginalIndex < 0 || tet.B.OriginalIndex < 0 ||
                               tet.C.OriginalIndex < 0 || tet.D.OriginalIndex < 0;
                if( isSuper )
                    continue;

                // Ensure positive volume winding for final output
                float vol = Tetrahedron.GetSignedVolume( tet.A.Position, tet.B.Position, tet.C.Position, tet.D.Position );

                // Second safety check for slivers that might have barely passed the epsilon
                if( Mathf.Abs( vol ) < 0.001 )
                    continue;

                FlowTetrahedron newTet;
                if( vol < 0 )
                    newTet = new FlowTetrahedron( tet.A.NodeReference, tet.C.NodeReference, tet.B.NodeReference, tet.D.NodeReference );
                else
                    newTet = new FlowTetrahedron( tet.A.NodeReference, tet.B.NodeReference, tet.C.NodeReference, tet.D.NodeReference );

                finalTets.Add( newTet );

                // Extract Edges
                FlowNode[] verts = { newTet.v0, newTet.v1, newTet.v2, newTet.v3 };
                int[] vidx = new[]
                {
                    nodeToIndex[verts[0]],
                    nodeToIndex[verts[1]],
                    nodeToIndex[verts[2]],
                    nodeToIndex[verts[3]]
                };

                void TryAddEdge( int i, int j )
                {
                    int u = vidx[i];
                    int v = vidx[j];
                    long key = ((long)Math.Min( u, v ) << 32) | (uint)Math.Max( u, v );
                    if( edgeSet.Add( key ) )
                    {
                        finalEdges.Add( new FlowEdge( u, v, -1.0 ) );
                    }
                }

                TryAddEdge( 0, 1 );
                TryAddEdge( 0, 2 );
                TryAddEdge( 0, 3 );
                TryAddEdge( 1, 2 );
                TryAddEdge( 1, 3 );
                TryAddEdge( 2, 3 );
            }

            double volume = 0.0;
            foreach( var tet in finalTets )
            {
                volume += tet.GetVolume();
            }

            return (nodes, finalEdges, finalTets);
        }
    }
}