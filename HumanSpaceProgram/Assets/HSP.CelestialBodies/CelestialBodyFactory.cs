using System;
using System.Security.Cryptography;
using UnityEngine;

namespace HSP.CelestialBodies
{
    /// <summary>
    /// Creates celestial body instances (game objects).
    /// </summary>
    public class CelestialBodyFactory
    {
#warning TODO - celestial bodies are just serialized json gameobjects (prefabs).
        public string ID { get; }

        //const float radius = 1000; //6371000f; // m
        //const float mass = 20e16f; //5.97e24f; // kg  // 20e16f for 1km radius is good
        public float radius = 6371000f;
        public float mass = 5.97e24f;
        public const int subdivs = 7; // 7 is the maximum value for a single plane that won't cause issues here.

        public CelestialBodyFactory( string id )
        {
            this.ID = id;
        }

        public CelestialBody Create( Vector3Dbl airfPosition, QuaternionDbl airfRotation )
        {
            GameObject gameObject = new GameObject( "celestialbody" );

            //SphereCollider c = cbGO.AddComponent<SphereCollider>();

            CelestialBody body = gameObject.AddComponent<CelestialBody>();
            body.ID = this.ID;
            body.ReferenceFrameTransform.AbsolutePosition = airfPosition;
            body.ReferenceFrameTransform.AbsoluteRotation = airfRotation;
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