using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Effects.Lights
{
    public class LightEffectDefinition : ILightEffectData
    {
        /// <summary>
        /// The transform that the playing audio will follow.
        /// </summary>
        public Transform TargetTransform { get; set; }

        //
        // Driven properties:

        public ConstantEffectValue<float> Intensity { get; set; } = new( 1f );

        public void OnInit( LightEffectHandle handle )
        {
            handle.TargetTransform = this.TargetTransform;
        }

        public void OnUpdate( LightEffectHandle handle )
        {
        }

        [MapsInheritingFrom( typeof( LightEffectHandle ) )]
        public static SerializationMapping LightEffectHandleMapping()
        {
            return new MemberwiseSerializationMapping<LightEffectHandle>()
                /*.WithMember( "audio_clip", ObjectContext.Asset, o => o.Clip )
                .WithMember( "audio_channel", o => o.Channel )
                .WithMember( "volume", o => o.Volume )
                .WithMember( "pitch", o => o.Pitch )
                .WithMember( "loop", o => o.Loop )*/;
        }

        public IEffectHandle Play()
        {
            return LightEffectManager.Play( this );
        }
    }
}