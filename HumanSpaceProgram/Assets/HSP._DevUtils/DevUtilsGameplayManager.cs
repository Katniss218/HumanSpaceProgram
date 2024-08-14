using HSP.CelestialBodies;
using HSP.CelestialBodies.Surfaces;
using HSP.Content;
using HSP.Content.Vessels.Serialization;
using HSP.ReferenceFrames;
using HSP.ResourceFlow;
using HSP.Timelines;
using HSP.Trajectories;
using HSP.Vanilla.Components;
using HSP.Vanilla.Scenes.AlwaysLoadedScene;
using HSP.Vanilla.Scenes.DesignScene;
using HSP.Vessels;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization;
using UnityPlus.Serialization.DataHandlers;

namespace HSP._DevUtils
{
    /// <summary>
    /// Game manager for testing.
    /// </summary>
    public class DevUtilsGameplayManager : SingletonMonoBehaviour<DevUtilsGameplayManager>
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

        static Vessel launchSite;
        static Vessel vessel;

        public const string LOAD_PLACEHOLDER_CONTENT = "devutils.load_game_data";
        public const string CREATE_PLACEHOLDER_UNIVERSE = "devutils.timeline.new.after";

        [HSPEventListener( HSPEvent_STARTUP_IMMEDIATELY.ID, LOAD_PLACEHOLDER_CONTENT )]
        private static void LoadGameData( object e )
        {
            AssetRegistry.Register( "substance.f", new Substance() { Density = 1000, DisplayName = "Fuel", UIColor = new Color( 1.0f, 0.3764706f, 0.2509804f ) } );
            AssetRegistry.Register( "substance.ox", new Substance() { Density = 1000, DisplayName = "Oxidizer", UIColor = new Color( 0.2509804f, 0.5607843f, 1.0f ) } );
        }

        [HSPEventListener( HSPEvent_AFTER_TIMELINE_NEW.ID, CREATE_PLACEHOLDER_UNIVERSE )]
        private static void OnAfterCreateDefault( object e )
        {
            CelestialBody body = CelestialBodyManager.Get( "main" );
            Vector3 localPos = CoordinateUtils.GeodeticToEuclidean( 28.5857702f, -80.6507262f, (float)(body.Radius + 12.5) );

            launchSite = VesselFactory.CreatePartless( Vector3Dbl.zero, QuaternionDbl.identity, Vector3Dbl.zero, Vector3Dbl.zero );
            launchSite.gameObject.name = "launchsite";
            launchSite.Pin( body, localPos, Quaternion.FromToRotation( Vector3.up, localPos.normalized ) );

            GameObject launchSitePrefab = AssetRegistry.Get<GameObject>( "builtin::Resources/Prefabs/testlaunchsite" );
            GameObject root = InstantiateLocal( launchSitePrefab, launchSite.transform, Vector3.zero, Quaternion.identity );
            launchSite.RootPart = root.transform;

            var v = CreateVessel( launchSite );
            ActiveObjectManager.ActiveObject = v.RootPart.GetVessel().gameObject.transform;
            vessel = v;
        }

        private static Vessel CreateVessel( Vessel launchSite )
        {
            if( launchSite == null )
            {
                throw new ArgumentNullException( nameof( launchSite ), "launchSite is null" );
            }

            FLaunchSiteMarker launchSiteSpawner = launchSite.gameObject.GetComponentInChildren<FLaunchSiteMarker>();
            Vector3Dbl spawnerPosAirf = SceneReferenceFrameManager.ReferenceFrame.TransformPosition( launchSiteSpawner.transform.position );
            QuaternionDbl spawnerRotAirf = SceneReferenceFrameManager.ReferenceFrame.TransformRotation( launchSiteSpawner.transform.rotation );

            var v2 = CreateDummyVessel( spawnerPosAirf, spawnerRotAirf ); // position is temp.

            Vector3 bottomBoundPos = v2.GetBottomPosition();
            Vector3Dbl closestBoundAirf = SceneReferenceFrameManager.ReferenceFrame.TransformPosition( bottomBoundPos );
            Vector3Dbl closestBoundToVesselAirf = v2.AIRFPosition - closestBoundAirf;
            Vector3Dbl airfPos = spawnerPosAirf + closestBoundToVesselAirf;
            v2.AIRFPosition = airfPos;
            return v2;
        }

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

        void Update()
        {
            if( UnityEngine.Input.GetKeyDown( KeyCode.F4 ) )
            {
                VesselMetadata loadedVesselMetadata = VesselMetadata.LoadFromDisk( "vessel2" );

                // load current vessel from the files defined by metadata's ID.
                Directory.CreateDirectory( loadedVesselMetadata.GetRootDirectory() );
                JsonSerializedDataHandler _designObjDataHandler = new JsonSerializedDataHandler( Path.Combine( loadedVesselMetadata.GetRootDirectory(), "gameobjects.json" ) );
                var data = _designObjDataHandler.Read();

                GameObject loadedObj = SerializationUnit.Deserialize<GameObject>( data );
               
                FLaunchSiteMarker launchSiteSpawner = launchSite.gameObject.GetComponentInChildren<FLaunchSiteMarker>();
                Vector3Dbl spawnerPosAirf = SceneReferenceFrameManager.ReferenceFrame.TransformPosition( launchSiteSpawner.transform.position );
                QuaternionDbl spawnerRotAirf = SceneReferenceFrameManager.ReferenceFrame.TransformRotation( launchSiteSpawner.transform.rotation );

                Vessel v2 = VesselFactory.CreatePartless( spawnerPosAirf, spawnerRotAirf, Vector3Dbl.zero, Vector3Dbl.zero );

                v2.RootPart = loadedObj.transform;
                v2.RootPart.localPosition = Vector3.zero;
                v2.RootPart.localRotation = Quaternion.identity;

                Vector3 bottomBoundPos = v2.GetBottomPosition();
                Vector3Dbl closestBoundAirf = SceneReferenceFrameManager.ReferenceFrame.TransformPosition( bottomBoundPos );
                Vector3Dbl closestBoundToVesselAirf = v2.AIRFPosition - closestBoundAirf;
                Vector3Dbl airfPos = spawnerPosAirf + closestBoundToVesselAirf;
                v2.AIRFPosition = airfPos;
            }
            if( UnityEngine.Input.GetKeyDown( KeyCode.F5 ) )
            {
                CreateVessel( launchSite );
            }
            if( UnityEngine.Input.GetKeyDown( KeyCode.F1 ) )
            {
                JsonSerializedDataHandler handler;

                string gameDataPath = HumanSpaceProgramContent.GetContentDirectoryPath();
                string partDir;

                VesselMetadata vm;
                partDir = HumanSpaceProgram.GetSavedVesselsDirectoryPath() + "/vessel";
                Directory.CreateDirectory( partDir );
                vm = new VesselMetadata( "vessel" )
                {
                    Name = "Vessel",
                    Description = "default",
                    Author = "Katniss"
                };
                vm.SaveToDisk();
                var data = SerializationUnit.Serialize( ActiveObjectManager.ActiveObject.GetVessel().RootPart.gameObject );
                handler = new JsonSerializedDataHandler( partDir + "/gameobjects.json" );
                handler.Write( data );

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
                handler.Write( data );


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
                handler.Write( data );

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
                handler.Write( data );


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
                handler.Write( data );

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
                handler.Write( data );
            }
        }

        private static GameObject InstantiateLocal( GameObject original, Transform parent, Vector3 pos, Quaternion rot )
        {
            GameObject go = Instantiate( original, parent );
            go.transform.localPosition = pos;
            go.transform.localRotation = rot;
            return go;
        }

        private static Vessel CreateDummyVessel( Vector3Dbl airfPosition, QuaternionDbl rotation )
        {
            GameObject capsulePrefab = AssetRegistry.Get<GameObject>( "builtin::Resources/Prefabs/Parts/capsule" );
            GameObject intertankPrefab = AssetRegistry.Get<GameObject>( "builtin::Resources/Prefabs/Parts/intertank" );
            GameObject tankPrefab = AssetRegistry.Get<GameObject>( "builtin::Resources/Prefabs/Parts/tank" );
            GameObject tankLongPrefab = AssetRegistry.Get<GameObject>( "builtin::Resources/Prefabs/Parts/tank_long" );
            GameObject enginePrefab = AssetRegistry.Get<GameObject>( "builtin::Resources/Prefabs/Parts/engine" );

            Vessel v = VesselFactory.CreatePartless( airfPosition, rotation, Vector3Dbl.zero, Vector3Dbl.zero );
            Transform root = InstantiateLocal( intertankPrefab, v.transform, Vector3.zero, Quaternion.identity ).transform;

            Transform tankP = InstantiateLocal( tankPrefab, root, new Vector3( 0, -1.625f, 0 ), Quaternion.identity ).transform;
            Transform tankL1 = InstantiateLocal( tankLongPrefab, root, new Vector3( 0, 2.625f, 0 ), Quaternion.identity ).transform;
            Transform capsule = InstantiateLocal( capsulePrefab, tankL1, new Vector3( 0, 2.625f, 0 ), Quaternion.identity ).transform;
            Transform t1 = InstantiateLocal( tankLongPrefab, root, new Vector3( 20, 2.625f, 0 ), Quaternion.identity ).transform;
            Transform t2 = InstantiateLocal( tankLongPrefab, root, new Vector3( -20, 2.625f, 0 ), Quaternion.identity ).transform;
            Transform engineP1 = InstantiateLocal( enginePrefab, tankP, new Vector3( 2, -3.45533f, 0 ), Quaternion.identity ).transform;
            Transform engineP2 = InstantiateLocal( enginePrefab, tankP, new Vector3( -2, -3.45533f, 0 ), Quaternion.identity ).transform;
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

            FBulkConnection conn21 = engineP1.gameObject.AddComponent<FBulkConnection>();
            conn21.End1.ConnectTo( tankP.GetComponent<FBulkContainer_Sphere>() );
            conn21.End1.Position = new Vector3( 0.0f, -1.5f, 0.0f );
            conn21.End2.ConnectTo( engineP1.GetComponent<FRocketEngine>() );
            conn21.End2.Position = new Vector3( 0.0f, 0.0f, 0.0f );
            conn21.CrossSectionArea = 60f;
            
            FBulkConnection conn22 = engineP2.gameObject.AddComponent<FBulkConnection>();
            conn22.End1.ConnectTo( tankP.GetComponent<FBulkContainer_Sphere>() );
            conn22.End1.Position = new Vector3( 0.0f, -1.5f, 0.0f );
            conn22.End2.ConnectTo( engineP2.GetComponent<FRocketEngine>() );
            conn22.End2.Position = new Vector3( 0.0f, 0.0f, 0.0f );
            conn22.CrossSectionArea = 60f;

            FVesselSeparator t1Sep = t1.gameObject.AddComponent<FVesselSeparator>();
            FVesselSeparator t2Sep = t2.gameObject.AddComponent<FVesselSeparator>();

            TrailRenderer tr = v.gameObject.AddComponent<TrailRenderer>();
            tr.material = FindObjectOfType<DevUtilsGameplayManager>().Material;
            tr.time = 250;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey( 0, 5.0f );
            curve.AddKey( 1, 2.5f );
            tr.widthCurve = curve;
            tr.minVertexDistance = 50f;

            FPlayerInputAvionics av = capsule.GetComponent<FPlayerInputAvionics>();
            FAttitudeAvionics atv = capsule.GetComponent<FAttitudeAvionics>();
            FGimbalActuatorController gc = capsule.GetComponent<FGimbalActuatorController>();
            FRocketEngine eng1 = engineP1.GetComponent<FRocketEngine>();
            F2AxisActuator ac1 = engineP1.GetComponent<F2AxisActuator>();
            FRocketEngine eng2 = engineP2.GetComponent<FRocketEngine>();
            F2AxisActuator ac2 = engineP2.GetComponent<F2AxisActuator>();
            av.OnSetThrottle.TryConnect( eng1.SetThrottle );
           // av.OnSetThrottle.TryConnect( eng2.SetThrottle );
           // only 1 output is allowed. This is annoying.

            av.OnSetAttitude.TryConnect( gc.SetAttitude );
            //atv.OnSetAttitude.TryConnect( gc.SetAttitude );

            gc.Actuators2D[0] = new FGimbalActuatorController.Actuator2DGroup();
            gc.Actuators2D[0].GetReferenceTransform.TryConnect( ac1.GetReferenceTransform );
            gc.Actuators2D[0].OnSetXY.TryConnect( ac1.SetXY );
            gc.Actuators2D[1] = new FGimbalActuatorController.Actuator2DGroup();
            gc.Actuators2D[1].GetReferenceTransform.TryConnect( ac2.GetReferenceTransform );
            gc.Actuators2D[1].OnSetXY.TryConnect( ac2.SetXY );

            FSequencer seq = capsule.GetComponent<FSequencer>();

            seq.Sequence = new()
            {
                Elements = new List<SequenceElement>()
                {
                    new KeyboardSequenceElement()
                    {
                        Actions = new List<SequenceActionBase>()
                        {
                            new SequenceAction<float>()
                            {
                                OnInvokeTyped = new ControlSystems.Controls.ControllerOutput<float>(),
                                SignalValue = 1f
                            },
                            new SequenceAction<float>()
                            {
                                OnInvokeTyped = new ControlSystems.Controls.ControllerOutput<float>(),
                                SignalValue = 1f
                            }
                        },
                        Key = KeyCode.Space
                    },
                    new TimedSequenceElement()
                    {
                        Actions = new List<SequenceActionBase>()
                        {
                            new SequenceAction()
                            {
                                OnInvokeTyped = new ControlSystems.Controls.ControllerOutput()
                            },
                            new SequenceAction()
                            {
                                OnInvokeTyped = new ControlSystems.Controls.ControllerOutput()
                            }
                        },
                        Delay = 5f
                    }
                }
            };

            ((SequenceAction<float>)seq.Sequence.Elements[0].Actions[0]).OnInvoke.TryConnect( eng1.SetThrottle );
            ((SequenceAction<float>)seq.Sequence.Elements[0].Actions[1]).OnInvoke.TryConnect( eng2.SetThrottle );
            ((SequenceAction)seq.Sequence.Elements[1].Actions[0]).OnInvoke.TryConnect( t1Sep.Separate );
            ((SequenceAction)seq.Sequence.Elements[1].Actions[1]).OnInvoke.TryConnect( t2Sep.Separate );

            FControlFrame fc = capsule.gameObject.GetComponent<FControlFrame>();
            FControlFrame.VesselControlFrame = fc;

            return v;
        }
    }
}