using HSP.Audio;
using HSP.Vanilla.Components;
using HSP.Vessels;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Vanilla.Effects
{
    public sealed class RocketEngineThrustValueGetter : IValueGetter<float>
    {
        public FRocketEngine Engine { get; }

        public RocketEngineThrustValueGetter( FRocketEngine engine )
        {
            Engine = engine;
        }

        public float Get()
        {
            return Engine.Thrust / Engine.MaxThrust;
        }


        [MapsInheritingFrom( typeof( RocketEngineThrustValueGetter ) )]
        public static SerializationMapping RocketEngineThrustValueGetterMapping()
        {
            return new MemberwiseSerializationMapping<RocketEngineThrustValueGetter>()
                .WithReadonlyMember( "engine", ObjectContext.Ref, o => o.Engine )
                .WithFactory<FRocketEngine>( ( engine ) => new RocketEngineThrustValueGetter( engine ) );
        }
    }

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

        public void OnInit( AudioClip clip )
        {
            _startPlaybackTime = UnityEngine.Time.time;
            _clipLength = clip.length;
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

    public sealed class FadeOutValueGetter : IValueGetter<float>, IAudioInitValueGetter
    {
        public float FadeDuration { get; }

        public bool LoopFade { get; set; } = true;

        float _startFadingTime; // 'start playback', offset by the clip length and fade duration.
        float _clipLength;

        public FadeOutValueGetter( float fadeDuration )
        {
            this.FadeDuration = fadeDuration;
        }

        public void OnInit( AudioClip clip )
        {
            _startFadingTime = UnityEngine.Time.time + clip.length - FadeDuration;
            _clipLength = clip.length;
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

    public class ParticleSystemEffectShaper
    {
        // particle plumes
    }

    public class MeshEffectShaper
    {
        // mesh plumes
    }

    public class FRocketEngineAudio : MonoBehaviour
    {
        public FRocketEngine Engine;

        public AudioEffectDefinition IgnitionAudio;
        public AudioEffectDefinition ShutdownAudio;
        public AudioEffectDefinition LoopAudio;

        void OnEnable()
        {
            if( Engine == null )
                Engine = this.GetComponent<FRocketEngine>();

            Engine.OnAfterIgnite += OnIgnite;
            Engine.OnAfterShutdown += OnShutdown;
        }

        void OnDisable()
        {
            if( Engine == null )
                return;

            Engine.OnAfterIgnite -= OnIgnite;
            Engine.OnAfterShutdown -= OnShutdown;
        }

        void OnIgnite()
        {
            if( Engine == null )
                return;

            Transform t = this.transform.GetVessel().ReferenceTransform;

            IgnitionAudio?.TryStop();
            IgnitionAudio?.PlayInWorld( t );

            LoopAudio?.TryStop();
            LoopAudio?.PlayInWorld( t );

            ShutdownAudio?.TryStop();
        }

        void OnShutdown()
        {
            if( Engine == null )
                return;

            IgnitionAudio?.TryStop();

            LoopAudio?.TryStop();

            ShutdownAudio?.TryStop();
            ShutdownAudio?.PlayInWorld( this.transform.GetVessel().ReferenceTransform );
        }


        [MapsInheritingFrom( typeof( FRocketEngineAudio ) )]
        public static SerializationMapping FRocketEngineAudioMapping()
        {
            return new MemberwiseSerializationMapping<FRocketEngineAudio>()
                .WithMember( "engine", ObjectContext.Ref, o => o.Engine )
                .WithMember( "ignition_audio", o => o.IgnitionAudio )
                .WithMember( "loop_audio", o => o.LoopAudio )
                .WithMember( "shutdown_audio", o => o.ShutdownAudio );
        }
    }
}