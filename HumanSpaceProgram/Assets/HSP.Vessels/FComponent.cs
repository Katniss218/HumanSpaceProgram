using UnityEngine;

namespace HSP.Vessels
{
    public interface ISimulatable
    {
        void Update();
        void FixedUpdate();
        void BackgroundUpdate();
    }

    /// <summary>
    /// The base class for all custom functions that can be added to a vessel part.
    /// </summary>
    public abstract class FComponent : ISimulatable
    {
        public VesselPart Part { get; internal set; }

        public Transform transform { get; internal set; }
        public GameObject gameObject { get; internal set; }

        public IVessel Vessel => Part?.Vessel;

        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void BackgroundUpdate() { }

        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
    }
}