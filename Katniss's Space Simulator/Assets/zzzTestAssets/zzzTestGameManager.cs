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

        void Start()
        {
            CelestialBody cb = new CelestialBodyFactory().Create( Vector3Dbl.zero );

            CelestialBody cb1 = new CelestialBodyFactory().Create( new Vector3Dbl( 440_000_000, 0, 0 ) );
            CelestialBody cb2 = new CelestialBodyFactory().Create( new Vector3Dbl( 440_000_000, 100_000_000, 0 ) );
            CelestialBody cb_farawayTEST = new CelestialBodyFactory().Create( new Vector3Dbl( 440_000_000_0.0, 100_000_000, 0 ) );
            CelestialBody cb_farawayTEST2 = new CelestialBodyFactory().Create( new Vector3Dbl( 440_000_000_00.0, 100_000_000, 0 ) );

#warning TODO - stuff really far away throws invalid world AABB and such. do not enable these, you can't see them anyway. 100 ly seems to work, but further away is a no-no.
            CelestialBody cb_farawayTEST3FAR = new CelestialBodyFactory().Create( new Vector3Dbl( 10e17, 100_000_000, 0 ) ); // 10e17 is 100 ly away.


            Vector3 pos = CoordinateUtils.UVToCartesian( 1900f / 4096f, 1200f / 4096f, CelestialBodyFactory.radius );
            pos = new Vector3( pos.x, pos.z, pos.y );

            Vessel v = CreateDummyVessel( pos );

            FRocketEngine rn = v.RootPart.gameObject.AddComponent<FRocketEngine>();
            rn.MaxThrust = 100.0f;
            rn.ThrustTransform = v.RootPart.transform.Find( "thrust" );

            v.RootPart.RegisterModule( rn );


            // Vessel v2 = CreateDummyVessel( pos + Vector3Dbl.one * 6 );


            VesselManager.ActiveVessel = v.RootPart.Vessel;
            FindObjectOfType<CameraController>().ReferenceObject = v.RootPart.transform;

            VesselManager.ActiveVessel.transform.GetComponent<Rigidbody>().angularDrag = 1; // temp, doesn't veer off course.


            //v2.SetPosition( new Vector3Dbl( 100_000_000, 0, 0 ) );
            // v2.SetPosition( new Vector3Dbl( 0, 0, 0 ) );
            // v2.SetPosition( pos + Vector3Dbl.one * 6 );

        }

        Vessel CreateDummyVessel( Vector3Dbl airfPosition )
        {
            VesselFactory fac = new VesselFactory();
            PartFactory pfac = new PartFactory();

            Vessel v = fac.CreatePartless( airfPosition, Quaternion.identity );
            pfac.CreateRoot( v );

            v.RootPart.DisplayName = "0";

            pfac.Create( v.RootPart, new Vector3( 0, 2, 0 ), Quaternion.identity );
            v.RootPart.Children[0].DisplayName = "0.0";

            pfac.Create( v.RootPart.Children[0], new Vector3( 2, 2, 0 ), Quaternion.identity );
            v.RootPart.Children[0].Children[0].DisplayName = "0.0.0";
            pfac.Create( v.RootPart.Children[0], new Vector3( -2, 2, 0 ), Quaternion.identity );
            v.RootPart.Children[0].Children[1].DisplayName = "0.0.1";

            // pfac.Create( v.RootPart.Children[0].Children[0], new Vector3( 4, 2, 0 ), Quaternion.identity );
            // v.RootPart.Children[0].Children[0].Children[0].DisplayName = "0.0.0.0";


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