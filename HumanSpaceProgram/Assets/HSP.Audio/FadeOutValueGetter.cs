using UnityPlus.Serialization;

namespace HSP.Audio
{
    public sealed class FadeOutValueGetter : IValueGetter<float>, IInitValueGetter<AudioEffectHandle>
    {
        public float FadeDuration { get; }

        public bool LoopFade { get; set; } = true;

        float _startFadingTime; // 'start playback', offset by the clip length and fade duration.
        float _clipLength;

        public FadeOutValueGetter( float fadeDuration )
        {
            this.FadeDuration = fadeDuration;
        }

        public void OnInit( AudioEffectHandle handle )
        {
            _startFadingTime = UnityEngine.Time.time + handle.Clip.length - FadeDuration;
            _clipLength = handle.Clip.length;
        }

        public float Get()
        {
            // Lerp from 1 to 0 over the duration of the fade.
            // Handles looping audios by looping the fade.
            float timeSinceStart = (UnityEngine.Time.time - _startFadingTime);

            if( LoopFade )
                timeSinceStart %= _clipLength;

            if( timeSinceStart < 0 )
                return 1.0f;

            if( timeSinceStart < FadeDuration )
                return 1.0f - (timeSinceStart / FadeDuration);

            return 0.0f;
        }


        [MapsInheritingFrom( typeof( FadeOutValueGetter ) )]
        public static SerializationMapping FadeOutValueGetterMapping()
        {
            return new MemberwiseSerializationMapping<FadeOutValueGetter>()
                .WithReadonlyMember( "fade_duration", o => o.FadeDuration )
                .WithFactory<float>( ( fadeDuration ) => new FadeOutValueGetter( fadeDuration ) )
                .WithMember( "loop_fade", o => o.LoopFade );
        }
    }
}