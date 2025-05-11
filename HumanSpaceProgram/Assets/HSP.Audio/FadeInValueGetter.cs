using UnityPlus.Serialization;

namespace HSP.Audio
{
    public sealed class FadeInValueGetter : IValueGetter<float>, IAudioInitValueGetter
    {
        public float FadeDuration { get; }

        public bool LoopFade { get; set; } = true;

        float _startPlaybackTime;
        float _clipLength;

        public FadeInValueGetter( float fadeDuration )
        {
            this.FadeDuration = fadeDuration;
        }

        public void OnInit( IAudioHandle handle )
        {
            _startPlaybackTime = UnityEngine.Time.time;
            _clipLength = handle.Clip.length;
        }

        public float Get()
        {
            // Lerp from 0 to 1 over the duration of the fade.
            // Handles looping audios by looping the fade.
            float timeSinceStart = (UnityEngine.Time.time - _startPlaybackTime);

            if( LoopFade )
                timeSinceStart %= _clipLength;

            if( timeSinceStart < 0 )
                return 0.0f;

            if( timeSinceStart < FadeDuration )
                return timeSinceStart / FadeDuration;

            return 1.0f;
        }


        [MapsInheritingFrom( typeof( FadeInValueGetter ) )]
        public static SerializationMapping FadeInValueGetterMapping()
        {
            return new MemberwiseSerializationMapping<FadeInValueGetter>()
                .WithReadonlyMember( "fade_duration", o => o.FadeDuration )
                .WithFactory<float>( ( fadeDuration ) => new FadeInValueGetter( fadeDuration ) )
                .WithMember( "loop_fade", o => o.LoopFade );
        }
    }
}