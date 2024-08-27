using HSP.CelestialBodies;
using HSP.ReferenceFrames;
using HSP.Vessels;
using UnityEngine;

namespace HSP.Vanilla
{
    public static class VesselPinning_Ex
    {
        public static bool IsPinned( this Vessel vessel )
        {
            return vessel.ReferenceFrameTransform is PinnedReferenceFrameTransform;
        }

        /// <summary>
        /// Pins the vessel to the celestial body at the specified location.
        /// </summary>
        public static void Pin( this Vessel vessel, CelestialBody body, Vector3Dbl referencePosition, QuaternionDbl referenceRotation )
        {
#warning TODO - Figure out something to make this more general and logical
            
            // another way is to have 2 separate interfaces and just use `get => null ? cache and return : return;` in the celestialbody/vessel getters (this will make it more general)
            // - I think I'll go with this.

            IReferenceFrameTransform oldReferenceFrameTransform = vessel.ReferenceFrameTransform;
            IPhysicsTransform oldPhysTransform = vessel.PhysicsTransform;

            UnityEngine.Object.DestroyImmediate( (Component)vessel.PhysicsTransform );
            var ppo = vessel.gameObject.AddComponent<PinnedReferenceFrameTransform>();
            ppo.ReferenceBody = body;
            ppo.ReferencePosition = referencePosition;
            ppo.ReferenceRotation = referenceRotation;

            ppo.Mass = oldPhysTransform.Mass;
            ppo.LocalCenterOfMass = oldPhysTransform.LocalCenterOfMass;
            ppo.MomentOfInertiaTensor = oldPhysTransform.MomentOfInertiaTensor;
        }

        /// <summary>
        /// Unpins the vessel from a celestial body at its current location.
        /// </summary>
        public static void Unpin( this Vessel vessel )
        {
            IReferenceFrameTransform oldReferenceFrameTransform = vessel.ReferenceFrameTransform;
            IPhysicsTransform oldPhysTransform = vessel.PhysicsTransform;

            UnityEngine.Object.DestroyImmediate( (Component)vessel.PhysicsTransform );
            var ppo = vessel.gameObject.AddComponent<FreeReferenceFrameTransform>();

            ppo.Mass = oldPhysTransform.Mass;
            ppo.LocalCenterOfMass = oldPhysTransform.LocalCenterOfMass;
            ppo.MomentOfInertiaTensor = oldPhysTransform.MomentOfInertiaTensor;
        }
    }
}