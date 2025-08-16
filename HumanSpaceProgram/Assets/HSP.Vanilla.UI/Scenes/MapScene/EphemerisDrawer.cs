using HSP.CelestialBodies;
using HSP.Trajectories;
using HSP.UI;
using HSP.Vanilla.ReferenceFrames;
using HSP.Vanilla.Scenes.MapScene;
using HSP.Vanilla.Scenes.MapScene.Cameras;
using UnityEngine;

namespace HSP.Vanilla.UI.Scenes.MapScene
{
    public class EphemerisDrawer
    {
        [HSPEventListener( HSPEvent_MAP_SCENE_ACTIVATE.ID, "recalc" )]
        private static void Recalc()
        {
            const int LINE_POINT_COUNT = 800;

            var bodies = TrajectoryManager.PredictionSimulator.GetBodies();
            foreach( var (body, ephemeris) in bodies )
            {
                if( body.gameObject.TryGetComponent<CelestialBody>( out var cb ) )
                {
                    Draw2( body, ephemeris );
                }
            }

            void Draw2( ITrajectoryTransform body, IReadonlyEphemeris ephemeris )
            {
                GameObject point = new GameObject( $"orbit line" );
                RectTransform rectTransform = point.AddComponent<RectTransform>();
                rectTransform.SetParent( MapSceneM.Instance.GetBackgroundCanvas().transform, false );
                UILineRenderer r = point.AddComponent<UILineRenderer>();

                EphemerisPosSetter setter = point.AddComponent<EphemerisPosSetter>();
                setter.SceneReferenceFrameProvider = new MapSceneReferenceFrameProvider();
                setter.camera = MapSceneCameraManager.FarCamera;

                Vector3Dbl[] points = new Vector3Dbl[LINE_POINT_COUNT];
                for( int i = 0; i < LINE_POINT_COUNT; i++ )
                {
                    double time = MathD.Lerp( ephemeris.LowUT, ephemeris.HighUT, ((double)i / (double)LINE_POINT_COUNT) );
                    var pos = ephemeris.Evaluate( time );
                    points[i] = pos.AbsolutePosition;
                }
                setter.Points = points;
            }
        }
    }
}