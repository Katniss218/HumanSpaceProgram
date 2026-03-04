using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Effects.Particles.SimulationFrames
{
    public sealed class LocalSimulationFrame : IParticleEffectSimulationFrame
    {
        public void OnInit( ParticleEffectHandle handle )
        {
            var main = handle.poolItem.main;

            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.customSimulationSpace = null;
        }

        public void OnUpdate( ParticleEffectHandle handle )
        {
        }


        [MapsInheritingFrom( typeof( LocalSimulationFrame ) )]
        public static IDescriptor LocalSimulationFrameMapping()
        {
            return new MemberwiseDescriptor<LocalSimulationFrame>();
        }
    }
}