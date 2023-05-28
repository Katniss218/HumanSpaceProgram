using KSS.Camera;
using KSS.Core;
using KSS.Core.Buildings;
using KSS.Core.ReferenceFrames;
using KSS.Core.ResourceFlowSystem;
using KSS.Core.Serialization;
using KSS.Functionalities;
using KSS.Terrain;
using UnityEngine;
using UnityEngine.UI;

namespace KSS.DevUtils
{
    /// <summary>
    /// Game manager for testing.
    /// </summary>
    public class zzzDevUtilsGameManager : MonoBehaviour
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

        CelestialBody CreateCB( Vector3Dbl pos )
        {
            CelestialBody cb = new CelestialBodyFactory().Create( pos );
            LODQuadSphere lqs = cb.gameObject.AddComponent<LODQuadSphere>();
            return cb;
        }

        private void Awake()
        {
            LODQuadSphere.cbShader = this.cbShader;
            LODQuadSphere.cbTex = this.cbTextures;
        }

        void Start()
        {
            /*normalmap = new RenderTexture( heightmap.width, heightmap.height, 8, RenderTextureFormat.ARGB32 );
            normalmap.enableRandomWrite = true;

            shader.SetTexture( shader.FindKernel( "CalculateNormalMap" ), Shader.PropertyToID( "heightMap" ), heightmap );
            shader.SetTexture( shader.FindKernel( "CalculateNormalMap" ), Shader.PropertyToID( "normalMap" ), normalmap );
            shader.SetFloat( Shader.PropertyToID( "strength" ), 5.0f );
            shader.Dispatch( shader.FindKernel( "CalculateNormalMap" ), heightmap.width / 8, heightmap.height / 8, 1 );

            uiImage.texture = normalmap;*/

            CelestialBody cb = CreateCB( Vector3Dbl.zero );

            CelestialBody cb1 = CreateCB( new Vector3Dbl( 440_000_000, 0, 0 ) );
            CelestialBody cb2 = CreateCB( new Vector3Dbl( 440_000_000, 100_000_000, 0 ) );
            CelestialBody cb_farawayTEST = CreateCB( new Vector3Dbl( 440_000_000_0.0, 100_000_000, 0 ) );
            CelestialBody cb_farawayTEST2 = CreateCB( new Vector3Dbl( 440_000_000_00.0, 100_000_000, 0 ) );

            CelestialBody cb_farawayTEST3FAR = CreateCB( new Vector3Dbl( 1e18, 100_000_000, 0 ) ); // 1e18 is 100 ly away.
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

            PartFactory intertank = new PartFactory( new AssetPartSource( "part.intertank" ) );
            PartFactory tank = new PartFactory( new AssetPartSource( "part.tank" ) );
            PartFactory tankLong = new PartFactory( new AssetPartSource( "part.tank_long" ) );
            PartFactory engine = new PartFactory( new AssetPartSource( "part.engine" ) );

            Vessel v = fac.CreatePartless( airfPosition, rotation, Vector3.zero, Vector3.zero );
            Part root = intertank.CreateRoot( v );

            Part tankP = tank.Create( root, new Vector3( 0, -1.625f, 0 ), Quaternion.identity );
            Part tankL1 = tankLong.Create( root, new Vector3( 0, 2.625f, 0 ), Quaternion.identity );
            var t1 = tankLong.Create( root, new Vector3( 2, 2.625f, 0 ), Quaternion.identity );
            var t2 = tankLong.Create( root, new Vector3( -2, 2.625f, 0 ), Quaternion.identity );
            Part engineP = engine.Create( tankP, new Vector3( 0, -3.45533f, 0 ), Quaternion.identity );

            FBulkConnection conn = tankP.gameObject.AddComponent<FBulkConnection>();
            conn.End1.ConnectTo( tankL1.GetComponent<FBulkContainer_Sphere>() );
            conn.End1.Position = new Vector3( 0.0f, -2.5f, 0.0f );
            conn.End2.ConnectTo( tankP.GetComponent<FBulkContainer_Sphere>() );
            conn.End2.Position = new Vector3( 0.0f, 1.5f, 0.0f );
            conn.CrossSectionArea = 0.1f;

            Substance sbs1 = Substance.RegisteredResources["substance.f"];
            Substance sbs2 = Substance.RegisteredResources["substance.ox"];

            var tankSmallTank = tankP.GetComponent<FBulkContainer_Sphere>();
            tankSmallTank.Contents = new SubstanceStateCollection(
                new SubstanceState[] {
                    new SubstanceState( tankSmallTank.MaxVolume * ((sbs1.Density + sbs2.Density) / 2f) / 2f, sbs1 ),
                    new SubstanceState( tankSmallTank.MaxVolume * ((sbs1.Density + sbs2.Density) / 2f) / 2f, sbs2 )} );

            FBulkConnection conn2 = engineP.gameObject.AddComponent<FBulkConnection>();
            conn2.End1.ConnectTo( tankP.GetComponent<FBulkContainer_Sphere>() );
            conn2.End1.Position = new Vector3( 0.0f, -1.5f, 0.0f );
            conn2.End2.ConnectTo( engineP.GetComponent<FRocketEngine>() );
            conn2.End2.Position = new Vector3( 0.0f, 0.0f, 0.0f );
            conn2.CrossSectionArea = 60f;

            t1.gameObject.AddComponent<FVesselSeparator>();
            t2.gameObject.AddComponent<FVesselSeparator>();

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