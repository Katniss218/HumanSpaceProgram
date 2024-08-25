using HSP.CelestialBodies;
using HSP.Vessels;
using UnityEngine;

namespace HSP.Vanilla
{
    public static class VesselPinning_Ex
    {
        public static bool IsPinned( this Vessel vessel )
        {
            return vessel.ReferenceFrameTransform is PinnedPhysicsTransform;
        }

        /// <summary>
        /// Pins the vessel to the celestial body at the specified location.
        /// </summary>
        public static void Pin( this Vessel vessel, CelestialBody body, Vector3Dbl localPosition, QuaternionDbl localRotation )
        {
            UnityEngine.Object.DestroyImmediate( (Component)vessel.PhysicsTransform );
            var ppo = vessel.gameObject.AddComponent<PinnedPhysicsTransform>();
            ppo.ReferenceBody = body;
            ppo.ReferencePosition = localPosition;
            ppo.ReferenceRotation = localRotation;
#warning TODO - copy properties of the old phystransform over.

            vessel.ReferenceFrameTransform = ppo;
            vessel.PhysicsTransform = ppo;
        }

        /// <summary>
        /// Unpins the vessel from a celestial body at its current location.
        /// </summary>
        public static void Unpin( this Vessel vessel )
        {
            UnityEngine.Object.DestroyImmediate( (Component)vessel.PhysicsTransform );
            var ppo = vessel.gameObject.AddComponent<FreePhysicsTransform>();
#warning TODO - copy properties of the old phystransform over.

            vessel.PhysicsTransform = ppo;
            vessel.ReferenceFrameTransform = ppo;
        }
    }
}