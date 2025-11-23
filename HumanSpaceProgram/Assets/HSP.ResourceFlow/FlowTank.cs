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
        private Dictionary<FlowNode, double> _inletNodes;

        private Vector3 _fluidAcceleration = Vector3.zero;
        private Vector3 _fluidAngularVelocity = Vector3.zero;

        public IReadOnlyList<FlowTetrahedron> Tetrahedra => _tetrahedra;
        public IReadOnlyList<FlowNode> Nodes => _nodes;
        public IReadOnlyList<FlowEdge> Edges => _edges;
        public IReadOnlyDictionary<FlowNode, double> InletNodes => _inletNodes;

        public ISubstanceStateCollection Contents { get; set; } = new SubstanceStateCollection();
        public ISubstanceStateCollection Inflow { get; set; }
        public ISubstanceStateCollection Outflow { get; set; }
        public FluidState FluidState { get; set; }

        public double CalculatedVolume { get; private set; }
        public double Volume { get; private set; }

        public Vector3 FluidAcceleration
        {
            get => _fluidAcceleration;
            set
            {
                // Simple equality check to prevent trashing the cache on identical vectors
                if( _fluidAcceleration != value )
                {
                    _fluidAcceleration = value;
                    InvalidateGeometryAndFluids();
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
                    InvalidateGeometryAndFluids();
                }
            }
        }

        public bool IsEmpty => Contents == null || Contents.IsEmpty();

        public FlowTank( double volume )
        {
            this.Volume = volume;
        }


        /// <summary>
        /// Samples the fluid state (Pressure, Temperature) at a specific local position.
        /// Calculates hydrostatic pressure based on the potential depth.
        /// </summary>
        public FluidState Sample( Vector3 localPosition, double holeArea )
        {
            RecalculateCache();

            FluidState result = FluidState; // Copy structural defaults (Temp, etc.)
            double pressure = result.Pressure;
            double pointPotential = GetPotentialAt( localPosition );

            // Optimization: If point is above the highest fluid, we are in gas.
            if( Contents.Count == 0 || pointPotential >= _fluidInSlices[Contents.Count - 1].PotentialEnd )
            {
                return result;
            }

            // Iterate backwards (Lightest -> Heaviest) to accumulate pressure
            // Pressure at point = Sum(Density_i * Height_i)
            for( int i = Contents.Count - 1; i >= 0; i-- )
            {
                ref FluidInSlice layer = ref _fluidInSlices[i];

                if( pointPotential < layer.PotentialStart )
                {
                    // We are strictly below this layer. Add its full weight.
                    pressure += layer.Density * (layer.PotentialEnd - layer.PotentialStart);
                }
                else
                {
                    // We are inside this layer. Add weight from top of layer to point.
                    double effectiveTop = Math.Max( layer.PotentialEnd, pointPotential );
                    pressure += layer.Density * (effectiveTop - pointPotential);
                    break; // We reached the fluid containing the point
                }
            }

            result.Pressure = pressure;
            return result;
        }

        /// <summary>
        /// Determines what substances flow out at a specific position over a specific time.
        /// Returns a read-only collection representing the mass extracted.
        /// </summary>
        public IReadonlySubstanceStateCollection SampleSubstances( Vector3 localPosition, double flowRate, double dt )
        {
            RecalculateCache();

            var result = new SubstanceStateCollection();

            if( Contents == null || Contents.Count == 0 )
                return result;

            double p = GetPotentialAt( localPosition );

            // Build boundary list: [layer0.Start, layer1.Start, ..., layerN.Start, layerN.End]
            int n = _fluidInSlices.Length;
            var bounds = new List<double>( n + 1 );
            for( int i = 0; i < n; i++ ) bounds.Add( _fluidInSlices[i].PotentialStart );
            bounds.Add( _fluidInSlices[n - 1].PotentialEnd );

            int idx = FindIntervalIndex( bounds, p );
            if( idx >= 0 && idx < n )
            {
                ref var layer = ref _fluidInSlices[idx];
                double massExtracted = flowRate * dt * layer.Density;
                result.Add( layer.Substance, massExtracted );
            }

            return result;
        }

        public Vector3 GetCenterOfMass()
        {
            RecalculateCache();

            if( _fluidInSlices == null || _fluidInSlices.Length == 0 )
                return Vector3.zero;

            Vector3 totalMoment = Vector3.zero;
            double totalMass = 0.0;

            int sliceIdx = 0;
            for( int f = 0; f < _fluidInSlices.Length; f++ )
            {
                ref var layer = ref _fluidInSlices[f];
                double fStart = layer.PotentialStart;
                double fEnd = layer.PotentialEnd;
                double density = layer.Density;

                // advance sliceIdx until we reach any overlap
                while( sliceIdx < _slices.Length && _slices[sliceIdx].PotentialTop <= fStart ) sliceIdx++;

                for( int s = sliceIdx; s < _slices.Length; s++ )
                {
                    ref var slice = ref _slices[s];
                    if( slice.PotentialBottom >= fEnd )
                        break;

                    double intersectStart = Math.Max( fStart, slice.PotentialBottom );
                    double intersectEnd = Math.Min( fEnd, slice.PotentialTop );
                    double span = intersectEnd - intersectStart;
                    if( span <= EPS_POT )
                        continue;

                    double sliceSpan = slice.PotentialTop - slice.PotentialBottom;
                    double fillRatio = (sliceSpan > EPS_POT) ? (span / sliceSpan) : 1.0;
                    double volInOverlap = slice.VolumeCapacity * fillRatio;
                    double mass = volInOverlap * density;

                    totalMoment += slice.Centroid * (float)mass;
                    totalMass += mass;
                }
            }

            return (totalMass > 1e-9) ? totalMoment / (float)totalMass : Vector3.zero;
        }

        private void SetTetrahedralization( List<FlowNode> nodes, List<FlowEdge> edges, List<FlowTetrahedron> tets )
        {
            _nodes = nodes.ToArray();
            _edges = edges.ToArray();
            _tetrahedra = tets.ToArray();

            RecalculateEdgeVolumes();
        }

        private static (int, int) GetCanonicalEdgeKey( FlowNode node1, int i1, FlowNode node2, int i2 )
        {
            // Create a canonical ordering for edges (smaller position first)
            Vector3 pos1 = node1.pos;
            Vector3 pos2 = node2.pos;

            if( pos1.x < pos2.x ||
               (pos1.x == pos2.x && pos1.y < pos2.y) ||
               (pos1.x == pos2.x && pos1.y == pos2.y && pos1.z < pos2.z) )
            {
                return (i1, i2);
            }
            return (i2, i1);
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

            Dictionary<FlowNode, int> nodeToIndex = new Dictionary<FlowNode, int>( _nodes.Length );
            for( int i = 0; i < _nodes.Length; i++ )
            {
                nodeToIndex[_nodes[i]] = i;
            }

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
            Dictionary<(int, int), double> edgeVolumes = new();

            for( int i = 0; i < _tetrahedra.Length; i++ )
            {
                FlowTetrahedron tet = _tetrahedra[i];
                FlowNode[] tetNodes = new[] { tet.v0, tet.v1, tet.v2, tet.v3 };
                double tetVolume = actualVolumes[i];

                // Calculate total edge length for this tetrahedron
                double totalEdgeLength = 0.0f;
                List<(int, int, double)> tetEdges = new();

                for( int j = 0; j < 4; j++ )
                {
                    for( int k = j + 1; k < 4; k++ )
                    {
                        FlowNode node1 = tetNodes[j];
                        FlowNode node2 = tetNodes[k];
                        double length = Vector3.Distance( node1.pos, node2.pos );
                        totalEdgeLength += length;

                        (int, int) edgeKey = GetCanonicalEdgeKey( node1, nodeToIndex[node1], node2, nodeToIndex[node2] );
                        tetEdges.Add( (edgeKey.Item1, edgeKey.Item2, length) );
                    }
                }

                // Distribute tetrahedron volume to edges proportionally to edge length
                if( totalEdgeLength > 0 )
                {
                    foreach( (int node1, int node2, double length) in tetEdges )
                    {
                        (int, int) edgeKey = (node1, node2);
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
            List<FlowEdge> edgeGeometries = new();

            foreach( var kvp in edgeVolumes )
            {
                FlowEdge edge = new FlowEdge( kvp.Key.Item1, kvp.Key.Item2, kvp.Value );
                edgeGeometries.Add( edge );
            }

            _edges = edgeGeometries.ToArray();

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
            (List<FlowNode> nodes, List<FlowEdge> edges, List<FlowTetrahedron> tets) = DelaunayTetrahedralizer.ComputeTetrahedralization( allPositions );

            Debug.Log( nodes.Count + " : " + edges.Count + " : " + tets.Count );
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

            // 5) Apply tetrahedralization.
            SetTetrahedralization( nodes, edges, tets );
            // Reset caches.
            InvalidateGeometryAndFluids();
        }

        // --- Cache Data ---

        /// <summary>
        /// Represents a slice of the tank at a specific potential interval.
        /// Acts as the "Container Geometry" cache.
        /// </summary>
        private struct PotentialSlice
        {
            public double PotentialBottom;
            public double PotentialTop;

            /// <summary>
            /// The total geometric volume (m^3) this slice can hold.
            /// </summary>
            public double VolumeCapacity;

            /// <summary>
            /// The Geometric Moment (Volume * Centroid) of the FULL slice.
            /// Used to calculate Center of Mass.
            /// </summary>
            public Vector3 GeometricMoment;

            /// <summary>
            /// The centroid of the full slice (Pre-calculated for interpolation).
            /// </summary>
            public Vector3 Centroid;

            public bool IsEmpty => VolumeCapacity <= 1e-9;
        }

        /// <summary>
        /// Represents a specific fluid occupying a specific range of potential.
        /// Acts as the "Content State".
        /// </summary>
        private struct FluidInSlice
        {
            public ISubstance Substance;
            public double Density;
            public double Volume;

            // The calculated potential boundaries for this specific fluid
            public double PotentialStart;
            public double PotentialEnd;
        }

        const double EPS_POT = 1e-9;
        const double EPS_DUP = 1e-2;

        private double[] _nodePotentials; // Cached potentials per node.
        private PotentialSlice[] _slices; // The baked geometry (Null = Dirty)

        private FluidInSlice[] _fluidInSlices; // The calculated fluid stack (Null = Dirty)

        private void InvalidateGeometryAndFluids()
        {
            _slices = null;
            _nodePotentials = null;
            InvalidateFluids(); // If geometry changes, fluid levels definitely change
        }

        private void InvalidateFluids()
        {
            _fluidInSlices = null;
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
        public double GetPotentialAt( Vector3 localPosition )
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

        public void ForceRecalculateCache()
        {
            RecalculateCache( true );
        }

        private void RecalculateCache( bool force = false )
        {
            // Lazy evaluation pipeline
            if( _slices == null || force )
            {
                BakePotentialSlices();
                _fluidInSlices = null; // Geometry changed, so fluids definitely need restacking
            }

            if( _fluidInSlices == null || force )
            {
                DistributeFluidsToPotentials();
            }
        }

        /// <summary>
        /// Rebuilds the cached potential slices. This is the O(Edges) operation.
        /// </summary>
        private void BakePotentialSlices()
        {
            if( _nodes == null || _edges == null )
                return;

            int n = _nodes.Length;
            _nodePotentials = new double[n];
            List<double> distinct = new( n );

            for( int i = 0; i < n; i++ )
            {
                double p = GetPotentialAt( _nodes[i].pos );
                _nodePotentials[i] = p;
                distinct.Add( p );
            }

            distinct.Sort();
            // dedupe with tolerance
            int write = 1;
            for( int i = 1; i < distinct.Count; i++ )
            {
                if( distinct[i] - distinct[write - 1] > EPS_DUP )
                    distinct[write++] = distinct[i];
            }
            if( write < distinct.Count )
                distinct.RemoveRange( write, distinct.Count - write );

            int sliceCount = Math.Max( 0, distinct.Count - 1 );
            if( sliceCount == 0 )
            {
                _slices = Array.Empty<PotentialSlice>();
                return;
            }

            _slices = new PotentialSlice[sliceCount];
            for( int i = 0; i < sliceCount; i++ )
            {
                _slices[i].PotentialBottom = distinct[i];
                _slices[i].PotentialTop = distinct[i + 1];
                _slices[i].VolumeCapacity = 0.0;
                _slices[i].GeometricMoment = Vector3.zero;
            }

            // integrate edges
            for( int ei = 0; ei < _edges.Length; ei++ )
            {
                ref var edge = ref _edges[ei];
                double p1 = _nodePotentials[edge.end1];
                double p2 = _nodePotentials[edge.end2];
                var pos1 = _nodes[edge.end1].pos;
                var pos2 = _nodes[edge.end2].pos;

                if( p1 > p2 )
                {
                    (p1, p2) = (p2, p1);
                    (pos1, pos2) = (pos2, pos1);
                }

                double potDiff = p2 - p1;
                double edgeVol = edge.Volume;

                if( potDiff <= EPS_POT )
                {
                    int idx = FindIntervalIndex( distinct, p1 );
                    if( idx >= 0 && idx < sliceCount )
                    {
                        var center = (pos1 + pos2) * 0.5f;
                        _slices[idx].VolumeCapacity += edgeVol;
                        _slices[idx].GeometricMoment += center * (float)edgeVol;
                    }
                    continue;
                }

                double invPotDiff = 1.0 / potDiff;
                int startIdx = FindIntervalIndex( distinct, p1 );
                if( startIdx < 0 )
                    startIdx = 0;

                for( int s = startIdx; s < sliceCount; s++ )
                {
                    double sb = _slices[s].PotentialBottom;
                    double st = _slices[s].PotentialTop;
                    if( sb >= p2 )
                        break;
                    if( st <= p1 )
                        continue;

                    double overlapMin = Math.Max( p1, sb );
                    double overlapMax = Math.Min( p2, st );
                    double overlap = overlapMax - overlapMin;
                    if( overlap <= EPS_POT )
                        continue;

                    double frac = overlap * invPotDiff;
                    double segVol = edgeVol * frac;

                    float tmin = (float)((overlapMin - p1) * invPotDiff);
                    float tmax = (float)((overlapMax - p1) * invPotDiff);
                    var segStart = Vector3.Lerp( pos1, pos2, tmin );
                    var segEnd = Vector3.Lerp( pos1, pos2, tmax );
                    var segCenter = (segStart + segEnd) * 0.5f;

                    _slices[s].VolumeCapacity += segVol;
                    _slices[s].GeometricMoment += segCenter * (float)segVol;
                }
            }

            for( int i = 0; i < sliceCount; i++ )
            {
                if( _slices[i].VolumeCapacity > EPS_POT )
                    _slices[i].Centroid = _slices[i].GeometricMoment / (float)_slices[i].VolumeCapacity;
            }
        }

        /// <summary>
        /// Maps the fluids to the potential slices. The "Pouring" step.
        /// </summary>
        private void DistributeFluidsToPotentials()
        {
#warning TODO - overfill/hydraulic lock should distribute according to volumes.
            // Prepare Fluids
            int fluidCount = Contents.Count;
            if( _fluidInSlices == null || _fluidInSlices.Length != fluidCount )
                _fluidInSlices = new FluidInSlice[fluidCount];

            if( fluidCount == 0 )
                return;

            // Sort Fluids by Density (Heaviest First)
            List<(ISubstance sub, double mass, double density)> fluidList = new();
            foreach( (ISubstance substance, double mass) in Contents )
            {
                double density = substance.GetDensity( FluidState.Temperature, FluidState.Pressure );
                fluidList.Add( (substance, mass, density) );
            }
            // Sort Descending Density
            fluidList.Sort( ( a, b ) => b.density.CompareTo( a.density ) );

            // Total Tank Volume Calculation for scaling (Overflow handling)
            double totalTankVolume = 0;
            for( int i = 0; i < _slices.Length; i++ )
                totalTankVolume += _slices[i].VolumeCapacity;

            // Calculate Fluid Volumes
            double totalFluidVolume = 0;
            for( int i = 0; i < fluidCount; i++ )
            {
                double vol = fluidList[i].mass / fluidList[i].density;
                totalFluidVolume += vol;
                _fluidInSlices[i] = new FluidInSlice()
                {
                    Substance = fluidList[i].sub,
                    Density = fluidList[i].density,
                    Volume = vol
                };
            }

            // Scale if overflowing
            double scale = 1.0;
            if( totalFluidVolume > totalTankVolume && totalTankVolume > 0 )
                scale = totalTankVolume / totalFluidVolume;

            // Map Layers to Potentials
            double currentVolumeConsumed = 0;

            // Helper to find Potential for a specific volume accumulation
            // We keep state between calls to optimize (slices are visited sequentially)
            int sliceWalkerIdx = 0;
            double volumeInPreviousSlices = 0;

            for( int i = 0; i < fluidCount; i++ )
            {
                ref FluidInSlice layer = ref _fluidInSlices[i];
                layer.Volume *= scale; // Apply compression/overflow scale

                // Start Potential is where the previous fluid ended
                layer.PotentialStart = (i == 0)
                    ? _slices[0].PotentialBottom
                    : _fluidInSlices[i - 1].PotentialEnd;

                double volumeTarget = currentVolumeConsumed + layer.Volume;

                // Find the potential corresponding to volumeTarget
                while( sliceWalkerIdx < _slices.Length )
                {
                    double sliceCap = _slices[sliceWalkerIdx].VolumeCapacity;
                    double totalAtSliceTop = volumeInPreviousSlices + sliceCap;

                    if( volumeTarget <= totalAtSliceTop )
                    {
                        // The end is inside this slice
                        // Linear Interpolation within the slice
                        double volInSlice = volumeTarget - volumeInPreviousSlices;
                        double fraction = (sliceCap > 1e-9) ? (volInSlice / sliceCap) : 1.0;

                        // Linear assumption: Potential scales linearly with volume in the slice
                        layer.PotentialEnd = Lerp( _slices[sliceWalkerIdx].PotentialBottom, _slices[sliceWalkerIdx].PotentialTop, fraction );
                        break;
                    }
                    else
                    {
                        // Move to next slice
                        volumeInPreviousSlices += sliceCap;
                        sliceWalkerIdx++;
                    }
                }

                // Clamp if we ran out of tank
                if( sliceWalkerIdx >= _slices.Length )
                {
                    layer.PotentialEnd = _slices[^1].PotentialTop;
                }

                currentVolumeConsumed += layer.Volume;
            }
        }

        // Find index i such that arr[i] <= val <= arr[i+1], returns -1 if out of range.
        public static int FindIntervalIndex( IList<double> sortedDistinct, double val )
        {
            int n = sortedDistinct.Count;
            if( n < 2 )
                return -1;
            int lo = 0, hi = n - 2;
            while( lo <= hi )
            {
                int mid = lo + (hi - lo) / 2;
                double a = sortedDistinct[mid], b = sortedDistinct[mid + 1];
                if( val + EPS_POT < a )
                    hi = mid - 1;
                else if( val - EPS_POT > b )
                    lo = mid + 1;
                else return mid;
            }
            return -1;
        }

        private static double Lerp( double a, double b, double t )
        {
            return a + (b - a) * t;
        }
    }
}