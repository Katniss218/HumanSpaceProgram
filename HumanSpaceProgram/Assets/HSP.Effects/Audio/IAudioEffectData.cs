namespace HSP.Effects.Audio
{
    public interface IAudioEffectData
    {
        void OnInit( AudioEffectHandle handle );
        void OnUpdate( AudioEffectHandle handle );
    }
}