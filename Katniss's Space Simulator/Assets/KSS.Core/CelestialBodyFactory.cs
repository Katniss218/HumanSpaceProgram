using KSS.Core.ReferenceFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KSS.Core
{
    public class CelestialBodyFactory
    {
        //const float radius = 1000; //6371000f; // m
        public const float radius = 6371000f;
        //const float mass = 20e16f; //5.97e24f; // kg  // 20e16f for 1km radius is good
        public const float mass = 5.97e24f;
        public const int subdivs = 7; // 7 is the maximum value for a single plane that won't cause issues here.

        public CelestialBody Create( Vector3Dbl AIRFPosition )
        {
            GameObject cbGO = new GameObject( "celestialbody" );
            cbGO.transform.position = SceneReferenceFrameManager.SceneReferenceFrame.InverseTransformPosition( AIRFPosition );
            cbGO.transform.localScale = Vector3.one;
            cbGO.transform.forward = Vector3.up;

            //SphereCollider c = cbGO.AddComponent<SphereCollider>();

            CelestialBody cb = cbGO.AddComponent<CelestialBody>();
            cb.AIRFPosition = AIRFPosition;
            cb.Mass = mass;
            cb.Radius = radius;

            CelestialBodySurface stf = cbGO.AddComponent<CelestialBodySurface>();

            return cb;
        }
    }
}