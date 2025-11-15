using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HSP.ResourceFlow
{
    public sealed class GenericConsumer : IResourceConsumer
    {

    }

    public sealed class GenericProducer : IResourceProducer
    {

    }


    public sealed class FlowTank : IResourceConsumer, IResourceProducer
    {
        private FlowTetrahedron[] _tetrahedra;
        private FlowNode[] _nodes;
        private FlowEdge[] _edges;
        private SubstanceStateCollection[] _contentsInEdges;

        private Dictionary<FlowNode, float> _inletNodes; // inlets and outlets (ports/holes in the tank). If nothing is attached, the inlet is treated as a hole.

        public SubstanceStateCollection Contents { get; private set; } // should always equal the sum of what is in the edges.
        public SubstanceStateCollection Inflow { get; private set; }
        public SubstanceStateCollection Outflow { get; private set; }

        public bool IsEmpty => Contents == null || Contents.IsEmpty();

        /// <summary>
        /// Gets the volume calculated from the tetrahedralization. Used for scaling.
        /// </summary>
        public float CalculatedVolume { get; private set; }
        public float Volume { get; private set; }

        public Vector3 Acceleration { get; set; } = Vector3.zero; // in tank-space, acceleration of tank relative to fluid.
        public Vector3 AngularVelocity { get; set; } = Vector3.zero;
       // public Vector3 LocalCenterOfMass { get; set; } = Vector3.zero;

        public IReadOnlyList<FlowNode> Nodes => _nodes;
        public IReadOnlyList<FlowEdge> Edges => _edges;
        public IReadonlySubstanceStateCollection[] ContentsInEdges => _contentsInEdges;

        /// <summary>
        /// Gets the vector for the acceleration felt by the fluid at the specified point in local space of the tank.
        /// </summary>
        public Vector3 GetAccelerationAtPoint( Vector3 localPoint )
        {
           // Vector3 comPoint = localPoint - LocalCenterOfMass;
            Vector3 angularAccel = Vector3.Cross( AngularVelocity, Vector3.Cross( AngularVelocity, localPoint ) );
            return Acceleration + angularAccel;
        }

        private void SetTetrahedralization( List<FlowNode> nodes, List<FlowEdge> edges, List<FlowTetrahedron> tets )
        {
            // figure out how many and which tetrahedra each edge is part of, then

            // distribute tetrahedra volumes to edges.

            /*
            To get the 'proper' volume of each edge, we start with the user-defined 'desired' total tank volume. This can be anything, from 0 to infinity.
            We then calculate the 'desired' volumes for each tetrahedron (and their total 'desired' volume, i.e. the sum), which will be used for proportional scaling.
            The 'actual' volume of a *tetrahedron* is then: `actual = (desired / desired_total) * actual_total`.
            That is then split up between the edges that are part of this tetrahedron, according to the edge length (similar proportionality as above).
            Then we can get the 'proper' volume of the *edge*, which is just the sum of the contributions from each tetrahedron that this edge is a part of.
            */

            _nodes = nodes.ToArray();
            _tetrahedra = tets.ToArray();

            RecalculateEdgeVolumes();
        }

        private static (FlowNode, FlowNode) GetCanonicalEdgeKey( FlowNode node1, FlowNode node2 )
        {
            // Create a canonical ordering for edges (smaller position first)
            Vector3 pos1 = node1.pos;
            Vector3 pos2 = node2.pos;

            if( pos1.x < pos2.x ||
               (pos1.x == pos2.x && pos1.y < pos2.y) ||
               (pos1.x == pos2.x && pos1.y == pos2.y && pos1.z < pos2.z) )
            {
                return (node1, node2);
            }
            return (node2, node1);
        }

        private void RecalculateEdgeVolumes()
        {
            if( _tetrahedra == null || _edges == null )
                return;

            // Calculate desired volumes for each tetrahedron
            float totalDesiredVolume = 0.0f;
            float[] desiredVolumes = new float[_tetrahedra.Length];

            for( int i = 0; i < _tetrahedra.Length; i++ )
            {
                desiredVolumes[i] = _tetrahedra[i].GetVolume();
                totalDesiredVolume += desiredVolumes[i];
            }

            // Calculate actual volumes for each tetrahedron
            float[] actualVolumes = new float[_tetrahedra.Length];
            if( totalDesiredVolume > 0 )
            {
                for( int i = 0; i < _tetrahedra.Length; i++ )
                {
                    actualVolumes[i] = (desiredVolumes[i] / totalDesiredVolume) * Volume;
                }
            }

            // Calculate edge volumes by distributing tetrahedron volumes to edges
            Dictionary<(FlowNode, FlowNode), float> edgeVolumes = new();

            for( int i = 0; i < _tetrahedra.Length; i++ )
            {
                FlowTetrahedron tet = _tetrahedra[i];
                FlowNode[] tetNodes = new[] { tet.v0, tet.v1, tet.v2, tet.v3 };
                float tetVolume = actualVolumes[i];

                // Calculate total edge length for this tetrahedron
                float totalEdgeLength = 0.0f;
                List<(FlowNode, FlowNode, float)> tetEdges = new();

                for( int j = 0; j < 4; j++ )
                {
                    for( int k = j + 1; k < 4; k++ )
                    {
                        FlowNode node1 = tetNodes[j];
                        FlowNode node2 = tetNodes[k];
                        float length = Vector3.Distance( node1.pos, node2.pos );
                        totalEdgeLength += length;

                        (FlowNode, FlowNode) edgeKey = GetCanonicalEdgeKey( node1, node2 );
                        tetEdges.Add( (edgeKey.Item1, edgeKey.Item2, length) );
                    }
                }

                // Distribute tetrahedron volume to edges proportionally to edge length
                if( totalEdgeLength > 0 )
                {
                    foreach( (FlowNode node1, FlowNode node2, float length) in tetEdges )
                    {
                        (FlowNode, FlowNode) edgeKey = (node1, node2);
                        float contribution = (length / totalEdgeLength) * tetVolume;

                        if( !edgeVolumes.ContainsKey( edgeKey ) )
                        {
                            edgeVolumes[edgeKey] = 0.0f;
                        }
                        edgeVolumes[edgeKey] += contribution;
                    }
                }
            }

            // Create new edges with calculated volumes
            List<FlowEdge> newEdges = new();

            foreach( var kvp in edgeVolumes )
            {
                FlowEdge edge = new FlowEdge( kvp.Key.Item1, kvp.Key.Item2, kvp.Value );
                newEdges.Add( edge );
            }

            _edges = newEdges.ToArray();
            _contentsInEdges = new SubstanceStateCollection[_edges.Length];

            // Initialize edge contents
            for( int i = 0; i < _contentsInEdges.Length; i++ )
            {
                _contentsInEdges[i] = new SubstanceStateCollection();
            }

            // Calculate total calculated volume
            CalculatedVolume = edgeVolumes.Values.Sum();
        }

        public void SetNodes( Vector3[] localPositions, ResourceInlet[] inlets )
        {
            // If there are no provided nodes, ensure arrays are non-null for later logic.
            if( localPositions == null ) localPositions = new Vector3[0];
            if( inlets == null ) inlets = new ResourceInlet[0];

            // Save old contents so we can re-distribute after re-tetrahedralizing.
            SubstanceStateCollection oldContents = Contents?.Clone();

            // Make sure internal arrays exist so other code won't null-ref.
            if( _nodes == null ) _nodes = new FlowNode[0];

            const float SNAP_DISTANCE = 0.05f;   // if a provided node is within this distance to exactly one inlet, we will skip adding it (it will be represented by the inlet)
            const float DEDUPE_DISTANCE = 0.01f; // positions closer than this to an already-added position will be treated as duplicates

            List<Vector3> allPositions = new List<Vector3>();

            // Helper to test if a candidate is duplicate of any already in allPositions
            bool IsDuplicate( Vector3 candidate )
            {
                for( int i = 0; i < allPositions.Count; i++ )
                {
                    if( Vector3.Distance( allPositions[i], candidate ) <= DEDUPE_DISTANCE )
                        return true;
                }
                return false;
            }

            // 1) Process user-supplied positions:
            //    - If a position is within SNAP_DISTANCE to exactly one inlet -> skip it (we'll add the inlet position below).
            //    - If within SNAP_DISTANCE to multiple inlets -> keep it (ambiguous snap).
            //    - Otherwise -> add it if not a duplicate.
            for( int i = 0; i < localPositions.Length; i++ )
            {
                Vector3 pos = localPositions[i];

                int nearbyInletCount = 0;
                float nearestInletDist = float.MaxValue;

                for( int j = 0; j < inlets.Length; j++ )
                {
                    Vector3 inletPos = inlets[j].LocalPosition;
                    float d = Vector3.Distance( pos, inletPos );
                    if( d <= SNAP_DISTANCE )
                    {
                        nearbyInletCount++;
                    }
                    if( d < nearestInletDist )
                    {
                        nearestInletDist = d;
                    }
                }

                if( nearbyInletCount == 1 )
                {
                    // If exactly one inlet is within the snap distance, we intentionally skip adding this position
                    // because the inlet will be added later (keeps user-specified nodes from duplicating inlet nodes).
                    continue;
                }

                // If it's ambiguous (multiple nearby inlets) or no nearby inlet, add the position if not duplicate
                if( !IsDuplicate( pos ) )
                {
                    allPositions.Add( pos );
                }
            }

            // 2) Ensure all inlet positions are included (deduped).
            for( int i = 0; i < inlets.Length; i++ )
            {
                Vector3 inletPos = inlets[i].LocalPosition;
                if( !IsDuplicate( inletPos ) )
                {
                    allPositions.Add( inletPos );
                }
            }

            // Edge-case: if we still have zero positions, add a single origin node to avoid tetrahedralizer errors.
            if( allPositions.Count == 0 )
            {
                allPositions.Add( Vector3.zero );
            }

            // 3) Compute tetrahedralization from the position list.
            (List<FlowNode> nodes, List<FlowEdge> edges, List<FlowTetrahedron> tets) =
                DelaunayTetrahedralizer.ComputeDelaunayTetrahedralization( allPositions );

            // 4) Populate inlet-node mapping (_inletNodes) by matching inlet positions to produced FlowNode positions.
            _inletNodes = new Dictionary<FlowNode, float>();

            // Use a matching threshold slightly larger than dedupe (so matching succeeds)
            const float MATCH_NODE_TO_INLET_DISTANCE = 0.02f;

            for( int i = 0; i < inlets.Length; i++ )
            {
                Vector3 inletPos = inlets[i].LocalPosition;

                // find nearest node to this inlet position
                FlowNode bestNode = null;
                float bestDist = float.MaxValue;
                foreach( var node in nodes )
                {
                    float d = Vector3.Distance( node.pos, inletPos );
                    if( d < bestDist )
                    {
                        bestDist = d;
                        bestNode = node;
                    }
                }

                if( bestNode != null && bestDist <= MATCH_NODE_TO_INLET_DISTANCE )
                {
                    // For now map to 0f (no forced inflow/outflow); the float slot can be used later for metadata like max flow rate or openness.
                    if( !_inletNodes.ContainsKey( bestNode ) )
                        _inletNodes.Add( bestNode, 0.0f );
                }
                else
                {
                    // If no nearby node was found (unlikely), attempt to create a synthetic mapping by finding the closest produced node anyway.
                    if( bestNode != null && !_inletNodes.ContainsKey( bestNode ) )
                    {
                        _inletNodes.Add( bestNode, 0.0f );
                    }
                }
            }

            // 5) Apply tetrahedralization and re-distribute any previously existing fluid.
            SetTetrahedralization( nodes, edges, tets );

            // Restore old contents and redistribute (the caller may prefer different behavior; this preserves fluid as-is).
            if( oldContents != null && !oldContents.IsEmpty() )
            {
                Contents = oldContents;
                DistributeFluids();
            }
        }

        internal void DistributeFluids()
        {
            // distributes the fluids by density, based on height intervals of the edges projected onto the acceleration vector.

        }
    }
}