using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HSP.ResourceFlow
{
    public sealed class FlowTank : IResourceConsumer, IResourceProducer
    {
        private FlowTetrahedron[] _tetrahedra;
        private FlowNode[] _nodes;
        private FlowEdge[] _edges;
        private ISubstanceStateCollection[] _contentsInEdges;

        private Vector3 _fluidAcceleration = Vector3.zero;
        private Vector3 _fluidAngularVelocity = Vector3.zero;

        private Dictionary<FlowNode, double> _inletNodes; // inlets and outlets (ports/holes in the tank). If nothing is attached, the inlet is treated as a hole.

        public ISubstanceStateCollection Contents { get; set; } // should always equal the sum of what is in the edges.
        public ISubstanceStateCollection Inflow { get; set; }
        public ISubstanceStateCollection Outflow { get; set; }
        public FluidState FluidState { get; set; }

        public bool IsEmpty => Contents == null || Contents.IsEmpty();

        /// <summary>
        /// Gets the volume calculated from the tetrahedralization. Used for scaling.
        /// </summary>
        public double CalculatedVolume { get; private set; }
        /// <summary>
        /// The volume, in [m^3].
        /// </summary>
        public double Volume { get; private set; }

        public Vector3 FluidAcceleration
        {
            get => _fluidAcceleration;
            set
            {
                if( _fluidAcceleration != value )
                {
                    _fluidAcceleration = value;
                    _nodePotentials = null; // invalidate
                }
            }
        }

        public Vector3 FluidAngularVelocity
        {
            get => _fluidAngularVelocity;
            set
            {
                if( _fluidAngularVelocity != value )
                {
                    _fluidAngularVelocity = value;
                    _nodePotentials = null; // invalidate
                }
            }
        }

        public IReadOnlyList<FlowNode> Nodes => _nodes;
        public IReadOnlyList<FlowEdge> Edges => _edges;
        public IReadonlySubstanceStateCollection[] ContentsInEdges => _contentsInEdges;
        public IReadOnlyDictionary<FlowNode, double> InletNodes => _inletNodes;

        public FlowTank( double volume )
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

        public FluidState Sample( Vector3 localPosition, double holeArea )
        {
            throw new NotImplementedException();
        }

        public IReadonlySubstanceStateCollection SampleSubstances( Vector3 localPosition, double flowRate, double dt )
        {
            throw new NotImplementedException();
        }

        private void SetTetrahedralization( List<FlowNode> nodes, List<FlowEdge> edges, List<FlowTetrahedron> tets )
        {
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
            /*
            
            To get the 'proper' volume of each edge, we start with the user-defined 'desired' total tank volume. This can be anything, from 0 to infinity.
            We then calculate the 'desired' volumes for each tetrahedron (and their total 'desired' volume, i.e. the sum), which will be used for proportional scaling.
            The 'actual' volume of a *tetrahedron* is then: `actual = (desired / desired_total) * actual_total`.
            That is then split up between the edges that are part of this tetrahedron, according to the edge length (similar proportionality as above).
            Then we can get the 'proper' volume of the *edge*, which is just the sum of the contributions from each tetrahedron that this edge is a part of.

            */

            if( _tetrahedra == null || _edges == null )
                return;

            // Calculate desired volumes for each tetrahedron
            double totalDesiredVolume = 0.0f;
            double[] desiredVolumes = new double[_tetrahedra.Length];

            for( int i = 0; i < _tetrahedra.Length; i++ )
            {
                desiredVolumes[i] = _tetrahedra[i].GetVolume();
                totalDesiredVolume += desiredVolumes[i];
            }

            // Calculate actual volumes for each tetrahedron
            double[] actualVolumes = new double[_tetrahedra.Length];
            if( totalDesiredVolume > 0 )
            {
                for( int i = 0; i < _tetrahedra.Length; i++ )
                {
                    actualVolumes[i] = (desiredVolumes[i] / totalDesiredVolume) * Volume;
                }
            }

            // Calculate edge volumes by distributing tetrahedron volumes to edges
            Dictionary<(FlowNode, FlowNode), double> edgeVolumes = new();

            for( int i = 0; i < _tetrahedra.Length; i++ )
            {
                FlowTetrahedron tet = _tetrahedra[i];
                FlowNode[] tetNodes = new[] { tet.v0, tet.v1, tet.v2, tet.v3 };
                double tetVolume = actualVolumes[i];

                // Calculate total edge length for this tetrahedron
                double totalEdgeLength = 0.0f;
                List<(FlowNode, FlowNode, double)> tetEdges = new();

                for( int j = 0; j < 4; j++ )
                {
                    for( int k = j + 1; k < 4; k++ )
                    {
                        FlowNode node1 = tetNodes[j];
                        FlowNode node2 = tetNodes[k];
                        double length = Vector3.Distance( node1.pos, node2.pos );
                        totalEdgeLength += length;

                        (FlowNode, FlowNode) edgeKey = GetCanonicalEdgeKey( node1, node2 );
                        tetEdges.Add( (edgeKey.Item1, edgeKey.Item2, length) );
                    }
                }

                // Distribute tetrahedron volume to edges proportionally to edge length
                if( totalEdgeLength > 0 )
                {
                    foreach( (FlowNode node1, FlowNode node2, double length) in tetEdges )
                    {
                        (FlowNode, FlowNode) edgeKey = (node1, node2);
                        double contribution = (length / totalEdgeLength) * tetVolume;

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
            ISubstanceStateCollection oldContents = Contents?.Clone();

            // Make sure internal arrays exist so other code won't null-ref.
            if( _nodes == null )
                _nodes = new FlowNode[0];

            _edgeEnd1Indices = null;
            _edgeEnd2Indices = null;

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
            _inletNodes = new Dictionary<FlowNode, double>();

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

            // Restore old contents and redistribute (this preserves fluid as-is).
            if( oldContents != null && !oldContents.IsEmpty() )
            {
                Contents = oldContents;
                DistributeContents();
            }
        }

        /// <summary>
        /// Calculates the scalar potential energy at a point. <br/>
        /// Fluids effectively "fall" from high to low potential.
        /// </summary>
        /// <remarks>
        /// Note: Standard physics defines Force = -Gradient(Potential). <br/>
        /// If FluidAcceleration is "Gravity" (pointing down), Potential = -g.y (increases going up).
        /// </remarks>
        /// <param name="localPosition">The position, in tank-space.</param>
        public double GetPotential( Vector3 localPosition )
        {
            double linearContrib = 0.0;
            double rotationalContrib = 0.0;

            if( FluidAcceleration.sqrMagnitude > 0.001 )
            {
                linearContrib = -Vector3.Dot( FluidAcceleration, localPosition );
            }
            if( FluidAngularVelocity.sqrMagnitude > 0.001 )
            {
                Vector3 tang = Vector3.Cross( FluidAngularVelocity, localPosition );
                rotationalContrib = -0.5 * tang.sqrMagnitude;
            }

            return linearContrib + rotationalContrib;
        }

        //
        //  Stratification Algorithm.
        //

        private struct FluidEntryDensityComparer : IComparer<FluidEntry>
        {
            public int Compare( FluidEntry lhs, FluidEntry rhs ) => rhs.Density.CompareTo( lhs.Density );
        }

        public struct FluidEntry
        {
            public ISubstance Substance;
            public double Mass;
            public double Volume;
            public double Density;
        }

        // Reusable buffers to limit allocations.
        private double[] _nodePotentials;
        private double[] _edgePotentialsMin; // min and max for edges (2 ends)
        private double[] _edgePotentialsMax;
        private int[] _edgeEnd1Indices;
        private int[] _edgeEnd2Indices;

        private readonly List<double> _sortedNodePotentials = new();
        private readonly List<double> _distinctPotentials = new();

        private FluidEntry[] _sortedFluids;
        private double[] _sortedFluidVolumesRemaining;
        private double[] _intervalVolumes; // Intervals are the spaces between distinct potentials. And potentials are sampled at each node.

        private List<(int edgeIndex, double contribution)>[] _intervalContributions; // how much each edge contributes to an interval (used for scaling during fill).

        private const double TOLERANCE = 1e-4;
        private readonly FluidEntryDensityComparer _fluidComparer = new();

        public void DistributeContents()
        {
            // The idea is to calculate the potential for each node/edge, and then figure out where they fall.
            // Then pour heaviest-first, scaling the volumes to account for overflow.

            if( _edges == null || _edges.Length == 0 )
                return;
            if( Contents == null || Contents.IsEmpty() )
                return;

            int nodeCount = _nodes.Length;
            int edgeCount = _edges.Length;

            // 1. Ensure Caches (Topology and Memory)
            EnsureTopologyCache();

            ClearEdgeContents();

            // 3. Collect sorted potential breakpoints from node potentials.
            _sortedNodePotentials.Clear();
            double totalTankVolume = 0;
            for( int i = 0; i < edgeCount; i++ )
            {
                // HOT PATH OPTIMIZATION: 
                // Direct array access via pre-computed indices. No Dictionary, no Search.
                double potEnd1 = _nodePotentials[_edgeEnd1Indices[i]];
                double potEnd2 = _nodePotentials[_edgeEnd2Indices[i]];

                double min, max;
                if( potEnd1 < potEnd2 )
                {
                    min = potEnd1;
                    max = potEnd2;
                }
                else
                {
                    min = potEnd2;
                    max = potEnd1;
                }

                _edgePotentialsMin[i] = min;
                _edgePotentialsMax[i] = max;

                totalTankVolume += _edges[i].Volume;

                _sortedNodePotentials.Add( min );
                _sortedNodePotentials.Add( max );
            }
            _sortedNodePotentials.Sort();
            PopulateDistinctPotentials( _sortedNodePotentials, _distinctPotentials );

            int intervalCount = _distinctPotentials.Count - 1;
            if( intervalCount <= 0 )
            {
                DistributeUniformly( totalTankVolume );
                return;
            }

            // 4. Map edges to intervals
            EnsureIntervalBuckets( intervalCount );

            if( _intervalVolumes == null || _intervalVolumes.Length < intervalCount )
            {
                Array.Resize( ref _intervalVolumes, intervalCount );
            }
            Array.Clear( _intervalVolumes, 0, intervalCount );

            for( int i = 0; i < edgeCount; i++ )
            {
                double edgePotentialLow = _edgePotentialsMin[i];
                double edgePotentialHigh = _edgePotentialsMax[i];
                double edgeVolume = _edges[i].Volume;
                double potentialDifference = edgePotentialHigh - edgePotentialLow;

                // Gradient potential
                if( potentialDifference > TOLERANCE )
                {
                    double invSpan = edgeVolume / potentialDifference;
                    int startIndex = FindFirstIntervalIndex( _distinctPotentials, edgePotentialLow );

                    for( int intervalIndex = startIndex; intervalIndex < intervalCount; intervalIndex++ )
                    {
                        double intervalLowPotential = _distinctPotentials[intervalIndex];
                        double intervalHighPotential = _distinctPotentials[intervalIndex + 1];

                        if( intervalLowPotential >= edgePotentialHigh - TOLERANCE )
                            break;

                        double overlapLow = (edgePotentialLow > intervalLowPotential) 
                            ? edgePotentialLow 
                            : intervalLowPotential;
                        double overlapHigh = (edgePotentialHigh < intervalHighPotential)
                            ? edgePotentialHigh 
                            : intervalHighPotential;
                        double overlap = overlapHigh - overlapLow;

                        if( overlap > TOLERANCE )
                        {
                            double contribution = overlap * invSpan;
                            _intervalContributions[intervalIndex].Add( (i, contribution) );
                            _intervalVolumes[intervalIndex] += contribution;
                        }
                    }
                }
                // Flat potential
                else
                {
                    int intervalIndex = FindFirstIntervalIndex( _distinctPotentials, edgePotentialLow );
                    if( intervalIndex >= 0 && intervalIndex < intervalCount )
                    {
                        _intervalContributions[intervalIndex].Add( (i, edgeVolume) );
                        _intervalVolumes[intervalIndex] += edgeVolume;
                    }
                }
            }

            // 5. Prepare fluids
            RebuildSortedFluidsArray( out double totalFluidReferenceVolume );

            // 6. Calculate scale (stratified overflow)
            double volumeScale = 1.0;
            if( totalFluidReferenceVolume > totalTankVolume && totalTankVolume > TOLERANCE )
            {
                volumeScale = totalTankVolume / totalFluidReferenceVolume;
            }

            // 7. Fill
            FillIntervals( intervalCount, volumeScale );
        }

        private void EnsureTopologyCache()
        {
            int edgeCount = _edges.Length;
            int nodeCount = _nodes.Length;

            if( _edgePotentialsMin == null || _edgePotentialsMin.Length != edgeCount )
            {
                _edgePotentialsMin = new double[edgeCount];
                _edgePotentialsMax = new double[edgeCount];
            }

            if( _edgeEnd1Indices == null || _edgeEnd1Indices.Length != edgeCount )
            {
                _edgeEnd1Indices = new int[edgeCount];
                _edgeEnd2Indices = new int[edgeCount];

                // Temporarily map FlowNode -> Index for O(N) setup instead of O(N^2)
                Dictionary<FlowNode, int> nodeMap = new ( nodeCount );
                for( int i = 0; i < nodeCount; i++ )
                {
                    nodeMap[_nodes[i]] = i;
                }

                for( int i = 0; i < edgeCount; i++ )
                {
                    FlowEdge edge = _edges[i];
                    // Assuming edge nodes exist in the node array. 
                    // If not found, default to 0 to prevent crash, though data will be wrong.
                    if( nodeMap.TryGetValue( edge.end1, out int end1 ) )
                        _edgeEnd1Indices[i] = end1;
                    if( nodeMap.TryGetValue( edge.end2, out int end2 ) )
                        _edgeEnd2Indices[i] = end2;
                }
            }

            if( _nodePotentials == null || _nodePotentials.Length != nodeCount )
            {
                _nodePotentials = new double[nodeCount];
                for( int i = 0; i < nodeCount; i++ )
                {
                    _nodePotentials[i] = GetPotential( _nodes[i].pos );
                }
            }
        }

        private void FillIntervals( int intervalCount, double volumeScale )
        {
            // Fills, using the calculated volumes and densities *at tank pressure* to match the calculated scaling factor exactly.
            int fluidCount = _sortedFluids.Length;

#warning TODO - needs to use tank pressure instead, for accurate compression and distribution of mixed fluid-gas contents.
            if( _sortedFluidVolumesRemaining == null || _sortedFluidVolumesRemaining.Length < fluidCount )
                _sortedFluidVolumesRemaining = new double[fluidCount];

            for( int i = 0; i < fluidCount; i++ )
                _sortedFluidVolumesRemaining[i] = _sortedFluids[i].Volume * volumeScale;

            double inverseVolumeScale = 1.0 / volumeScale;

            for( int i = 0; i < intervalCount; i++ )
            {
                double capacity = _intervalVolumes[i];
                if( capacity <= TOLERANCE )
                    continue;

                double initialCapacity = capacity;
                var contributions = _intervalContributions[i];
                int contributionCount = contributions.Count;

                for( int j = 0; j < fluidCount; j++ )
                {
                    double remaining = _sortedFluidVolumesRemaining[j];
                    if( remaining <= TOLERANCE )
                        continue;

                    double pourVolume = (remaining < capacity) ? remaining : capacity;
                    double fillRatio = pourVolume / initialCapacity;
                    double massFactor = fillRatio * _sortedFluids[j].Density * inverseVolumeScale;
                    ISubstance substance = _sortedFluids[j].Substance;

                    for( int k = 0; k < contributionCount; k++ )
                    {
                        (int edgeIdx, double geoVolume) = contributions[k];
                        _contentsInEdges[edgeIdx].Add( substance, geoVolume * massFactor );
                    }

                    _sortedFluidVolumesRemaining[j] -= pourVolume;
                    capacity -= pourVolume;

                    if( capacity <= TOLERANCE )
                        break;
                }
            }

            // Sync remaining volume back for correctness (unscaled)
            for( int i = 0; i < fluidCount; i++ )
            {
                _sortedFluids[i].Volume = _sortedFluidVolumesRemaining[i] * inverseVolumeScale;
            }
        }

        private void RebuildSortedFluidsArray( out double totalVolume )
        {
            // Calculates the volumes and densities *at tank pressure* to match the pouring algorithm and overfill volume scaling.

            if( _sortedFluids == null || _sortedFluids.Length != Contents.Count )
                Array.Resize( ref _sortedFluids, Contents.Count );

            double temperature = FluidState.Temperature;
            double pressure = FluidState.Pressure;
            totalVolume = 0;

            int i = 0;
            foreach( (ISubstance substance, double mass) in Contents )
            {
                double density = substance.GetDensity( temperature, pressure );
                double volume = mass / density;

                _sortedFluids[i].Substance = substance;
                _sortedFluids[i].Mass = mass;
                _sortedFluids[i].Volume = volume;
                _sortedFluids[i].Density = density;

                totalVolume += volume;
                i++;
            }

            Array.Sort( _sortedFluids, 0, i, _fluidComparer );
        }

        private void PopulateDistinctPotentials( List<double> sortedSource, List<double> destination )
        {
            destination.Clear();
            if( sortedSource.Count == 0 )
                return;

            double last = sortedSource[0];
            destination.Add( last );

            int count = sortedSource.Count;
            for( int i = 1; i < count; i++ )
            {
                double val = sortedSource[i];
                if( val - last > TOLERANCE )
                {
                    destination.Add( val );
                    last = val;
                }
            }
        }

        private void EnsureIntervalBuckets( int requiredCount )
        {
            if( _intervalContributions == null || _intervalContributions.Length < requiredCount )
            {
                int oldLen = _intervalContributions?.Length ?? 0;
                Array.Resize( ref _intervalContributions, requiredCount );
                for( int i = oldLen; i < requiredCount; i++ )
                    _intervalContributions[i] = new List<(int, double)>();
            }
            for( int i = 0; i < requiredCount; i++ )
                _intervalContributions[i].Clear();
        }

        private static int FindFirstIntervalIndex( List<double> distinctPotentials, double value )
        {
            int low = 0;
            int high = distinctPotentials.Count - 1;
            int result = ~0;

            while( low <= high )
            {
                int mid = low + ((high - low) >> 1);
                double midVal = distinctPotentials[mid];

                if( midVal == value )
                {
                    result = mid;
                    break;
                }
                if( midVal < value )
                    low = mid + 1;
                else
                    high = mid - 1;
            }
            if( low > high )
                result = ~low;

            if( result < 0 )
                result = ~result - 1;

            return Math.Clamp( result, 0, distinctPotentials.Count - 2 );
        }

        private void ClearEdgeContents()
        {
            if( _contentsInEdges == null || _contentsInEdges.Length != _edges.Length )
            {
                _contentsInEdges = new SubstanceStateCollection[_edges.Length];
                for( int i = 0; i < _contentsInEdges.Length; i++ )
                    _contentsInEdges[i] = new SubstanceStateCollection();
            }
            else
            {
                for( int i = 0; i < _contentsInEdges.Length; i++ )
                    _contentsInEdges[i].Clear();
            }
        }

        private void DistributeUniformly( double totalVolume )
        {
            if( totalVolume <= TOLERANCE )
                return;
            double invTotal = 1.0 / totalVolume;

            foreach( (ISubstance substance, double totalMass) in Contents )
            {
                for( int i = 0; i < _edges.Length; i++ )
                {
                    double fraction = _edges[i].Volume * invTotal;
                    _contentsInEdges[i].Add( substance, totalMass * fraction );
                }
            }
        }
    }
}