namespace HSP.Effects.Particles
{
    public interface IParticleEffectData
    {
        void OnInit( ParticleEffectHandle handle );
        void OnUpdate( ParticleEffectHandle handle );
    }
}