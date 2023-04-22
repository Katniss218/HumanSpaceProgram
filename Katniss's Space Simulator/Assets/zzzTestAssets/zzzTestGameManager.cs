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
        public Mesh CBMesh;

        public Mesh Mesh;
        public Material Material;

        void Start()
        {
            VesselFactory fac = new VesselFactory();
            PartFactory pfac = new PartFactory();
            Vessel v = fac.Create( pfac.CreateRoot );
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
            tr.time = 50;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey( 0, 5.0f );
            curve.AddKey( 1, 2.5f );
            tr.widthCurve = curve;
            tr.minVertexDistance = 5f;


            const float radius = 1000; //6371000f; // m
            const float mass = 20e16f; //5.97e24f; // kg  // 20e16f for 1km radius is good
            GameObject cbGO = new GameObject( "celestialbody" );
            cbGO.transform.position = new Vector3( 0, -radius, 0 );
            cbGO.transform.localScale = Vector3.one * 2 * radius;

            SphereCollider c = cbGO.AddComponent<SphereCollider>();

            CelestialBody cb = cbGO.AddComponent<CelestialBody>();
            cb.GIRFPosition = new Vector3Large( 0, -radius, 0 );
            cb.Mass = mass;

            MeshFilter cbMesh = cbGO.AddComponent<MeshFilter>();
            cbMesh.mesh = this.CBMesh;

            MeshRenderer mr = cbGO.AddComponent<MeshRenderer>();
            mr.material = this.Material;

            CelestialBodyManager.Bodies.Add( cb );
        }
    }

    // Shadows seem like they will be a tough nut to crack.
}
