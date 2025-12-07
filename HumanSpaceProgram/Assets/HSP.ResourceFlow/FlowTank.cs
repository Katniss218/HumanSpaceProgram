using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.ResourceFlow
{
    /// <summary>
    /// An implementation of a fluid tank that uses tetrahedralization to model fluid distribution and potential-based flow.
    /// </summary>
    public sealed class FlowTank : IResourceConsumer, IResourceProducer, IStiffnessProvider
    {
        /*
        
        Uses the edges of a tetrahedralization to figure out the volume distribution of an arbitrary shape
        Then, for some specific acceleration+angular velocity, group the volume into buckets (slices) by potential
        Then pour the fluids into the sorted and deduplicated potential buckets (slices)
        This allows easy and very fast lookup of the fluid surface at any point, and checking which fluids can drain from an inlet with some potential.

        */

        private readonly FlowTankCache _cache;

        internal FlowTetrahedron[] _tetrahedra;
        internal FlowNode[] _nodes;
        internal FlowEdge[] _edges;
        internal Dictionary<FlowNode, double> _inletNodes;

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
        public double Demand { get; set; } = double.PositiveInfinity;

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
            _cache = new FlowTankCache( this );
        }


        public double GetAvailableOutflowVolume()
        {
            if( Contents == null || IsEmpty ) return 0.0;
            // Calculate pressure based on current contents, not stale FluidState property, to get correct gas density.
            double currentPressure = VaporLiquidEquilibrium.ComputePressureOnly( Contents, FluidState, Volume );
            return Contents.GetVolume( FluidState.Temperature, currentPressure );
        }

        public double GetAvailableInflowVolume( double dt )
        {
            double liquidVolume = 0.0;
            if( Contents != null )
            {
                // Use current pressure for density, as it's more accurate than stale state pressure
                double currentPressure = VaporLiquidEquilibrium.ComputePressureOnly( Contents, FluidState, Volume );
                foreach( var (s, m) in Contents )
                {
                    if( s.Phase == SubstancePhase.Liquid || s.Phase == SubstancePhase.Solid )
                    {
                        liquidVolume += m / s.GetDensity( FluidState.Temperature, currentPressure );
                    }
                }
            }

            double capacityVolume = Math.Max( 0, Volume - liquidVolume );
            return capacityVolume;
        }

        /// <summary>
        /// Samples the fluid state. Returns potential in [J/kg].
        /// Potential = Geometric_Potential + (Pressure / Density).
        /// </summary>
        public FluidState Sample( Vector3 localPosition, double holeArea )
        {
            return _cache.Sample( localPosition, holeArea );
        }

        public ISampledSubstanceStateCollection SampleSubstances( Vector3 localPosition, double flowRate, double dt )
        {
            return _cache.SampleSubstances( localPosition, flowRate, dt );
        }

        public Vector3 GetCenterOfMass()
        {
            return _cache.GetCenterOfMass();
        }

        private void SetTetrahedralization( List<FlowNode> nodes, List<FlowEdge> edges, List<FlowTetrahedron> tets )
        {
            _nodes = nodes.ToArray();
            _edges = edges.ToArray();
            _tetrahedra = tets.ToArray();

            RecalculateEdgeVolumes();
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
                    Array.IndexOf( _nodes, t.v0 ),
                    Array.IndexOf( _nodes, t.v1 ),
                    Array.IndexOf( _nodes, t.v2 ),
                    Array.IndexOf( _nodes, t.v3 )
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

                if( totalLen <= FlowTankCache.EPSILON_OVERLAP )
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

            // 3. Rebuild Array with calculated volumes
            FlowEdge[] newEdges = new FlowEdge[_edges.Length];
            CalculatedVolume = 0;

            for( int i = 0; i < _edges.Length; i++ )
            {
                FlowEdge oldEdge = _edges[i];
                long key = PackCanonicalEdgeKey( GetCanonicalEdgeKeyByIndex( oldEdge.end1, oldEdge.end2 ) );

                edgeToVolume.TryGetValue( key, out double volume );

                newEdges[i] = new FlowEdge( oldEdge.end1, oldEdge.end2, volume );
                CalculatedVolume += volume;
            }
            _edges = newEdges;
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


        public void InvalidateGeometryAndFluids() => _cache.InvalidateGeometryAndFluids();
        public void InvalidateFluids() => _cache.InvalidateFluids();
        public void ForceRecalculateCache() => _cache.RecalculateCache( true );

        public void ApplyFlows( double deltaTime )
        {
            if( Outflow != null && !Outflow.IsEmpty() )
                Contents.Add( Outflow, -1.0 );

            if( Inflow != null && !Inflow.IsEmpty() )
                Contents.Add( Inflow, 1.0 );

            // Recalculate pressure based on new contents.
            double newPressure = VaporLiquidEquilibrium.ComputePressureOnly( this.Contents, this.FluidState, this.Volume );
            this.FluidState = new FluidState( newPressure, this.FluidState.Temperature, this.FluidState.Velocity );

            InvalidateFluids();
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
            return _cache.GetPotentialAt( localPosition );
        }

        public void RunInternalSimulationStep( double deltaTime )
        {
            if( Contents.IsEmpty() || FluidState.Pressure <= 0.0 )
            {
                return;
            }
            (ISubstanceStateCollection newContents, FluidState newState) = VaporLiquidEquilibrium.ComputeFlash2( Contents, FluidState, Volume, deltaTime );
            if( newContents != null )
            {
                Contents = newContents;
                FluidState = newState;
                InvalidateFluids();
            }
        }

        public double GetPotentialDerivativeWrtVolume()
        {
            // Per the roadmap and interface remarks, we approximate d(Potential)/dV ≈ dP/dM.
            var (pressure, dPdM) = VaporLiquidEquilibrium.ComputePressureAndDerivativeWrtMass( this.Contents, this.FluidState, this.Volume );
            return dPdM;
        }

        private static (int a, int b) GetCanonicalEdgeKeyByIndex( int i1, int i2 )
        {
            if( i1 <= i2 )
                return (i1, i2);
            return (i2, i1);
        }

        private static long PackCanonicalEdgeKey( (int a, int b) canonicalKeyTuple )
        {
            return ((long)canonicalKeyTuple.a << 32) | (uint)canonicalKeyTuple.b;
        }

        private static (int a, int b) UnpackCanonicalEdgeKey( long packedKey )
        {
            int a = (int)(packedKey >> 32);
            int b = (int)(packedKey & 0xFFFFFFFF);
            return (a, b);
        }
    }
}