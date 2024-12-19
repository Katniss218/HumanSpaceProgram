using HSP.CelestialBodies;
using HSP.CelestialBodies.Surfaces;
using HSP.ReferenceFrames;
using HSP.Timelines;
using HSP.Trajectories;
using HSP.Vanilla;
using HSP.Vanilla.CelestialBodies;
using HSP.Vanilla.ReferenceFrames;
using HSP.Vanilla.Scenes.GameplayScene.Cameras;
using HSP.Vanilla.Trajectories;
using HSP.Vessels;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityPlus.AssetManagement;

namespace HSP._DevUtils
{
    public static class VanillaPlanetarySystemFactory
    {
        public const string CREATE_CELESTIAL_BODIES = HSPEvent.NAMESPACE_HSP + ".create_universe";

        private static Material[] _earthMaterial = new Material[6];

        private static void A()
        {
            var cbShader = AssetRegistry.Get<Shader>( "builtin::HSP.CelestialBodies/Surfaces/TerrainShader" );
            var cbTex = new Texture2D[]
            {
                AssetRegistry.Get<Texture2D>( "Vanilla::Assets/earth_color_xn" ),
                AssetRegistry.Get<Texture2D>( "Vanilla::Assets/earth_color_xp" ),
                AssetRegistry.Get<Texture2D>( "Vanilla::Assets/earth_color_yn2" ),
                AssetRegistry.Get<Texture2D>( "Vanilla::Assets/earth_color_yp" ),
                AssetRegistry.Get<Texture2D>( "Vanilla::Assets/earth_color_zn" ),
                AssetRegistry.Get<Texture2D>( "Vanilla::Assets/earth_color_zp" )
            };
            var cbNormal = AssetRegistry.Get<Texture2D>( "Vanilla::Assets/Moon_NRM" );

            for( int i = 0; i < 6; i++ )
            {
                Material mat = new Material( cbShader );
                mat.SetTexture( "_MainTex", cbTex[i] );
                mat.SetTexture( "_NormalTex", cbNormal );
                mat.SetFloat( "_Glossiness", 0.05f );
                mat.SetFloat( "_NormalStrength", 0.0f );
                _earthMaterial[i] = mat;
            }
        }

        private static CelestialBody CreateCB( string id, Vector3Dbl airfPos, QuaternionDbl airfRot )
        {
            CelestialBody cb = new CelestialBodyFactory( id ) { radius = 696_340_000.0, mass = 1.989e30 }.Create( airfPos, airfRot );
            LODQuadSphere lqs = cb.gameObject.AddComponent<LODQuadSphere>();
            lqs.SetMode( LODQuadMode.VisualAndCollider );
            lqs.EdgeSubdivisions = 4;
            lqs.MaxDepth = 10;
            lqs.Materials = _earthMaterial;
            lqs.PoIGetter = () => VesselManager.LoadedVessels.Select( v => v.ReferenceFrameTransform.AbsolutePosition );
            lqs.SetJobs( new ILODQuadModifier[]
            {
                new LODQuadModifier_InitializeMesh(),
            }, new ILODQuadModifier[]
            {
                new LODQuadModifier_FinalizeMesh(),
            } );

            TrajectoryTransform comp = cb.gameObject.AddComponent<TrajectoryTransform>();
            comp.IsAttractor = true;
            comp.Trajectory = new FixedOrbit( Time.TimeManager.UT, airfPos, airfRot, cb.Mass );
            return cb;
        }

        private static CelestialBody CreateCB( string id, string parentId, double semiMajorAxis, double eccentricity, double inclination, double longitudeOfAscendingNode, double argumentOfPeriapsis, double meanAnomaly, QuaternionDbl airfRot )
        {
            CelestialBody cb = new CelestialBodyFactory( id ).Create( Vector3Dbl.zero, airfRot );
            LODQuadSphere lqs = cb.gameObject.AddComponent<LODQuadSphere>();
            lqs.SetMode( LODQuadMode.VisualAndCollider );
            lqs.EdgeSubdivisions = 5;
            lqs.MaxDepth = 16;
            lqs.Materials = _earthMaterial;
            lqs.PoIGetter = () => VesselManager.LoadedVessels.Select( v => v.ReferenceFrameTransform.AbsolutePosition );

            TrajectoryTransform comp = cb.gameObject.AddComponent<TrajectoryTransform>();
            comp.IsAttractor = true;
            comp.Trajectory = new KeplerianOrbit( Time.TimeManager.UT, parentId, semiMajorAxis, eccentricity, inclination, longitudeOfAscendingNode, argumentOfPeriapsis, meanAnomaly, cb.Mass );

            return cb;
        }

        public static CelestialBody CreateCB( string id, Vector3Dbl airfPos, Vector3Dbl airfVel, QuaternionDbl airfRot )
        {
            CelestialBody cb = new CelestialBodyFactory( id ).Create( airfPos, airfRot );
            LODQuadSphere lqs = cb.gameObject.AddComponent<LODQuadSphere>();
            lqs.SetMode( LODQuadMode.Visual );
            lqs.EdgeSubdivisions = 6;
            lqs.MaxDepth = 14;
            lqs.Materials = _earthMaterial;
            lqs.PoIGetter = () => new Vector3Dbl[]
            {
                SceneReferenceFrameManager.ReferenceFrame.TransformPosition( GameplaySceneCameraManager.NearCamera.transform.position ),
                SceneReferenceFrameManager.ReferenceFrame.TransformPosition( GameplaySceneCameraManager.NearCamera.transform.position + GameplaySceneCameraManager.NearCamera.transform.forward * 500 ),
            };
            lqs.SetJobs( new ILODQuadModifier[]
            {
                new LODQuadModifier_InitializeMesh(),
                new LODQuadModifier_Heightmap()
                {
                    HeightmapXn = AssetRegistry.Get<Texture2D>( "Vanilla::Assets/EarthHeight2" ),
                    HeightmapXp = AssetRegistry.Get<Texture2D>( "Vanilla::Assets/EarthHeight2" ),
                    HeightmapYn = AssetRegistry.Get<Texture2D>( "Vanilla::Assets/EarthHeight2" ),
                    HeightmapYp = AssetRegistry.Get<Texture2D>( "Vanilla::Assets/EarthHeight2" ),
                    HeightmapZn = AssetRegistry.Get<Texture2D>( "Vanilla::Assets/EarthHeight2" ),
                    HeightmapZp = AssetRegistry.Get<Texture2D>( "Vanilla::Assets/EarthHeight2" ),
                    MinLevel = -10921,
                    MaxLevel = 9606
                }
            }, new ILODQuadModifier[]
            {
                new LODQuadModifier_FinalizeMesh(),
            } );

            LODQuadSphere lqs2 = cb.gameObject.AddComponent<LODQuadSphere>();
            lqs2.SetMode( LODQuadMode.Collider );
            lqs2.EdgeSubdivisions = 5;
            lqs2.MaxDepth = 14;
            lqs2.PoIGetter = () => VesselManager.LoadedVessels.Select( v => v.ReferenceFrameTransform.AbsolutePosition );
            lqs2.SetJobs( new ILODQuadModifier[]
            {
                new LODQuadModifier_InitializeMesh(),
                new LODQuadModifier_Heightmap()
                {
                    HeightmapXn = AssetRegistry.Get<Texture2D>( "Vanilla::Assets/EarthHeight" ),
                    HeightmapXp = AssetRegistry.Get<Texture2D>( "Vanilla::Assets/EarthHeight" ),
                    HeightmapYn = AssetRegistry.Get<Texture2D>( "Vanilla::Assets/EarthHeight" ),
                    HeightmapYp = AssetRegistry.Get<Texture2D>( "Vanilla::Assets/EarthHeight" ),
                    HeightmapZn = AssetRegistry.Get<Texture2D>( "Vanilla::Assets/EarthHeight" ),
                    HeightmapZp = AssetRegistry.Get<Texture2D>( "Vanilla::Assets/EarthHeight" ),
                    MinLevel = -10921,
                    MaxLevel = 9606
                }
            }, new ILODQuadModifier[]
            {
                new LODQuadModifier_FinalizeMesh(),
            }, new ILODQuadModifier[]
            {
                new LODQuadModifier_BakeCollisionData(),
            } );

            LODQuadSphere lqsWater = cb.gameObject.AddComponent<LODQuadSphere>();
            lqsWater.SetMode( LODQuadMode.Visual );
            lqsWater.EdgeSubdivisions = 6;
            lqsWater.MaxDepth = 10;
            var mat = AssetRegistry.Get<Material>( "builtin::Resources/New Material 2" );
            lqsWater.Materials = new Material[] { mat, mat, mat, mat, mat, mat };
            lqsWater.PoIGetter = () => new Vector3Dbl[]
            {
                SceneReferenceFrameManager.ReferenceFrame.TransformPosition( GameplaySceneCameraManager.NearCamera.transform.position ),
                SceneReferenceFrameManager.ReferenceFrame.TransformPosition( GameplaySceneCameraManager.NearCamera.transform.position + GameplaySceneCameraManager.NearCamera.transform.forward * 500 ),
            };
            lqsWater.SetJobs( new ILODQuadModifier[]
            {
                new LODQuadModifier_InitializeMesh(),
                new LODQuadModifier_Displace()
                {
                    Offset = 760
                }
            }, new ILODQuadModifier[]
            {
                new LODQuadModifier_FinalizeMesh(),
            } );

            TrajectoryTransform comp = cb.gameObject.AddComponent<TrajectoryTransform>();
            comp.IsAttractor = true;
            comp.Trajectory = new NewtonianOrbit( Time.TimeManager.UT, airfPos, airfVel, Vector3Dbl.zero, cb.Mass );
            return cb;
        }

        public static CelestialBody CreateCBNonAttractor( string id, Vector3Dbl airfPos, Vector3Dbl airfVel, QuaternionDbl airfRot )
        {
            CelestialBody cb = new CelestialBodyFactory( id ).Create( airfPos, airfRot );
            LODQuadSphere lqs = cb.gameObject.AddComponent<LODQuadSphere>();
            lqs.Materials = _earthMaterial;
            lqs.PoIGetter = () => VesselManager.LoadedVessels.Select( v => v.ReferenceFrameTransform.AbsolutePosition );

            TrajectoryTransform comp = cb.gameObject.AddComponent<TrajectoryTransform>();
            comp.IsAttractor = false;
            comp.Trajectory = new NewtonianOrbit( Time.TimeManager.UT, airfPos, airfVel, Vector3Dbl.zero, cb.Mass );
            return cb;
        }

        [HSPEventListener( HSPEvent_BEFORE_TIMELINE_NEW.ID, CREATE_CELESTIAL_BODIES )]
        [HSPEventListener( HSPEvent_BEFORE_TIMELINE_LOAD.ID, CREATE_CELESTIAL_BODIES )]
        public static void CreateDefaultPlanetarySystem()
        {
            A();

            QuaternionDbl orientation = Quaternion.Euler( 270, 0, 0 );

            CelestialBody cbSun = CreateCB( "sun", Vector3Dbl.zero, orientation );
            //CelestialBody cb = CreateCB( "main", "sun", 150_000_000_000, 0, 0, 0, 0, 0, orientation );
            CelestialBody cb = CreateCB( "main", new Vector3Dbl( 150_000_000_000, 0, 0 ), new Vector3Dbl( 0, 29749.1543788567, 0 ), orientation );

            //CelestialBody cb = CreateCB( "main", Vector3Dbl.zero, Vector3Dbl.zero, orientation );
            //CelestialBody cb = CreateCB( "main", new Vector3Dbl( 150_000_000_000_000_000, 0, 0 ), new Vector3Dbl( 0, 29749.1543788567, 0 ), orientation );

            //CelestialBody cb1 = CreateCB( "moon1", "main", 440_000_00, 0, 0, 0, 0, 0, orientation );
            //CelestialBody cb1 = CreateCB( "moon2", new Vector3Dbl( 440_000_00, 100_000_000, 0 ), new Vector3Dbl( 0, 0, 0 ), orientation );
            //CelestialBody cb1 = CreateCB( "moon1", new Vector3Dbl( 440_000_00, 0, 0 ), orientation );
            //CelestialBody cb2 = CreateCB( "moon2", new Vector3Dbl( 440_000_000, 100_000_000, 0 ), orientation );
            //CelestialBody cb_farawayTEST = CreateCB( "far", new Vector3Dbl( 440_000_000_0.0, 100_000_000, 0 ), orientation );
            //CelestialBody cb_farawayTEST2 = CreateCB( "further", new Vector3Dbl( 440_000_000_00.0, 100_000_000, 0 ), orientation );

            //CelestialBody cb_farawayTEST3FAR = CreateCB( "100ly", new Vector3Dbl( 1e18, 100_000_000, 0 ), QuaternionDbl.identity ); // 1e18 is 100 ly away.
            // stuff really far away throws invalid world AABB and such. do not enable these, you can't see them anyway. 100 ly seems to work, but further away is a no-no.



            CelestialBodyManager.Get( "sun" ).ReferenceFrameTransform.AbsoluteAngularVelocity = new Vector3Dbl( 0, -1, 0 );
            CelestialBodyManager.Get( "main" ).ReferenceFrameTransform.AbsoluteAngularVelocity = new Vector3Dbl( 0, -7.2921159e-5, 0 );
        }
    }
}