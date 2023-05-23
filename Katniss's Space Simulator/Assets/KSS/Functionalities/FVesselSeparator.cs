using KSS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Functionalities
{
    public class FVesselSeparator : MonoBehaviour
    {
        Vessel v;
        Part p;

        bool separated = false;

        void Start()
        {
            p = this.GetComponent<Part>();
            v = this.transform.parent.GetComponent<Vessel>();
        }

        void Update()
        {
            if( separated )
            {
                return;
            }
            if( Input.GetKeyDown( KeyCode.Space ) )
            {
#warning TODO - disconnect pipes, and stuff. Probably event based.

                VesselStateUtils.SetParent( p, null );
            }
        }
    }
}