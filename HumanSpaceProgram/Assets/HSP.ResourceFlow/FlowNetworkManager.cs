using HSP.Time;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HSP.ResourceFlow
{
    public class FlowNetworkManager : SingletonMonoBehaviour<FlowNetworkManager>
    {
        private readonly List<FlowNetworkSnapshot> _networks = new List<FlowNetworkSnapshot>();
        private readonly List<IBuildsFlowNetwork> _invalidComponentsCache = new List<IBuildsFlowNetwork>();

        public static void Register( FlowNetworkSnapshot network )
        {
#warning TODO - clean up and make safe.
            if( network != null && !instance._networks.Contains( network ) )
            {
                instance._networks.Add( network );
            }
        }

        public static void Unregister( FlowNetworkSnapshot network )
        {
            if( network != null )
            {
                instance._networks.Remove( network );
            }
        }

        void FixedUpdate()
        {
            // Iterate backwards over the list for safe removal if a network becomes invalid or is destroyed during the loop.
            for( int i = _networks.Count - 1; i >= 0; i-- )
            {
                var network = _networks[i];
                if( network == null )
                {
                    _networks.RemoveAt( i );
                    continue;
                }

                // --- PHASE 1: Synchronize (Unity -> Sim) ---
                network.SynchronizeStateWithComponents();

                // --- PHASE 2: Detect and Apply Structural Changes ---
                _invalidComponentsCache.Clear();
                network.GetInvalidComponents( _invalidComponentsCache );

                if( _invalidComponentsCache.Count > 0 )
                {
                    FlowNetworkBuilder transaction = new FlowNetworkBuilder();
                    foreach( var component in _invalidComponentsCache )
                    {
                        // TODO: Handle Retry/Failure results properly.
                        component.BuildFlowNetwork( transaction );
                    }
                    network.ApplyTransaction( transaction );
                }

                // --- PHASE 3: Step Simulation ---
                network.Step( TimeManager.FixedDeltaTime );

                // --- PHASE 4: Apply (Sim -> Unity) ---
                network.ApplySnapshotToComponents();
            }
        }
    }
}