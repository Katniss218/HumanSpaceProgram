using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HSP.ResourceFlow
{
    public sealed class GenericConsumer : IResourceConsumer
    {
        // TODO...
        public Vector3 FluidAcceleration { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Vector3 FluidAngularVelocity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public SubstanceStateCollection Inflow { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public FluidState Sample( Vector3 localPosition, float holeArea )
        {
            throw new NotImplementedException();
        }
    }

    public sealed class GenericProducer : IResourceProducer
    {
        // TODO...
        public Vector3 FluidAcceleration { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Vector3 FluidAngularVelocity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public SubstanceStateCollection Outflow { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public FluidState Sample( Vector3 localPosition, float holeArea )
        {
            throw new NotImplementedException();
        }

        public IReadonlySubstanceStateCollection SampleSubstances( Vector3 localPosition, float flowRate, float dt )
        {
            throw new NotImplementedException();
        }
    }


    public sealed class FlowTank : IResourceConsumer, IResourceProducer
    {
        private FlowTetrahedron[] _tetrahedra;
        private FlowNode[] _nodes;
        private FlowEdge[] _edges;
        private SubstanceStateCollection[] _contentsInEdges;

        private Dictionary<FlowNode, float> _inletNodes; // inlets and outlets (ports/holes in the tank). If nothing is attached, the inlet is treated as a hole.

        public SubstanceStateCollection Contents { get; set; } // should always equal the sum of what is in the edges.
        public SubstanceStateCollection Inflow { get; set; }
        public SubstanceStateCollection Outflow { get; set; }

        public bool IsEmpty => Contents == null || Contents.IsEmpty();

        /// <summary>
        /// Gets the volume calculated from the tetrahedralization. Used for scaling.
        /// </summary>
        public float CalculatedVolume { get; private set; }
        public float Volume { get; private set; }
        public float Temperature { get; set; }

        public Vector3 FluidAcceleration { get; set; } = Vector3.zero;
        public Vector3 FluidAngularVelocity { get; set; } = Vector3.zero;

        public IReadOnlyList<FlowNode> Nodes => _nodes;
        public IReadOnlyList<FlowEdge> Edges => _edges;
        public IReadonlySubstanceStateCollection[] ContentsInEdges => _contentsInEdges;

        public FlowTank( float volume )
        {
            this.Volume = volume;
        }

        /// <summary>
        /// Gets the vector for the acceleration felt by the fluid at the specified point in local space of the tank.
        /// </summary>
        public Vector3 GetAccelerationAtPoint( Vector3 localPoint )
        {
            // Vector3 comPoint = localPoint - LocalCenterOfMass;
            Vector3 angularAccel = Vector3.Cross( FluidAngularVelocity, Vector3.Cross( FluidAngularVelocity, localPoint ) );
            return FluidAcceleration + angularAccel;
        }

        public FluidState Sample( Vector3 localPosition, float holeArea )
        {
            throw new System.NotImplementedException();
        }

        public IReadonlySubstanceStateCollection SampleSubstances( Vector3 localPosition, float flowRate, float dt )
        {
            throw new System.NotImplementedException();
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
            _edges = edges.ToArray();
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

        /// <summary>
        /// Sets the tetrahedralization vertices and rebuilds the tetrahedralization.
        /// </summary>
        /// <param name="localPositions">The tank-space positions of each non-inlet vertex to tetrahedralize.</param>
        /// <param name="inlets">Positions of additional vertices, that will become inlets.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void SetNodes( Vector3[] localPositions, ResourceInlet[] inlets )
        {
            // If there are no provided nodes, ensure arrays are non-null for later logic.
            if( localPositions == null )
                throw new ArgumentNullException( nameof( localPositions ) );
            if( inlets == null )
                throw new ArgumentNullException( nameof( inlets ) );

            // Save old contents so we can re-distribute after re-tetrahedralizing.
            SubstanceStateCollection oldContents = Contents?.Clone();

            // Make sure internal arrays exist so other code won't null-ref.
            if( _nodes == null )
                _nodes = new FlowNode[0];

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
        public void DistributeFluids()
        {
            // Single-step settlement under linear acceleration (no angular/centrifugal part yet).
            // Steps:
            // 1) Gather total Contents (if null -> nothing to distribute)
            // 2) Build breakpoints from edge endpoint heights (project onto -accel direction)
            // 3) For each interval compute per-edge capacity contributions
            // 4) Sort fluids by density desc and pour bottom-up
            // 5) Write back per-edge SubstanceStateCollection and global Contents
#warning TODO - ensure that 'overfilled' tank still correctly proportionally distributes fluids (at extreme pressure).

            // quick sanity
            if( _edges == null || _edges.Length == 0 )
                return;

            // Ensure contents exist
            if( Contents == null || Contents.IsEmpty() )
            {
                // Clear per-edge contents
                _contentsInEdges = new SubstanceStateCollection[_edges.Length];
                for( int i = 0; i < _contentsInEdges.Length; i++ )
                    _contentsInEdges[i] = new SubstanceStateCollection();
                return;
            }

            // --- 1) compute projection axis (u = -accelNormalized). If accel is zero, nothing to do (no "down" direction).
            Vector3 accel = FluidAcceleration;
            float accelMag = accel.magnitude;
            if( accelMag <= 1e-8f )
            {
                // No preferred direction: leave distribution as-is or choose arbitrary axis. We'll leave as-is.
                // Option: you could default to world -Y or similar. For now do nothing.
                return;
            }
            Vector3 upDir = -accel / accelMag; // points "up" opposite acceleration, so lower potential = smaller dot

            int edgeCount = _edges.Length;

            // --- 2) collect all endpoint projected heights
            List<float> breakpoints = new List<float>( edgeCount * 2 );
            // We'll store per-edge projected endpoints and lengths
            float[] h0 = new float[edgeCount];
            float[] h1 = new float[edgeCount];
            float[] edgeProjLength = new float[edgeCount];
            float[] edgeVolume = new float[edgeCount];

            for( int i = 0; i < edgeCount; i++ )
            {
                FlowEdge edge = _edges[i];
                // assumed accessors:
                Vector3 pA = edge.end1.pos;
                Vector3 pB = edge.end2.pos;
                float ha = Vector3.Dot( pA, upDir );
                float hb = Vector3.Dot( pB, upDir );
                h0[i] = ha;
                h1[i] = hb;
                float hmin = Math.Min( ha, hb );
                float hmax = Math.Max( ha, hb );
                edgeProjLength[i] = hmax - hmin; // may be zero
                edgeVolume[i] = Mathf.Max( 0.0f, edge.Volume ); // assume edge.Volume exists and valid

                // add endpoints
                breakpoints.Add( ha );
                breakpoints.Add( hb );
            }

            // unique-sort breakpoints with small epsilon dedupe
            const float EPS = 1e-6f;
            breakpoints.Sort();
            List<float> uniq = new List<float>();
            for( int i = 0; i < breakpoints.Count; i++ )
            {
                if( i == 0 || Math.Abs( breakpoints[i] - breakpoints[i - 1] ) > EPS )
                    uniq.Add( breakpoints[i] );
            }

            if( uniq.Count < 2 )
            {
                // All endpoints at same potential -> everything is a point. Put fluids into edges proportionally to edge volume.
                // Compute total volume and distribute substances proportional to edgeVolume[i]
                float totalCap = 0.0f;
                for( int i = 0; i < edgeCount; i++ )
                    totalCap += edgeVolume[i];
                if( totalCap <= 0.0f )
                    return;

                // Gather fluids from Contents
                var fluids = Contents.ToArray();
                float pressure2 = SubstanceState.GetMixturePressure( fluids, Volume, Temperature );

                // clear per-edge contents
                _contentsInEdges = new SubstanceStateCollection[edgeCount];
                for( int i = 0; i < edgeCount; i++ )
                    _contentsInEdges[i] = new SubstanceStateCollection();

                for( int f = 0; f < fluids.Length; f++ )
                {
                    var fs = fluids[f];
                    float remaining2 = fs.GetVolumeAtPressure( pressure2, Temperature );
                    // distribute proportionally
                    for( int ei = 0; ei < edgeCount; ei++ )
                    {
                        float share = edgeVolume[ei] / totalCap;
                        float assignVol = remaining2 * share;
                        if( assignVol <= 0 )
                            continue;
                        _contentsInEdges[ei].Add( fs );
                    }
                }
                return;
            }

            // Build intervals [U_j, U_{j+1})
            int B = uniq.Count - 1;
            float[] U = uniq.ToArray();

            // Precompute per-edge, per-interval contributions. We'll store for each interval a list of (edgeIndex, contribution)
            // To avoid heavy NxM arrays, use sparse lists.
            List<(int edgeIndex, float contribution)>[] intervalContribs = new List<(int, float)>[B];
            float[] intervalTotalCap = new float[B];
            for( int j = 0; j < B; j++ )
            {
                intervalContribs[j] = new List<(int, float)>();
                intervalTotalCap[j] = 0.0f;
            }

            for( int i = 0; i < edgeCount; i++ )
            {
                float hi0 = h0[i];
                float hi1 = h1[i];
                float hmin = Math.Min( hi0, hi1 );
                float hmax = Math.Max( hi0, hi1 );
                float Vi = edgeVolume[i];
                float ell = edgeProjLength[i];

                if( ell > EPS )
                {
                    // uniform per-projected-length -> density nu = Vi / ell
                    float nu = Vi / ell;

                    // find intervals that intersect [hmin, hmax]
                    // naive loop over intervals (B usually small relative to E)
                    for( int j = 0; j < B; j++ )
                    {
                        float low = U[j];
                        float high = U[j + 1];
                        float overlap = Math.Max( 0.0f, Math.Min( hmax, high ) - Math.Max( hmin, low ) );
                        if( overlap <= 0.0f )
                            continue;
                        float contribution = nu * overlap; // volume portion in this interval
                        intervalContribs[j].Add( (i, contribution) );
                        intervalTotalCap[j] += contribution;
                    }
                }
                else
                {
                    // point-capacity: assign full volume to the interval that contains the point (or nearest).
                    float pt = hi0; // hi0 == hi1
                    // find j such that U[j] <= pt < U[j+1], with edge-case pt == U[B] -> put into last interval
                    int jfound = -1;
                    for( int j = 0; j < B; j++ )
                    {
                        if( (pt >= U[j] && pt < U[j + 1]) || (j == B - 1 && Math.Abs( pt - U[j + 1] ) <= EPS) )
                        {
                            jfound = j;
                            break;
                        }
                    }
                    if( jfound == -1 )
                    {
                        // rare numeric issue: clamp to nearest interval
                        if( pt < U[0] ) jfound = 0;
                        else jfound = B - 1;
                    }
                    intervalContribs[jfound].Add( (i, Vi) );
                    intervalTotalCap[jfound] += Vi;
                }
            }

            // --- 3) Gather fluids (from Contents) and sort by density descending
            SubstanceState[] fluidsList = Contents.ToArray(); // returns list of FluidSummary {Substance, Volume, Density}
            float pressure = SubstanceState.GetMixturePressure( fluidsList, Volume, Temperature );
            // sort by density descending (heaviest first)
            Array.Sort( fluidsList, ( a, b ) => b.Substance.Density.CompareTo( a.Substance.Density ) );

            int F = fluidsList.Length;
            if( F == 0 )
            {
                // nothing to distribute
                return;
            }

            float[] remaining = new float[F];
            for( int f = 0; f < F; f++ )
                remaining[f] = fluidsList[f].GetVolumeAtPressure( pressure, Temperature );

            // result allocation per-edge per-fluid: we will accumulate into per-edge SubstanceStateCollections directly
            SubstanceStateCollection[] newEdgeContents = new SubstanceStateCollection[edgeCount];
            for( int i = 0; i < edgeCount; i++ )
                newEdgeContents[i] = new SubstanceStateCollection();

            // --- 4) pour fluids bottom-up (intervals from lowest U to highest U)
            for( int j = 0; j < B; j++ )
            {
                float cap = intervalTotalCap[j];
                if( cap <= 0.0f )
                    continue;

                // iterate fluids heaviest -> lightest
                for( int f = 0; f < F; f++ )
                {
                    if( remaining[f] <= 0.0f )
                        continue;

                    if( remaining[f] >= cap - 1e-9f )
                    {
                        // fill entire interval with this fluid
                        float assign = cap;
                        // distribute to each edge proportional to its contribution
                        foreach( var (edgeIdx, contrib) in intervalContribs[j] )
                        {
                            if( contrib <= 0 )
                                continue;
                            float share = contrib / cap;
                            float vol = assign * share;
                            if( vol <= 0 )
                                continue;
                            var fs = fluidsList[f];
                            newEdgeContents[edgeIdx].Add( fs );
                        }
                        remaining[f] -= assign;
                        cap = 0.0f;
                        intervalTotalCap[j] = 0.0f;
                        break; // interval fully consumed, go to next interval
                    }
                    else
                    {
                        // partial fill by this fluid: fraction ffrac = remaining[f] / cap
                        float assign = remaining[f];
                        float frac = assign / cap;
                        foreach( var (edgeIdx, contrib) in intervalContribs[j] )
                        {
                            if( contrib <= 0 )
                                continue;
                            float vol = contrib * frac;
                            if( vol <= 0 )
                                continue;
                            var fs = fluidsList[f];
                            newEdgeContents[edgeIdx].Add( fs );
                        }
                        // reduce remaining capacity and zero out this fluid
                        cap -= assign;
                        intervalTotalCap[j] = cap;
                        remaining[f] = 0.0f;
                        // continue with next (lighter) fluid to fill leftover cap in this interval
                    }
                } // fluids
            } // intervals

            // If any remaining fluid beyond all intervals (overflow), distribute proportionally to edge total volumes (rare)
            for( int f = 0; f < F; f++ )
            {
                if( remaining[f] <= 1e-9f )
                    continue;
                float overflow = remaining[f];
                // compute total capacity across edges
                float totalEdgeVol = 0.0f;
                for( int i = 0; i < edgeCount; i++ )
                    totalEdgeVol += edgeVolume[i];
                if( totalEdgeVol <= 0.0f )
                    continue;
                for( int ei = 0; ei < edgeCount; ei++ )
                {
                    float share = edgeVolume[ei] / totalEdgeVol;
                    float vol = overflow * share;
                    var fs = fluidsList[f];
                    newEdgeContents[ei].Add( fs );
                }
                remaining[f] = 0.0f;
            }

            // write results back
            _contentsInEdges = newEdgeContents;
        }
    }
}