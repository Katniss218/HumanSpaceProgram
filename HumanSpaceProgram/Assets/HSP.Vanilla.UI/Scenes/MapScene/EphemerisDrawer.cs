using HSP.CelestialBodies;
using HSP.ReferenceFrames;
using HSP.Trajectories;
using HSP.UI;
using HSP.Vanilla.ReferenceFrames;
using HSP.Vanilla.Scenes.MapScene;
using HSP.Vanilla.Scenes.MapScene.Cameras;
using System;
using System.Collections.Generic;
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
            public readonly Vector2 screenPos;

            public Sample( double ut, Vector3Dbl absolutePos, Vector2 screenPos )
            {
                this.ut = ut;
                this.absolutePos = absolutePos;
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

        const int INITIAL_POINT_COUNT = 10;
        const int MAX_POINT_COUNT = 600;
        const float AREA_THRESHOLD_PIXELS = 10.0f;
        const int MAX_SUBDIVIDE_DEPTH = 12;
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

            Vector3Dbl absolutePos = ephemeris.Evaluate( ut ).AbsolutePosition;
            Vector3 screenPos = camera.WorldToScreenPoint( (Vector3)sceneReferenceFrame.InverseTransformPosition( (Vector3)absolutePos ) );

            s = new Sample( ut, absolutePos, screenPos);
            entry.evalCache[ut] = s;
            return s;
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

            Camera setterCam = entry.Setter.camera;

            IReferenceFrame sceneReferenceFrame = MapSceneReferenceFrameManager.ReferenceFrame;

            double lowUT = ephemeris.LowUT;
            double highUT = ephemeris.HighUT;

            Profiler.BeginSample( "EphemerisDrawer.AdaptiveResample" );

            entry.initial.Clear();
            entry.output.Clear();
            entry.evalCache.Clear();

            for( int i = 0; i < INITIAL_POINT_COUNT; i++ )
            {
                double t = lowUT + (highUT - lowUT) * ((double)i / (double)(INITIAL_POINT_COUNT - 1));
                entry.initial.Add( EvalCached( t, ephemeris, entry, setterCam, sceneReferenceFrame ) );
            }

            Stack<Segment> subdivisionStack = new();

            int pointCount = 0;
            for( int i = 0; i < entry.initial.Count - 1; i++ )
            {
                if( entry.output.Count >= MAX_POINT_COUNT )
                    break;

                Sample left = entry.initial[i];
                Sample right = entry.initial[i + 1];
                int depth = 0;

                subdivisionStack.Clear();

                while( true )
                {
                    if( pointCount >= MAX_POINT_COUNT )
                        break;

#warning TODO - don't subdivide offscreen and clip to screen bounds.

                    if( depth >= MAX_SUBDIVIDE_DEPTH || Math.Abs( right.ut - left.ut ) < UT_THRESHOLD_SECONDS )
                    {
                        entry.output.Add( left.screenPos );
                        pointCount++;
                    }
                    else
                    {
                        double midUT = 0.5 * (left.ut + right.ut);
                        Sample mid = EvalCached( midUT, ephemeris, entry, setterCam, sceneReferenceFrame );

                        float area = ScreenTriangleArea( left, mid, right );

                        if( area > AREA_THRESHOLD_PIXELS && pointCount + 2 <= MAX_POINT_COUNT )
                        {
                            subdivisionStack.Push( new Segment( mid, right, depth + 1 ) );
                            right = mid;
                            depth++;
                            continue;
                        }
                        else
                        {
                            entry.output.Add( left.screenPos );
                            pointCount++;
                        }
                    }

                    // Advance to next segment.
                    if( subdivisionStack.Count > 0 )
                    {
                        var seg = subdivisionStack.Pop();
                        left = seg.Left;
                        right = seg.Right;
                        depth = seg.Depth;
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // append final endpoint if there's room
            if( entry.initial.Count > 0 && entry.output.Count < MAX_POINT_COUNT )
            {
                entry.output.Add( entry.initial[entry.initial.Count - 1].screenPos );
            }

            Profiler.EndSample();

            entry.Setter.Points = entry.output;
        }
    }
}