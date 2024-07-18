using HSP.ReferenceFrames;
using UnityEngine;

namespace HSP.Vessels
{
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

            vessel.PhysicsObject.Velocity = sceneVelocity;
            vessel.PhysicsObject.AngularVelocity = sceneAngularVelocity;

            return vessel;
        }

        private static Vessel CreateGO( Vector3Dbl airfPosition, QuaternionDbl airfRotation )
        {
            GameObject gameObject = new GameObject( $"Vessel, '{name}'" );

            ReferenceFrameTransform ro = gameObject.AddComponent<ReferenceFrameTransform>();
            //FreePhysicsObject fpo = gameObject.AddComponent<FreePhysicsObject>();
#warning TODO - when a vessel is created, there should be an event and this adding of free/pinned phys object should be hooked into that.
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
#warning TODO - when a vessel is destroyed, there should be an event.
        }
    }
}