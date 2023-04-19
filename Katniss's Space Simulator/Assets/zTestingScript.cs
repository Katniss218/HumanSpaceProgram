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
        private void Start()
        {
            VesselFactory fac = new VesselFactory();
            PartFactory pfac = new PartFactory();
            Vessel v = fac.Create( pfac.CreateRoot );

            pfac.Create( v.RootPart );

            pfac.Create( v.RootPart.Children[0] );
            pfac.Create( v.RootPart.Children[0] );

            pfac.Create( v.RootPart.Children[0].Children[0] );
        }
    }
}
