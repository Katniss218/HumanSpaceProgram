using HSP.Time;
using System.Collections.Generic;
using UnityEngine;

namespace HSP.ResourceFlow
{
    public class FlowNetworkManager : SingletonMonoBehaviour<FlowNetworkManager>
    {
        FlowNetworkSnapshot[] _cachedFlowNetworks;
        private readonly List<IBuildsFlowNetwork> _invalidComponentsCache = new();

#warning TODO - needs some component per vessel that will handle discovery and initial build of the network?
        private void FixedUpdate()
        {
            // This logic assumes a single network for simplicity of demonstration.
            // A real implementation would loop over `_cachedFlowNetworks`.
            if( _cachedFlowNetworks == null || _cachedFlowNetworks.Length == 0 )
            {
                // TODO: Discover root objects and perform initial build.
                // For now, let's assume it's initialized elsewhere.
                return;
            }

            for( int i = 0; i < _cachedFlowNetworks.Length; i++ )
            {
                var network = _cachedFlowNetworks[i];
                if( network == null )
                {
                    // Full rebuild if network doesn't exist.
                    // network = FlowNetworkSnapshot.GetNetworkSnapshot( someRootObject );
                    // _cachedFlowNetworks[i] = network;
                    continue; // Skip if no network to process.
                }

                // --- PHASE 1: State Synchronization (Cheap) ---
                // Push latest state (physics, etc.) from components into the live simulation objects.
                network.SynchronizeStateWithComponents();

                // --- PHASE 2: Detect Structural Changes (Potentially Expensive) ---
                _invalidComponentsCache.Clear();
                network.GetInvalidComponents( _invalidComponentsCache );

                if( _invalidComponentsCache.Count > 0 )
                {
                    // --- PHASE 3: Partial Rebuild Transaction ---
                    FlowNetworkBuilder transaction = new FlowNetworkBuilder();
                    foreach( var component in _invalidComponentsCache )
                    {
                        // TODO: Handle Retry/Failure results properly.
                        component.BuildFlowNetwork( transaction );
                    }

                    // --- PHASE 4: Apply Transaction ---
                    network.ApplyTransaction( transaction );
                }

                // --- PHASE 5: Step Simulation ---
                network.Step( TimeManager.FixedDeltaTime );
            }
        }
    }
}