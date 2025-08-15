using HSP.CelestialBodies;
using HSP.Time;
using HSP.Trajectories;
using HSP.Vanilla.ReferenceFrames;
using HSP.Vanilla.Scenes.MapScene;
using UnityEngine;
using UnityPlus.AssetManagement;

namespace HSP.Vanilla.UI.Scenes.MapScene
{
    public class EphemerisDrawer
    {
        static GameObject[] _points = new GameObject[0];

        [HSPEventListener( HSPEvent_MAP_SCENE_ACTIVATE.ID, "recalc" )]
        private static void Recalc()
        {
            foreach( var point in _points )
            {
                if( point != null )
                    GameObject.Destroy( point );
            }

            const int max = 200;
            _points = new GameObject[max + max];

            var bodies = TrajectoryManager.PredictionSimulator.GetBodies();
            foreach( var (body, ephemeris) in bodies )
            {
                if( body.gameObject.TryGetComponent<CelestialBody>( out var cb ) )
                {
                    if( cb.ID == "main" )
                    {
                        Draw(body, ephemeris, 0, 2e10f);
                    }
                    if( cb.ID == "sun" )
                    {
                        Draw(body, ephemeris, max, 1e10f);
                    }
                }
            }

            void Draw( ITrajectoryTransform body, IReadonlyEphemeris ephemeris, int offset, float scale )
            {
                for( int i = 0; i < max; i++ )
                {
                    double time = MathD.Lerp( ephemeris.LowUT, ephemeris.HighUT, ((double)i / (double)max) );
                    var pos = ephemeris.Evaluate( time );
                    GameObject point = new GameObject( $"point {time:0} UT {pos.AbsolutePosition}" );
                    point.AddComponent<MeshFilter>().sharedMesh = AssetRegistry.Get<Mesh>( "builtin::Cube" );
                    point.AddComponent<MeshRenderer>().sharedMaterial = AssetRegistry.Get<Material>( "builtin::Resources/New Material 1" );
                    point.layer = (int)Layer.MAP_ONLY;
                    point.transform.localScale = Vector3.one * scale; // 100km radius
                    var trans = point.AddComponent<FixedReferenceFrameTransform>();
                    trans.SceneReferenceFrameProvider = new MapSceneReferenceFrameProvider();
                    trans.AbsolutePosition = pos.AbsolutePosition;
                    _points[offset + i] = point;
                }
            }
        }
    }
}