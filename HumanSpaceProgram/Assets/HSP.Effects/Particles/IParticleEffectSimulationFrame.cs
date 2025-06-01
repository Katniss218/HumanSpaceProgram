namespace HSP.Effects.Particles
{
    public interface IParticleEffectSimulationFrame
    {
        public void OnInit( ParticleEffectHandle handle );
        public void OnUpdate( ParticleEffectHandle handle );
    }
}