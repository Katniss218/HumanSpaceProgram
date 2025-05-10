
namespace HSP.Audio
{
    public static class IAudioHandle_Ex
    {
        public static void TryPlay( this IAudioHandle handle )
        {
            if( handle == null || handle.State != AudioHandleState.Ready )
                return;

            handle.Play();
        }

        public static void TryStop( this IAudioHandle handle )
        {
            if( handle == null || handle.State != AudioHandleState.Playing )
                return;

            handle.Stop();
        }
    }
}