namespace HSP.Effects.Particles
{
    public interface IParticleEffectEmissionShape
    {
        public void OnInit( ParticleEffectHandle handle );

        public void OnUpdate( ParticleEffectHandle handle );
    }
}