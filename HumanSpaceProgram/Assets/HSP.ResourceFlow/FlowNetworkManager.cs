using HSP.Time;
using UnityEngine;

namespace HSP.ResourceFlow
{
    public class FlowNetworkManager : SingletonMonoBehaviour<FlowNetworkManager>
    {
        FlowNetworkSnapshot[] _cachedFlowNetworks;

        private void FixedUpdate()
        {
            for( int i = 0; i < _cachedFlowNetworks.Length; i++ )
            {
                var network = _cachedFlowNetworks[i]; // reuse built networks if valid.
                if( !network.IsValid() )
                {
                    network = FlowNetworkSnapshot.GetNetworkSnapshot( network.RootObject );
                    _cachedFlowNetworks[i] = network;
                }

                network.Step( TimeManager.FixedDeltaTime );
            }
        }
    }
}