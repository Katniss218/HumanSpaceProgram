using KatnisssSpaceSimulator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Functionalities
{
    public class FVesselSeparator : MonoBehaviour
    {
        Vessel v;
        Part p;

        private void Start()
        {
            p = this.GetComponent<Part>();
            v = this.transform.parent.GetComponent<Vessel>();
        }

        private void Update()
        {
            if( Input.GetKeyDown( KeyCode.Space ) )
            {
#warning TODO - disconnect pipes, and stuff. event based?
                VesselStateUtils.SetParent( p, null );
            }
        }
    }
}