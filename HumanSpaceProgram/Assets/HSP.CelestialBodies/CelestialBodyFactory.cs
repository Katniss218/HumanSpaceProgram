using HSP.SceneManagement;
using UnityEngine;

namespace HSP.CelestialBodies
{
    public static class HSPEvent_ON_CELESTIAL_BODY_CREATED
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".celestial_body_created";
    }

    /// <summary>
    /// A class responsible for instantiating celestial bodies.
    /// </summary>
    public class CelestialBodyFactory
    {
        public string ID { get; }

        //const float radius = 1000; //6371000f; // m
        //const float mass = 20e16f; //5.97e24f; // kg  // 20e16f for 1km radius is good
        public double radius = 6_371_000.0;
        public double mass = 5.97e24;
        public const int subdivs = 7; // 7 is the maximum value for a single plane that won't cause issues here.

        public CelestialBodyFactory( string id )
        {
            this.ID = id;
        }

        /// <summary>
        /// Creates a new empty celestial body in the specified scene.
        /// </summary>
        /// <param name="absolutePosition">The position where the vessel will be created, in `Absolute Inertial Reference Frame`.</param>
        /// <param name="absoluteRotation">The rotation of the vessel, in `Absolute Inertial Reference Frame`.</param>
        public CelestialBody Create( IHSPScene scene, Vector3Dbl absolutePosition, QuaternionDbl absoluteRotation )
        {
            GameObject gameObject = new GameObject( $"celestialbody-{this.ID}" );
            HSPSceneManager.MoveGameObjectToScene( gameObject, scene );

            CelestialBody celestialBody = gameObject.AddComponent<CelestialBody>();
            celestialBody.ID = this.ID;
            celestialBody.Mass = mass;
            celestialBody.Radius = radius;

            HSPEvent.EventManager.TryInvoke( HSPEvent_ON_CELESTIAL_BODY_CREATED.ID, celestialBody );

            celestialBody.ReferenceFrameTransform.AbsolutePosition = absolutePosition;
            celestialBody.ReferenceFrameTransform.AbsoluteRotation = absoluteRotation;

            return celestialBody;
        }
    }
}