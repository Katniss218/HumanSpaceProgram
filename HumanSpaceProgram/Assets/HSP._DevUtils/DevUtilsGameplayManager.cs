using HSP.Effects;
using HSP.CelestialBodies;
using HSP.Content;
using HSP.Content.Vessels;
using HSP.Content.Vessels.Serialization;
using HSP.ReferenceFrames;
using HSP.ResourceFlow;
using HSP.Time;
using HSP.Timelines;
using HSP.Vanilla;
using HSP.Vanilla.Components;
using HSP.Vanilla.Scenes.AlwaysLoadedScene;
using HSP.Vessels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;

namespace HSP._DevUtils
{
    public class UpdateTester : MonoBehaviour
    {
        protected void Update()
        {
            Debug.Log( "A" );
        }
    }
    public class UpdateTesterDerived : UpdateTester
    {
        protected new void Update()
        {
            Debug.Log( "B" );
            base.Update();
        }
    }
    /// <summary>
    /// Game manager for testing.
    /// </summary>
    public class DevUtilsGameplayManager : SingletonMonoBehaviour<DevUtilsGameplayManager>
    {
        public Mesh Mesh;
        public Material Material;

        public GameObject TestLaunchSite;
        public Rigidbody obj;

        public Texture2D heightmap;
        public RenderTexture normalmap;
        public ComputeShader shader;
        public RawImage uiImage;

        public const string LOAD_PLACEHOLDER_CONTENT = "devutils.load_game_data";
        public const string CREATE_PLACEHOLDER_UNIVERSE = "devutils.timeline.new.after";

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, LOAD_PLACEHOLDER_CONTENT )]
        private static void LoadGameData()
        {
            AssetRegistry.Register( "substance.f", new Substance() { Density = 1000, DisplayName = "Fuel", UIColor = new Color( 1.0f, 0.3764706f, 0.2509804f ) } );
            AssetRegistry.Register( "substance.ox", new Substance() { Density = 1000, DisplayName = "Oxidizer", UIColor = new Color( 0.2509804f, 0.5607843f, 1.0f ) } );
        }

        void Awake()
        {
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

        bool isPressed = false;
        bool wasFired = false;
        int bodyI;

        void FixedUpdate()
        {
            if( isPressed )
            {
                isPressed = false;

                var body = CelestialBodyManager.Get( "main" );

                System.Random r = new System.Random();
                Vector3Dbl rand = new Vector3Dbl( 0, r.Next( -50000000, 50000000 ), r.Next( -50000000, 50000000 ) );
                CelestialBody cbi = VanillaPlanetarySystemFactory.CreateCBNonAttractor( $"rand{bodyI}", new Vector3Dbl( 149_500_000_000, 0, 0 ) + rand, rand * 0.01, QuaternionDbl.identity );

                bodyI++;

                Debug.Log( body.ReferenceFrameTransform.AbsoluteVelocity );

                if( !wasFired )
                {
                    CelestialBody cb = VanillaPlanetarySystemFactory.CreateCB( "moon2", new Vector3Dbl( 150_200_000_000, 0, 0 ), new Vector3Dbl( 0, -129749.1543788567, 0 ), QuaternionDbl.identity );
                    body = cb;

                    var vessel = VesselManager.LoadedVessels.Skip( 1 ).First();
                    vessel.ReferenceFrameTransform.AbsolutePosition = body.ReferenceFrameTransform.AbsolutePosition + new Vector3Dbl( body.Radius + 200_000, 0, 0 );
                    vessel.ReferenceFrameTransform.AbsoluteVelocity = body.ReferenceFrameTransform.AbsoluteVelocity + new Vector3Dbl( 0, 8500, 0 );

                    SceneReferenceFrameManager.RequestSceneReferenceFrameSwitch( new CenteredInertialReferenceFrame( TimeManager.UT,
                        SceneReferenceFrameManager.TargetObject.AbsolutePosition, SceneReferenceFrameManager.TargetObject.AbsoluteVelocity ) );
                }
                wasFired = true;
            }
        }

        void Update()
        {
            if( UnityEngine.Input.GetKeyDown( KeyCode.F6 ) )
            {
                
            }
            if( UnityEngine.Input.GetKeyDown( KeyCode.F3 ) )
            {
                isPressed = true;
            }
            if( UnityEngine.Input.GetKeyDown( KeyCode.F4 ) )
            {
                VesselMetadata loadedVesselMetadata = VesselMetadata.LoadFromDisk( "vessel2" );

                // load current vessel from the files defined by metadata's ID.
                Directory.CreateDirectory( loadedVesselMetadata.GetRootDirectory() );
                JsonSerializedDataHandler _designObjDataHandler = new JsonSerializedDataHandler( Path.Combine( loadedVesselMetadata.GetRootDirectory(), "gameobjects.json" ) );
                var data = _designObjDataHandler.Read();

                GameObject loadedObj = SerializationUnit.Deserialize<GameObject>( data );

                FLaunchSiteMarker launchSiteSpawner = VesselManager.LoadedVessels.First().gameObject.GetComponentInChildren<FLaunchSiteMarker>();
                Vector3Dbl spawnerPosAirf = SceneReferenceFrameManager.ReferenceFrame.TransformPosition( launchSiteSpawner.transform.position );
                QuaternionDbl spawnerRotAirf = SceneReferenceFrameManager.ReferenceFrame.TransformRotation( launchSiteSpawner.transform.rotation );

                Vessel v2 = VesselFactory.CreatePartless( spawnerPosAirf, spawnerRotAirf, Vector3Dbl.zero, Vector3Dbl.zero );

                v2.RootPart = loadedObj.transform;
                v2.RootPart.localPosition = Vector3.zero;
                v2.RootPart.localRotation = Quaternion.identity;

                Vector3 bottomBoundPos = v2.GetBottomPosition();
                Vector3Dbl closestBoundAirf = SceneReferenceFrameManager.ReferenceFrame.TransformPosition( bottomBoundPos );
                Vector3Dbl closestBoundToVesselAirf = v2.ReferenceFrameTransform.AbsolutePosition - closestBoundAirf;
                Vector3Dbl airfPos = spawnerPosAirf + closestBoundToVesselAirf;
                v2.ReferenceFrameTransform.AbsolutePosition = airfPos;
            }
            if( UnityEngine.Input.GetKeyDown( KeyCode.F5 ) )
            {
                DevDefaultScenarioCreator.CreateScenario();
                //CreateVessel( launchSite );
            }
            // disabled to prevent accidental overwrite with data that needs to be edited manually (unity can't serialize everything we need)
            /*if( UnityEngine.Input.GetKeyDown( KeyCode.F1 ) )
            {
                JsonSerializedDataHandler handler;

                string gameDataPath = HumanSpaceProgramContent.GetContentDirectoryPath();
                string partDir;

                VesselMetadata vm;
                partDir = HumanSpaceProgramContent.GetSavedVesselsDirectoryPath() + "/vessel";
                Directory.CreateDirectory( partDir );
                vm = new VesselMetadata( "vessel" )
                {
                    Name = "Vessel",
                    Description = "default",
                    Author = "Katniss"
                };
                vm.SaveToDisk();
                var data = SerializationUnit.Serialize( ActiveVesselManager.ActiveObject.GetVessel().RootPart.gameObject );
                handler = new JsonSerializedDataHandler( partDir + "/gameobjects.json" );
                //handler.Write( data );

                PartMetadata pm;

                partDir = gameDataPath + "/Vanilla/Parts/engine";
                Directory.CreateDirectory( partDir );
                pm = new PartMetadata( partDir )
                {
                    Name = "Engine",
                    Author = "Katniss",
                    Categories = new string[] { "engine" }
                };
                pm.SaveToDisk();
                data = SerializationUnit.Serialize( AssetRegistry.Get<GameObject>( "builtin::Resources/Prefabs/Parts/engine" ) );
                handler = new JsonSerializedDataHandler( partDir + "/gameobjects.json" );
                //handler.Write( data );


                partDir = gameDataPath + "/Vanilla/Parts/intertank";
                Directory.CreateDirectory( partDir );
                pm = new PartMetadata( partDir )
                {
                    Name = "Intertank",
                    Author = "Katniss",
                    Categories = new string[] { "structural" }
                };
                pm.SaveToDisk();
                data = SerializationUnit.Serialize( AssetRegistry.Get<GameObject>( "builtin::Resources/Prefabs/Parts/intertank" ) );
                handler = new JsonSerializedDataHandler( partDir + "/gameobjects.json" );
                //handler.Write( data );

                partDir = gameDataPath + "/Vanilla/Parts/tank";
                Directory.CreateDirectory( partDir );
                pm = new PartMetadata( partDir )
                {
                    Name = "Tank",
                    Author = "Katniss",
                    Categories = new string[] { "fuel_tank" }
                };
                pm.SaveToDisk();
                data = SerializationUnit.Serialize( AssetRegistry.Get<GameObject>( "builtin::Resources/Prefabs/Parts/tank" ) );
                handler = new JsonSerializedDataHandler( partDir + "/gameobjects.json" );
                //handler.Write( data );

                partDir = gameDataPath + "/Vanilla/Parts/tank_long";
                Directory.CreateDirectory( partDir );
                pm = new PartMetadata( partDir )
                {
                    Name = "Long Tank",
                    Author = "Katniss",
                    Categories = new string[] { "fuel_tank" }
                };
                pm.SaveToDisk();
                data = SerializationUnit.Serialize( AssetRegistry.Get<GameObject>( "builtin::Resources/Prefabs/Parts/tank_long" ) );
                handler = new JsonSerializedDataHandler( partDir + "/gameobjects.json" );
                //handler.Write( data );

                partDir = gameDataPath + "/Vanilla/Parts/capsule";
                Directory.CreateDirectory( partDir );
                pm = new PartMetadata( partDir )
                {
                    Name = "Gemini Capsule",
                    Author = "Katniss",
                    Categories = new string[] { "command" }
                };
                pm.SaveToDisk();
                data = SerializationUnit.Serialize( AssetRegistry.Get<GameObject>( "builtin::Resources/Prefabs/Parts/capsule" ) );
                handler = new JsonSerializedDataHandler( partDir + "/gameobjects.json" );
                //handler.Write( data );

                partDir = gameDataPath + "/Vanilla/Parts/testlaunchsite";
                Directory.CreateDirectory( partDir );
                pm = new PartMetadata( partDir )
                {
                    Name = "Test Launch Site",
                    Author = "Katniss",
                    Categories = new string[] { }
                };
                pm.SaveToDisk();
                data = SerializationUnit.Serialize( AssetRegistry.Get<GameObject>( "builtin::Resources/Prefabs/testlaunchsite" ) );
                handler = new JsonSerializedDataHandler( partDir + "/gameobjects.json" );
                //handler.Write( data );
            }*/
        }
    }
}