using HSP.ReferenceFrames;

namespace HSP.CelestialBodies
{
    public interface ICelestialBody
    {
        public string ID { get; }
        public double Radius { get; }
        public double Mass { get; }
        public IPhysicsTransform PhysicsTransform { get; }
        public IReferenceFrameTransform ReferenceFrameTransform { get; }
    }
}