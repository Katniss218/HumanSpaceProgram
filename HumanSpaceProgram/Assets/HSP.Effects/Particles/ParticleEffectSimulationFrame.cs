namespace HSP.Effects.Particles
{
    public interface IParticleEffectSimulationFrame
    {
        public bool OnInit( ParticleEffectHandle handle );
        public void OnUpdate( ParticleEffectHandle handle );
    }
}