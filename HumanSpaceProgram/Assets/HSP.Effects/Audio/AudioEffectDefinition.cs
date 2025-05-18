using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Effects.Audio
{
    /// <summary>
    /// A 'definition' of an audio effect. <br/>
    /// It can be used to start audio playback using its settings and shaping effects.
    /// </summary>
    /// <remarks>
    /// Each instance of this class can play a single audio clip at a time. <br/><br/>
    /// This class is higher-level than just playing clips directly using the <see cref="AudioEffectManager"/>.
    /// </remarks>
    public class AudioEffectDefinition : IAudioEffectData
    {
        /// <summary>
        /// The audio clip to play.
        /// </summary>
        public AudioClip Clip { get; set; }

        /// <summary>
        /// Whether the audio should loop or not.
        /// </summary>
        public bool Loop { get; set; }

        /// <summary>
        /// The audio channel to use for playback.
        /// </summary>
        public AudioChannel Channel { get; set; }

        /// <summary>
        /// The transform that the playing audio will follow.
        /// </summary>
        public Transform TargetTransform { get; set; }

        //
        // Driven properties:

        public ConstantEffectValue<float> Volume { get; set; } = new( 1f );

        public ConstantEffectValue<float> Pitch { get; set; } = new( 1f );


        public void OnInit( AudioEffectHandle handle )
        {
            handle.Clip = this.Clip;

            handle.Loop = this.Loop;
            handle.Channel = this.Channel;
            handle.TargetTransform = this.TargetTransform;

            // Set non-driven properties first. Properties might need to initialize.

            if( this.Volume != null )
            {
                this.Volume.OnInit( handle );
                handle.Volume = this.Volume.Get();
            }
            if( this.Pitch != null )
            {
                this.Pitch.OnInit( handle );
                handle.Pitch = this.Pitch.Get();
            }
        }

        public void OnUpdate( AudioEffectHandle handle )
        {
            if( this.Volume?.drivers != null )
                handle.Volume = this.Volume.Get();
            if( this.Pitch?.drivers != null )
                handle.Pitch = this.Pitch.Get();
        }

        [MapsInheritingFrom( typeof( AudioEffectDefinition ) )]
        public static SerializationMapping AudioEffectDefinitionMapping()
        {
            return new MemberwiseSerializationMapping<AudioEffectDefinition>()
                // .WithMember( "shapers", o => o.Shapers )
                .WithMember( "audio_clip", ObjectContext.Asset, o => o.Clip )
                .WithMember( "audio_channel", o => o.Channel )
                .WithMember( "volume", o => o.Volume )
                .WithMember( "pitch", o => o.Pitch )
                .WithMember( "loop", o => o.Loop );
        }
    }
}