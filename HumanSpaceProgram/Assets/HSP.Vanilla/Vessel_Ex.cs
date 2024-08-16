using HSP.CelestialBodies;
using HSP.Vessels;
using UnityEngine;

namespace HSP.Vanilla
{
    public static class Vessel_Ex2
    {
        public static bool IsPinned( this Vessel vessel )
        {
            return vessel.PhysicsTransform is PinnedPhysicsTransform;
        }

        /// <summary>
        /// Pins the vessel to the celestial body at the specified location.
        /// </summary>
        public static void Pin( this Vessel vessel, CelestialBody body, Vector3Dbl localPosition, QuaternionDbl localRotation )
        {
            UnityEngine.Object.DestroyImmediate( (Component)vessel.PhysicsTransform );
            PinnedPhysicsTransform ppo = vessel.gameObject.AddComponent<PinnedPhysicsTransform>();
            ppo.ReferenceBody = body;
            ppo.ReferencePosition = localPosition;
            ppo.ReferenceRotation = localRotation;
            vessel.PhysicsTransform = ppo;
        }

        /// <summary>
        /// Unpins the vessel from a celestial body at its current location.
        /// </summary>
        public static void Unpin( this Vessel vessel )
        {
            UnityEngine.Object.DestroyImmediate( (Component)vessel.PhysicsTransform );
            vessel.PhysicsTransform = vessel.gameObject.AddComponent<FreePhysicsTransform>();
        }
    }
}