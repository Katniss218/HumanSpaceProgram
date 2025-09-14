using HSP.CelestialBodies;
using HSP.ReferenceFrames;
using HSP.Trajectories;
using HSP.UI;
using HSP.Vanilla.ReferenceFrames;
using HSP.Vanilla.Scenes.MapScene;
using HSP.Vanilla.Scenes.MapScene.Cameras;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Profiling;

namespace HSP.Vanilla.UI.Scenes.MapScene
{
    public class EphemerisDrawer : SingletonMonoBehaviour<EphemerisDrawer>
    {
        readonly struct Sample
        {
            public readonly double ut;
            public readonly Vector3Dbl absolutePos;
            public readonly Vector3 worldPos;
            public readonly Vector3 screenPos;

            public Sample( double ut, Vector3Dbl absolutePos, Vector3 worldPos, Vector3 screenPos )
            {
                this.ut = ut;
                this.absolutePos = absolutePos;
                this.worldPos = worldPos;
                this.screenPos = screenPos;
            }
        }

        /// <summary>
        /// Per-body cached data.
        /// </summary>
        class BodyEntry
        {
            public EphemerisPosSetter Setter { get; internal set; }

            public readonly Dictionary<double, Sample> evalCache = new();
            public readonly List<Sample> initial = new();
            public readonly List<Vector2> output = new();
        }

        readonly struct Segment
        {
            public readonly Sample Left;
            public readonly Sample Right;
            public readonly int Depth;

            public Segment( Sample left, Sample right, int depth )
            {
                this.Left = left;
                this.Right = right;
                this.Depth = depth;
            }
        }

        const int INITIAL_POINT_COUNT = 16;
        const int MAX_POINT_COUNT = 600;
        const float AREA_THRESHOLD_PIXELS = 10.0f;
        const int MAX_SUBDIVIDE_DEPTH = 30;
        const double UT_THRESHOLD_SECONDS = 1;

        readonly Dictionary<ITrajectoryTransform, BodyEntry> _bodies = new();

        void Update()
        {
            Recalc();
        }

        private void Recalc()
        {
            var bodies = TrajectoryManager.PredictionSimulator.GetBodies();
            foreach( var (body, ephemeris) in bodies )
            {
                if( !body.gameObject.TryGetComponent<CelestialBody>( out _ ) )
                    continue;

                if( !_bodies.TryGetValue( body, out var entry ) )
                {
                    entry = new BodyEntry();
                    _bodies[body] = entry;
                }

                DrawOptimized( body, ephemeris, entry );
            }
        }

        private static float ScreenTriangleArea( in Sample a, in Sample b, in Sample c )
        {
            float ux = b.screenPos.x - a.screenPos.x;
            float uy = b.screenPos.y - a.screenPos.y;
            float vx = c.screenPos.x - a.screenPos.x;
            float vy = c.screenPos.y - a.screenPos.y;
            float cross = (ux * vy - uy * vx);
            return 0.5f * Mathf.Abs( cross );
        }

        private static Sample EvalCached( double ut, IReadonlyEphemeris ephemeris, BodyEntry entry, Camera camera, IReferenceFrame sceneReferenceFrame )
        {
            if( entry.evalCache.TryGetValue( ut, out var s ) )
                return s;

            if( entry.evalCache.Count >= 10000 )
                entry.evalCache.Clear();

            Vector3Dbl absolutePos = ephemeris.Evaluate( ut ).AbsolutePosition;
            Vector3 worldPos = (Vector3)sceneReferenceFrame.InverseTransformPosition( (Vector3)absolutePos );
            Vector3 screenPos = camera.WorldToScreenPoint( worldPos );

            s = new Sample( ut, absolutePos, worldPos, screenPos );
            entry.evalCache[ut] = s;
            return s;
        }

        private static bool IsPointInViewFrustum( Vector3 screenPos, Camera camera )
        {
            if( screenPos.z < 0 )
                return false;

            return screenPos.x >= 0 && screenPos.x <= camera.pixelWidth &&
                   screenPos.y >= 0 && screenPos.y <= camera.pixelHeight;
        }

        /// <summary>
        /// Clips a line segment (p0, p1) to the screen rectangle.
        /// Returns true if any part of the line is inside and outputs the clipped endpoints.
        /// </summary>
        public static bool ClipLineToScreen( ref Vector2 p0, ref Vector2 p1, Camera camera )
        {
            float xmin = 0f;
            float ymin = 0f;
            float xmax = camera.pixelWidth;
            float ymax = camera.pixelHeight;

            return LiangBarsky( xmin, xmax, ymin, ymax, ref p0, ref p1 );
        }

        private static bool LiangBarsky( float xmin, float xmax, float ymin, float ymax,
                                        ref Vector2 p0, ref Vector2 p1 )
        {
            float dx = p1.x - p0.x;
            float dy = p1.y - p0.y;

            float t0 = 0f, t1 = 1f;

            bool clip( float p, float q )
            {
                if( Mathf.Approximately( p, 0 ) )
                {
                    // Line parallel to this clipping edge
                    return q >= 0;
                }

                float r = q / p;

                if( p < 0 )
                {
                    if( r > t1 ) return false;
                    if( r > t0 ) t0 = r;
                }
                else if( p > 0 )
                {
                    if( r < t0 ) return false;
                    if( r < t1 ) t1 = r;
                }
                return true;
            }

            if( clip( -dx, p0.x - xmin ) ) // left
                if( clip( dx, xmax - p0.x ) )  // right
                    if( clip( -dy, p0.y - ymin ) ) // bottom
                        if( clip( dy, ymax - p0.y ) )  // top
                        {
                            if( t1 < 1f ) p1 = new Vector2( p0.x + t1 * dx, p0.y + t1 * dy );
                            if( t0 > 0f ) p0 = new Vector2( p0.x + t0 * dx, p0.y + t0 * dy );
                            return true;
                        }

            return false;
        }

        public static bool ClipLineSegmentToCameraFrustum( Vector3 worldA, Vector3 worldB, ref Vector3 screenA, ref Vector3 screenB, Camera camera, Plane[] planes )
        {
            const float eps = 1e-4f;
            // Parametric line: P(t) = worldA + t*(worldB - worldA), t in [0,1]
            float tEnter = 0f;
            float tExit = 1f;

            float d0, d1;
            Vector3 dir = worldB - worldA;

            foreach( var plane in planes )
            {
                d0 = plane.GetDistanceToPoint( worldA );
                d1 = plane.GetDistanceToPoint( worldB );

                // If both endpoints are strictly outside the same plane -> rejected
                if( d0 < -eps && d1 < -eps )
                    return false;

                // If they are on different sides, compute intersection t where distance = 0:
                float denom = d0 - d1; // = planeDist(worldA) - planeDist(worldB)
                if( Mathf.Abs( denom ) > eps )
                {
                    float t = d0 / denom; // solve d0 + t*(d1-d0) = 0 -> t = d0/(d0-d1)
                                          // If worldA is outside, we move tEnter forward
                    if( d0 < 0f && d1 >= 0f )
                    {
                        // entering the frustum
                        if( t > tEnter ) tEnter = t;
                    }
                    else if( d0 >= 0f && d1 < 0f )
                    {
                        // exiting the frustum
                        if( t < tExit ) tExit = t;
                    }
                    // else if both inside or both outside handled above
                }
                else
                {
                    // denom ~ 0 -> segment is nearly parallel to plane (distances nearly equal)
                    // if both distances are negative we've already returned false above.
                    // If distances are small but one negative and one tiny positive, the previous logic still works.
                }

                // Early reject
                if( tEnter > tExit )
                    return false;
            }

            // After processing all planes, check resulting interval
            if( tEnter > tExit ) return false;

            // Compute clipped world-space points and reproject to screen space
            Vector3 clippedWorldA = (tEnter <= 0f) ? worldA : worldA + dir * tEnter;
            Vector3 clippedWorldB = (tExit >= 1f) ? worldB : worldA + dir * tExit;

            screenA = camera.WorldToScreenPoint( clippedWorldA );
            screenB = camera.WorldToScreenPoint( clippedWorldB );

            return true;
        }

        private static bool PointsEqual( Vector2 a, Vector2 b )
        {
            const float sqrEps = 0.25f; // 0.5 pixel tolerance
            return (a - b).sqrMagnitude <= sqrEps;
        }

        private static void AddPointAvoidDuplicate( List<Vector2> output, Vector2 left, Vector2 right, Camera camera )
        {
            if( left.magnitude > 2600 || right.magnitude > 2600 )
            {
                // Trying to draw a line with points that are far outside of the screen bounds results in infinite runaway memory allocation (for some reason).
                // - even with clipping.
                // doesn't happen if entry.Setter.Points are not being set.
                Debug.LogWarning( "MAG " + left + " : " + right );
                return;
            }
            if( output.Count == 0 )
            {
                output.Add( left );
            }
            else
            {
                if( !PointsEqual( output[^1], left ) )
                    output.Add( left );
            }
            if( !PointsEqual( output[^1], right ) )
                output.Add( right );
        }

        private void DrawOptimized( ITrajectoryTransform body, IReadonlyEphemeris ephemeris, BodyEntry entry )
        {
            if( entry.Setter == null )
            {
                GameObject point = new GameObject( $"orbit line" );
                RectTransform rectTransform = point.AddComponent<RectTransform>();
                rectTransform.SetParent( MapSceneM.Instance.GetBackgroundCanvas().transform, false );
                UILineRenderer r = point.AddComponent<UILineRenderer>();

                var setter = point.AddComponent<EphemerisPosSetter>();
                setter.SceneReferenceFrameProvider = new MapSceneReferenceFrameProvider();
                setter.camera = MapSceneCameraManager.FarCamera;

                entry.Setter = setter;
            }

            Profiler.BeginSample( "EphemerisDrawer.AdaptiveResample" );

            entry.initial.Clear();
            entry.output.Clear();
            entry.evalCache.Clear();

            IReferenceFrame sceneReferenceFrame = MapSceneReferenceFrameManager.ReferenceFrame;

            double lowUT = ephemeris.LowUT;
            double highUT = ephemeris.HighUT;

            Camera camera = entry.Setter.camera;

            for( int i = 0; i < INITIAL_POINT_COUNT; i++ )
            {
                double t = lowUT + (highUT - lowUT) * ((double)i / (double)(INITIAL_POINT_COUNT - 1));
                entry.initial.Add( EvalCached( t, ephemeris, entry, camera, sceneReferenceFrame ) );
            }

            Stack<Segment> subdivisionStack = new(); // Can't do BFS (queue) without sorting.

            for( int i = entry.initial.Count - 2; i >= 0; i-- )
            {
                subdivisionStack.Push( new Segment( entry.initial[i], entry.initial[i + 1], 0 ) );
            }

            Plane[] planes = GeometryUtility.CalculateFrustumPlanes( camera );

            int iters = 0;
            while( subdivisionStack.Count > 0 && iters < 10000 )
            {
                iters++;
                if( entry.output.Count >= MAX_POINT_COUNT )
                    break;

                Segment segment = subdivisionStack.Pop();
                Sample left = segment.Left;
                Sample right = segment.Right;
                int depth = segment.Depth;
                Vector3 clippedLeft = left.screenPos;
                Vector3 clippedRight = right.screenPos;

                // subdiv
                double midUT = 0.5 * (left.ut + right.ut);
                Sample mid = EvalCached( midUT, ephemeris, entry, camera, sceneReferenceFrame );
                float area = ScreenTriangleArea( left, mid, right );

                // this midpoint length thing seems to almost work...
                bool midpointCloserThanLineLength = (mid.screenPos).magnitude <= (left.screenPos - right.screenPos).magnitude;
                bool lineInView = ClipLineSegmentToCameraFrustum( left.worldPos, right.worldPos, ref clippedLeft, ref clippedRight, camera, planes );

                if( (midpointCloserThanLineLength || lineInView) && area > AREA_THRESHOLD_PIXELS && depth < MAX_SUBDIVIDE_DEPTH && Math.Abs( right.ut - left.ut ) >= UT_THRESHOLD_SECONDS )
                {
                    subdivisionStack.Push( new Segment( mid, right, depth + 1 ) );
                    subdivisionStack.Push( new Segment( left, mid, depth + 1 ) );
                    continue;
                }

                // clip and view
                if( lineInView )
                {
                    AddPointAvoidDuplicate( entry.output, clippedLeft, clippedRight, camera );
                }
            }
            Debug.Log( $"EphemerisDrawer: body={body.gameObject.name} iters={iters} output={entry.output.Count}" );

            Profiler.EndSample();

            entry.Setter.Points = entry.output;
        }
    }
}