using System;
using System.Security.Cryptography;
using UnityEngine;

namespace HSP.CelestialBodies
{
    public static class HSPEvent_ON_CELESTIAL_BODY_CREATED
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".celestial_body_created";
    }

    /// <summary>
    /// Creates celestial body instances (game objects).
    /// </summary>
    public class CelestialBodyFactory
    {
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

        public CelestialBody Create( Vector3Dbl absolutePosition, QuaternionDbl absoluteRotation )
        {
            GameObject gameObject = new GameObject( "celestialbody" );

            CelestialBody celestialBody = gameObject.AddComponent<CelestialBody>();
            celestialBody.ID = this.ID;
            celestialBody.Mass = mass;
            celestialBody.Radius = radius;

            HSPEvent.EventManager.TryInvoke( HSPEvent_ON_CELESTIAL_BODY_CREATED.ID, celestialBody );

            celestialBody.ReferenceFrameTransform.AbsolutePosition = absolutePosition;
            celestialBody.ReferenceFrameTransform.AbsoluteRotation = absoluteRotation;

            return celestialBody;
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