using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HSP.ResourceFlow
{
    public class FResourceContainer_FlowTank : IResourceContainer, IResourceProducer, IResourceConsumer
    {
        public Vector3 triangulationPositions; // initial pos for triangulation.

        public FlowTank tank;



        public float MaxVolume => throw new NotImplementedException();

        public SubstanceStateCollection Contents => throw new NotImplementedException();

        public float Mass => throw new NotImplementedException();

        public UnityEngine.Transform transform => throw new NotImplementedException();

        public UnityEngine.GameObject gameObject => throw new NotImplementedException();

        public SubstanceStateCollection Outflow => throw new NotImplementedException();

        public SubstanceStateCollection Inflow => throw new NotImplementedException();

        public event IHasMass.MassChange OnAfterMassChanged;

        public void ClampIn( SubstanceStateCollection inflow, float dt )
        {
            throw new NotImplementedException();
        }

        public FluidState Sample( UnityEngine.Vector3 localPosition, UnityEngine.Vector3 localAcceleration, float holeArea )
        {
            throw new NotImplementedException();
        }

        public (SubstanceStateCollection, FluidState) SampleFlow( UnityEngine.Vector3 localPosition, UnityEngine.Vector3 localAcceleration, float holeArea, float dt, FluidState opposingFluid )
        {
            throw new NotImplementedException();
        }
    }

    public sealed class FlowInlet
    {
        public float nominalArea; // m^2
        public FlowNode node;

        /// <summary>
        /// The producer connected to this inlet (if any). Can be null if inlet is only a consumer.
        /// </summary>
        public IResourceProducer Producer { get; private set; }

        /// <summary>
        /// The consumer connected to this inlet (if any). Can be null if inlet is only a producer.
        /// </summary>
        public IResourceConsumer Consumer { get; private set; }

        public Vector3 LocalPosition => node.pos;

        /// <summary>
        /// Cached flow rate for this inlet, in [m^3/s]. Updated by background thread.
        /// </summary>
        private float _cachedFlowRate;
        private readonly object _flowRateLock = new object();

        /// <summary>
        /// Gets the current flow rate for this inlet, in [m^3/s]. Thread-safe.
        /// </summary>
        public float GetCurrentFlowRate()
        {
            lock( _flowRateLock )
            {
                return _cachedFlowRate;
            }
        }

        /// <summary>
        /// Sets the current flow rate for this inlet. Called by background thread.
        /// </summary>
        internal void SetFlowRate( float flowRate )
        {
            lock( _flowRateLock )
            {
                _cachedFlowRate = flowRate;
            }
        }

        /// <summary>
        /// Connects this inlet to a producer and consumer (e.g., a tank that can both produce and consume).
        /// </summary>
        public void ConnectTo<T>( T obj ) where T : IResourceProducer, IResourceConsumer
        {
            Producer = obj;
            Consumer = obj;
        }

        /// <summary>
        /// Connects this inlet to a consumer (e.g., an engine).
        /// </summary>
        public void ConnectTo( IResourceConsumer consumer )
        {
            Consumer = consumer;
            Producer = consumer as IResourceProducer;
        }

        /// <summary>
        /// Connects this inlet to a producer (e.g., a tank).
        /// </summary>
        public void ConnectTo( IResourceProducer producer )
        {
            Producer = producer;
            Consumer = producer as IResourceConsumer;
        }

        /// <summary>
        /// Creates a standalone inlet (not attached to a FlowTank node).
        /// </summary>
        public static FlowInlet CreateStandalone( Vector3 localPosition, float area )
        {
            return new FlowInlet
            {
                nominalArea = area,
                node = new FlowNode( localPosition )
            };
        }
    }

    public sealed class FlowTank
    {
        private FlowTetrahedron[] _tetrahedra;
        private FlowNode[] _nodes;
        private FlowEdge[] _edges;
        private SubstanceStateCollection[] _contentsInEdges;

        private Dictionary<FlowNode, FlowInlet> _inletNodes; // inlets and outlets (ports/holes in the tank). If nothing is attached, the inlet is treated as a hole.

        private SubstanceStateCollection _contents;
        private SubstanceStateCollection _inflow;
        private SubstanceStateCollection _outflow;
        private Vector3 _acceleration; // in tank-space, acceleration of tank relative to fluid.
#warning TODO - mark whether node was an inlet or not (snapping) and don't remove it entirely if it wasn't.

        private float _calculatedVolume; // volume calculated from tetrahedra
        private float _volume;

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
                    actualVolumes[i] = (desiredVolumes[i] / totalDesiredVolume) * _volume;
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
            _calculatedVolume = edgeVolumes.Values.Sum();
        }

        private FlowNode[] AddNodes( params Vector3[] localPositions )
        {
            // todo - add nodes and triangulate them. redistribute volume and settle already existing fluid.
            if( _nodes == null )
            {
                _nodes = new FlowNode[0];
            }

            List<FlowNode> newNodes = new List<FlowNode>( _nodes );
            List<FlowNode> addedNodes = new List<FlowNode>();

            foreach( var pos in localPositions )
            {
                FlowNode newNode = new FlowNode( pos );
                newNodes.Add( newNode );
                addedNodes.Add( newNode );
            }

            // Re-triangulate with new nodes
            List<Vector3> allPositions = newNodes.Select( n => n.pos ).ToList();
            var (nodes, edges, tets) = DelaunayTetrahedralizer.ComputeDelaunayTetrahedralization( allPositions );

            // Preserve existing fluid contents before redistributing
            SubstanceStateCollection oldContents = _contents?.Clone();

            SetTetrahedralization( nodes, edges, tets );

            // Redistribute existing fluid
            if( oldContents != null && !oldContents.IsEmpty() )
            {
                _contents = oldContents;
                DistributeFluids();
            }

            return addedNodes.ToArray();
        }

        private void RemoveNodes( params FlowNode[] nodesToRemove )
        {
            if( _nodes == null || nodesToRemove == null || nodesToRemove.Length == 0 )
                return;

            HashSet<FlowNode> toRemove = new HashSet<FlowNode>( nodesToRemove );
            List<FlowNode> remainingNodes = _nodes.Where( n => !toRemove.Contains( n ) ).ToList();

            if( remainingNodes.Count < 4 )
            {
                // Need at least 4 nodes for tetrahedralization
                return;
            }

            // Remove inlets associated with removed nodes
            if( _inletNodes != null )
            {
                var keysToRemove = _inletNodes.Keys.Where( n => toRemove.Contains( n ) ).ToList();
                foreach( var key in keysToRemove )
                {
                    _inletNodes.Remove( key );
                }
            }

            // Re-triangulate with remaining nodes
            List<Vector3> remainingPositions = remainingNodes.Select( n => n.pos ).ToList();
            var (nodes, edges, tets) = DelaunayTetrahedralizer.ComputeDelaunayTetrahedralization( remainingPositions );

            // Preserve existing fluid contents
            SubstanceStateCollection oldContents = _contents?.Clone();

            SetTetrahedralization( nodes, edges, tets );

            // Redistribute existing fluid
            if( oldContents != null && !oldContents.IsEmpty() )
            {
                _contents = oldContents;
                DistributeFluids();
            }
        }

        private FlowInlet[] AddInlets( params (Vector3 localPosition, float area)[] inlets )
        {
            // replace/move existing nodes if close enough, unless node is already an inlet.

            if( _inletNodes == null )
            {
                _inletNodes = new Dictionary<FlowNode, FlowInlet>();
            }

            const float SNAP_THRESHOLD = 0.1f; // meters
            List<FlowInlet> addedInlets = new List<FlowInlet>();

            foreach( var (localPosition, area) in inlets )
            {
                // Find closest existing node
                FlowNode closestNode = null;
                float closestDistance = float.MaxValue;

                if( _nodes != null )
                {
                    foreach( var node in _nodes )
                    {
                        float dist = Vector3.Distance( node.pos, localPosition );
                        if( dist < closestDistance )
                        {
                            closestDistance = dist;
                            closestNode = node;
                        }
                    }
                }

                FlowNode inletNode;
                if( closestNode != null && closestDistance < SNAP_THRESHOLD && !_inletNodes.ContainsKey( closestNode ) )
                {
                    // Snap to existing node
                    inletNode = closestNode;
                }
                else
                {
                    // Create new node and add to tetrahedralization
                    FlowNode[] newNodes = AddNodes( localPosition );
                    if( newNodes.Length > 0 )
                    {
                        inletNode = newNodes[0];
                    }
                    else
                    {
                        continue; // Failed to add node
                    }
                }

                // Create inlet
                FlowInlet inlet = new FlowInlet()
                {
                    nominalArea = area,
                    node = inletNode
                };

                _inletNodes[inletNode] = inlet;
                addedInlets.Add( inlet );
            }

            return addedInlets.ToArray();
        }

        private void RemoveInlets( params FlowInlet[] inlets )
        {
            if( _inletNodes == null || inlets == null || inlets.Length == 0 )
                return;

            HashSet<FlowInlet> toRemove = new( inlets );
            List<FlowNode> keysToRemove = _inletNodes.Where( kvp => toRemove.Contains( kvp.Value ) ).Select( kvp => kvp.Key ).ToList();

            foreach( var key in keysToRemove )
            {
                _inletNodes.Remove( key );
            }
        }

        private void DistributeFluids()
        {
            // distributes the fluids by density, based on height intervals of the edges projected onto the acceleration vector.

            /*
            Project every edge onto the height axis (scalar heights).

            Build the set of sorted unique breakpoints formed by all endpoints' heights. These define height intervals (bins).

            For each interval [Hj,Hj+1) compute how much capacity (volume) lives in that slice across the whole graph (sum of contributions from each edge that intersects that interval).

            Sort fluids by density descending (heaviest first). Starting from the lowest interval (global minimum height) pour each fluid into the available capacity bottom-up until its volume is exhausted, distributing its volume into edges proportionally to how much capacity they contributed to each interval.

            Result: for every edge, you have exact volumes per fluid. Optionally compute per-edge layer heights inside an edge.
            */
            const float HEIGHT_EPS = 1e-6f;
            // Quick null/empty guard
            if( _edges == null || _edges.Length == 0 )
            {
                // Nothing to fill, clear contents
                if( _contentsInEdges != null )
                {
                    for( int i = 0; i < _contentsInEdges.Length; i++ )
                        _contentsInEdges[i] = new SubstanceStateCollection();
                }
                return;
            }

            // Ensure accelDir (unit) defined
            Vector3 accelDir = _acceleration.magnitude > 1e-6f ? _acceleration.normalized : Vector3.down;

            int edgeCount = _edges.Length;

            // --- Build node mapping & union-find for connectivity ---
            // We'll map endpoint object references to integer node ids.
            // This assumes end1/end2 are reference types or stable keys.
            Dictionary<object, int> nodeIndexMap = new ( edgeCount * 2 );
            int nodeCounter = 0;

            for( int i = 0; i < edgeCount; i++ )
            {
                FlowEdge e = _edges[i];
                object n1 = e.end1;
                object n2 = e.end2;
                if( !nodeIndexMap.ContainsKey( n1 ) )
                    nodeIndexMap[n1] = nodeCounter++;
                if( !nodeIndexMap.ContainsKey( n2 ) )
                    nodeIndexMap[n2] = nodeCounter++;
            }

            UnionFind uf = new UnionFind( nodeCounter );
            for( int i = 0; i < edgeCount; i++ )
            {
                FlowEdge e = _edges[i];
                int a = nodeIndexMap[e.end1];
                int b = nodeIndexMap[e.end2];
                uf.Union( a, b );
            }

            // Group edges by component root id
            Dictionary<int, List<int>> compToEdges = new();
            for( int i = 0; i < edgeCount; i++ )
            {
                FlowEdge e = _edges[i];
                int a = nodeIndexMap[e.end1];
                int root = uf.Find( a );
                if( !compToEdges.TryGetValue( root, out List<int> list ) )
                {
                    list = new List<int>();
                    compToEdges[root] = list;
                }
                list.Add( i );
            }

            // Prepare output containers: clear all edges
            for( int i = 0; i < _contentsInEdges.Length; i++ )
                _contentsInEdges[i] = new SubstanceStateCollection();

            // Decide whether per-edge contents are present and should be used as source inventory
            bool hasPerEdgeSource = false;
            for( int i = 0; i < _contentsInEdges.Length && !hasPerEdgeSource; i++ )
            {
                if( _contentsInEdges[i] != null && !_contentsInEdges[i].IsEmpty() )
                {
                    hasPerEdgeSource = true;
                }
            }

            // For each component, compute inventory and do the pouring independently
            foreach( var kv in compToEdges )
            {
                List<int> compEdges = kv.Value;
                if( compEdges.Count == 0 ) 
                    continue;

                // --- Build height breakpoints from endpoints of edges in this component ---
                List<float> breakpoints = new List<float>( compEdges.Count * 2 );
                // Also cache per-edge projected data to avoid recomputing
                float[] edgeHMin = new float[edgeCount];
                float[] edgeHMax = new float[edgeCount];
                float[] edgeProjectedLen = new float[edgeCount];

                foreach( int ei in compEdges )
                {
                    FlowEdge edge = _edges[ei];
                    float h1 = Vector3.Dot( edge.end1.pos, accelDir );
                    float h2 = Vector3.Dot( edge.end2.pos, accelDir );
                    float hmin = Mathf.Min( h1, h2 );
                    float hmax = Mathf.Max( h1, h2 );
                    edgeHMin[ei] = hmin;
                    edgeHMax[ei] = hmax;
                    edgeProjectedLen[ei] = hmax - hmin;
                    breakpoints.Add( hmin );
                    breakpoints.Add( hmax );
                }

                // Sort & unique breakpoints with epsilon merging
                breakpoints.Sort();
                List<float> uniqueHeights = new List<float>( breakpoints.Count );
                for( int i = 0; i < breakpoints.Count; i++ )
                {
                    if( i == 0 || Math.Abs( breakpoints[i] - breakpoints[i - 1] ) > HEIGHT_EPS )
                        uniqueHeights.Add( breakpoints[i] );
                }

                // If there is < 2 unique heights, that means all edges project to the same height:
                // we treat the component as a single-level bucket: distribute by volume proportionally (no vertical ordering).
                if( uniqueHeights.Count < 2 )
                {
                    // Compute component total capacity and per-edge share
                    float compCapacity = 0f;
                    foreach( int ei in compEdges ) compCapacity += _edges[ei].Volume;

                    if( compCapacity <= 0f ) 
                        continue;

                    // Build component inventory (per-substance mass) from source
                    Dictionary<Substance, float> compInventory = new (); // Substance -> mass
                    if( hasPerEdgeSource )
                    {
                        foreach( int ei in compEdges )
                        {
                            SubstanceStateCollection coll = _contentsInEdges[ei];
                            if( coll == null || coll.IsEmpty() ) 
                                continue;
                            for( int s = 0; s < coll.SubstanceCount; s++ )
                            {
                                SubstanceState st = coll[s];
                                compInventory.TryGetValue( st.Substance, out float m0 );
                                compInventory[st.Substance] = m0 + st.MassAmount;
                            }
                        }
                    }
                    else
                    {
                        // fallback: use global _contents as all inventory (split by density/mass)
                        if( _contents != null && !_contents.IsEmpty() )
                        {
                            for( int s = 0; s < _contents.SubstanceCount; s++ )
                            {
                                SubstanceState st = _contents[s];
                                compInventory.TryGetValue( st.Substance, out float m0 );
                                compInventory[st.Substance] = m0 + st.MassAmount;
                            }
                        }
                    }

                    // Pour within the single-level bucket: heavier fluids go into bottom (no vertical difference),
                    // but since there is no vertical axis, we just distribute each substance mass proportionally to edge volume.
                    foreach( var kv2 in compInventory )
                    {
                        Substance substance = kv2.Key;
                        float mass = kv2.Value;
                        // For each edge, add mass * (edge.Volume / compCapacity)
                        foreach( int ei in compEdges )
                        {
                            float share = _edges[ei].Volume / compCapacity;
                            float addMass = mass * share;
                            // add to _contentsInEdges[ei]
                            AddMassToEdgeSubstance( ei, addMass, substance );
                        }
                    }

                    // done with this component
                    continue;
                }

                // Build intervals: [U0,U1), [U1,U2), ...
                List<(float minH, float maxH, Dictionary<int, float> edgeContrib)> intervals = new ( uniqueHeights.Count - 1 );
                for( int bi = 0; bi < uniqueHeights.Count - 1; bi++ )
                {
                    float ihMin = uniqueHeights[bi];
                    float ihMax = uniqueHeights[bi + 1];
                    Dictionary<int, float> contributions = new ();
                    // For each edge in component, compute contribution in this interval
                    foreach( int ei in compEdges )
                    {
                        float eMin = edgeHMin[ei];
                        float eMax = edgeHMax[ei];

                        // intersection logic: overlap in projected coordinate
                        float overlapMin = Math.Max( ihMin, eMin );
                        float overlapMax = Math.Min( ihMax, eMax );
                        float overlapLen = overlapMax - overlapMin;

                        if( overlapLen > HEIGHT_EPS )
                        {
                            float eLen = edgeProjectedLen[ei];
                            if( eLen > HEIGHT_EPS )
                            {
                                float contribution = (_edges[ei].Volume) * (overlapLen / eLen);
                                if( contribution > 0f )
                                    contributions[ei] = contribution;
                            }
                            else
                            {
                                // practically zero-length but overlap length > 0 (shouldn't happen): treat as full volume
                                float contribution = _edges[ei].Volume;
                                contributions[ei] = contribution;
                            }
                        }
                    }
                    if( contributions.Count > 0 )
                        intervals.Add( (ihMin, ihMax, contributions) );
                }

                // Handle zero-length edges (edges with projected length == 0).
                // They might not have been picked up by the above loop (overlapLen > eps). We must assign their volume to a sensible interval.
                foreach( int ei in compEdges )
                {
                    if( edgeProjectedLen[ei] > HEIGHT_EPS ) 
                        continue; // already handled
                    float pH = edgeHMin[ei]; // point height
                                             // find interval index k such that U_k <= pH < U_{k+1}
                    int k = -1;
                    for( int bi = 0; bi < uniqueHeights.Count - 1; bi++ )
                    {
                        float ihMin = uniqueHeights[bi];
                        float ihMax = uniqueHeights[bi + 1];
                        if( pH + HEIGHT_EPS >= ihMin && pH - HEIGHT_EPS < ihMax )
                        {
                            k = bi;
                            break;
                        }
                    }
                    if( k == -1 )
                    {
                        // If pH >= last height, attach to last interval; if pH < first, attach to first
                        if( pH <= uniqueHeights[0] + HEIGHT_EPS ) k = 0;
                        else k = uniqueHeights.Count - 2;
                    }

                    // add contribution to interval k (create if missing)
                    // find interval item with minH == uniqueHeights[k] and maxH == uniqueHeights[k+1]
                    int foundIdx = -1;
                    for( int ii = 0; ii < intervals.Count; ii++ )
                    {
                        if( Math.Abs( intervals[ii].minH - uniqueHeights[k] ) < HEIGHT_EPS &&
                            Math.Abs( intervals[ii].maxH - uniqueHeights[k + 1] ) < HEIGHT_EPS )
                        {
                            foundIdx = ii;
                            break;
                        }
                    }
                    if( foundIdx >= 0 )
                    {
                        var dict = intervals[foundIdx].edgeContrib;
                        if( dict.TryGetValue( ei, out float prev ) ) dict[ei] = prev + _edges[ei].Volume;
                        else dict[ei] = _edges[ei].Volume;
                    }
                    else
                    {
                        var dict = new Dictionary<int, float> { [ei] = _edges[ei].Volume };
                        intervals.Add( (uniqueHeights[k], uniqueHeights[k + 1], dict) );
                        // keep stable ordering by sorting intervals by minH afterwards
                        intervals.Sort( ( a, b ) => a.minH.CompareTo( b.minH ) );
                    }
                }

                if( intervals.Count == 0 )
                {
                    // Nothing to pour into (shouldn't happen if edges had volume). Skip.
                    continue;
                }

                // --- Build component inventory: Substance -> mass ---
                var compInventoryByMass = new Dictionary<Substance, float>();
                if( hasPerEdgeSource )
                {
                    // sum per-edge contents present in _contentsInEdges
                    foreach( int ei in compEdges )
                    {
                        var coll = _contentsInEdges[ei];
                        if( coll == null || coll.IsEmpty() )
                            continue;
                        for( int s = 0; s < coll.SubstanceCount; s++ )
                        {
                            var st = coll[s];
                            compInventoryByMass.TryGetValue( st.Substance, out float m0 );
                            compInventoryByMass[st.Substance] = m0 + st.MassAmount;
                        }
                    }
                }
                else
                {
                    // fallback to global _contents
                    if( _contents != null && !_contents.IsEmpty() )
                    {
                        for( int s = 0; s < _contents.SubstanceCount; s++ )
                        {
                            var st = _contents[s];
                            compInventoryByMass.TryGetValue( st.Substance, out float m0 );
                            compInventoryByMass[st.Substance] = m0 + st.MassAmount;
                        }
                    }
                }

                // If no inventory, nothing to do
                if( compInventoryByMass.Count == 0 ) 
                    continue;

                // Convert inventory to list of {substance, density, remainingVolume} sorted by density desc
                var fluids = new List<(Substance sub, float density, float remainingVolume)>();
                foreach( var kv2 in compInventoryByMass )
                {
                    Substance sub = kv2.Key;
                    float mass = kv2.Value;
                    float density = sub.Density;
                    float volume = density > 0f ? mass / density : 0f; // guard zero density
                    fluids.Add( (sub, density, volume) );
                }
                fluids.Sort( ( a, b ) => b.density.CompareTo( a.density ) ); // heaviest first

                // Precompute total capacity per interval
                var intervalTotalCapacity = new float[intervals.Count];
                for( int ii = 0; ii < intervals.Count; ii++ )
                {
                    float sum = 0f;
                    foreach( var c in intervals[ii].edgeContrib.Values ) sum += c;
                    intervalTotalCapacity[ii] = sum;
                }

                // --- Pour each fluid from bottom interval upward ---
                for( int f = 0; f < fluids.Count; f++ )
                {
                    var tup = fluids[f];
                    Substance substance = tup.sub;
                    float density = tup.density;
                    float remainingVolume = tup.remainingVolume;
                    if( remainingVolume <= 0f )
                        continue;

                    for( int ii = 0; ii < intervals.Count && remainingVolume > 0f; ii++ )
                    {
                        float cap = intervalTotalCapacity[ii];
                        if( cap <= HEIGHT_EPS )
                            continue; // nothing there

                        float volumeToTake = Math.Min( remainingVolume, cap );
                        if( volumeToTake <= 0f ) 
                            continue;

                        // Distribute into edges proportionally
                        var contribs = intervals[ii].edgeContrib;
                        foreach( var kv3 in contribs )
                        {
                            int ei = kv3.Key;
                            float edgeCap = kv3.Value;
                            if( edgeCap <= 0f )
                                continue;
                            float share = edgeCap / cap;
                            float addedVolume = volumeToTake * share;
                            float addedMass = addedVolume * density;
                            AddMassToEdgeSubstance( ei, addedMass, substance );
                        }

                        // update remaining
                        remainingVolume -= volumeToTake;
                        intervalTotalCapacity[ii] = cap - volumeToTake; // leftover capacity
                    }
                } // end pouring fluids for this component
            } // end components loop
        }

        // Helper: add mass of a Substance to an edge's SubstanceStateCollection
        // merges into existing state if present; creates new if not.
        private void AddMassToEdgeSubstance( int edgeIndex, float addMass, Substance substance )
        {
            if( addMass <= 0f ) return;
            var coll = _contentsInEdges[edgeIndex];
            if( coll == null || coll.IsEmpty() )
            {
                _contentsInEdges[edgeIndex] = new SubstanceStateCollection( new SubstanceState( addMass, substance ) );
                return;
            }

            // find existing index
            for( int i = 0; i < _contentsInEdges[edgeIndex].SubstanceCount; i++ )
            {
                var st = _contentsInEdges[edgeIndex][i];
                if( st.Substance == substance )
                {
                    // replace with updated mass (substance equality by reference/type assumed)
                    _contentsInEdges[edgeIndex][i] = new SubstanceState( st.MassAmount + addMass, substance );
                    return;
                }
            }

            // not found -> add new
            _contentsInEdges[edgeIndex].Add( new SubstanceState( addMass, substance ) );
        }


        // Simple Union-Find (disjoint set) implementation
        private class UnionFind
        {
            private int[] parent;
            private int[] rank;
            public UnionFind( int n )
            {
                parent = new int[n];
                rank = new int[n];
                for( int i = 0; i < n; i++ ) parent[i] = i;
            }
            public int Find( int x )
            {
                if( parent[x] != x ) parent[x] = Find( parent[x] );
                return parent[x];
            }
            public void Union( int a, int b )
            {
                int ra = Find( a );
                int rb = Find( b );
                if( ra == rb ) return;
                if( rank[ra] < rank[rb] ) parent[ra] = rb;
                else if( rank[ra] > rank[rb] ) parent[rb] = ra;
                else { parent[rb] = ra; rank[ra]++; }
            }
        }
    }
}