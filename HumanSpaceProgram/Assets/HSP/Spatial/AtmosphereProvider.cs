using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.OverridableValueProviders;

namespace HSP.Spatial
{
    public readonly struct AtmosphereData
    {
        public double SpecificGasConstant { get; }
        public float Pressure { get; }      // Pa
        public float Temperature { get; }   // K
        public Vector3 WindVelocity { get; } // m/s (scene space)
    }

    public static class AtmosphereProvider
    {

        static OverridableValueProviderRegistry<Vector3, AtmosphereData> providerRegistry = new();



    }
}