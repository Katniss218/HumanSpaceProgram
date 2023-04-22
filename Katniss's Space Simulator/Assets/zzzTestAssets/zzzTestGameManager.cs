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

        public static Vector3 GetPosFromUV( float u, float v, float radius )
        {
            float latitude = v * Mathf.PI;
            float longitude = u * 2 * Mathf.PI;

            // Calculate the unit vector on the sphere's surface
            float x = radius * Mathf.Sin( latitude ) * Mathf.Cos( longitude );
            float y = radius * Mathf.Cos( latitude );
            float z = -radius * Mathf.Sin( latitude ) * Mathf.Sin( longitude );
            return new Vector3( x, y, z );

            // with current mapping, seems to generate the antipode. 
        }

        void Start()
        {
            VesselFactory fac = new VesselFactory();
            PartFactory pfac = new PartFactory();
            Vessel v = fac.Create( GetPosFromUV( 1800f / 4096f, 1150f / 4096f, CelestialBodyFactory.radius ), Quaternion.identity, pfac.CreateRoot );
            v.RootPart.DisplayName = "0";

            Part origRoot = v.RootPart;

            pfac.Create( v.RootPart, new Vector3( 0, 2, 0 ), Quaternion.identity );
            v.RootPart.Children[0].DisplayName = "0.0";

            pfac.Create( v.RootPart.Children[0], new Vector3( 2, 2, 0 ), Quaternion.identity );
            v.RootPart.Children[0].Children[0].DisplayName = "0.0.0";
            pfac.Create( v.RootPart.Children[0], new Vector3( -2, 2, 0 ), Quaternion.identity );
            v.RootPart.Children[0].Children[1].DisplayName = "0.0.1";

            // pfac.Create( v.RootPart.Children[0].Children[0], new Vector3( 4, 2, 0 ), Quaternion.identity );
            // v.RootPart.Children[0].Children[0].Children[0].DisplayName = "0.0.0.0";

            Part p = v.RootPart.Children[0];
            VesselStateUtils.SetParent( p, null );
            // VesselStateUtils.SetParent( p, v.RootPart.Children[0] );

            VesselStateUtils.SetParent( v.RootPart, p );

            FRocketEngine rn = origRoot.gameObject.AddComponent<FRocketEngine>();
            rn.MaxThrust = 100.0f;
            rn.ThrustTransform = origRoot.transform.Find( "thrust" );

            origRoot.RegisterModule( rn );

            VesselManager.ActiveVessel = origRoot.Vessel;

            FindObjectOfType<CameraController>().ReferenceObject = p.Vessel.RootPart.transform;


            TrailRenderer tr = VesselManager.ActiveVessel.gameObject.AddComponent<TrailRenderer>();
            tr.material = Material;
            tr.time = 250;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey( 0, 5.0f );
            curve.AddKey( 1, 2.5f );
            tr.widthCurve = curve;
            tr.minVertexDistance = 5f;

            VesselManager.ActiveVessel.transform.GetComponent<Rigidbody>().angularDrag = 1; // temp, doesn't veer off course.

            CelestialBody cb = new CelestialBodyFactory().Create();
        }
    }

    // Shadows seem like they will be a tough nut to crack.
}
