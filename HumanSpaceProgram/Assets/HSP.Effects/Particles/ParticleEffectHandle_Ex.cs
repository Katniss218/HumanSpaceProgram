
namespace HSP.Effects.Particles
{
    public static class ParticleEffectHandle_Ex
    {
        public static void TryPlay( this ParticleEffectHandle handle )
        {
            if( !handle.IsValid() || handle.State != ParticleEffectState.Ready )
                return;

            handle.Play();
        }

        public static void TryStop( this ParticleEffectHandle handle )
        {
            if( !handle.IsValid() || handle.State != ParticleEffectState.Playing )
                return;

            handle.Stop();
        }
    }
}