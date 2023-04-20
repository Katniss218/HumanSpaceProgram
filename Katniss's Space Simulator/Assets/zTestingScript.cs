using KatnisssSpaceSimulator.Core;
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

            pfac.Create( v.RootPart, new Vector3( 0, 2, 0 ), Quaternion.identity );

            pfac.Create( v.RootPart.Children[0], new Vector3( 2, 2, 0 ), Quaternion.identity );
            pfac.Create( v.RootPart.Children[0], new Vector3( -2, 2, 0 ), Quaternion.identity );

            pfac.Create( v.RootPart.Children[0].Children[0], new Vector3( 4, 2, 0 ), Quaternion.identity );

            (_, Vessel newV) = VesselUtils.DetachVesselPart( v.RootPart.Children[0].Children[0] );

            VesselUtils.AttachVesselPart( newV, v.RootPart.Children[0] );
        }
    }
}
