using HSP.ResourceFlow;
using HSP.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HSP._DevUtils
{
    /// <summary>
    /// Play-mode visualizer for a FlowEdge graph. Draws each edge and its fill (stacked by substance density).
    /// Designed to be fast (few allocations) and easy to drop in.
    /// </summary>
    [RequireComponent( typeof( Camera ) )]
    [Obsolete( "untested, might not work, WIP" )]
    public class FlowGraphVisualizer : MonoBehaviour
    {
        public FResourceContainer_FlowTank tank;
        //public FlowEdge[] edges;
        //public SubstanceStateCollection[] contentsInEdges;

        public ScreenSpaceLineRenderer lineRenderer;
        public Vector3 acceleration = new Vector3( 0, -9.81f, 0 );

        public Color outlineColor = Color.grey;
        public float outlineThicknessPx = 1f;

        public float minThicknessPx = 2f;
        public float maxThicknessPx = 12f;

        public bool drawCapsForZeroLength = true;



        // --- reusable scratch buffers to avoid allocations in Update
        Vector3[] _twoPoint = new Vector3[2];                       // reused point buffer
        List<SubstanceState> _scratchStates = new List<SubstanceState>( 8 ); // per-edge small list reused

        void LateUpdate()
        {
            var edges = tank.tank.Edges;
            var contentsInEdges = tank.tank.ContentsInEdges;
            if( lineRenderer == null || edges == null || contentsInEdges == null )
                return;
            lineRenderer.Clear();

            Vector3 accelDir = acceleration.sqrMagnitude > 1e-6f ? acceleration.normalized : Vector3.down;

            int E = edges.Count;
            for( int ei = 0; ei < E; ei++ )
            {
                var edge = edges[ei];

                // endpoints in world space
                Vector3 p0 = edge.end1.pos;
                Vector3 p1 = edge.end2.pos;

                // projected heights along acceleration direction
                float h0 = Vector3.Dot( p0, accelDir );
                float h1 = Vector3.Dot( p1, accelDir );

                // bottom is the point with smaller projected height
                bool p0IsBottom = h0 <= h1;
                Vector3 bottomPos = p0IsBottom ? p0 : p1;
                Vector3 topPos = p0IsBottom ? p1 : p0;

                float edgeVol = edge.Volume;
                if( edgeVol <= 0f )
                {
                    // Nothing to draw in filled sense: draw thin outline and continue
                    DrawOutline( edge, bottomPos, topPos );
                    continue;
                }

                // gather substances present in this edge (reuse scratch list)
                _scratchStates.Clear();
                var col = (contentsInEdges != null && ei < contentsInEdges.Length) ? contentsInEdges[ei] : null;
                if( col != null && !col.IsEmpty() )
                {
                    foreach( var s in col )
                    {
                        _scratchStates.Add( s );
                    }
                }

                // compute per-substance volumes and total volume (volume = mass / density)
                float totalVol = 0f;
                // We'll store volumes in a small parallel array (same order as _scratchStates)
                int S = _scratchStates.Count;
                float[] subVolumes = (S > 0) ? (new float[S]) : null;
                for( int si = 0; si < S; si++ )
                {
                    var st = _scratchStates[si];
                    float rho = Math.Max( 1e-9f, st.Substance.Density ); // guard
                    float vol = st.MassAmount / rho;
                    subVolumes[si] = Mathf.Max( 0f, vol );
                    totalVol += subVolumes[si];
                }

                float fillFrac = Mathf.Clamp01( totalVol / edgeVol );

                // draw outline (thin)
                DrawOutline( edge, bottomPos, topPos );

                // if nothing to fill, skip further drawing
                if( fillFrac <= 0f || S == 0 )
                {
                    continue;
                }

                // compute the overlay thickness for this edge from fillFrac
                float overlayThickness = Mathf.Lerp( minThicknessPx, maxThicknessPx, fillFrac );

                // sort substances by density descending (heaviest first)
                // For small S this is fine; we micro-opt by making an index array and sorting it
                int[] perm = new int[S];
                for( int i = 0; i < S; i++ ) perm[i] = i;
                Array.Sort( perm, ( a, b ) =>
                {
                    float da = _scratchStates[a].Substance.Density;
                    float db = _scratchStates[b].Substance.Density;
                    return db.CompareTo( da ); // descending
                } );

                // draw stacked segments from bottom->top according to per-substance volumes,
                // but clamped to the overall fill fraction (so overflow doesn't paint above top)
                float tStart = 0f;
                float availableFrac = fillFrac; // fraction of edge that is actually filled
                for( int pi = 0; pi < S && availableFrac > 1e-6f; pi++ )
                {
                    int si = perm[pi];
                    float vol = subVolumes[si];
                    if( vol <= 0f ) continue;

                    float frac = Mathf.Clamp01( vol / edgeVol );
                    // clamp to remaining available fraction
                    float useFrac = Mathf.Min( frac, availableFrac );
                    if( useFrac <= 0f ) continue;

                    // compute segment endpoints in worldspace along the edge
                    float tEnd = tStart + useFrac;
                    tEnd = Mathf.Clamp01( tEnd );

                    Vector3 a = Vector3.Lerp( bottomPos, topPos, tStart );
                    Vector3 b = Vector3.Lerp( bottomPos, topPos, tEnd );

                    // draw the colored segment
                    _twoPoint[0] = a;
                    _twoPoint[1] = b;
                    Color c = _scratchStates[si].Substance.UIColor;
                    lineRenderer.AddLine( _twoPoint, c, overlayThickness );

                    tStart = tEnd;
                    availableFrac -= useFrac;
                }

                // If there remains a small fraction which wasn't drawn due to numeric issues, optionally draw it:
                // (not necessary; kept out for perf)
            }
        }

        void DrawOutline( FlowEdge edge, Vector3 bottomPos, Vector3 topPos )
        {
            // If the edge is zero-length (endpoints equal), draw a small cap/dot optionally.
            float len = Vector3.Distance( bottomPos, topPos );
            if( len <= Mathf.Epsilon )
            {
                if( drawCapsForZeroLength )
                {
                    // draw a tiny line as a dot representation
                    Vector3 a = bottomPos + Vector3.up * 0.001f;
                    Vector3 b = bottomPos - Vector3.up * 0.001f;
                    _twoPoint[0] = a;
                    _twoPoint[1] = b;
                    lineRenderer.AddLine( _twoPoint, outlineColor, outlineThicknessPx );
                }
                return;
            }

            _twoPoint[0] = bottomPos;
            _twoPoint[1] = topPos;
            lineRenderer.AddLine( _twoPoint, outlineColor, outlineThicknessPx );
        }
    }
}
