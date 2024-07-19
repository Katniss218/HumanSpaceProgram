using HSP.ReferenceFrames;
using UnityEngine;

namespace HSP.Vessels
{
    public static class HSPEvent_ON_VESSEL_CREATED
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".vessel_created";
    }

    public static class HSPEvent_AFTER_VESSEL_DESTROYED
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".vessel_destroyed";
    }

    /// <summary>
    /// A class responsible for instantiating a vessel from a source (save file, on launch, etc).
    /// </summary>
    public static class VesselFactory
    {
        // add source (save file / in memory scene change, etc).

        const string name = "tempname_vessel";

        /// <summary>
        /// Creates a new partless vessel at the specified global position.
        /// </summary>
        /// <param name="airfPosition">The `Absolute Inertial Reference Frame` position of the vessel to create.</param>
        /// <param name="airfRotation">Rotation of the vessel in the `Absolute Inertial Reference Frame`</param>
        /// <returns>The created partless vessel.</returns>
        public static Vessel CreatePartless( Vector3Dbl airfPosition, QuaternionDbl airfRotation, Vector3 sceneVelocity, Vector3 sceneAngularVelocity )
        {
            Vessel vessel = CreateGO( airfPosition, airfRotation );

            HSPEvent.EventManager.TryInvoke( HSPEvent_ON_VESSEL_CREATED.ID, vessel );

            vessel.PhysicsObject.Velocity = sceneVelocity;
            vessel.PhysicsObject.AngularVelocity = sceneAngularVelocity;

            return vessel;
        }

        private static Vessel CreateGO( Vector3Dbl airfPosition, QuaternionDbl airfRotation )
        {
            GameObject gameObject = new GameObject( $"Vessel, '{name}'" );

            ReferenceFrameTransform ro = gameObject.AddComponent<ReferenceFrameTransform>();
            //FreePhysicsObject fpo = gameObject.AddComponent<FreePhysicsObject>();
#warning TODO - Hook the free/pinned physobj into the event.
            // it also registers the vessel with an appropriate manager?

            Vessel vessel = gameObject.AddComponent<Vessel>();
            vessel.DisplayName = name;
            ro.AIRFPosition = airfPosition;
            ro.AIRFRotation = airfRotation;

            return vessel;
        }

        /// <summary>
        /// Completely deletes a vessel and cleans up after it.
        /// </summary>
        public static void Destroy( Vessel vessel )
        {
            UnityEngine.Object.Destroy( vessel.gameObject );

            HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_VESSEL_DESTROYED.ID, vessel );
        }
    }
}