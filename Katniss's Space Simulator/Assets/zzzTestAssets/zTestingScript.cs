using KatnisssSpaceSimulator.Core;
using KatnisssSpaceSimulator.Functionalities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator
{
    public class zTestingScript : MonoBehaviour
    {
        public Mesh Mesh;
        public Material Material;

        private void Start()
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

            pfac.Create( v.RootPart.Children[0].Children[0], new Vector3( 4, 2, 0 ), Quaternion.identity );
            v.RootPart.Children[0].Children[0].Children[0].DisplayName = "0.0.0.0";

            Part p = v.RootPart.Children[0].Children[0];
            VesselStateUtils.SetParent( p, null );
           // VesselStateUtils.SetParent( p, v.RootPart.Children[0] );

            VesselStateUtils.SetParent( v.RootPart, p );

            origRoot.AddModule( new FRocketEngine()
            {
                MaxThrust = 50.0f,
                ThrustTransform = origRoot.transform.Find( "thrust" )
            } );
        }
    }

    // Shadows seem like they will be a tough nut to crack.
}
