using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Effects.Particles.EmissionShapes
{
    public sealed class HemisphereEmissionShape : IParticleEffectEmissionShape
    {
        public enum SpawnLocation
        {
            Volume,    // fill the volume (or face)
            Shell,     // emit from the shell/edge/surface
        }

        public SpawnLocation SpawnFrom { get; set; } = SpawnLocation.Shell;

        public ConstantEffectValue<float> Radius { get; set; } = new( 1f );
        public ConstantEffectValue<float> InnerRadius { get; set; } = null;

        public void OnInit( ParticleEffectHandle handle )
        {
            var shape = handle.poolItem.particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;

            if( Radius != null ) 
                shape.radius = Radius.Get();
            if( SpawnFrom == SpawnLocation.Volume && InnerRadius != null )
                shape.radiusThickness = Mathf.Clamp01( 1.0f - (InnerRadius.Get() / shape.radius) );
        }

        public void OnUpdate( ParticleEffectHandle handle )
        {
            var shape = handle.poolItem.particleSystem.shape;
            if( Radius != null )
                shape.radius = Radius.Get();
            if( SpawnFrom == SpawnLocation.Volume && InnerRadius != null )
                shape.radiusThickness = Mathf.Clamp01( 1.0f - (InnerRadius.Get() / shape.radius) );
        }

        [MapsInheritingFrom( typeof( HemisphereEmissionShape ) )]
        public static SerializationMapping HemisphereEmissionShapeMapping()
        {
            return new MemberwiseSerializationMapping<HemisphereEmissionShape>()
                .WithMember( "spawn_from", o => o.SpawnFrom )
                .WithMember( "radius", o => o.Radius )
                .WithMember( "inner_radius", o => o.InnerRadius );
        }
    }
}