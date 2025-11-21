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
        public double Volume { get; private set; }

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

            // Restore old contents and redistribute (the caller may prefer different behavior; this preserves fluid as-is).
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
        public double GetPotential( Vector3 localPoint )
        {
            double linearContrib = 0.0;
            double rotationalContrib = 0.0;

            if( FluidAcceleration.sqrMagnitude > 0.001 )
            {
                linearContrib = -Vector3.Dot( FluidAcceleration, localPoint );
            }
            if( FluidAngularVelocity.sqrMagnitude > 0.001 )
            {
                Vector3 tang = Vector3.Cross( FluidAngularVelocity, localPoint );
                rotationalContrib = -0.5 * tang.sqrMagnitude;
            }

            return linearContrib + rotationalContrib;
        }

        public struct FluidEntry
        {
            public ISubstance Substance;
            public double Mass;
            public double VolumeAtReferencePressure;
            public double Density;
        }

        // Cached buffers (reused to minimize allocations on a semi-hot path)
        private double[] _nodePotential;
        private double[] _edgePotentialMin;
        private double[] _edgePotentialMax;

        private readonly List<double> _breakpointBuffer = new();
        private FluidEntry[] _sortedFluids;

        private List<(int edgeIndex, double contribution)>[] _intervalContributions;

        private const double Tolerance = 1e-9;

        public void DistributeContents()
        {
            if( _edges is not { Length: > 0 } ) 
                return;
            if( Contents is null || Contents.IsEmpty() ) 
                return;

            int nodeCount = _nodes.Length;
            int edgeCount = _edges.Length;

            InitializeBuffers( nodeCount, edgeCount );
            ClearEdgeContents();

            // 1. Compute gravitational/pressure potential for every node
            for( int i = 0; i < nodeCount; i++ )
                _nodePotential[i] = GetPotential( _nodes[i].pos );

            // 2. Determine potential range for each edge and collect breakpoints
            _breakpointBuffer.Clear();
            double totalSystemVolume = 0;

            for( int i = 0; i < edgeCount; i++ )
            {
                var edge = _edges[i];
                double potentialA = GetPotential( edge.end1.pos );
                double potentialB = GetPotential( edge.end2.pos );

                double min = Math.Min( potentialA, potentialB );
                double max = Math.Max( potentialA, potentialB );

                _edgePotentialMin[i] = min;
                _edgePotentialMax[i] = max;

                totalSystemVolume += edge.Volume;

                _breakpointBuffer.Add( min );
                _breakpointBuffer.Add( max );
            }

            _breakpointBuffer.Sort();
            double[] distinctPotentials = GetDistinctSortedValues( _breakpointBuffer );
            int intervalCount = distinctPotentials.Length - 1;

            // Special case: no potential gradient → uniform distribution
            if( intervalCount == 0 )
            {
                DistributeUniformly( totalSystemVolume );
                return;
            }

            // 3. Map edges to potential intervals (bucket per interval)
            EnsureIntervalBuckets( intervalCount );
            double[] intervalAvailableVolume = new double[intervalCount];

            for( int edgeIdx = 0; edgeIdx < edgeCount; edgeIdx++ )
            {
                double low = _edgePotentialMin[edgeIdx];
                double high = _edgePotentialMax[edgeIdx];
                double edgeVolume = _edges[edgeIdx].Volume;

                if( high - low > Tolerance ) // Gradient edge
                {
                    double volumePerPotential = edgeVolume / (high - low);

                    int startIdx = FindFirstIntervalIndex( distinctPotentials, low );

                    for( int intervalIdx = startIdx; intervalIdx < intervalCount; intervalIdx++ )
                    {
                        double intervalLow = distinctPotentials[intervalIdx];
                        double intervalHigh = distinctPotentials[intervalIdx + 1];

                        if( intervalLow >= high - Tolerance )
                            break;

                        double overlapLow = Math.Max( low, intervalLow );
                        double overlapHigh = Math.Min( high, intervalHigh );
                        double overlap = overlapHigh - overlapLow;

                        if( overlap > Tolerance )
                        {
                            double contribution = overlap * volumePerPotential;
                            _intervalContributions[intervalIdx].Add( (edgeIdx, contribution) );
                            intervalAvailableVolume[intervalIdx] += contribution;
                        }
                    }
                }
                else // Equipotential edge
                {
                    int intervalIdx = FindIntervalIndexForValue( distinctPotentials, low );
                    _intervalContributions[intervalIdx].Add( (edgeIdx, edgeVolume) );
                    intervalAvailableVolume[intervalIdx] += edgeVolume;
                }
            }

            // 4. Sort fluids by density (heaviest first)
            BuildSortedFluidList();

            // 5. Fill intervals from bottom to top
            FillIntervals( intervalCount, intervalAvailableVolume );

            // 6. Any leftover fluid is distributed uniformly (overflow / incompressible case)
#warning TODO - this is wrong, needs to be distributed from the start, not after, because then it is no longer stratified correctly.
            DistributeRemainingFluidUniformly( totalSystemVolume );
        }

        private void FillIntervals( int intervalCount, double[] intervalCapacity )
        {
            int fluidCount = _sortedFluids.Length;
            double[] remainingVolume = new double[fluidCount];
            for( int i = 0; i < fluidCount; i++ )
                remainingVolume[i] = _sortedFluids[i].VolumeAtReferencePressure;

            for( int intervalIdx = 0; intervalIdx < intervalCount; intervalIdx++ )
            {
                double capacity = intervalCapacity[intervalIdx];
                if( capacity <= Tolerance )
                    continue;

                double initialCapacity = capacity;

                foreach( int fluidIdx in Enumerable.Range( 0, fluidCount ) )
                {
                    if( remainingVolume[fluidIdx] <= Tolerance ) 
                        continue;

                    double pourVolume = Math.Min( remainingVolume[fluidIdx], capacity );

                    double fillRatio = pourVolume / initialCapacity;

                    var contributions = _intervalContributions[intervalIdx];
                    foreach( (int edgeIdx, double contribVolume) in contributions )
                    {
                        double volumeToAdd = contribVolume * fillRatio;
                        double massToAdd = volumeToAdd * _sortedFluids[fluidIdx].Density;

                        _contentsInEdges[edgeIdx].Add( _sortedFluids[fluidIdx].Substance, massToAdd );
                    }

                    remainingVolume[fluidIdx] -= pourVolume;
                    capacity -= pourVolume;

                    if( capacity <= Tolerance )
                        break; // interval is full
                }
            }

            // Update buffer with leftover volumes for overflow handling
            for( int i = 0; i < fluidCount; i++ )
            {
                _sortedFluids[i].VolumeAtReferencePressure = remainingVolume[i];
            }
        }

        private void BuildSortedFluidList()
        {
            if( _sortedFluids == null || _sortedFluids.Length != Contents.Count )
            {
                Array.Resize( ref _sortedFluids, Contents.Count );
            }
            Array.Clear( _sortedFluids, 0, _sortedFluids.Length );
            double temperature = FluidState.Temperature;
            double pressure = FluidState.Pressure;

            int i = 0;
            foreach( (ISubstance substance, double mass) in Contents )
            {
                double density = substance.GetDensity( temperature, pressure );
                double volume = mass / density;

                _sortedFluids[i] = new FluidEntry()
                {
                    Substance = substance,
                    Mass = mass,
                    VolumeAtReferencePressure = volume,
                    Density = density
                };
                i++;
            }

            Array.Sort( _sortedFluids, ( a, b ) => b.Density.CompareTo( a.Density ) ); // heaviest first
        }

        private void DistributeRemainingFluidUniformly( double totalVolume )
        {
            if( totalVolume <= Tolerance ) 
                return;

            foreach( var fluid in _sortedFluids )
            {
                if( fluid.VolumeAtReferencePressure <= Tolerance )
                    continue;

                double remainingMass = fluid.VolumeAtReferencePressure * fluid.Density;
                double massPerVolumeUnit = remainingMass / totalVolume;

                foreach( (int edgeIdx, var edge) in _edges.Select( ( e, i ) => (i, e) ) )
                {
                    double massToAdd = edge.Volume * massPerVolumeUnit;
                    _contentsInEdges[edgeIdx].Add( fluid.Substance, massToAdd );
                }
            }
        }

        private void InitializeBuffers( int nodeCount, int edgeCount )
        {
            _nodePotential ??= new double[nodeCount];
            if( _nodePotential.Length != nodeCount ) _nodePotential = new double[nodeCount];

            _edgePotentialMin ??= new double[edgeCount];
            _edgePotentialMax ??= new double[edgeCount];
            if( _edgePotentialMin.Length != edgeCount )
            {
                _edgePotentialMin = new double[edgeCount];
                _edgePotentialMax = new double[edgeCount];
            }

            _intervalContributions ??= Array.Empty<List<(int, double)>>();
        }

        private void ClearEdgeContents()
        {
            if( _contentsInEdges is null || _contentsInEdges.Length != _edges.Length )
            {
                _contentsInEdges = new SubstanceStateCollection[_edges.Length];
                for( int i = 0; i < _contentsInEdges.Length; i++ )
                    _contentsInEdges[i] = new SubstanceStateCollection();
            }
            else
            {
                foreach( var collection in _contentsInEdges )
                    collection.Clear();
            }
        }

        private double[] GetDistinctSortedValues( List<double> sortedList )
        {
            if( sortedList.Count == 0 ) 
                return Array.Empty<double>();

            var result = new List<double> { sortedList[0] };
            foreach( double value in sortedList )
                if( value - result[^1] > Tolerance )
                    result.Add( value );

            return result.ToArray();
        }

        private void EnsureIntervalBuckets( int requiredCount )
        {
            if( _intervalContributions.Length < requiredCount )
                Array.Resize( ref _intervalContributions, requiredCount );

            for( int i = 0; i < requiredCount; i++ )
            {
                if( _intervalContributions[i] is null )
                    _intervalContributions[i] = new List<(int, double)>();
                else
                    _intervalContributions[i].Clear();
            }
        }

        private static int FindFirstIntervalIndex( double[] sortedPotentials, double value )
        {
            int idx = Array.BinarySearch( sortedPotentials, value );
            if( idx < 0 )
                idx = ~idx - 1;
            return Math.Clamp( idx, 0, sortedPotentials.Length - 2 );
        }

        private static int FindIntervalIndexForValue( double[] sortedPotentials, double value )
        {
            int idx = Array.BinarySearch( sortedPotentials, value );
            if( idx < 0 )
                idx = ~idx - 1;
            return Math.Clamp( idx, 0, sortedPotentials.Length - 2 );
        }

        private void DistributeUniformly( double totalVolume )
        {
            if( totalVolume <= Tolerance ) 
                return;

            foreach( (ISubstance substance, double totalMass) in Contents )
            {
                foreach( (int edgeIdx, var edge) in _edges.Select( ( e, i ) => (i, e) ) )
                {
                    double fraction = edge.Volume / totalVolume;
                    _contentsInEdges[edgeIdx].Add( substance, totalMass * fraction );
                }
            }
        }
    }
}