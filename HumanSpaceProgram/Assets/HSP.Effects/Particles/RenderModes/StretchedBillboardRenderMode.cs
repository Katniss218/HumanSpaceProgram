
using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Effects.Particles.RenderModes
{
    /// <summary>
    /// Renders the particles as flat quads, but stretched in the direction of motion.
    /// </summary>
    public sealed class StretchedBillboardRenderMode : IParticleEffectRenderMode
    {
        /// <summary>
        /// The base length-to-width ratio of the particles.
        /// </summary>
        public ConstantEffectValue<float> Scale { get; set; } = new( 1.0f );

        /// <summary>
        /// Multiplier to the length, but proportional to the velocity of the particles.
        /// </summary>
        public ConstantEffectValue<float> VelocityScale { get; set; } = new( 1.0f );

        public void OnInit( ParticleEffectHandle handle )
        {
            handle.poolItem.renderer.renderMode = ParticleSystemRenderMode.Stretch;
            handle.poolItem.renderer.enableGPUInstancing = true;

            if( Scale != null )
            {
                Scale.InitDrivers( handle );
                handle.poolItem.renderer.lengthScale = Scale.Get();
            }
            if( VelocityScale != null )
            {
                VelocityScale.InitDrivers( handle );
                handle.poolItem.renderer.velocityScale = VelocityScale.Get();
            }
        }

        public void OnUpdate( ParticleEffectHandle handle )
        {
            if( Scale != null && Scale.drivers != null )
                handle.poolItem.renderer.lengthScale = Scale.Get();
            if( VelocityScale != null && VelocityScale.drivers != null )
                handle.poolItem.renderer.velocityScale = VelocityScale.Get();
        }


        [MapsInheritingFrom( typeof( StretchedBillboardRenderMode ) )]
        public static SerializationMapping StretchedBillboardRenderModeMapping()
        {
            return new MemberwiseSerializationMapping<StretchedBillboardRenderMode>()
                .WithMember( "scale", o => o.Scale )
                .WithMember( "velocity_scale", o => o.VelocityScale );
        }
    }
}