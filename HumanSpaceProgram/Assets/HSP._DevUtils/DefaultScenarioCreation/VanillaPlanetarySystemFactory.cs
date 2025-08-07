using HSP.CelestialBodies;
using HSP.CelestialBodies.Atmospheres;
using HSP.CelestialBodies.Surfaces;
using HSP.ReferenceFrames;
using HSP.Trajectories;
using HSP.Trajectories.AccelerationProviders;
using HSP.Trajectories.TrajectoryIntegrators;
using HSP.Vanilla.CelestialBodies;
using HSP.Vanilla.Scenes.GameplayScene;
using HSP.Vanilla.Scenes.GameplayScene.Cameras;
using HSP.Vanilla.Trajectories;
using HSP.Vessels;
using System;
using System.Linq;
using UnityEngine;
using UnityPlus.AssetManagement;

namespace HSP._DevUtils
{
    public static class VanillaPlanetarySystemFactory
    {
        public const string CREATE_CELESTIAL_BODIES = HSPEvent.NAMESPACE_HSP + ".create_universe";

        private static Material[] _earthMaterial = new Material[6];

        /*private static void A()
        {
            var cbShader = AssetRegistry.Get<Shader>( "builtin::HSP.CelestialBodies/Surfaces/TerrainShader" );
            var cbTex = new Texture2D[]
            {
                AssetRegistry.Get<Texture2D>( "Vanilla::Assets/earth_color_xn" ),
                AssetRegistry.Get<Texture2D>( "Vanilla::Assets/earth_color_xp" ),
                AssetRegistry.Get<Texture2D>( "Vanilla::Assets/earth_color_yn" ),
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

                var shader = mat.shader;
                string name = shader.GetPropertyName( i );
                var pn = Shader.PropertyToID( "_MainTex" );
                var tex = mat.GetTexture( pn );

                var data = SerializationUnit.Serialize( mat );
                new JsonSerializedDataHandler( HumanSpaceProgramContent.GetAssetPath( "Vanilla::Assets/earth_material_" + i, "jsonmat" ) )
                    .Write( data );
            }
        }*/

        private static CelestialBody CreateCB( string id, Vector3Dbl airfPos, QuaternionDbl airfRot )
        {
            CelestialBody cb = new CelestialBodyFactory( id ) { radius = 696_340_000.0, mass = 1.989e30 }.Create( GameplaySceneM.Instance, airfPos, airfRot );
            LODQuadSphere lqs = cb.gameObject.AddComponent<LODQuadSphere>();
            lqs.SetMode( LODQuadMode.VisualAndCollider );
            lqs.EdgeSubdivisions = 4;
            lqs.MaxDepth = 10;
            lqs.Materials = _earthMaterial;
            lqs.PoIGetter = new AllLoadedVesselsPOIGetter();
            lqs.SetJobs( new ILODQuadModifier[]
            {
                new LODQuadModifier_InitializeMesh(),
            }, new ILODQuadModifier[]
            {
                new LODQuadModifier_FinalizeMesh(),
            } );

            TrajectoryTransform comp = cb.gameObject.AddComponent<TrajectoryTransform>();
            comp.IsAttractor = true;
            comp.Integrator = new EulerIntegrator();
            comp.SetAccelerationProviders( new ITrajectoryStepProvider[] { } );
            //comp.TrajectoryIntegrator = new FixedOrbit( Time.TimeManager.UT, airfPos, airfRot, cb.Mass );
            return cb;
        }

        private static CelestialBody CreateCB( string id, string parentId, double semiMajorAxis, double eccentricity, double inclination, double longitudeOfAscendingNode, double argumentOfPeriapsis, double meanAnomaly, QuaternionDbl airfRot )
        {
            throw new NotImplementedException();
            CelestialBody cb = new CelestialBodyFactory( id ).Create( GameplaySceneM.Instance, Vector3Dbl.zero, airfRot );
            LODQuadSphere lqs = cb.gameObject.AddComponent<LODQuadSphere>();
            lqs.SetMode( LODQuadMode.VisualAndCollider );
            lqs.EdgeSubdivisions = 5;
            lqs.MaxDepth = 16;
            lqs.Materials = _earthMaterial;
            lqs.PoIGetter = new AllLoadedVesselsPOIGetter();

            TrajectoryTransform comp = cb.gameObject.AddComponent<TrajectoryTransform>();
            comp.IsAttractor = true;
            //comp.TrajectoryIntegrator = new KeplerianOrbit( Time.TimeManager.UT, parentId, semiMajorAxis, eccentricity, inclination, longitudeOfAscendingNode, argumentOfPeriapsis, meanAnomaly, cb.Mass );

            return cb;
        }

        public static CelestialBody CreateCB( string id, Vector3Dbl airfPos, Vector3Dbl airfVel, QuaternionDbl airfRot )
        {
            CelestialBody cb = new CelestialBodyFactory( id ).Create( GameplaySceneM.Instance, airfPos, airfRot );
            LODQuadSphere lqs = cb.gameObject.AddComponent<LODQuadSphere>();
            lqs.SetMode( LODQuadMode.Visual );
            lqs.EdgeSubdivisions = 6;
            lqs.MaxDepth = 14;
            lqs.Materials = _earthMaterial;
            lqs.PoIGetter = new ActiveCameraPOIGetter();
            lqs.SetJobs( new ILODQuadModifier[]
            {
                new LODQuadModifier_InitializeMesh(),
                new LODQuadModifier_Heightmap()
                {
                    HeightmapXn = AssetRegistry.Get<Texture2D>( "Vanilla::Assets/earth_height_xn" ),
                    HeightmapXp = AssetRegistry.Get<Texture2D>( "Vanilla::Assets/earth_height_xp" ),
                    HeightmapYn = AssetRegistry.Get<Texture2D>( "Vanilla::Assets/earth_height_yn" ),
                    HeightmapYp = AssetRegistry.Get<Texture2D>( "Vanilla::Assets/earth_height_yp" ),
                    HeightmapZn = AssetRegistry.Get<Texture2D>( "Vanilla::Assets/earth_height_zn" ),
                    HeightmapZp = AssetRegistry.Get<Texture2D>( "Vanilla::Assets/earth_height_zp" ),
                    MinLevel = -11684,
                    MaxLevel = 8848
                }
            }, new ILODQuadModifier[]
            {
                new LODQuadModifier_FinalizeMesh(),
            } );

            LODQuadSphere lqs2 = cb.gameObject.AddComponent<LODQuadSphere>();
            lqs2.SetMode( LODQuadMode.Collider );
            lqs2.EdgeSubdivisions = 5;
            lqs2.MaxDepth = 14;
            lqs2.PoIGetter = new AllLoadedVesselsPOIGetter();
            lqs2.SetJobs( new ILODQuadModifier[]
            {
                new LODQuadModifier_InitializeMesh(),
                new LODQuadModifier_Heightmap()
                {
                    HeightmapXn = AssetRegistry.Get<Texture2D>( "Vanilla::Assets/earth_height_xn" ),
                    HeightmapXp = AssetRegistry.Get<Texture2D>( "Vanilla::Assets/earth_height_xp" ),
                    HeightmapYn = AssetRegistry.Get<Texture2D>( "Vanilla::Assets/earth_height_yn" ),
                    HeightmapYp = AssetRegistry.Get<Texture2D>( "Vanilla::Assets/earth_height_yp" ),
                    HeightmapZn = AssetRegistry.Get<Texture2D>( "Vanilla::Assets/earth_height_zn" ),
                    HeightmapZp = AssetRegistry.Get<Texture2D>( "Vanilla::Assets/earth_height_zp" ),
                    MinLevel = -11684,
                    MaxLevel = 8848
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
            lqsWater.PoIGetter = new ActiveCameraPOIGetter();
            lqsWater.SetJobs( new ILODQuadModifier[]
            {
                new LODQuadModifier_InitializeMesh()
            }, new ILODQuadModifier[]
            {
                new LODQuadModifier_FinalizeMesh()
            } );

            TrajectoryTransform comp = cb.gameObject.AddComponent<TrajectoryTransform>();
            comp.IsAttractor = true;
            comp.Integrator = new EulerIntegrator();
            comp.SetAccelerationProviders( new NBodyAccelerationProvider() );
            comp.ReferenceFrameTransform.AbsoluteVelocity = airfVel;
            //comp.TrajectoryIntegrator = new NewtonianOrbit( Time.TimeManager.UT, airfPos, airfVel, Vector3Dbl.zero, cb.Mass );
            return cb;
        }

        public static CelestialBody CreateCBNonAttractor( string id, Vector3Dbl airfPos, Vector3Dbl airfVel, QuaternionDbl airfRot )
        {
            CelestialBody cb = new CelestialBodyFactory( id ).Create( GameplaySceneM.Instance, airfPos, airfRot );
            LODQuadSphere lqs = cb.gameObject.AddComponent<LODQuadSphere>();
            lqs.Materials = _earthMaterial;
            lqs.PoIGetter = new AllLoadedVesselsPOIGetter();

            TrajectoryTransform comp = cb.gameObject.AddComponent<TrajectoryTransform>();
            comp.IsAttractor = false;
            comp.Integrator = new EulerIntegrator();
            comp.SetAccelerationProviders( new NBodyAccelerationProvider() );
            comp.ReferenceFrameTransform.AbsoluteVelocity = airfVel;
            //comp.TrajectoryIntegrator = new NewtonianOrbit( Time.TimeManager.UT, airfPos, airfVel, Vector3Dbl.zero, cb.Mass );
            return cb;
        }

        public static void CreateDefaultPlanetarySystem()
        {
            _earthMaterial[0] = AssetRegistry.Get<Material>( "Vanilla::Assets/earth_material_0" );
            _earthMaterial[1] = AssetRegistry.Get<Material>( "Vanilla::Assets/earth_material_1" );
            _earthMaterial[2] = AssetRegistry.Get<Material>( "Vanilla::Assets/earth_material_2" );
            _earthMaterial[3] = AssetRegistry.Get<Material>( "Vanilla::Assets/earth_material_3" );
            _earthMaterial[4] = AssetRegistry.Get<Material>( "Vanilla::Assets/earth_material_4" );
            _earthMaterial[5] = AssetRegistry.Get<Material>( "Vanilla::Assets/earth_material_5" );

            QuaternionDbl orientation = Quaternion.Euler( 270, 0, 0 );

            CelestialBody cbSun = CreateCB( "sun", Vector3Dbl.zero, orientation );
            //CelestialBody cb = CreateCB( "main", "sun", 150_000_000_000, 0, 0, 0, 0, 0, orientation );
            CelestialBody cb = CreateCB( "main", new Vector3Dbl( 150_000_000_000, 0, 0 ), new Vector3Dbl( 0, 29749.1543788567, 0 ), orientation );
            var atm = cb.gameObject.AddComponent<Atmosphere>();
            atm.Height = 140_000;
            atm.sharedMaterial = AssetRegistry.Get<Material>( "builtin::Resources/Materials/Atmosphere" );

            //cb = CreateCB( "main2", new Vector3Dbl( 150_000_000_000, 400_000_000, 0 ), new Vector3Dbl( 0, 29749.1543788567, 0 ), orientation );
            //atm = cb.gameObject.AddComponent<Atmosphere>();
            //atm.Height = 140_000;
            //atm.sharedMaterial = AssetRegistry.Get<Material>( "builtin::Resources/Materials/Atmosphere" );

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