using System;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Audio
{
    public struct AudioShaper
    {
        public IValueGetter<float> Getter { get; set; }
        public IAudioValueSetter<float> Setter { get; set; }

        /// <summary>
        /// Maps what is returned by the getter to what is passed into the setter.
        /// </summary>
        public AnimationCurve Curve { get; set; }

        [MapsInheritingFrom( typeof( AudioShaper ) )]
        public static SerializationMapping ShaperMapping()
        {
            return new MemberwiseSerializationMapping<AudioShaper>()
                .WithMember( "getter", o => o.Getter )
                .WithMember( "setter", o => o.Setter )
                .WithMember( "curve", o => o.Curve );
        }
    }

    public class AudioEffectPlayer : SingletonMonoBehaviour<AudioEffectPlayer>
    {
        private List<AudioEffectDefinition> _playingEffects = new();

        internal static List<AudioEffectDefinition> playingEffects => instance._playingEffects;


        void Update()
        {
            // check if any of the effects are finished
            for( int i = _playingEffects.Count - 1; i >= 0; i-- )
            {
                var effect = _playingEffects[i];

                if( effect.State == AudioHandleState.Playing )
                {
                    foreach( var shaper in effect.Shapers )
                    {
                        if( shaper.Getter is IValueGetter<float> getter )
                        {
                            float value = getter.Get();
                            float mappedValue = shaper.Curve?.Evaluate( value ) ?? value;
#warning TODO - needs to get the 'max' value corresponding to the value on the audio we want to set, then multiply it by the mapped value to get correct layering.
                            shaper.Setter.Set( effect._handle, mappedValue );
                        }
                    }
                }

                if( effect.State == AudioHandleState.Finished )
                {
                    effect._handle = null;
                    _playingEffects.RemoveAt( i );
                }
            }
        }
    }

    /// <summary>
    /// A 'definition' of an audio effect. <br/>
    /// It can be used to start audio playback using its settings and shaping effects.
    /// </summary>
    /// <remarks>
    /// Each instance of this class can play a single audio clip at a time. <br/><br/>
    /// This class is higher-level than just playing clips directly using the <see cref="AudioManager"/>.
    /// </remarks>
    public class AudioEffectDefinition
    {
        public AudioShaper[] Shapers { get; set; }

        public AudioHandleState State => _handle?.State ?? AudioHandleState.Ready;

        public AudioClip Clip { get; set; }

        public AudioChannel AudioChannel { get; set; }

        public float Volume { get; set; }
        public float Pitch { get; set; }
        public bool Loop { get; set; }

        internal IAudioHandle _handle;

        public void Play()
        {
            _handle = AudioManager.Prepare( this.Clip, this.Loop, this.AudioChannel, this.Volume, this.Pitch );
            AudioEffectPlayer.playingEffects.Add( this );

            foreach( var shaper in Shapers )
            {
                if( shaper.Getter is IAudioInitValueGetter evg )
                    evg.OnInit( _handle );
            }

            _handle.Play();
        }

        public void PlayInWorld( Transform transform )
        {
            _handle = AudioManager.PrepareInWorld( transform, this.Clip, this.Loop, this.AudioChannel, this.Volume, this.Pitch );
            AudioEffectPlayer.playingEffects.Add( this );

            foreach( var shaper in Shapers )
            {
                if( shaper.Getter is IAudioInitValueGetter evg )
                    evg.OnInit( _handle );
            }

            _handle.Play();
        }

        public void TryStop()
        {
            _handle?.TryStop();
            _handle = null;
        }


        [MapsInheritingFrom( typeof( AudioEffectDefinition ) )]
        public static SerializationMapping AudioEffectDefinitionMapping()
        {
            return new MemberwiseSerializationMapping<AudioEffectDefinition>()
                .WithMember( "shapers", o => o.Shapers )
                .WithMember( "audio_clip", ObjectContext.Asset, o => o.Clip )
                .WithMember( "audio_channel", o => o.AudioChannel )
                .WithMember( "volume", o => o.Volume )
                .WithMember( "pitch", o => o.Pitch )
                .WithMember( "loop", o => o.Loop );
        }
    }
}