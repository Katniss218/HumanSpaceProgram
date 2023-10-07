using KSS.Core.ReferenceFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace KSS.Core
{
    public class CelestialBodyFactory
    {
        //const float radius = 1000; //6371000f; // m
        public float radius = 6371000f;
        //const float mass = 20e16f; //5.97e24f; // kg  // 20e16f for 1km radius is good
        public float mass = 5.97e24f;
        public const int subdivs = 7; // 7 is the maximum value for a single plane that won't cause issues here.

        public CelestialBody Create( Vector3Dbl AIRFPosition )
        {
            GameObject gameObject = new GameObject( "celestialbody" );
            gameObject.transform.position = SceneReferenceFrameManager.SceneReferenceFrame.InverseTransformPosition( AIRFPosition );
            gameObject.transform.localScale = Vector3.one;
            gameObject.transform.forward = Vector3.up;

            //SphereCollider c = cbGO.AddComponent<SphereCollider>();

            PreexistingReference pr = gameObject.AddComponent<PreexistingReference>();
            pr.SetGuid( GuidFromHash( Encoding.ASCII.GetBytes( $"{radius}{mass}" ) ) );

            CelestialBody body = gameObject.AddComponent<CelestialBody>();
            body.AIRFPosition = AIRFPosition;
            body.Mass = mass;
            body.Radius = radius;

            CelestialBodySurface bodySurface = gameObject.AddComponent<CelestialBodySurface>();

            return body;
        }

        public static Guid GuidFromHash( byte[] dataToHash )
        {
            byte[] guidBytes;
            using( HashAlgorithm algorithm = MD5.Create() )
            {
                guidBytes = algorithm.ComputeHash( dataToHash );
            }

            guidBytes[6] = (byte)((guidBytes[6] & 0x0F) | (3 << 4));
            guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80);

            return new Guid( guidBytes );
        }
    }
}