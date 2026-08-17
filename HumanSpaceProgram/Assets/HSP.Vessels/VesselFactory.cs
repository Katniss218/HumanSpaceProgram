using HSP.ReferenceFrames;
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
        const string name = "tempname_vessel";

        /// <summary>
        /// Creates a new partless (empty) vessel graph of type T in the specified scene.
        /// </summary>
        public static T CreatePartless<T>( IHSPScene scene, Vector3Dbl absolutePosition, QuaternionDbl absoluteRotation, Vector3Dbl absoluteVelocity, Vector3Dbl absoluteAngularVelocity ) where T : Component, IVessel
        {
            GameObject gameObject = new GameObject( $"Vessel, '{name}'" );
            HSPSceneManager.MoveGameObjectToScene( gameObject, scene );

            T vessel = gameObject.AddComponent<T>();
            // vessel.DisplayName = name; // If T implements a display name we could set it, but we don't strictly know.

            HSPEvent.EventManager.TryInvoke( HSPEvent_ON_VESSEL_CREATED.ID, vessel );

            vessel.ReferenceFrameTransform?.SetAbsoluteState(
                position: absolutePosition,
                rotation: absoluteRotation,
                velocity: absoluteVelocity,
                angularVelocity: absoluteAngularVelocity
            );

            return vessel;
        }

        public static T CreateFromGraph<T>( IHSPScene scene, VesselAttachmentGraph graph, Vector3Dbl absolutePosition, QuaternionDbl absoluteRotation, Vector3Dbl absoluteVelocity, Vector3Dbl absoluteAngularVelocity ) where T : Component, IVessel
        {
            T vessel = CreatePartless<T>(scene, absolutePosition, absoluteRotation, absoluteVelocity, absoluteAngularVelocity);
            vessel.SetGraph(graph);
            return vessel;
        }

        /// <summary>
        /// Completely deletes a vessel and cleans up after it.
        /// </summary>
        public static void Destroy( IVessel vessel )
        {
            if (vessel is Component comp)
                UnityEngine.Object.Destroy( comp.gameObject );
        }
    }
}