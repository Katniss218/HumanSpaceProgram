using KSS.Cameras;
using KSS.Core;
using KSS.Core.ReferenceFrames;
using KSS.Core.ResourceFlowSystem;
using KSS.Components;
using KSS.CelestialBodies.Surface;
using UnityEngine;
using UnityEngine.UI;
using KSS.Core.Serialization;
using KSS.Core.Components;
using System;

namespace KSS.DevUtils
{
    /// <summary>
    /// Game manager for testing.
    /// </summary>
    public class DevUtilsGameplayManager : MonoBehaviour
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

        void Awake()
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
        }

        static Building launchSite;

        [HSPEventListener( HSPEvent.TIMELINE_AFTER_NEW, "devutils.timeline.new.after" )]
        static void OnAfterCreateDefault( object e )
        {
            CelestialBody body = CelestialBodyManager.Get( "main" );
            Vector3 localPos = CoordinateUtils.GeodeticToEuclidean( 28.5857702f, -80.6507262f, (float)(body.Radius + 1.0) );

            PartFactory launchSitePart = new PartFactory( new AssetPartSource( "builtin::Resources/Prefabs/testlaunchsite" ) );
            launchSite = new BuildingFactory().CreatePartless( body, localPos, Quaternion.FromToRotation( Vector3.up, localPos.normalized ) );

            Transform root = launchSitePart.CreateRoot( launchSite );

            var v = CreateVessel( launchSite );
            VesselManager.ActiveVessel = v.RootPart.GetVessel();
            FindObjectOfType<CameraController>().ReferenceObject = v.RootPart.transform;

            VesselManager.ActiveVessel.transform.GetComponent<Rigidbody>().angularDrag = 1; // temp, doesn't veer off course.
        }

        static Vessel CreateVessel( Building launchSite )
        {
            if( launchSite == null )
            {
                throw new ArgumentNullException( nameof( launchSite ), "launchSite is null" );
            }
            Vector3Dbl spawnerPosAirf = SceneReferenceFrameManager.SceneReferenceFrame.TransformPosition(
                launchSite.gameObject.GetComponentInChildren<FLaunchSiteMarker>().transform.position );
            QuaternionDbl spawnerRotAirf = SceneReferenceFrameManager.SceneReferenceFrame.TransformRotation(
                launchSite.gameObject.GetComponentInChildren<FLaunchSiteMarker>().transform.rotation );

            var v2 = CreateDummyVessel( spawnerPosAirf, spawnerRotAirf ); // position is temp.

            Vector3 bottomBoundPos = v2.GetBottomPosition();
            Vector3Dbl closestBoundAirf = SceneReferenceFrameManager.SceneReferenceFrame.TransformPosition( bottomBoundPos );
            Vector3Dbl closestBoundToVesselAirf = v2.AIRFPosition - closestBoundAirf;
            Vector3Dbl airfPos = spawnerPosAirf + closestBoundToVesselAirf;
            v2.AIRFPosition = airfPos;
            return v2;
        }

        private void Update()
        {
            if( Input.GetKeyDown( KeyCode.F5 ) )
            {
                CreateVessel( launchSite );
            }
        }

        static Vessel CreateDummyVessel( Vector3Dbl airfPosition, QuaternionDbl rotation )
        {
            VesselFactory fac = new VesselFactory();

            PartFactory intertank = new PartFactory( new AssetPartSource( "builtin::Resources/Prefabs/Parts/part.intertank" ) );
            PartFactory tank = new PartFactory( new AssetPartSource( "builtin::Resources/Prefabs/Parts/part.tank" ) );
            PartFactory tankLong = new PartFactory( new AssetPartSource( "builtin::Resources/Prefabs/Parts/part.tank_long" ) );
            PartFactory engine = new PartFactory( new AssetPartSource( "builtin::Resources/Prefabs/Parts/part.engine" ) );

            Vessel v = fac.CreatePartless( airfPosition, rotation, Vector3.zero, Vector3.zero );
            Transform root = intertank.CreateRoot( v );

            Transform tankP = tank.Create( root, new Vector3( 0, -1.625f, 0 ), Quaternion.identity );
            Transform tankL1 = tankLong.Create( root, new Vector3( 0, 2.625f, 0 ), Quaternion.identity );
            Transform t1 = tankLong.Create( root, new Vector3( 2, 2.625f, 0 ), Quaternion.identity );
            Transform t2 = tankLong.Create( root, new Vector3( -2, 2.625f, 0 ), Quaternion.identity );
            Transform engineP = engine.Create( tankP, new Vector3( 0, -3.45533f, 0 ), Quaternion.identity );

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

            TrailRenderer tr = v.gameObject.AddComponent<TrailRenderer>();
            tr.material = FindObjectOfType<DevUtilsGameplayManager>().Material;
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