
namespace HSP.Audio
{
    public static class AudioEffectHandle_Ex
    {
        public static void TryPlay( this AudioEffectHandle handle )
        {
            if( !handle.IsValid() || handle.State != AudioEffectState.Ready )
                return;

            handle.Play();
        }

        public static void TryStop( this AudioEffectHandle handle )
        {
            if( !handle.IsValid() || handle.State != AudioEffectState.Playing )
                return;

            handle.Stop();
        }
    }
}