using KatnisssSpaceSimulator.Buildings;
using KatnisssSpaceSimulator.Camera;
using KatnisssSpaceSimulator.Core;
using KatnisssSpaceSimulator.Core.Managers;
using KatnisssSpaceSimulator.Core.Serialization;
using KatnisssSpaceSimulator.Core.ReferenceFrames;
using KatnisssSpaceSimulator.Functionalities;
using KatnisssSpaceSimulator.Terrain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using KatnisssSpaceSimulator.Core.ResourceFlowSystem;
using KatnisssSpaceSimulator.UI;

namespace KatnisssSpaceSimulator
{
    /// <summary>
    /// Game manager for testing.
    /// </summary>
    public class zzzTestGameManager : MonoBehaviour
    {
        public Shader cbShader;
        public Texture2D[] cbTextures = new Texture2D[6];

        public Mesh Mesh;
        public Material Material;

        public GameObject TestLaunchSite;

        public Texture2D heightmap;
        public RenderTexture normalmap;
        public ComputeShader shader;
        public RawImage uiImage;

        void Start()
        {
            /*normalmap = new RenderTexture( heightmap.width, heightmap.height, 8, RenderTextureFormat.ARGB32 );
            normalmap.enableRandomWrite = true;

            shader.SetTexture( shader.FindKernel( "CalculateNormalMap" ), Shader.PropertyToID( "heightMap" ), heightmap );
            shader.SetTexture( shader.FindKernel( "CalculateNormalMap" ), Shader.PropertyToID( "normalMap" ), normalmap );
            shader.SetFloat( Shader.PropertyToID( "strength" ), 5.0f );
            shader.Dispatch( shader.FindKernel( "CalculateNormalMap" ), heightmap.width / 8, heightmap.height / 8, 1 );

            uiImage.texture = normalmap;*/

            CelestialBody cb = new CelestialBodyFactory().Create( Vector3Dbl.zero );

            CelestialBody cb1 = new CelestialBodyFactory().Create( new Vector3Dbl( 440_000_000, 0, 0 ) );
            CelestialBody cb2 = new CelestialBodyFactory().Create( new Vector3Dbl( 440_000_000, 100_000_000, 0 ) );
            CelestialBody cb_farawayTEST = new CelestialBodyFactory().Create( new Vector3Dbl( 440_000_000_0.0, 100_000_000, 0 ) );
            CelestialBody cb_farawayTEST2 = new CelestialBodyFactory().Create( new Vector3Dbl( 440_000_000_00.0, 100_000_000, 0 ) );

            CelestialBody cb_farawayTEST3FAR = new CelestialBodyFactory().Create( new Vector3Dbl( 1e18, 100_000_000, 0 ) ); // 1e18 is 100 ly away.
            // stuff really far away throws invalid world AABB and such. do not enable these, you can't see them anyway. 100 ly seems to work, but further away is a no-no.


            CelestialBodySurface srf = cb.GetComponent<CelestialBodySurface>();
            var group = srf.SpawnGroup( "aabb", 28.5857702f, -80.6507262f, (float)(cb.Radius + 1.0) );
            LaunchSite launchSite = new LaunchSiteFactory() { Prefab = this.TestLaunchSite }.Create( group, Vector3.zero, Quaternion.identity );

            Vector3Dbl spawnerPosAirf = launchSite.GetSpawnerAIRFPosition();

            Vessel v = CreateDummyVessel( new Vector3Dbl( 1, 0.0, 0.0 ), launchSite.Spawner.rotation ); // position is temp.

            Vector3 bottomBoundPos = v.GetBottomPosition();
            Vector3Dbl closestBoundAirf = SceneReferenceFrameManager.SceneReferenceFrame.TransformPosition( bottomBoundPos );
            Vector3Dbl closestBoundToVesselAirf = v.AIRFPosition - closestBoundAirf;
            Vector3Dbl pos = spawnerPosAirf + closestBoundToVesselAirf;
            v.SetPosition( pos );

            VesselManager.ActiveVessel = v.RootPart.Vessel;
            FindObjectOfType<CameraController>().ReferenceObject = v.RootPart.transform;

            VesselManager.ActiveVessel.transform.GetComponent<Rigidbody>().angularDrag = 1; // temp, doesn't veer off course.
        }

        Vessel CreateDummyVessel( Vector3Dbl airfPosition, Quaternion rotation )
        {
            VesselFactory fac = new VesselFactory();
            //PartFactory pfac = new PartFactory( new DummyPartSource() );
            //PartFactory efac = new PartFactory( new ResourcePartSource( "Prefabs/engine_part" ) );

            PartFactory intertank = new PartFactory( new AssetPartSource( "Prefabs/intertank" ) );
            PartFactory tank = new PartFactory( new AssetPartSource( "Prefabs/tank" ) );
            PartFactory tankLong = new PartFactory( new AssetPartSource( "Prefabs/tank_long" ) );
            PartFactory engine = new PartFactory( new AssetPartSource( "Prefabs/engine" ) );

            Vessel v = fac.CreatePartless( airfPosition, rotation );
            Part root = intertank.CreateRoot( v );

            Part tankP = tank.Create( root, new Vector3( 0, -1.625f, 0 ), Quaternion.identity );
            Part tankL1 = tankLong.Create( root, new Vector3( 0, 2.625f, 0 ), Quaternion.identity );
            tankLong.Create( root, new Vector3( 2, 2.625f, 0 ), Quaternion.identity );
            tankLong.Create( root, new Vector3( -2, 2.625f, 0 ), Quaternion.identity );
            Part engineP = engine.Create( tankP, new Vector3( 0, -3.45533f, 0 ), Quaternion.identity );

            FBulkConnection conn = tankP.gameObject.AddComponent<FBulkConnection>();
            conn.End1.ConnectTo( tankL1.GetComponent<FBulkContainer_Sphere>() );
            conn.End1.Position = new Vector3( 0.0f, -2.5f, 0.0f );
            conn.End2.ConnectTo( tankP.GetComponent<FBulkContainer_Sphere>() );
            conn.End2.Position = new Vector3( 0.0f, 1.5f, 0.0f );
            conn.CrossSectionArea = 0.1f;

            const float DENSITY = 1000f;

            var tankTank = tankP.GetComponent<FBulkContainer_Sphere>();
            tankTank.Contents = new SubstanceStateCollection(
                new SubstanceState[] { new SubstanceState( tankTank.MaxVolume * DENSITY, new Substance() { Density = DENSITY, DisplayName = "aa", ID = "substance.aa" } ) } );

            FBulkConnection conn2 = engineP.gameObject.AddComponent<FBulkConnection>();
            conn2.End1.ConnectTo( tankP.GetComponent<FBulkContainer_Sphere>() );
            conn2.End1.Position = new Vector3( 0.0f, -1.5f, 0.0f );
            conn2.End2.ConnectTo( engineP.GetComponent<FRocketEngine>() );
            conn2.End2.Position = new Vector3( 0.0f, 0.0f, 0.0f );
            conn2.CrossSectionArea = 60f;

            //FindObjectOfType<IResourceContainerUI>().Obj = tankL1.GetComponent<FBulkContainer_Sphere>();

            // conn.End1.Container.Volume = conn.End1.Container.MaxVolume; // 99999f;
            //conn.End1.Container.MaxVolume = 99999f;
            // conn.End2.Container.Volume = 0f;
            //conn.End2.Container.MaxVolume = 99999f;


            //const int partcount = 5;
            //const int engcount = 5;

            /*Part parent = v.RootPart;
            for( int i = 0; i < partcount; i++ )
            {
                Part part = pfac.Create( parent, new Vector3( 0, 1.25f * i + 1.25f * engcount, 0 ), Quaternion.identity );

                parent = parent.Children[0];
            }

            parent = v.RootPart;
            for( int i = 0; i < engcount; i++ )
            {
                Part engine = efac.Create( parent, new Vector3( 0, 1.125f * i, 0 ), Quaternion.identity );

                parent = parent.Children[0];
            }*/

            TrailRenderer tr = v.gameObject.AddComponent<TrailRenderer>();
            tr.material = Material;
            tr.time = 250;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey( 0, 5.0f );
            curve.AddKey( 1, 2.5f );
            tr.widthCurve = curve;
            tr.minVertexDistance = 50f;

            return v;
        }
    }
}