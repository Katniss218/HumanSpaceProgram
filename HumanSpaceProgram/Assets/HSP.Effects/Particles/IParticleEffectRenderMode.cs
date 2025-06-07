namespace HSP.Effects.Particles
{
    public interface IParticleEffectRenderMode
    {
        public void OnInit( ParticleEffectHandle handle );

        public void OnUpdate( ParticleEffectHandle handle );
    }
}