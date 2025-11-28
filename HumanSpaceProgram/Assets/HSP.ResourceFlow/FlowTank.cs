using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// An implementation of a fluid tank that uses tetrahedralization to model fluid distribution and potential-based flow.
    /// </summary>
    public sealed class FlowTank : IResourceConsumer, IResourceProducer
    {
        /*
        
        Uses the edges of a tetrahedralization to figure out the volume distribution of an arbitrary shape
        Then, for some specific acceleration+angular velocity, group the volume into buckets (slices) by potential
        Then pour the fluids into the sorted and deduplicated potential buckets (slices)
        This allows easy and very fast lookup of the fluid surface at any point, and checking which fluids can drain from an inlet with some potential.

        */

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

        public ISubstanceStateCollection Contents { get; set; } = SubstanceStateCollection.Empty;
        public ISubstanceStateCollection Inflow { get; set; } = SubstanceStateCollection.Empty;
        public ISubstanceStateCollection Outflow { get; set; } = SubstanceStateCollection.Empty;
        public FluidState FluidState { get; set; }

        public double CalculatedVolume { get; private set; }
        public double Volume { get; private set; }

        public Vector3 FluidAcceleration
        {
            get => _fluidAcceleration;
            set
            {
                if( (_fluidAcceleration - value).sqrMagnitude > 0.05f )
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
                if( (_fluidAngularVelocity - value).sqrMagnitude > 0.05f )
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
        /// Samples the fluid state (Pressure, Temperature, FluidSurfacePotential) at a specific local position.
        /// Calculates hydrostatic pressure based on the potential depth.
        /// </summary>
        public FluidState Sample( Vector3 localPosition, double holeArea )
        {
            RecalculateCache();

            FluidState result = FluidState; // Copy structural defaults (Temp, etc.)
            double pressure = result.Pressure;
            double pointPotential = GetPotentialAt( localPosition );

            // Calculate Fluid Surface Potential
            if( Contents.Count > 0 && _fluidInSlices.Length > 0 )
            {
                result.FluidSurfacePotential = _fluidInSlices[^1].PotentialEnd;
            }
            else
            {
                // If the tank is empty, the "surface" that fluid would flow against is the inlet port itself.
                // This makes the empty tank a sink relative to any tank with a higher fluid surface.
                result.FluidSurfacePotential = pointPotential;
            }

            // Optimization: If point is above the highest fluid, we are in gas.
            if( Contents.Count == 0 || pointPotential >= result.FluidSurfacePotential )
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
                else if( pointPotential < layer.PotentialEnd )
                {
                    // We are inside this layer. Add weight from top of layer to point.
                    pressure += layer.Density * (layer.PotentialEnd - pointPotential);
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
            List<double> bounds = new( n + 1 );
            for( int i = 0; i < n; i++ ) bounds.Add( _fluidInSlices[i].PotentialStart );
            bounds.Add( _fluidInSlices[n - 1].PotentialEnd );

#warning TODO - min max > error.
            double pClamped = Math.Clamp( p, bounds[0], bounds[^1] );

            int idx = FindIntervalIndex( bounds, pClamped );
            if( idx >= 0 && idx < n )
            {
                ref FluidInSlice layer = ref _fluidInSlices[idx];
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
                ref FluidInSlice layer = ref _fluidInSlices[f];
                double fStart = layer.PotentialStart;
                double fEnd = layer.PotentialEnd;
                double density = layer.Density;

                // advance sliceIdx until we reach any overlap
                while( sliceIdx < _slices.Length && _slices[sliceIdx].PotentialTop <= fStart ) sliceIdx++;

                for( int s = sliceIdx; s < _slices.Length; s++ )
                {
                    ref PotentialSlice slice = ref _slices[s];
                    if( slice.PotentialBottom >= fEnd )
                        break;

                    double intersectStart = Math.Max( fStart, slice.PotentialBottom );
                    double intersectEnd = Math.Min( fEnd, slice.PotentialTop );
                    double span = intersectEnd - intersectStart;
                    if( span <= EPSILON_OVERLAP )
                        continue;

                    double sliceSpan = slice.PotentialTop - slice.PotentialBottom;
                    double fillRatio = (sliceSpan > EPSILON_OVERLAP) ? (span / sliceSpan) : 1.0;
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

        private static (int a, int b) GetCanonicalEdgeKeyByIndex( int i1, int i2 )
        {
            if( i1 <= i2 )
                return (i1, i2);
            return (i2, i1);
        }

        private static long PackCanonicalEdgeKey( (int a, int b) keyTuple ) => ((long)keyTuple.a << 32) | (uint)keyTuple.b;
        private static (int a, int b) UnpackCanonicalEdgeKey( long packed )
        {
            int a = (int)(packed >> 32);
            int b = (int)(packed & 0xFFFFFFFF);
            return (a, b);
        }

        private void RecalculateEdgeVolumes()
        {
            if( _tetrahedra == null || _edges == null )
                return;

            // 1. Calculate Tetrahedra Volumes
            double totalDes = 0;
            double[] tetVolumes = new double[_tetrahedra.Length];
            for( int i = 0; i < _tetrahedra.Length; i++ )
            {
                tetVolumes[i] = _tetrahedra[i].GetVolume();
                totalDes += tetVolumes[i];
            }

            // Scale to match actual volume.
            double scale = (totalDes > 0)
                ? Volume / totalDes 
                : 0;
            for( int i = 0; i < tetVolumes.Length; i++ )
                tetVolumes[i] *= scale;

            // 2. Distribute to Edges
            Dictionary<long, double> edgeToVolume = new();

            for( int i = 0; i < _tetrahedra.Length; i++ )
            {
                var t = _tetrahedra[i];
                int[] ni = new[]
                {
                    Array.IndexOf(_nodes, t.v0),
                    Array.IndexOf(_nodes, t.v1),
                    Array.IndexOf(_nodes, t.v2),
                    Array.IndexOf(_nodes, t.v3)
                };

                // Calc lengths
                double totalLen = 0;
                double[] lens = new double[6];
                int k = 0;
                for( int a = 0; a < 4; a++ )
                {
                    for( int b = a + 1; b < 4; b++ )
                    {
                        lens[k] = Vector3.Distance( _nodes[ni[a]].pos, _nodes[ni[b]].pos );
                        totalLen += lens[k++];
                    }
                }

                if( totalLen <= EPSILON_OVERLAP )
                    continue;

                // Distribute
                k = 0;
                for( int a = 0; a < 4; a++ )
                {
                    for( int b = a + 1; b < 4; b++ )
                    {
                        long key = PackCanonicalEdgeKey( GetCanonicalEdgeKeyByIndex( ni[a], ni[b] ) );
                        double vol = (lens[k++] / totalLen) * tetVolumes[i];

                        edgeToVolume.TryGetValue( key, out double existing );
                        edgeToVolume[key] = existing + vol;
                    }
                }
            }

            // 3. Rebuild Array
            List<FlowEdge> newEdges = new( edgeToVolume.Count );
            CalculatedVolume = 0;
            foreach( var kvp in edgeToVolume )
            {
                var (a, b) = UnpackCanonicalEdgeKey( kvp.Key );
                newEdges.Add( new FlowEdge( a, b, kvp.Value ) );
                CalculatedVolume += kvp.Value;
            }
            _edges = newEdges.ToArray();
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

            List<Vector3> allPositions = new();

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

            //Debug.Log( nodes.Count + " : " + edges.Count + " : " + tets.Count );
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

            public void AddVolume( double volume, Vector3 center )
            {
                VolumeCapacity += volume;
                GeometricMoment += center * (float)volume;
            }

            public void FinalizeCentroid()
            {
                if( VolumeCapacity > EPSILON_OVERLAP )
                    Centroid = GeometricMoment / (float)VolumeCapacity;
            }
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

        const double EPSILON_OVERLAP = 1e-9;
        const double EPSILON_DEDUPE_POTENTIALS = 0.1;

        private double[] _nodePotentials; // Cached potentials per node.
        private PotentialSlice[] _slices; // The baked geometry (Null = Dirty)

        private FluidInSlice[] _fluidInSlices; // The calculated fluid stack (Null = Dirty)

        public void InvalidateGeometryAndFluids()
        {
            _slices = null;
            _nodePotentials = null;
            InvalidateFluids(); // If geometry changes, fluid levels definitely change
        }

        public void InvalidateFluids()
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
            // "cache" refers to the data that will change without changing the tank geometry.
            if( _slices == null || force )
            {
                BakePotentialSlices();
                _fluidInSlices = null; // Geometry changed, so fluid distribution doesn't match the volume distribution by potential.
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

            // 1. Calculate and Sort Potentials
            int n = _nodes.Length;
            _nodePotentials = new double[n];
            List<double> distinct = new( n );

            for( int i = 0; i < n; i++ )
            {
                _nodePotentials[i] = GetPotentialAt( _nodes[i].pos );
                distinct.Add( _nodePotentials[i] );
            }

            distinct.Sort();
            // Dedupe
            int write = 0;
            for( int i = 0; i < distinct.Count; i++ )
            {
                if( write == 0 || (distinct[i] - distinct[write - 1] > EPSILON_DEDUPE_POTENTIALS) )
                    distinct[write++] = distinct[i];
            }
            distinct.RemoveRange( write, distinct.Count - write );

            // 2. Init Slices
            int sliceCount = Math.Max( 1, distinct.Count - 1 ); // max 1 => no distinct breakpoints (no potential gradient), single slice.
            _slices = new PotentialSlice[sliceCount];
            for( int i = 0; i < sliceCount; i++ )
            {
                _slices[i].PotentialBottom = distinct[i];
                _slices[i].PotentialTop = distinct[i + 1];
            }

            if( sliceCount == 0 )
                return;

            // 3. Integrate Edges
            for( int ei = 0; ei < _edges.Length; ei++ )
            {
                ref FlowEdge edge = ref _edges[ei];
                double p1 = _nodePotentials[edge.end1];
                double p2 = _nodePotentials[edge.end2];
                Vector3 pos1 = _nodes[edge.end1].pos;
                Vector3 pos2 = _nodes[edge.end2].pos;

                // Ensure p1 < p2
                if( p1 > p2 )
                {
                    (p1, p2) = (p2, p1);
                    (pos1, pos2) = (pos2, pos1);
                }

                double potDiff = p2 - p1;

                // --- PERPENDICULAR EDGE ---
                if( potDiff <= EPSILON_OVERLAP )
                {
                    // Find the exact potential boundary this edge sits on
                    int k = FindClosestIndex( distinct, p1, EPSILON_DEDUPE_POTENTIALS * 1.5 );

                    // If found, split volume between slice below (k-1) and slice above (k)
                    if( k >= 0 )
                    {
                        Vector3 center = (pos1 + pos2) * 0.5f;
                        double halfVol = edge.Volume * 0.5;

                        bool hasBottom = (k - 1 >= 0 && k - 1 < sliceCount);
                        bool hasTop = (k < sliceCount);

                        // If we are at the very floor, put all volume above. If at ceiling, all below.
                        if( !hasBottom )
                            halfVol *= 2;
                        else if( !hasTop )
                            halfVol *= 2;

                        if( hasBottom )
                            _slices[k - 1].AddVolume( halfVol, center );
                        if( hasTop )
                            _slices[k].AddVolume( halfVol, center );
                    }
                    continue;
                }

                // --- STANDARD SLOPED EDGE ---
                double invDiff = 1.0 / potDiff;
                int startIdx = FindIntervalIndex( distinct, p1 ); // Use helper
                if( startIdx < 0 ) startIdx = 0;

                for( int s = startIdx; s < sliceCount; s++ )
                {
                    ref PotentialSlice slice = ref _slices[s];
                    if( slice.PotentialBottom >= p2 )
                        break;
                    if( slice.PotentialTop <= p1 )
                        continue;

                    // Calc Overlap
                    double oMin = Math.Max( p1, slice.PotentialBottom );
                    double oMax = Math.Min( p2, slice.PotentialTop );
                    double overlap = oMax - oMin;

                    if( overlap > EPSILON_OVERLAP )
                    {
                        double frac = overlap * invDiff;
                        double segVol = edge.Volume * frac;

                        // Centroid of the segment
                        float t1 = (float)((oMin - p1) * invDiff);
                        float t2 = (float)((oMax - p1) * invDiff);
                        Vector3 segCenter = Vector3.Lerp( pos1, pos2, (t1 + t2) * 0.5f );

                        slice.AddVolume( segVol, segCenter );
                    }
                }
            }

            // 4. Finalize
            for( int i = 0; i < sliceCount; i++ )
                _slices[i].FinalizeCentroid();
        }

        /// <summary>
        /// Maps the fluids to the potential slices. The "Pouring" step.
        /// </summary>
        private void DistributeFluidsToPotentials()
        {
            // This operation fills the precalculated sliced potential column with fluids according to their volumes and densities.

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

        /// <summary>
        /// Finds the index i such that sortedList[i] is within tolerance of value. 
        /// Returns -1 if no match found.
        /// </summary>
        public static int FindClosestIndex( IList<double> sortedList, double value, double tolerance )
        {
            // Use standard BinarySearch. 
            // If found, returns index. If not, returns bitwise complement of next largest element.
            int idx = (sortedList is List<double> list)
                ? list.BinarySearch( value )
                : -1; // Fallback if not List<T>, strictly not needed if we only use List

            if( idx >= 0 )
                return idx;

            int nextIdx = ~idx;

            // Check neighbors (nextIdx and nextIdx - 1) for tolerance match
            double diffNext = (nextIdx < sortedList.Count)
                ? Math.Abs( sortedList[nextIdx] - value )
                : double.MaxValue;
            double diffPrev = (nextIdx > 0)
                ? Math.Abs( sortedList[nextIdx - 1] - value )
                : double.MaxValue;

            if( diffNext < diffPrev && diffNext <= tolerance )
                return nextIdx;
            if( diffPrev <= diffNext && diffPrev <= tolerance )
                return nextIdx - 1;

            return -1;
        }

        /// <summary>
        /// Finds index i such that list[i] <= val <= list[i+1].
        /// </summary>
        public static int FindIntervalIndex( IList<double> sortedList, double val, double epsilon = EPSILON_OVERLAP )
        {
            int n = sortedList.Count;
            if( n < 2 )
                return -1;

            // Bounds check
            if( val < sortedList[0] - epsilon || val > sortedList[n - 1] + epsilon )
                return -1;

            int lo = 0, hi = n - 2;
            while( lo <= hi )
            {
                int mid = lo + (hi - lo) / 2;
                if( val < sortedList[mid] - epsilon )
                    hi = mid - 1;
                else if( val > sortedList[mid + 1] + epsilon )
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