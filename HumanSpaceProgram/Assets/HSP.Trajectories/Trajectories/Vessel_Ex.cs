using HSP.Core.Physics;
using HSP.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HSP.Vessels;
using HSP.CelestialBodies;

namespace HSP.Trajectories
{
    public static class Vessel_Ex
    {
        [HSPEventListener( HSPEvent_VesselCreated.EventID, "add_physicsobject" )]
        public static void OnVesselCreated( Vessel v )
        {
            v.PhysicsObject = v.gameObject.AddComponent<FreePhysicsObject>();
        }

        [HSPEventListener( HSPEvent_VesselHierarchyChanged.EventID, "try_pin_physicsobject" )]
        public static void OnVesselCreated( (Vessel v, Transform oldRootPart, Transform newRootPart) e )
        {
            if( e.oldRootPart == null )
                return;

            if( FAnchor.IsAnchored( e.v.RootPart ) )
            {
                PinnedPhysicsObject ppo = e.oldRootPart.GetVessel().GetComponent<PinnedPhysicsObject>();
                e.v.Pin( ppo.ReferenceBody, ppo.ReferencePosition, ppo.ReferenceRotation );
            }
        }

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