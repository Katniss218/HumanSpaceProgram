using HSP.CelestialBodies;
using HSP.Trajectories;
using HSP.UI;
using HSP.Vanilla.ReferenceFrames;
using HSP.Vanilla.Scenes.MapScene;
using HSP.Vanilla.Scenes.MapScene.Cameras;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace HSP.Vanilla.UI.Scenes.MapScene
{
    public class EphemerisDrawer : SingletonMonoBehaviour<EphemerisDrawer>
    {
        void Update()
        {
            Recalc();
        }

        Dictionary<ITrajectoryTransform, EphemerisPosSetter> _bodies = new();

        void Recalc()
        {
#warning TODO - adaptive line count and/or line plotting. Adapt to camera distance and ephemeris curvature.
            const int LINE_POINT_COUNT = 400;

            var bodies = TrajectoryManager.PredictionSimulator.GetBodies();
            foreach( var (body, ephemeris) in bodies )
            {
                if( body.gameObject.TryGetComponent<CelestialBody>( out var cb ) )
                {
                    if( !_bodies.TryGetValue( body, out var setter ) )
                    {
                        setter = null;
                    }
                    Draw2( body, ephemeris, ref setter );
                    _bodies[body] = setter;
                }
            }

            void Draw2( ITrajectoryTransform body, IReadonlyEphemeris ephemeris, ref EphemerisPosSetter setter )
            {
                if( setter == null )
                {
                    GameObject point = new GameObject( $"orbit line" );
                    RectTransform rectTransform = point.AddComponent<RectTransform>();
                    rectTransform.SetParent( MapSceneM.Instance.GetBackgroundCanvas().transform, false );
                    UILineRenderer r = point.AddComponent<UILineRenderer>();

                    setter = point.AddComponent<EphemerisPosSetter>();
                    setter.SceneReferenceFrameProvider = new MapSceneReferenceFrameProvider();
                    setter.camera = MapSceneCameraManager.FarCamera;
                }

                Vector3Dbl[] points = new Vector3Dbl[LINE_POINT_COUNT];
                Profiler.BeginSample( "EphemerisDrawer.Draw2" );
                for( int i = 0; i < LINE_POINT_COUNT; i++ )
                {
                    double time = MathD.Lerp( ephemeris.LowUT, ephemeris.HighUT, ((double)i / (double)LINE_POINT_COUNT) );
                    var pos = ephemeris.Evaluate( time );
                    points[i] = pos.AbsolutePosition;
                }
                Profiler.EndSample();
                setter.Points = points;
            }
        }
    }
}