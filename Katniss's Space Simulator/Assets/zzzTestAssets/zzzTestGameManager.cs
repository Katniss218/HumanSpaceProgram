using KatnisssSpaceSimulator.Buildings;
using KatnisssSpaceSimulator.Camera;
using KatnisssSpaceSimulator.Core;
using KatnisssSpaceSimulator.Core.Managers;
using KatnisssSpaceSimulator.Core.ReferenceFrames;
using KatnisssSpaceSimulator.Functionalities;
using KatnisssSpaceSimulator.Terrain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator
{
    /// <summary>
    /// Game manager for testing.
    /// </summary>
    public class zzzTestGameManager : MonoBehaviour
    {
        public Material CBMaterial;

        public Mesh Mesh;
        public Material Material;

        public GameObject TestLaunchSite;

        void Start()
        {
            CelestialBody cb = new CelestialBodyFactory().Create( Vector3Dbl.zero );

            CelestialBody cb1 = new CelestialBodyFactory().Create( new Vector3Dbl( 440_000_000, 0, 0 ) );
            CelestialBody cb2 = new CelestialBodyFactory().Create( new Vector3Dbl( 440_000_000, 100_000_000, 0 ) );
            CelestialBody cb_farawayTEST = new CelestialBodyFactory().Create( new Vector3Dbl( 440_000_000_0.0, 100_000_000, 0 ) );
            CelestialBody cb_farawayTEST2 = new CelestialBodyFactory().Create( new Vector3Dbl( 440_000_000_00.0, 100_000_000, 0 ) );

            CelestialBody cb_farawayTEST3FAR = new CelestialBodyFactory().Create( new Vector3Dbl( 10e17, 100_000_000, 0 ) ); // 10e17 is 100 ly away.
#warning TODO - stuff really far away throws invalid world AABB and such. do not enable these, you can't see them anyway. 100 ly seems to work, but further away is a no-no.


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
            PartFactory pfac = new PartFactory();

            Vessel v = fac.CreatePartless( airfPosition, rotation );
            pfac.CreateRoot( v );

            const int partcount = 300;
            const int engcount = 5;

            Part parent = v.RootPart;
            for( int i = 0; i < partcount; i++ )
            {
                pfac.Create( parent, new Vector3( 0, 1.25f * i + 1.25f * engcount, 0 ), Quaternion.identity );

                parent = parent.Children[0];
            }

            parent = v.RootPart;
            for( int i = 0; i < engcount; i++ )
            {
                pfac.Create( parent, new Vector3( 0, 1.125f * i, 0 ), Quaternion.identity );

                FRocketEngine rn = v.RootPart.gameObject.AddComponent<FRocketEngine>();
                rn.MaxThrust = (100 / engcount) * (partcount / 20);
                rn.ThrustTransform = v.RootPart.transform.Find( "thrust" );

                parent = parent.Children[0];
            }

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