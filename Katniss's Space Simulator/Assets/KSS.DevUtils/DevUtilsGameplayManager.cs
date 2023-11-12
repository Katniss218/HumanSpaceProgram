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
using UnityPlus.AssetManagement;
using KSS.AssetLoaders;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Strategies;
using System.IO;
using System.Collections;

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
        static Vessel vessel;

        [HSPEventListener( HSPEvent.STARTUP_IMMEDIATELY, "devutils.load_game_data" )]
        static void LoadGameData( object e )
        {
            AssetRegistry.Register( "substance.f", new Substance() { Density = 1000, DisplayName = "Fuel", UIColor = new Color( 1.0f, 0.3764706f, 0.2509804f ) } );
            AssetRegistry.Register( "substance.ox", new Substance() { Density = 1000, DisplayName = "Oxidizer", UIColor = new Color( 0.2509804f, 0.5607843f, 1.0f ) } );
        }

        [HSPEventListener( HSPEvent.TIMELINE_AFTER_NEW, "devutils.timeline.new.after" )]
        static void OnAfterCreateDefault( object e )
        {
            CelestialBody body = CelestialBodyManager.Get( "main" );
            Vector3 localPos = CoordinateUtils.GeodeticToEuclidean( 28.5857702f, -80.6507262f, (float)(body.Radius + 1.0) );

            launchSite = BuildingFactory.CreatePartless( body, localPos, Quaternion.FromToRotation( Vector3.up, localPos.normalized ) );

            GameObject launchSitePrefab = AssetRegistry.Get<GameObject>( "builtin::Resources/Prefabs/testlaunchsite" );
            GameObject root = InstantiateLocal( launchSitePrefab, launchSite.transform, Vector3.zero, Quaternion.identity );

            var v = CreateVessel( launchSite );
            ActiveObjectManager.ActiveObject = v.RootPart.GetVessel().gameObject;
            vessel = v;
        }

        static Vessel CreateVessel( Building launchSite )
        {
            if( launchSite == null )
            {
                throw new ArgumentNullException( nameof( launchSite ), "launchSite is null" );
            }

            FLaunchSiteMarker launchSiteSpawner = launchSite.gameObject.GetComponentInChildren<FLaunchSiteMarker>();
            Vector3Dbl spawnerPosAirf = SceneReferenceFrameManager.SceneReferenceFrame.TransformPosition( launchSiteSpawner.transform.position );
            QuaternionDbl spawnerRotAirf = SceneReferenceFrameManager.SceneReferenceFrame.TransformRotation( launchSiteSpawner.transform.rotation );

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
            if( Input.GetKeyDown( KeyCode.F4 ) )
            {
                JsonSeparateFileSerializedDataHandler _designObjDataHandler = new JsonSeparateFileSerializedDataHandler();
                JsonSingleExplicitHierarchyStrategy _designObjStrategy = new JsonSingleExplicitHierarchyStrategy( _designObjDataHandler, () => null );

                VesselMetadata loadedVesselMetadata = new VesselMetadata( "vessel2" );
                loadedVesselMetadata.ReadDataFromDisk();

                // load current vessel from the files defined by metadata's ID.
                Directory.CreateDirectory( loadedVesselMetadata.GetRootDirectory() );
                _designObjDataHandler.ObjectsFilename = Path.Combine( loadedVesselMetadata.GetRootDirectory(), "objects.json" );
                _designObjDataHandler.DataFilename = Path.Combine( loadedVesselMetadata.GetRootDirectory(), "data.json" );

                HSPEvent.EventManager.TryInvoke( HSPEvent.DESIGN_BEFORE_LOAD, null );

                Loader _loader = new Loader( null, null, new Action<ILoader>[] { _designObjStrategy.Load_Object }, new Action<ILoader>[] { _designObjStrategy.Load_Data } );

                _loader.Load();

                FLaunchSiteMarker launchSiteSpawner = launchSite.gameObject.GetComponentInChildren<FLaunchSiteMarker>();
                Vector3Dbl spawnerPosAirf = SceneReferenceFrameManager.SceneReferenceFrame.TransformPosition( launchSiteSpawner.transform.position );
                QuaternionDbl spawnerRotAirf = SceneReferenceFrameManager.SceneReferenceFrame.TransformRotation( launchSiteSpawner.transform.rotation );

                Vessel v2 = VesselFactory.CreatePartless( spawnerPosAirf, spawnerRotAirf, Vector3.zero, Vector3.zero );

                v2.RootPart = _designObjStrategy.LastSpawnedRoot.transform;
                v2.RootPart.localPosition = Vector3.zero;
                v2.RootPart.localRotation = Quaternion.identity;

                Vector3 bottomBoundPos = v2.GetBottomPosition();
                Vector3Dbl closestBoundAirf = SceneReferenceFrameManager.SceneReferenceFrame.TransformPosition( bottomBoundPos );
                Vector3Dbl closestBoundToVesselAirf = v2.AIRFPosition - closestBoundAirf;
                Vector3Dbl airfPos = spawnerPosAirf + closestBoundToVesselAirf;
                v2.AIRFPosition = airfPos;
            }
            if( Input.GetKeyDown( KeyCode.F5 ) )
            {
                CreateVessel( launchSite );
            }
            if( Input.GetKeyDown( KeyCode.F1 ) )
            {
                JsonSeparateFileSerializedDataHandler handler = new JsonSeparateFileSerializedDataHandler();
                JsonSingleExplicitHierarchyStrategy strat = new JsonSingleExplicitHierarchyStrategy( handler, () => null );
                Saver saver = new Saver( null, null, strat.Save_Object, strat.Save_Data );

                string gameDataPath = HumanSpaceProgram.GetGameDataDirectoryPath();
                string partDir;

                VesselMetadata vm;
                partDir = HumanSpaceProgram.GetSavedVesselsDirectoryPath() + "/vessel";
                Directory.CreateDirectory( partDir );
                vm = new VesselMetadata( "vessel" );
                vm.Name = "Engine"; vm.Description = "default"; vm.Author = "Katniss";
                vm.WriteToDisk();
                strat.RootObjectGetter = () => vessel.RootPart.gameObject;
                handler.ObjectsFilename = partDir + "/objects.json";
                handler.DataFilename = partDir + "/data.json";
                saver.Save();

                PartMetadata pm;

                partDir = gameDataPath + "/Vanilla/Parts/engine";
                Directory.CreateDirectory( partDir );
                pm = new PartMetadata( partDir );
                pm.Name = "Engine"; pm.Author = "Katniss"; pm.Categories = new string[] { "engine" };
                pm.WriteToDisk();
                strat.RootObjectGetter = () => AssetRegistry.Get<GameObject>( "builtin::Resources/Prefabs/Parts/engine" );
                handler.ObjectsFilename = partDir + "/objects.json";
                handler.DataFilename = partDir + "/data.json";
                saver.Save();


                partDir = gameDataPath + "/Vanilla/Parts/intertank";
                Directory.CreateDirectory( partDir );
                pm = new PartMetadata( partDir );
                pm.Name = "Intertank"; pm.Author = "Katniss"; pm.Categories = new string[] { "structural" };
                pm.WriteToDisk();
                strat.RootObjectGetter = () => AssetRegistry.Get<GameObject>( "builtin::Resources/Prefabs/Parts/intertank" );
                handler.ObjectsFilename = partDir + "/objects.json";
                handler.DataFilename = partDir + "/data.json";
                saver.Save();


                partDir = gameDataPath + "/Vanilla/Parts/tank";
                Directory.CreateDirectory( partDir );
                pm = new PartMetadata( partDir );
                pm.Name = "Tank"; pm.Author = "Katniss"; pm.Categories = new string[] { "fuel_tank" };
                pm.WriteToDisk();
                strat.RootObjectGetter = () => AssetRegistry.Get<GameObject>( "builtin::Resources/Prefabs/Parts/tank" );
                handler.ObjectsFilename = partDir + "/objects.json";
                handler.DataFilename = partDir + "/data.json";
                saver.Save();


                partDir = gameDataPath + "/Vanilla/Parts/tank_long";
                Directory.CreateDirectory( partDir );
                pm = new PartMetadata( partDir );
                pm.Name = "Long Tank"; pm.Author = "Katniss"; pm.Categories = new string[] { "fuel_tank" };
                pm.WriteToDisk();
                strat.RootObjectGetter = () => AssetRegistry.Get<GameObject>( "builtin::Resources/Prefabs/Parts/tank_long" );
                handler.ObjectsFilename = partDir + "/objects.json";
                handler.DataFilename = partDir + "/data.json";
                saver.Save();
            }
        }

        static GameObject InstantiateLocal( GameObject original, Transform parent, Vector3 pos, Quaternion rot )
        {
            GameObject go = Instantiate( original, parent );
            go.transform.localPosition = pos;
            go.transform.localRotation = rot;
            return go;
        }

        static Vessel CreateDummyVessel( Vector3Dbl airfPosition, QuaternionDbl rotation )
        {
            GameObject intertankPrefab = AssetRegistry.Get<GameObject>( "builtin::Resources/Prefabs/Parts/intertank" );
            GameObject tankPrefab = AssetRegistry.Get<GameObject>( "builtin::Resources/Prefabs/Parts/tank" );
            GameObject tankLongPrefab = AssetRegistry.Get<GameObject>( "builtin::Resources/Prefabs/Parts/tank_long" );
            GameObject enginePrefab = AssetRegistry.Get<GameObject>( "builtin::Resources/Prefabs/Parts/engine" );

            Vessel v = VesselFactory.CreatePartless( airfPosition, rotation, Vector3.zero, Vector3.zero );
            Transform root = InstantiateLocal( intertankPrefab, v.transform, Vector3.zero, Quaternion.identity ).transform;

            Transform tankP = InstantiateLocal( tankPrefab, root, new Vector3( 0, -1.625f, 0 ), Quaternion.identity ).transform;
            Transform tankL1 = InstantiateLocal( tankLongPrefab, root, new Vector3( 0, 2.625f, 0 ), Quaternion.identity ).transform;
            Transform t1 = InstantiateLocal( tankLongPrefab, root, new Vector3( 2, 2.625f, 0 ), Quaternion.identity ).transform;
            Transform t2 = InstantiateLocal( tankLongPrefab, root, new Vector3( -2, 2.625f, 0 ), Quaternion.identity ).transform;
            Transform engineP = InstantiateLocal( enginePrefab, tankP, new Vector3( 0, -3.45533f, 0 ), Quaternion.identity ).transform;
            v.RootPart = root;

            FBulkConnection conn = tankP.gameObject.AddComponent<FBulkConnection>();
            conn.End1.ConnectTo( tankL1.GetComponent<FBulkContainer_Sphere>() );
            conn.End1.Position = new Vector3( 0.0f, -2.5f, 0.0f );
            conn.End2.ConnectTo( tankP.GetComponent<FBulkContainer_Sphere>() );
            conn.End2.Position = new Vector3( 0.0f, 1.5f, 0.0f );
            conn.CrossSectionArea = 0.1f;

            Substance sbsF = AssetRegistry.Get<Substance>( "substance.f" );
            Substance sbsOX = AssetRegistry.Get<Substance>( "substance.ox" );

            var tankSmallTank = tankP.GetComponent<FBulkContainer_Sphere>();
            tankSmallTank.Contents = new SubstanceStateCollection(
                new SubstanceState[] {
                    new SubstanceState( tankSmallTank.MaxVolume * ((sbsF.Density + sbsOX.Density) / 2f) / 2f, sbsF ),
                    new SubstanceState( tankSmallTank.MaxVolume * ((sbsF.Density + sbsOX.Density) / 2f) / 2f, sbsOX )} );

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