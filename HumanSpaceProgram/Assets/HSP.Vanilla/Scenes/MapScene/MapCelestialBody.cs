using HSP.CelestialBodies;
using HSP.ReferenceFrames;
using UnityEngine;

namespace HSP.Vanilla.Scenes.MapScene
{
    public class MapCelestialBody : MonoBehaviour, ICelestialBody
    {
        public CelestialBody Source { get; internal set; }

        public string ID => ((ICelestialBody)Source).ID;

        public double Radius => ((ICelestialBody)Source).Radius;

        public double Mass => ((ICelestialBody)Source).Mass;

#warning TODO - provide a transform that just transforms from gameplay scene to map scene's frame, using a source transform field
        public IPhysicsTransform PhysicsTransform { get; internal set; }

        public IReferenceFrameTransform ReferenceFrameTransform { get; internal set; }
    }
}