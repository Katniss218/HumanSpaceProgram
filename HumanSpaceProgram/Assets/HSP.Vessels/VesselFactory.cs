using HSP.SceneManagement;
using UnityEngine;

namespace HSP.Vessels
{
    public static class HSPEvent_ON_VESSEL_CREATED
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".vessel_created";
    }
    
    /// <summary>
    /// A class responsible for instantiating vessels.
    /// </summary>
    public static class VesselFactory
    {
        // add source (save file / in memory scene change, etc).

        const string name = "tempname_vessel";

        /// <summary>
        /// Creates a new partless (empty) vessel in the specified scene.
        /// </summary>
        /// <param name="absolutePosition">The position where the vessel will be created, in `Absolute Inertial Reference Frame`.</param>
        /// <param name="absoluteRotation">The rotation of the vessel, in `Absolute Inertial Reference Frame`.</param>
        /// <returns>The created partless vessel.</returns>
        public static Vessel CreatePartless( IHSPScene scene, Vector3Dbl absolutePosition, QuaternionDbl absoluteRotation, Vector3Dbl absoluteVelocity, Vector3Dbl absoluteAngularVelocity )
        {
            GameObject gameObject = new GameObject( $"Vessel, '{name}'" );
            HSPSceneManager.MoveGameObjectToScene( gameObject, scene );

            Vessel vessel = gameObject.AddComponent<Vessel>();
            vessel.DisplayName = name;

            HSPEvent.EventManager.TryInvoke( HSPEvent_ON_VESSEL_CREATED.ID, vessel );

            vessel.ReferenceFrameTransform.AbsolutePosition = absolutePosition;
            vessel.ReferenceFrameTransform.AbsoluteRotation = absoluteRotation;
            vessel.ReferenceFrameTransform.AbsoluteVelocity = absoluteVelocity;
            vessel.ReferenceFrameTransform.AbsoluteAngularVelocity = absoluteAngularVelocity;

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