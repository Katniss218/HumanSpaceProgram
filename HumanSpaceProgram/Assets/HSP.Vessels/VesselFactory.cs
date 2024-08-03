using UnityEngine;

namespace HSP.Vessels
{
    public static class HSPEvent_ON_VESSEL_CREATED
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".vessel_created";
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
        /// <param name="absolutePosition">The `Absolute Inertial Reference Frame` position of the vessel to create.</param>
        /// <param name="absoluteRotation">Rotation of the vessel in the `Absolute Inertial Reference Frame`</param>
        /// <returns>The created partless vessel.</returns>
        public static Vessel CreatePartless( Vector3Dbl absolutePosition, QuaternionDbl absoluteRotation, Vector3 sceneVelocity, Vector3 sceneAngularVelocity )
        {
            GameObject gameObject = new GameObject( $"Vessel, '{name}'" );

            Vessel vessel = gameObject.AddComponent<Vessel>();
            vessel.DisplayName = name;

            HSPEvent.EventManager.TryInvoke( HSPEvent_ON_VESSEL_CREATED.ID, vessel );

            vessel.ReferenceFrameTransform.AbsolutePosition = absolutePosition;
            vessel.ReferenceFrameTransform.AbsoluteRotation = absoluteRotation;
            vessel.ReferenceFrameTransform.Velocity = sceneVelocity;
            vessel.ReferenceFrameTransform.AngularVelocity = sceneAngularVelocity;

            return vessel;
        }

        /// <summary>
        /// Completely deletes a vessel and cleans up after it.
        /// </summary>
        public static void Destroy( Vessel vessel )
        {
            UnityEngine.Object.Destroy( vessel.gameObject );
        }
    }
}