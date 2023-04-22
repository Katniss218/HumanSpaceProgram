using KatnisssSpaceSimulator.Camera;
using KatnisssSpaceSimulator.Core;
using KatnisssSpaceSimulator.Core.ReferenceFrames;
using KatnisssSpaceSimulator.Functionalities;
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
        public Mesh Mesh;
        public Material Material;

        Vessel activeVessel;

        [SerializeField] private Vector3Large vesselGlobalPos;

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

            this.activeVessel = origRoot.Vessel;

            FindObjectOfType<CameraController>().ReferenceObject = p.Vessel.RootPart.transform;
        }

        public float MaxFloatingOriginRange = 100.0f;

        void LateUpdate()
        {
            float max = MaxFloatingOriginRange;
            float min = -max;

            if( activeVessel.transform.position.x < min || activeVessel.transform.position.x > max
             || activeVessel.transform.position.y < min || activeVessel.transform.position.y > max
             || activeVessel.transform.position.z < min || activeVessel.transform.position.z > max )
            {
                ReferenceFrameManager.SwitchReferenceFrame( ReferenceFrameManager.CurrentReferenceFrame.Shift( activeVessel.transform.position ) );
            }
            vesselGlobalPos = ReferenceFrameManager.CurrentReferenceFrame.TransformPosition( activeVessel.transform.position );
        }
    }

    // Shadows seem like they will be a tough nut to crack.
}
