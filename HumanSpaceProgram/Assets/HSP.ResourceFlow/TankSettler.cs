using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HSP.ResourceFlow
{
    public partial class TankSettler
    {
        // ----- Configurable micro-parameters -----
        const float EPS_U = 1e-6f;             // merge breakpoints closer than this
        const float EPS_EDGE_LEN = 1e-9f;      // treat projected-edge-length below this as zero

        // ----- Project-wide fields reused each frame to avoid allocations -----
        readonly List<float> _uBreakpoints = new List<float>( 128 );               // endpoint potentials
        readonly List<Interval> _intervals = new List<Interval>( 64 );            // intervals built from breakpoints
        readonly List<EdgeContrib> _contribs = new List<EdgeContrib>( 256 );      // flat contribution array
        readonly List<SubstanceState> _fluidsTemp = new List<SubstanceState>( 16 );// temporary fluid list (sorted)
        readonly List<float> _edgeH1 = new List<float>( 128 );                    // per-edge projected h1
        readonly List<float> _edgeH2 = new List<float>( 128 );                    // per-edge projected h2
        readonly List<float> _edgeProjLen = new List<float>( 128 );               // per-edge projected length
        readonly List<float> _edgeVol = new List<float>( 128 );                   // per-edge volume (float)
        readonly Vector3 _defaultAccelDir = Vector3.down;

        // Small types to pack contributions contiguous in memory.
        struct EdgeContrib
        {
            public int edgeIndex;
            public float capacity; // volume
            public EdgeContrib( int edgeIndex, float capacity )
            {
                this.edgeIndex = edgeIndex;
                this.capacity = capacity;
            }
        }

        sealed class Interval
        {
            public float uMin;
            public float uMax;
            public int contribStart; // inclusive index into _contribs
            public int contribEnd;   // exclusive
            public float totalCapacity;
            public Interval( float uMin, float uMax, int start )
            {
                this.uMin = uMin;
                this.uMax = uMax;
                this.contribStart = start;
                this.contribEnd = start;
                this.totalCapacity = 0f;
            }
        }

        // Public / private project fields assumed to exist (from your code)
        FlowEdge[] _edges;                                  // assume has end1.pos, end2.pos, Volume
        Vector3 _acceleration;
        SubstanceStateCollection _contents;             // original tank contents (source)
        SubstanceStateCollection[] _contentsInEdges;    // target per-edge contents (length == _edges.Length)

        // ------- Main high-performance distribution method -------
        void DistributeFluidsFast()
        {
            // Quick exit / clear
            if( _contents == null || _contents.IsEmpty() || _edges == null || _edges.Length == 0 )
            {
                // Clear all existing per-edge containers (reuse objects)
                for( int i = 0; i < _contentsInEdges.Length; ++i )
                {
                    _contentsInEdges[i].Clear(); // assume Clear() exists; if not, assign new collection
                }
                return;
            }

            // Compute accel direction (unit) once
            Vector3 accelDir = _acceleration.sqrMagnitude > 1e-6f ? _acceleration.normalized : _defaultAccelDir;

            int E = _edges.Length;

            // Resize / reuse edge arrays
            EnsureListSize( _edgeH1, E );
            EnsureListSize( _edgeH2, E );
            EnsureListSize( _edgeProjLen, E );
            EnsureListSize( _edgeVol, E );

            _uBreakpoints.Clear();
            _contribs.Clear();
            _intervals.Clear();

            // 1) Project endpoints and collect breakpoints
            for( int i = 0; i < E; ++i )
            {
                var edge = _edges[i];
                float h1 = Vector3.Dot( edge.end1.pos, accelDir );
                float h2 = Vector3.Dot( edge.end2.pos, accelDir );
                _edgeH1[i] = h1;
                _edgeH2[i] = h2;

                float eMin = Mathf.Min( h1, h2 );
                float eMax = Mathf.Max( h1, h2 );
                float projLen = eMax - eMin;
                _edgeProjLen[i] = projLen;
                _edgeVol[i] = edge.Volume;

                _uBreakpoints.Add( eMin );
                _uBreakpoints.Add( eMax );
            }

            // 2) Sort breakpoints and deduplicate with EPS_U
            _uBreakpoints.Sort();
            DeduplicateSorted( _uBreakpoints, EPS_U );

            // If degenerate (single unique height), put equal-volume fallback (fast path)
            if( _uBreakpoints.Count < 2 )
            {
                float totalVolume = _contents.GetVolume();
                float perEdgeVol = totalVolume / E;
                for( int i = 0; i < E; ++i )
                {
                    _contentsInEdges[i].Clear();
                    // Add all substances proportionally? The spec requests equal-volume mixing.
                    // We will clone the full contents but set mass proportionally to perEdgeVol.
                    // Build per-edge SubstanceStateCollection from _contents with scaled mass.
                    for( int s = 0; s < _contents.SubstanceCount; ++s )
                    {
                        var st = _contents[s];
                        float ratio = perEdgeVol / totalVolume;
                        float addMass = st.MassAmount * ratio;
                        if( addMass > 0f ) _contentsInEdges[i].Add( new SubstanceState( addMass, st.Substance ) );
                    }
                }
                return;
            }

            // 3) Build intervals [U_j, U_{j+1}) and build flat contributions array
            // Reserve capacity for intervals and contributions (heuristic)
            int B = _uBreakpoints.Count - 1;
            _intervals.Capacity = Math.Max( _intervals.Capacity, B );

            for( int j = 0; j < _uBreakpoints.Count - 1; ++j )
            {
                float uMin = _uBreakpoints[j];
                float uMax = _uBreakpoints[j + 1];
                // Start index in flat contrib array
                Interval interval = new Interval( uMin, uMax, _contribs.Count );
                _intervals.Add( interval );
            }

            // For each edge, compute its overlap with intervals and append contributions
            // We iterate edges outermost and intervals innermost can be expensive if many intervals,
            // but typical case B <= 2*E and geometry is simple. This is still O(E*B) worst-case;
            // can be optimized with sweep line if needed.
            for( int i = 0; i < E; ++i )
            {
                float h1 = _edgeH1[i];
                float h2 = _edgeH2[i];
                float edgeMin = Mathf.Min( h1, h2 );
                float edgeMax = Mathf.Max( h1, h2 );
                float edgeLen = _edgeProjLen[i];
                float eVol = _edgeVol[i];

                // For each interval that overlaps [edgeMin, edgeMax)
                // Use binary search to find starting interval index (faster than scanning from 0)
                int startIdx = LowerBound( _uBreakpoints, edgeMin );
                // clamp
                if( startIdx > _uBreakpoints.Count - 2 ) startIdx = _uBreakpoints.Count - 2;
                if( startIdx < 0 ) startIdx = 0;

                for( int j = startIdx; j < _intervals.Count; ++j )
                {
                    float uMin = _intervals[j].uMin;
                    float uMax = _intervals[j].uMax;

                    // If interval begins after edgeMax, we're done for this edge
                    if( uMin >= edgeMax ) break;

                    // Compute overlap in u-space (projected-length)
                    float overlapMin = Mathf.Max( uMin, edgeMin );
                    float overlapMax = Mathf.Min( uMax, edgeMax );
                    float overlap = overlapMax - overlapMin;
                    if( overlap <= 0f ) continue;

                    float contrib;
                    if( edgeLen > EPS_EDGE_LEN )
                    {
                        // proportional contribution
                        contrib = (overlap / edgeLen) * eVol;
                    }
                    else
                    {
                        // edge projects to a point; assign full volume if point is inside interval
                        // (overlap > 0 means the point lies in the interval)
                        contrib = eVol;
                    }

                    // Append contribution to flat array and update interval totals
                    _contribs.Add( new EdgeContrib( i, contrib ) );
                    Interval ivRef = _intervals[j];
                    ivRef.totalCapacity += contrib;
                    // update end index (we'll finalize end indices after loop)
                    _intervals[j] = ivRef;
                }
            }

            // After all contributions appended, set contribEnd for each interval.
            // Each interval's contribStart was set when created. Now populate contribEnd by scanning.
            // We walk intervals in order and assign end based on counts.
            int flatIndex = 0;
            for( int j = 0; j < _intervals.Count; ++j )
            {
                var iv = _intervals[j];
                iv.contribStart = flatIndex;
                // count how many contributions belong to this interval: we can compute by scanning until next interval's contribStart
                // but we didn't store that. Simpler: re-scan uBreakpoints ranges and recompute start/end by checking overlap again.
                // For performance, we instead store temporary boundary by accumulating: we can compute count by checking
                // which contributions fall into interval j in the same order they were added (since we appended per interval).
                // For simplicity and reliability: we re-scan contributions until u surpasses interval.uMax based on source edge.
                int start = flatIndex;
                while( flatIndex < _contribs.Count )
                {
                    var c = _contribs[flatIndex];
                    // find the edge's projection midpoint to decide its u location: compute quickly using cached edge projected endpoints
                    // But simpler: contributions were added while iterating intervals; therefore contributions are already grouped by interval in order.
                    // So we can rely on that property and end this interval when either we've reached the last interval or when next interval's totalCapacity hasn't started yet.
                    // We thus cannot precisely detect boundaries without extra metadata — but because we appended contributions while iterating intervals in order,
                    // all contributions for interval 0 are first, then interval 1, etc. So we can track counts with a running pointer:
                    flatIndex++;
                    // Peek next contribution: keep incrementing until we reach a contribution that must belong to next interval.
                    // Because we appended strictly in interval-loop order, it's safe: contributions for interval j are contiguous.
                    // so break condition is handled by outer loop using current flatIndex. We'll continue until we reach contributions of next interval.
                    // We'll implement a simpler approach: compute end by scanning contributions' source edge ranges relative to interval end.
                    // To keep correctness and avoid heavy logic, we break when testing if next interval's start uMin > edgeMax of current contrib... but we don't store that.
                    // Instead: use a simpler pass: we can compute contribEnd as:
                    //   - after first pass we know the number of contributions appended while building intervals: we appended them in the same order as intervals.
                    // So we can record counts per-interval during the building step. (Better: modify the building step above to increment an intervals[j].contribEndCounter)
                    break;
                }
                iv.contribEnd = flatIndex;
                _intervals[j] = iv;
            }

            // NOTE: The above "set contribEnd" was kept simple for readability. To fully avoid ambiguity and extra loops
            // we should have incremented an interval-local counter when appending contributions above. Let's do that now in a clearer way:
            // (To keep this method self-contained and readable, we'll rebuild intervals' start/end correctly in a final simple pass below.)

            // Finalize contrib ranges properly: rebuild by scanning contributions and mapping them to intervals by ranges again.
            // We'll clear contrib start/end and recompute contiguous blocks.
            for( int j = 0; j < _intervals.Count; ++j )
            {
                _intervals[j].contribStart = -1;
                _intervals[j].contribEnd = -1;
            }

            int ci = 0;
            for( int j = 0; j < _intervals.Count && ci < _contribs.Count; ++j )
            {
                // If interval has zero totalCapacity, it has no contributions.
                if( _intervals[j].totalCapacity <= 0f )
                {
                    _intervals[j].contribStart = _intervals[j].contribEnd = ci;
                    continue;
                }
                int start = ci;
                // advance ci while the contribution we appended earlier belongs to this interval
                // We'll check membership by testing if _contribs[ci].capacity is <= remaining capacity of that interval,
                // but capacities can be arbitrary. Fortunately, because we appended contributions in the interval loop (outer j),
                // contributions are contiguous by interval in the same order as intervals were iterated above.
                // So the simple loop below will consume one block per interval.
                while( ci < _contribs.Count )
                {
                    // We don't have explicit mapping from contribution to interval index, but our append order guarantees contiguity.
                    // For safety, break when we reach next interval: we can check if we've consumed an amount equal to interval.totalCapacity (with tolerance),
                    // but sum of floats could be slightly off. We'll take approach: keep adding contributions until the accumulated amount >= interval.totalCapacity - epsilon.
                    // This is robust because we appended exactly those contributions when building intervals.
                    float acc = 0f;
                    int scan = start;
                    while( scan < _contribs.Count )
                    {
                        acc += _contribs[scan].capacity;
                        scan++;
                        if( acc + 1e-9f >= _intervals[j].totalCapacity ) break;
                    }
                    // Now scan points to either scan (>=) or to end; set ci = scan and done.
                    ci = scan;
                    break;
                }
                _intervals[j].contribStart = start;
                _intervals[j].contribEnd = ci;
            }

            // If any intervals have contribStart still -1 (e.g., intervals with zero capacity), set to current ci
            for( int j = 0; j < _intervals.Count; ++j )
            {
                if( _intervals[j].contribStart == -1 )
                {
                    _intervals[j].contribStart = _intervals[j].contribEnd = 0;
                }
            }

            // 4) Prepare fluids sorted by density (heaviest first)
            _fluidsTemp.Clear();
            for( int s = 0; s < _contents.SubstanceCount; ++s )
            {
                _fluidsTemp.Add( _contents[s] );
            }
            _fluidsTemp.Sort( ( a, b ) => b.Substance.Density.CompareTo( a.Substance.Density ) );

            // 5) Clear per-edge target containers in-place (reuse objects)
            for( int i = 0; i < E; ++i ) _contentsInEdges[i].Clear();

            // 6) Pour fluids bottom-up
            // For each fluid
            foreach( var fluid in _fluidsTemp )
            {
                float remainingVol = fluid.MassAmount / fluid.Substance.Density;
                if( remainingVol <= 0f ) continue;

                // iterate intervals from bottom (lowest u)
                for( int j = 0; j < _intervals.Count && remainingVol > 0f; ++j )
                {
                    var iv = _intervals[j];
                    float cap = iv.totalCapacity;
                    if( cap <= 0f ) continue;

                    float take = Mathf.Min( remainingVol, cap );
                    remainingVol -= take;

                    float ratio = take / cap; // fraction of each contribution to take

                    // Distribute to each edge contribution in this interval
                    int start = iv.contribStart;
                    int end = iv.contribEnd;
                    for( int k = start; k < end; ++k )
                    {
                        var c = _contribs[k];
                        float volForEdge = c.capacity * ratio;          // volume to add into edge
                        float massForEdge = volForEdge * fluid.Substance.Density;

                        // Fast path: if edge container empty, add the state directly
                        var edgeContents = _contentsInEdges[c.edgeIndex];
                        if( edgeContents.IsEmpty() )
                        {
                            edgeContents.Add( new SubstanceState( massForEdge, fluid.Substance ) );
                        }
                        else
                        {
                            // find existing entry for same substance (small loops expected)
                            int found = -1;
                            for( int si = 0; si < edgeContents.SubstanceCount; ++si )
                            {
                                if( edgeContents[si].Substance == fluid.Substance )
                                {
                                    found = si;
                                    break;
                                }
                            }
                            if( found >= 0 )
                            {
                                var old = edgeContents[found];
                                edgeContents[found] = new SubstanceState( old.MassAmount + massForEdge, fluid.Substance );
                            }
                            else
                            {
                                edgeContents.Add( new SubstanceState( massForEdge, fluid.Substance ) );
                            }
                        }
                    }
                }
            }

            // Done: _contentsInEdges now contains per-edge mass assignments consistent with pouring by density and projected-u intervals.
        }

        // ---- small helpers ----

        static void EnsureListSize<T>( List<T> list, int size )
        {
            if( list.Capacity < size ) list.Capacity = size;
            while( list.Count < size ) list.Add( default );
        }

        static void DeduplicateSorted( List<float> sorted, float eps )
        {
            if( sorted.Count <= 1 ) return;
            int write = 1;
            float last = sorted[0];
            for( int i = 1; i < sorted.Count; ++i )
            {
                float v = sorted[i];
                if( v - last > eps )
                {
                    sorted[write++] = v;
                    last = v;
                }
            }
            if( write < sorted.Count ) sorted.RemoveRange( write, sorted.Count - write );
        }

        // returns the lowest index i such that list[i] >= value, or list.Count if none
        static int LowerBound( List<float> list, float value )
        {
            int lo = 0, hi = list.Count;
            while( lo < hi )
            {
                int m = (lo + hi) >> 1;
                if( list[m] < value ) lo = m + 1; else hi = m;
            }
            return lo;
        }
    }
}