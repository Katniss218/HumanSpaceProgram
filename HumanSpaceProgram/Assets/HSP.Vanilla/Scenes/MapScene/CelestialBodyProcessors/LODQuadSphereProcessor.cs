using HSP.CelestialBodies.Surfaces;
using HSP.Vanilla.CelestialBodies;

namespace HSP.Vanilla.Scenes.MapScene.CelestialBodyProcessors
{
    public static class LODQuadSphereProcessor
    {
        public const string RUN = HSPEvent.NAMESPACE_HSP + ".vanilla.map_scene.cbbuild.lodquadsphere";

        [HSPEventListener( HSPEvent_MAP_SCENE_CELESTIAL_BODY_BUILDER.ID, RUN )]
        public static void OnMapCelestialBodyBuild( HSPEvent_MAP_SCENE_CELESTIAL_BODY_BUILDER.Data data )
        {
            // copy all lodquadspheres that are visible, set them as visible noncolliding.

            var quadSpheres = data.source.gameObject.GetComponents<LODQuadSphere>();
            foreach( var quadSphere in quadSpheres )
            {
                if( quadSphere.Mode.HasFlag( LODQuadMode.Visual ) )
                {
                    var lqs = data.target.gameObject.AddComponent<LODQuadSphere>();
                    lqs.SetMode( LODQuadMode.Visual );
                    lqs.EdgeSubdivisions = quadSphere.EdgeSubdivisions;
                    lqs.MaxDepth = quadSphere.MaxDepth;
                    lqs.Materials = quadSphere.Materials;
                    lqs.PoIGetter = new MapActiveCameraPOIGetter();
                    lqs.Layer = Layer.MAP_ONLY;
                    lqs.SetJobs( quadSphere.Jobs );
                }
            }
        }
    }
}