using UnityEngine;

namespace HSP.ResourceFlow
{
    public sealed class ResourceInlet
    {
        public float nominalArea; // m^2

        public Vector3 LocalPosition;

        public IBuildsFlowNetwork owner;
    }
}