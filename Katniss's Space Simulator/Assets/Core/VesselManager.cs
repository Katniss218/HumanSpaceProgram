using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KatnisssSpaceSimulator.Core
{
    /// <summary>
    /// Manages loading, unloading, switching, etc of vessels.
    /// </summary>
    public class VesselManager : MonoBehaviour
    {
        public static Vessel ActiveVessel { get; set; }

        /*public static void SetActive( Vessel v )
        {
#warning TODO - first switch doesn't seem to switch the vessel pos to 0,0,0. Is this because rigidbody? idk, the other object with RB was in fact moved.
            ActiveVessel = v;
            ReferenceFrames.SceneReferenceFrameManager.TryFixActiveVesselOutOfBounds();
        }*/
    }
}
