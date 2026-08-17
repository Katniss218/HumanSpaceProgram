using HSP.ResourceFlow;
using HSP.Vessels;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Descriptors;

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
            if( _vessel != null && _vessel.Parts != null && _vessel.Parts.Any() )
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
            if( _vessel.Parts.Any() )
            {
                IEnumerable<IBuildsFlowNetwork> buildsFlowNetworks = null;// @@todo - implement this.
                _snapshot = FlowNetworkBuilder.Create( buildsFlowNetworks ).BuildSnapshot();
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
        public static IDescriptor FVesselFlowNetworkMapping()
        {
            return new MemberwiseDescriptor<VesselFlowNetwork>();
        }
    }
}