using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Effects.Particles.SimulationFrames
{
    public sealed class LocalSimulationFrame : IParticleEffectSimulationFrame
    {
        public bool OnInit( ParticleEffectHandle handle )
        {
            var main = handle.poolItem.main;

            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.customSimulationSpace = null;

            return false;
        }

        public void OnUpdate( ParticleEffectHandle handle )
        {
        }


        [MapsInheritingFrom( typeof( LocalSimulationFrame ) )]
        public static SerializationMapping LocalSimulationFrameMapping()
        {
            return new MemberwiseSerializationMapping<LocalSimulationFrame>();
        }
    }
}