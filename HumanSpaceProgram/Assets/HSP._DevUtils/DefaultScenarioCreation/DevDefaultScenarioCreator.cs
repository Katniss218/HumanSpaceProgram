using HSP.CelestialBodies;
using HSP.Content.Vessels;
using HSP.Content;
using HSP.Timelines;
using HSP.Timelines.Serialization;
using HSP.Vanilla;
using HSP.Vessels;
using UnityEngine;
using HSP.ReferenceFrames;
using HSP.Vanilla.Components;
using System;
using HSP.ResourceFlow;
using System.Collections.Generic;
using UnityPlus.AssetManagement;
using HSP.SceneManagement;
using HSP.Vanilla.Scenes.GameplayScene;

namespace HSP._DevUtils
{
    public static class DevDefaultScenarioCreator
    {
        static ScenarioMetadata scenario = new ScenarioMetadata( NamespacedID.Parse( "Vanilla::default" ) )
        {
            Name = "The default scenario",
            Description = "",
            Author = "Katniss",
        };

        /// <summary>
        /// Builds a loadable scenario directory with all the files and shit.
        /// </summary>
        public static void CreateScenario()
        {
            LoadGameplayScene();
        }

        //
        //
        //

        public static void LoadGameplayScene()
        {
            HSPSceneManager.ReplaceForegroundScene<GameplaySceneM, GameplaySceneM.LoadData>( new GameplaySceneM.LoadData(), onAfterLoaded: () =>
            {
                VanillaPlanetarySystemFactory.CreateDefaultPlanetarySystem();

                CreateVessels();

                TimelineManager.BeginScenarioSaveAsync( scenario );
            } );
        }

        private static void CreateVessels()
        {
            CelestialBody body = CelestialBodyManager.Get( "main" );
            Vector3 localPos = CoordinateUtils.GeodeticToEuclidean( 28.5857702f, -80.6507262f, (float)(body.Radius + 12.5) );

            var launchSite = VesselFactory.CreatePartless( GameplaySceneM.Instance, Vector3Dbl.zero, QuaternionDbl.identity, Vector3Dbl.zero, Vector3Dbl.zero );
            launchSite.gameObject.name = "launchsite";
            launchSite.Pin( body, localPos, Quaternion.FromToRotation( Vector3.up, localPos.normalized ) );

            GameObject root = DontInstantiateLocal( PartRegistry.Load( (NamespacedID)"Vanilla::testlaunchsite" ), launchSite.transform, Vector3.zero, Quaternion.identity );
            launchSite.RootPart = root.transform;

            var vessel = CreateVessel( launchSite );

            ActiveVesselManager.ActiveObject = vessel.RootPart.GetVessel().gameObject.transform;
        }

        //
        //
        //
        
        private static Vector3 GetLocalPositionRelativeToRoot( Transform target )
        {
            Vector3 relativePosition = target.localPosition;
            Transform current = target;

            while( current.parent != null )
            {
                current = current.parent;
                if( current.parent == null ) // break out before we calculate the root values.
                    break;

                relativePosition = current.localRotation * relativePosition + current.localPosition;
            }

            return relativePosition;
        }

        private static Vessel CreateVessel( Vessel launchSite )
        {
            if( launchSite == null )
            {
                throw new ArgumentNullException( nameof( launchSite ), "launchSite is null" );
            }

            FLaunchSiteMarker launchSiteSpawner = launchSite.gameObject.GetComponentInChildren<FLaunchSiteMarker>();
            Vector3Dbl zeroPosAirf = GameplaySceneReferenceFrameManager.ReferenceFrame.TransformPosition( Vector3Dbl.zero );
            Vector3Dbl spawnerPosAirf = launchSite.ReferenceFrameTransform.AbsolutePosition + GetLocalPositionRelativeToRoot( launchSiteSpawner.transform );
            QuaternionDbl spawnerRotAirf = GameplaySceneReferenceFrameManager.ReferenceFrame.TransformRotation( launchSiteSpawner.transform.rotation );

            var vessel = CreateDummyVessel( zeroPosAirf, spawnerRotAirf ); // position is temp.

            Vector3 bottomBoundPos = vessel.GetBottomPosition();
            Vector3Dbl closestBoundAirf = GameplaySceneReferenceFrameManager.ReferenceFrame.TransformPosition( bottomBoundPos );
            Vector3Dbl closestBoundToVesselAirf = vessel.ReferenceFrameTransform.AbsolutePosition - closestBoundAirf;
            Vector3Dbl airfPos = spawnerPosAirf + closestBoundToVesselAirf;

            Vector3Dbl airfVel = launchSite.ReferenceFrameTransform.AbsoluteVelocity;

            vessel.ReferenceFrameTransform.AbsolutePosition = airfPos;
            vessel.ReferenceFrameTransform.AbsoluteVelocity = airfVel;
            return vessel;
        }

        private static GameObject DontInstantiateLocal( GameObject original, Transform parent, Vector3 pos, Quaternion rot )
        {
            original.transform.SetParent( parent, false );
            original.transform.localPosition = pos;
            original.transform.localRotation = rot;
            return original;
        }

        private static Vessel CreateDummyVessel( Vector3Dbl airfPosition, QuaternionDbl rotation )
        {
            Vessel v = VesselFactory.CreatePartless( GameplaySceneM.Instance, airfPosition, rotation, Vector3Dbl.zero, Vector3Dbl.zero );
            Transform root = DontInstantiateLocal( PartRegistry.Load( (NamespacedID)"Vanilla::intertank" ), v.transform, Vector3.zero, Quaternion.identity ).transform;

            Transform tankP = DontInstantiateLocal( PartRegistry.Load( (NamespacedID)"Vanilla::tank" ), root, new Vector3( 0, -1.625f, 0 ), Quaternion.identity ).transform;
            Transform tankL1 = DontInstantiateLocal( PartRegistry.Load( (NamespacedID)"Vanilla::tank_long" ), root, new Vector3( 0, 2.625f, 0 ), Quaternion.identity ).transform;
            Transform capsule = DontInstantiateLocal( PartRegistry.Load( (NamespacedID)"Vanilla::capsule" ), tankL1, new Vector3( 0, 2.625f, 0 ), Quaternion.identity ).transform;
            Transform t1 = DontInstantiateLocal( PartRegistry.Load( (NamespacedID)"Vanilla::tank_long" ), root, new Vector3( 20, 2.625f, 0 ), Quaternion.identity ).transform;
            Transform t2 = DontInstantiateLocal( PartRegistry.Load( (NamespacedID)"Vanilla::tank_long" ), root, new Vector3( -20, 2.625f, 0 ), Quaternion.identity ).transform;
            Transform engineP1 = DontInstantiateLocal( PartRegistry.Load( (NamespacedID)"Vanilla::engine" ), tankP, new Vector3( 2, -3.45533f, 0 ), Quaternion.identity ).transform;
            Transform engineP2 = DontInstantiateLocal( PartRegistry.Load( (NamespacedID)"Vanilla::engine" ), tankP, new Vector3( -2, -3.45533f, 0 ), Quaternion.identity ).transform;
            v.RootPart = root;

            FResourceConnection_FlowPipe conn = tankP.gameObject.AddComponent<FResourceConnection_FlowPipe>();
            conn.Conductance = 100f;
            conn.FromInlet = tankL1.GetComponent<FResourceContainer_FlowTank>().Inlets[0];
            //conn.End1.Position = new Vector3( 0.0f, -2.5f, 0.0f );
            conn.FromInlet = tankP.GetComponent<FResourceContainer_FlowTank>().Inlets[0];
            //conn.End2.Position = new Vector3( 0.0f, 1.5f, 0.0f );
            conn.FromInlet.NominalArea = 0.1f;

            Substance sbsF = AssetRegistry.Get<Substance>( "Vanilla::Assets/substances/rp_1" );
            Substance sbsOX = AssetRegistry.Get<Substance>( "Vanilla::Assets/substances/lox" );

            var tankSmallTank = tankP.GetComponent<FResourceContainer_FlowTank>();
            tankSmallTank.Contents = new SubstanceStateCollection() 
            {
                { sbsF, tankSmallTank.MaxVolume * ((sbsF.GetDensityAtSTP() + sbsOX.GetDensityAtSTP()) / 2f) / 2f },
                { sbsOX, tankSmallTank.MaxVolume * ((sbsF.GetDensityAtSTP() + sbsOX.GetDensityAtSTP()) / 2f) / 2f }
            };

            FResourceConnection_FlowPipe conn21 = engineP1.gameObject.AddComponent<FResourceConnection_FlowPipe>();
            conn21.Conductance = 100f;
            conn21.FromInlet = tankP.GetComponent<FResourceContainer_FlowTank>().Inlets[1];
            //conn21.End1.Position = new Vector3( 0.0f, -1.5f, 0.0f );
            conn21.ToInlet = engineP1.GetComponent<FRocketEngine>().Inlets[0];
            //conn21.End2.Position = new Vector3( 0.0f, 0.0f, 0.0f );
            conn21.ToInlet.NominalArea = 60f;

            FResourceConnection_FlowPipe conn22 = engineP2.gameObject.AddComponent<FResourceConnection_FlowPipe>();
            conn22.Conductance = 100f;
            conn22.FromInlet = tankP.GetComponent<FResourceContainer_FlowTank>().Inlets[1];
            //conn22.End1.Position = new Vector3( 0.0f, -1.5f, 0.0f );
            conn22.ToInlet = engineP2.GetComponent<FRocketEngine>().Inlets[0];
            //conn22.End2.Position = new Vector3( 0.0f, 0.0f, 0.0f );
            conn22.ToInlet.NominalArea = 60f;

            FVesselSeparator t1Sep = t1.gameObject.AddComponent<FVesselSeparator>();
            FVesselSeparator t2Sep = t2.gameObject.AddComponent<FVesselSeparator>();

            /* trail completely breaks down when switching between far away things. I suppose this is caused by its mesh spanning more than 150 000 000 000 meters
            TrailRenderer tr = v.gameObject.AddComponent<TrailRenderer>();
            tr.material = FindObjectOfType<DevUtilsGameplayManager>().Material;
            tr.time = 250;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey( 0, 5.0f );
            curve.AddKey( 1, 2.5f );
            tr.widthCurve = curve;
            tr.minVertexDistance = 50f;
            */

            FPlayerInputAvionics av = capsule.GetComponent<FPlayerInputAvionics>();
            FAttitudeAvionics atv = capsule.GetComponent<FAttitudeAvionics>();
            FGimbalActuatorController gc = capsule.GetComponent<FGimbalActuatorController>();
            FRocketEngine eng1 = engineP1.GetComponent<FRocketEngine>();
            eng1.PropellantMixture = new SubstanceStateCollection() { { sbsF, 0.9 }, { sbsOX, 1.1 } };
            F2AxisActuator ac1 = engineP1.GetComponent<F2AxisActuator>();
            FRocketEngine eng2 = engineP2.GetComponent<FRocketEngine>();
            eng2.PropellantMixture = new SubstanceStateCollection() { { sbsF, 0.9 }, { sbsOX, 1.1 } };
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
                        }//,
                        //Key = KeyCode.Space
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
            SelectedControlFrameManager.ControlFrame = fc;

            return v;
        }
    }
}