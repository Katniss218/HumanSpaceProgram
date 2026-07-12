using UnityEngine;

namespace HSP.Vessels
{
    public interface ISimulatable
    {
        void Update();
        void FixedUpdate();
        void BackgroundUpdate();
    }

    public abstract class FComponent : ISimulatable
    {
        public VesselPart Part { get; internal set; }

        public Transform transform { get; internal set; }
        public GameObject gameObject { get; internal set; }

        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void BackgroundUpdate() { }
    }
}