using HSP.CelestialBodies;
using HSP.ReferenceFrames;
using UnityEngine;

namespace HSP.Vanilla.Scenes.MapScene
{
    public static class HSPEvent_AFTER_MAP_CELESTIAL_BODY_CREATED
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".map_celestial_body_created.after";
    }

    public static class HSPEvent_AFTER_MAP_CELESTIAL_BODY_DESTROYED
    {
        public const string ID = HSPEvent.NAMESPACE_HSP + ".map_celestial_body_destroyed.after";
    }

    public class MapCelestialBody : MonoBehaviour, ICelestialBody, IMapFocusable
    {
        public CelestialBody Source { get; internal set; }

        public string ID => ((ICelestialBody)Source).ID;

        public double Radius => ((ICelestialBody)Source).Radius;

        public double Mass => ((ICelestialBody)Source).Mass;

        public IReferenceFrameTransform ReferenceFrameTransform { get; internal set; }

        public IPhysicsTransform PhysicsTransform { get; internal set; }

        void Start()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_MAP_CELESTIAL_BODY_CREATED.ID, this );
        }

        void OnDestroy()
        {
            HSPEvent.EventManager.TryInvoke( HSPEvent_AFTER_MAP_CELESTIAL_BODY_DESTROYED.ID, this );
        }
    }
}