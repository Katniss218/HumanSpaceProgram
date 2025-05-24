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

            if( this.Intensity != null )
            {
                this.Intensity.InitDrivers( handle );
                handle.Intensity = this.Intensity.Get();
            }
        }

        public void OnUpdate( LightEffectHandle handle )
        {
            if( this.Intensity != null && this.Intensity.drivers != null )
                handle.Intensity = this.Intensity.Get();
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