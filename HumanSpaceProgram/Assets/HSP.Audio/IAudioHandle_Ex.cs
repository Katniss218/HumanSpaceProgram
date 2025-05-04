
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
        
        public static void TryPlay( this IAudioHandle handle, float delaySeconds, float fadeSeconds )
        {
            if( handle == null || handle.State != AudioHandleState.Ready )
                return;

            handle.Play( delaySeconds, fadeSeconds );
        }

        public static void TryStop( this IAudioHandle handle )
        {
            if( handle == null || handle.State == AudioHandleState.Finished )
                return;

            handle.Stop();
        }

        public static void TryStop( this IAudioHandle handle, float delaySeconds, float fadeSeconds )
        {
            if( handle == null || handle.State == AudioHandleState.Finished )
                return;

            handle.Stop( delaySeconds, fadeSeconds );
        }
    }
}