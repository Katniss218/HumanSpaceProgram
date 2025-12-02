using HSP.ResourceFlow;
using HSP.Vessels;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.ResourceFlow
{
    /// <summary>
    /// Manages the resource flow network for a single vessel.
    /// </summary>
    [RequireComponent( typeof( IVessel ) )]
    public class VesselFlowNetwork : MonoBehaviour
    {
        private IVessel _vessel;
        private FlowNetworkSnapshot _snapshot;

        void Awake()
        {
            _vessel = GetComponent<IVessel>();
        }

        void OnEnable()
        {
            if( _vessel != null && _vessel.RootPart != null )
            {
                BuildAndRegisterNetwork();
            }
            // VesselHierarchyUtils.OnAfterVesselHierarchyChanged += OnVesselHierarchyChanged;
        }

        void OnDisable()
        {
            UnregisterNetwork();
            // VesselHierarchyUtils.OnAfterVesselHierarchyChanged -= OnVesselHierarchyChanged;
        }

        private void BuildAndRegisterNetwork()
        {
            if( _vessel.RootPart != null )
            {
                _snapshot = FlowNetworkSnapshot.GetNetworkSnapshot( _vessel.RootPart.gameObject );
                FlowNetworkManager.Register( _snapshot );
            }
        }

        private void UnregisterNetwork()
        {
            if( _snapshot != null )
            {
                FlowNetworkManager.Unregister( _snapshot );
            }
            _snapshot = null;
        }

        [MapsInheritingFrom( typeof( VesselFlowNetwork ) )]
        public static SerializationMapping FVesselFlowNetworkMapping()
        {
            return new MemberwiseSerializationMapping<VesselFlowNetwork>();
        }
    }
}