using UnityEngine;
using UnityPlus.Serialization;

namespace HSP.Effects.Particles.EmissionShapes
{
    public sealed class CircleEmissionShape : IParticleEffectEmissionShape
    {
        public enum SpawnLocation
        {
            Volume,
            Shell
        }

        public SpawnLocation SpawnFrom { get; set; } = SpawnLocation.Volume;

        public ConstantEffectValue<float> Radius { get; set; } = new( 1f );
        /// <summary>
        /// How much of the circle’s circumference to use (0–1).
        /// </summary>
        public ConstantEffectValue<float> InnerRadius { get; set; } = null;

        public void OnInit( ParticleEffectHandle handle )
        {
            var shape = handle.poolItem.particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;

            if( Radius != null ) 
                shape.radius = Radius.Get();
            if( SpawnFrom == SpawnLocation.Volume && InnerRadius != null )
                shape.radiusThickness = Mathf.Clamp01( 1.0f - (InnerRadius.Get() / shape.radius));
        }

        public void OnUpdate( ParticleEffectHandle handle )
        {
            var shape = handle.poolItem.particleSystem.shape;

            if( Radius != null && Radius.drivers != null ) 
                shape.radius = Radius.Get();
            if( SpawnFrom == SpawnLocation.Volume && InnerRadius != null && InnerRadius.drivers != null )
                shape.radiusThickness = Mathf.Clamp01( 1.0f - (InnerRadius.Get() / shape.radius) );
        }

        [MapsInheritingFrom( typeof( CircleEmissionShape ) )]
        public static SerializationMapping CircleEmissionShapeMapping()
        {
            return new MemberwiseSerializationMapping<CircleEmissionShape>()
                .WithMember( "spawn_from", o => o.SpawnFrom )
                .WithMember( "radius", o => o.Radius )
                .WithMember( "inner_radius", o => o.InnerRadius );
        }
    }
}