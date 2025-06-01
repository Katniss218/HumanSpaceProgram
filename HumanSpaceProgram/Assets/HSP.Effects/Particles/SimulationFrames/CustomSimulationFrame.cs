using UnityEngine;

namespace HSP.Effects.Particles.SimulationFrames
{
    public abstract class CustomSimulationFrame : IParticleEffectSimulationFrame
    {
        public void OnInit( ParticleEffectHandle handle )
        {
            // Create the simulation frame object (if needed).
            // It will stay in the pool, being set/unset from the property as needed (instead of being destroyed).
            if( handle.poolItem.simulationGameObject == null )
            {
                var go = new GameObject( "CustomSimulationFrame" );
                go.transform.SetParent( handle.poolItem.transform, false );
                handle.poolItem.simulationGameObject = go;
            }

            var main = handle.poolItem.main;

            main.simulationSpace = ParticleSystemSimulationSpace.Custom;
            main.customSimulationSpace = handle.poolItem.simulationGameObject.transform;
        }

        public void OnUpdate( ParticleEffectHandle handle )
        {
            this.OnUpdateInternal( handle, handle.poolItem.simulationGameObject.transform );
        }

        protected abstract void OnUpdateInternal( ParticleEffectHandle handle, Transform customFrame );


        // No serialization becuase abstract
    }
}