using HSP.Core.Physics;
using HSP.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.Trajectories
{
    public static class Vessel_Ex
    {
        public static bool IsPinned( this Vessel vessel )
        {
            return vessel.PhysicsObject is PinnedPhysicsObject;
        }

        /// <summary>
        /// Pins the vessel to the celestial body at the specified location.
        /// </summary>
        public static void Pin( this Vessel vessel, CelestialBody body, Vector3Dbl localPosition, QuaternionDbl localRotation )
        {
            UnityEngine.Object.DestroyImmediate( (Component)vessel.PhysicsObject );
            PinnedPhysicsObject ppo = vessel.gameObject.AddComponent<PinnedPhysicsObject>();
            ppo.ReferenceBody = body;
            ppo.ReferencePosition = localPosition;
            ppo.ReferenceRotation = localRotation;
            vessel.PhysicsObject = ppo;
        }

        /// <summary>
        /// Unpins the vessel from a celestial body at its current location.
        /// </summary>
        public static void Unpin( this Vessel vessel )
        {
            UnityEngine.Object.DestroyImmediate( (Component)vessel.PhysicsObject );
            vessel.PhysicsObject = vessel.gameObject.AddComponent<FreePhysicsObject>();
        }
    }
}